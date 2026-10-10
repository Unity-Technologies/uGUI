using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace LayoutTests
{
    [UnityPlatform()]
    internal class LayoutGroupTests
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

        [UnityTest]
        public IEnumerator EmptyRecttransformUpdatesLayoutGroup()
        {
            // Canvas
            // ....LayoutGroup
            // ........"pure" RectTransform <--- we modify the dimensions of this
            // ........Image

            // Create VerticalLayoutGroup with spacing=10
            var layoutGroup = new GameObject("LayoutGroup").AddComponent<RectTransform>();
            layoutGroup.pivot = new Vector2(0, 1);
            layoutGroup.SetParent(m_Canvas.transform, false);
            var verticalLayoutGroup = layoutGroup.gameObject.AddComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.childForceExpandWidth = false;
            verticalLayoutGroup.childForceExpandHeight = false;
            verticalLayoutGroup.childControlWidth = false;
            verticalLayoutGroup.childControlHeight = false;
            verticalLayoutGroup.spacing = 10;

            // Add an empty RectTransfrom with height=100 to the layout group
            var emptyChild = new GameObject("EmptyRectTransform").AddComponent<RectTransform>();
            emptyChild.pivot = new Vector2(0, 1);
            emptyChild.SetParent(layoutGroup, false);
            emptyChild.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100f);

            // Add an image as a sibbling after the empty RectTransform
            var image = new GameObject("Image").AddComponent<RectTransform>();
            image.pivot = new Vector2(0, 1);
            image.transform.SetParent(layoutGroup, false);
            image.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100f);
            image.gameObject.AddComponent<Image>();

            yield return null;

            // Verify the test setup
            Assert.AreEqual(0f, emptyChild.anchoredPosition.y);
            Assert.AreEqual(-110f, image.anchoredPosition.y); // -10 for spacing

            // Expand the empty child
            emptyChild.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 200f);

            yield return null;

            // Expanding the empty child should have triggered the layout group to rebuild and push the image down
            Assert.AreEqual(0f, emptyChild.anchoredPosition.y);
            Assert.AreEqual(-210f, image.anchoredPosition.y);
        }

        // LayoutGroup.Reset turns childControl* off when the component is added, so the sizing
        // flags are set explicitly rather than inherited from the serialized defaults.
        static T CreateGroup<T>(Transform parent, RectOffset padding) where T : HorizontalOrVerticalLayoutGroup
        {
            var rectTransform = new GameObject(typeof(T).Name).AddComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            var group = rectTransform.gameObject.AddComponent<T>();
            group.padding = padding;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = false;
            return group;
        }

        static LayoutElement CreateElement(Transform parent, float preferredWidth)
        {
            var rectTransform = new GameObject("Element").AddComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            var element = rectTransform.gameObject.AddComponent<LayoutElement>();
            element.preferredWidth = preferredWidth;
            return element;
        }

        // A group with no children has no content, so it imposes no upper bound on its own
        // size: the maximum is DefaultMaxSize, not the padding.
        [Test]
        public void EmptyHorizontalLayoutGroupDoesNotConstrainMaxWidth()
        {
            var group = CreateGroup<HorizontalLayoutGroup>(m_Canvas.transform, new RectOffset(2, 4, 3, 5));

            group.CalculateLayoutInputHorizontal();

            Assert.AreEqual(LayoutUtility.DefaultMaxSize, group.maxWidth);
        }

        [Test]
        public void EmptyVerticalLayoutGroupDoesNotConstrainMaxHeight()
        {
            var group = CreateGroup<VerticalLayoutGroup>(m_Canvas.transform, new RectOffset(2, 4, 3, 5));

            group.CalculateLayoutInputVertical();

            Assert.AreEqual(LayoutUtility.DefaultMaxSize, group.maxHeight);
        }

        // rectChildren excludes inactive children, so a group whose children are all disabled
        // is an empty group.
        [Test]
        public void HorizontalLayoutGroupWithOnlyInactiveChildrenDoesNotConstrainMaxWidth()
        {
            var group = CreateGroup<HorizontalLayoutGroup>(m_Canvas.transform, new RectOffset(2, 4, 3, 5));
            CreateElement(group.transform, 100).gameObject.SetActive(false);
            CreateElement(group.transform, 200).gameObject.SetActive(false);

            group.CalculateLayoutInputHorizontal();

            Assert.AreEqual(LayoutUtility.DefaultMaxSize, group.maxWidth);
        }

        // LayoutElement (priority 1) supplies the preferred width and leaves maxWidth unset, so
        // the maximum resolves from the LayoutGroup (priority 0) on the same GameObject. An empty
        // group must not cap that preferred width.
        [Test]
        public void EmptyLayoutGroupDoesNotClampLayoutElementPreferredWidthOnSameGameObject()
        {
            var group = CreateGroup<HorizontalLayoutGroup>(m_Canvas.transform, new RectOffset());
            var element = group.gameObject.AddComponent<LayoutElement>();
            element.preferredWidth = 180;

            group.CalculateLayoutInputHorizontal();

            Assert.AreEqual(180, LayoutUtility.GetPreferredWidth(group.transform as RectTransform));
        }

        // On a group's non-primary axis the maximum is a Mathf.Min across children, so an empty
        // descendant must not cap the parent below what its siblings ask for.
        [Test]
        public void EmptyChildGroupDoesNotCollapseParentPreferredWidth()
        {
            var parent = CreateGroup<VerticalLayoutGroup>(m_Canvas.transform, new RectOffset());
            CreateElement(parent.transform, 180);
            CreateGroup<HorizontalLayoutGroup>(parent.transform, new RectOffset());

            LayoutRebuilder.ForceRebuildLayoutImmediate(parent.transform as RectTransform);

            Assert.AreEqual(180, parent.preferredWidth);
        }

        // A child that genuinely declares a maximum still caps the group.
        [Test]
        public void NonEmptyLayoutGroupStillHonoursChildMaxWidth()
        {
            var group = CreateGroup<HorizontalLayoutGroup>(m_Canvas.transform, new RectOffset());
            var element = CreateElement(group.transform, 200);
            element.maxWidth = 50;

            group.CalculateLayoutInputHorizontal();

            Assert.AreEqual(50, group.maxWidth);
            Assert.AreEqual(50, group.preferredWidth);
        }
    }
}
