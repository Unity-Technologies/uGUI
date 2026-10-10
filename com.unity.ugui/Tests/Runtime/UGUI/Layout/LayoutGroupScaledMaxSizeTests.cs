using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace LayoutTests
{
    // Test set covering how child scaling interacts with maximum size. An undeclared maximum
    // should keep meaning unbounded under any scale, never degrading into NaN or a finite
    // measurement.
    [UnityPlatform()]
    internal class LayoutGroupScaledMaxSizeTests
    {
        internal enum ChildKind
        {
            Image,
            EmptyLayoutGroup
        }

        enum ComponentOrder
        {
            GroupOnly,
            GroupThenImage,
            ImageThenGroup
        }

        Canvas m_Canvas;

        [SetUp]
        public void TestSetup()
        {
            m_Canvas = new GameObject("Canvas").AddComponent<Canvas>();
            m_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(m_Canvas.gameObject);
        }

        // Test layout group with child that has 0 scale does not result in a NaN value on both the
        // component and utility (object) level.
        [TestCase(ChildKind.Image)]
        [TestCase(ChildKind.EmptyLayoutGroup)]
        public void ZeroScaledChildWithNoDeclaredMaximumDoesNotMakeMaxWidthNaN(ChildKind childKind)
        {
            var group = CreateScalingGroup(m_Canvas.transform, ComponentOrder.GroupOnly);
            AddChildWithoutDeclaredMaximum(group.transform, childKind, Vector3.zero);

            group.CalculateLayoutInputHorizontal();

            Assert.AreEqual(
                LayoutUtility.DefaultMaxSize,
                group.maxWidth,
                "LayoutGroup reporting incorrect values. Check on the component level."
            );
            Assert.AreEqual(
                LayoutUtility.DefaultMaxSize,
                LayoutUtility.GetMaxWidth(group.transform as RectTransform),
                "LayoutUtility reporting incorrect values. Check on the utility level."
            );
        }

        // The resolved maximum does not depend on the order components sit in on the GameObject.
        [TestCase(ChildKind.Image)]
        [TestCase(ChildKind.EmptyLayoutGroup)]
        public void ComponentOrderDoesNotChangeResolvedMaxWidth(ChildKind childKind)
        {
            var groupBeforeImage = CreateScalingGroup(m_Canvas.transform, ComponentOrder.GroupThenImage);
            AddChildWithoutDeclaredMaximum(groupBeforeImage.transform, childKind, Vector3.zero);
            groupBeforeImage.CalculateLayoutInputHorizontal();

            var imageBeforeGroup = CreateScalingGroup(m_Canvas.transform, ComponentOrder.ImageThenGroup);
            AddChildWithoutDeclaredMaximum(imageBeforeGroup.transform, childKind, Vector3.zero);
            imageBeforeGroup.CalculateLayoutInputHorizontal();

            float resolvedWithGroupFirst = LayoutUtility.GetMaxWidth(groupBeforeImage.transform as RectTransform);
            float resolvedWithImageFirst = LayoutUtility.GetMaxWidth(imageBeforeGroup.transform as RectTransform);

            Assert.AreEqual(resolvedWithImageFirst, resolvedWithGroupFirst);
        }

        // A group's non-primary axis accumulates with Mathf.Min, which returns the non-NaN
        // operand, so only the primary axis can carry a NaN outward.
        [TestCase(ChildKind.Image)]
        [TestCase(ChildKind.EmptyLayoutGroup)]
        public void ZeroScaledChildDoesNotMakeGroupMaxHeightNaN(ChildKind childKind)
        {
            var group = CreateScalingGroup(m_Canvas.transform, ComponentOrder.GroupOnly);
            AddChildWithoutDeclaredMaximum(group.transform, childKind, Vector3.zero);

            group.CalculateLayoutInputHorizontal();
            group.CalculateLayoutInputVertical();

            Assert.AreEqual(LayoutUtility.DefaultMaxSize, group.maxHeight);
        }

        // A component reporting NaN is ignored, the same as one reporting a negative value.
        [Test]
        public void ResolvedMaxWidthIgnoresAComponentReportingNaN()
        {
            var rectTransform = new GameObject("NaNReporter").AddComponent<RectTransform>();
            rectTransform.SetParent(m_Canvas.transform, false);
            rectTransform.gameObject.AddComponent<NaNMaximumLayoutElement>();

            Assert.AreEqual(LayoutUtility.DefaultMaxSize, LayoutUtility.GetMaxWidth(rectTransform));
        }

        // A maximum the child actually declared is still scaled with the child.
        [Test]
        public void ScaledChildStillScalesADeclaredMaximumWidth()
        {
            var group = CreateScalingGroup(m_Canvas.transform, ComponentOrder.GroupOnly);
            var child = AddChildWithoutDeclaredMaximum(
                group.transform,
                ChildKind.Image,
                new Vector3(0.5f, 0.5f, 1f));
            var childSizing = child.gameObject.AddComponent<LayoutElement>();
            childSizing.preferredWidth = 100;
            childSizing.maxWidth = 80;

            group.CalculateLayoutInputHorizontal();

            Assert.AreEqual(40, group.maxWidth);
        }

        static HorizontalLayoutGroup CreateScalingGroup(Transform parent, ComponentOrder componentOrder)
        {
            var rectTransform = new GameObject("ScalingGroup").AddComponent<RectTransform>();
            rectTransform.SetParent(parent, false);

            if (componentOrder == ComponentOrder.ImageThenGroup)
            {
                rectTransform.gameObject.AddComponent<Image>();
            }

            var group = rectTransform.gameObject.AddComponent<HorizontalLayoutGroup>();
            ConfigureGroup(group, true);

            if (componentOrder == ComponentOrder.GroupThenImage)
            {
                rectTransform.gameObject.AddComponent<Image>();
            }

            return group;
        }

        static RectTransform AddChildWithoutDeclaredMaximum(Transform parent, ChildKind childKind, Vector3 localScale)
        {
            var rectTransform = new GameObject("Child").AddComponent<RectTransform>();
            rectTransform.SetParent(parent, false);

            if (childKind == ChildKind.Image)
            {
                rectTransform.gameObject.AddComponent<Image>();
            }
            else
            {
                var childGroup = rectTransform.gameObject.AddComponent<HorizontalLayoutGroup>();
                ConfigureGroup(childGroup, false);

                // A LayoutGroup reports its field defaults until CalculateLayoutInput* runs, so
                // the child computes its own inputs here and the parent reads calculated values.
                childGroup.CalculateLayoutInputHorizontal();
                childGroup.CalculateLayoutInputVertical();
            }

            rectTransform.localScale = localScale;
            return rectTransform;
        }

        static void ConfigureGroup(HorizontalOrVerticalLayoutGroup group, bool scaleChildren)
        {
            group.padding = new RectOffset();
            group.spacing = 0;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = false;
            group.childScaleWidth = scaleChildren;
            group.childScaleHeight = scaleChildren;
        }

        class NaNMaximumLayoutElement : MonoBehaviour, ILayoutElement
        {
            public void CalculateLayoutInputHorizontal() { }
            public void CalculateLayoutInputVertical() { }

            public float minWidth { get { return 0; } }
            public float preferredWidth { get { return 0; } }
            public float flexibleWidth { get { return 0; } }
            public float maxWidth { get { return float.NaN; } }
            public float minHeight { get { return 0; } }
            public float preferredHeight { get { return 0; } }
            public float flexibleHeight { get { return 0; } }
            public float maxHeight { get { return float.NaN; } }
            public int layoutPriority { get { return 0; } }
        }
    }
}
