using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace LayoutTests
{
    // Test set covering the maximum and preferred size a Horizontal or Vertical LayoutGroup reports.
    // Children the group does not resize (Control Child Size off) must not limit the group's size on
    // either axis, and when the children's sizes conflict the minimum wins over the maximum.
    [UnityPlatform()]
    [Category("RegressionTest")]
    [Description("CoveredBugID = UUM-159240 Vertical and Horizontal Layout Groups report a cross-axis preferred size smaller than their largest child when child sizes differ and Control Child Size is off")]
    internal class HorizontalOrVerticalLayoutGroupMaxSizeTests
    {
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

        // With child control off a child keeps its own size, so the group is as wide as its widest
        // child plus padding, and the narrower child does not cap it, whatever the child order.
        [TestCase(100f, 200f)]
        [TestCase(200f, 100f)]
        public void FixedSizeChildren_VerticalGroupCrossAxisFitsWidestChild(float firstWidth, float secondWidth)
        {
            var group = CreateFixedSizeGroup<VerticalLayoutGroup>(m_Canvas.transform, new RectOffset(2, 4, 3, 5),
                new Vector2(firstWidth, 30), new Vector2(secondWidth, 30));

            group.CalculateLayoutInputHorizontal();

            // Widest child 200 plus padding 2 + 4.
            Assert.AreEqual(206, group.minWidth);
            Assert.AreEqual(206, group.preferredWidth);
            Assert.AreEqual(LayoutUtility.DefaultMaxSize, group.maxWidth);
        }

        [Test]
        public void FixedSizeChildren_HorizontalGroupCrossAxisFitsTallestChild()
        {
            var group = CreateFixedSizeGroup<HorizontalLayoutGroup>(m_Canvas.transform, new RectOffset(),
                new Vector2(50, 30), new Vector2(50, 60));

            group.CalculateLayoutInputHorizontal();
            group.CalculateLayoutInputVertical();

            Assert.AreEqual(60, group.minHeight);
            Assert.AreEqual(60, group.preferredHeight);
            Assert.AreEqual(LayoutUtility.DefaultMaxSize, group.maxHeight);
        }

        // On the main axis the min and preferred sizes still sum the children's sizes, but the group
        // does not resize them, so extra space only realigns them and there is no maximum.
        [Test]
        public void FixedSizeChildren_MainAxisMaxIsUnbounded()
        {
            var group = CreateFixedSizeGroup<VerticalLayoutGroup>(m_Canvas.transform, new RectOffset(),
                new Vector2(100, 30), new Vector2(200, 30));

            group.CalculateLayoutInputHorizontal();
            group.CalculateLayoutInputVertical();

            Assert.AreEqual(60, group.minHeight);
            Assert.AreEqual(60, group.preferredHeight);
            Assert.AreEqual(LayoutUtility.DefaultMaxSize, group.maxHeight);
        }

        // A group of fixed-size children nested in a group that controls its children must not cap
        // the parent below a sibling's preferred width.
        [Test]
        public void NestedFixedSizeGroup_DoesNotCapParentPreferredWidth()
        {
            var parent = CreateControllingGroup<VerticalLayoutGroup>(m_Canvas.transform);
            CreateFixedSizeGroup<VerticalLayoutGroup>(parent.transform, new RectOffset(),
                new Vector2(100, 30), new Vector2(200, 30));
            var wide = CreateElement(parent.transform);
            wide.preferredWidth = 300;
            wide.preferredHeight = 30;

            LayoutRebuilder.ForceRebuildLayoutImmediate(parent.transform as RectTransform);

            Assert.AreEqual(300, LayoutUtility.GetPreferredWidth(parent.transform as RectTransform));
        }

        // Same as above with a horizontal group, whose width is its main axis: the sum of its
        // fixed-size children must not cap the parent either.
        [Test]
        public void NestedFixedSizeHorizontalGroup_DoesNotCapParentPreferredWidth()
        {
            var parent = CreateControllingGroup<VerticalLayoutGroup>(m_Canvas.transform);
            CreateFixedSizeGroup<HorizontalLayoutGroup>(parent.transform, new RectOffset(),
                new Vector2(100, 30), new Vector2(100, 30));
            var wide = CreateElement(parent.transform);
            wide.preferredWidth = 300;
            wide.preferredHeight = 30;

            LayoutRebuilder.ForceRebuildLayoutImmediate(parent.transform as RectTransform);

            Assert.AreEqual(300, LayoutUtility.GetPreferredWidth(parent.transform as RectTransform));
        }

        // One child's maximum is below another child's minimum. The group cannot be both, so the
        // minimum wins, as in ContentSizeFitter's Clamped mode.
        [Test]
        public void ControlledChildren_SiblingMaxBelowSiblingMin_MinWins()
        {
            var group = CreateControllingGroup<VerticalLayoutGroup>(m_Canvas.transform);
            CreateElement(group.transform).minWidth = 200;
            CreateElement(group.transform).maxWidth = 100;

            group.CalculateLayoutInputHorizontal();

            Assert.AreEqual(200, group.minWidth);
            Assert.AreEqual(200, group.maxWidth);
            Assert.AreEqual(200, group.preferredWidth);
        }

        [Test]
        public void ControlledChildren_ChildMaxBelowItsOwnMin_MinWins()
        {
            var group = CreateControllingGroup<VerticalLayoutGroup>(m_Canvas.transform);
            var element = CreateElement(group.transform);
            element.minWidth = 200;
            element.maxWidth = 100;

            group.CalculateLayoutInputHorizontal();

            Assert.AreEqual(200, group.maxWidth);
            Assert.AreEqual(200, group.preferredWidth);
        }

        // Child control off is the default when the component is added in the Editor.
        static T CreateFixedSizeGroup<T>(Transform parent, RectOffset padding, params Vector2[] childSizes)
            where T : HorizontalOrVerticalLayoutGroup
        {
            var group = CreateGroup<T>(parent, padding, false);
            foreach (var size in childSizes)
            {
                var child = new GameObject("FixedSizeChild").AddComponent<RectTransform>();
                child.SetParent(group.transform, false);
                child.sizeDelta = size;
            }
            return group;
        }

        static T CreateControllingGroup<T>(Transform parent) where T : HorizontalOrVerticalLayoutGroup
        {
            return CreateGroup<T>(parent, new RectOffset(), true);
        }

        static T CreateGroup<T>(Transform parent, RectOffset padding, bool controlChildSize)
            where T : HorizontalOrVerticalLayoutGroup
        {
            var rectTransform = new GameObject(typeof(T).Name).AddComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            var group = rectTransform.gameObject.AddComponent<T>();
            group.padding = padding;
            group.spacing = 0;
            group.childControlWidth = controlChildSize;
            group.childControlHeight = controlChildSize;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = false;
            return group;
        }

        static LayoutElement CreateElement(Transform parent)
        {
            var rectTransform = new GameObject("Element").AddComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            return rectTransform.gameObject.AddComponent<LayoutElement>();
        }
    }
}
