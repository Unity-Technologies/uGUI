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
    }
}
