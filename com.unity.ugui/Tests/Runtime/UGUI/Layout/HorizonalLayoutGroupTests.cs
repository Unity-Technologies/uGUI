using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;

namespace LayoutTests
{
    internal class HorizontalLayoutGroupTests : IPrebuildSetup
    {
        GameObject m_PrefabRoot;
        const string kPrefabPath = "Assets/Resources/HorizontalLayoutGroupPrefab.prefab";

        public void Setup()
        {
#if UNITY_EDITOR
            var rootGO = new GameObject("rootGo");
            var canvasGO = new GameObject("Canvas", typeof(Canvas));
            canvasGO.transform.SetParent(rootGO.transform);

            var groupGO = new GameObject("Group", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            groupGO.transform.SetParent(canvasGO.transform);
            var horizontalLayoutGroup = groupGO.GetComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.padding = new RectOffset(2, 4, 3, 5);
            horizontalLayoutGroup.spacing = 1;
            horizontalLayoutGroup.childForceExpandWidth = false;
            horizontalLayoutGroup.childForceExpandHeight = false;
            horizontalLayoutGroup.childControlWidth = true;
            horizontalLayoutGroup.childControlHeight = true;

            var element1GO = new GameObject("Element1", typeof(RectTransform), typeof(LayoutElement));
            element1GO.transform.SetParent(groupGO.transform);
            var layoutElement1 = element1GO.GetComponent<LayoutElement>();
            layoutElement1.minWidth = 5;
            layoutElement1.minHeight = 10;
            layoutElement1.preferredWidth = 100;
            layoutElement1.preferredHeight = 50;
            layoutElement1.flexibleWidth = 0;
            layoutElement1.flexibleHeight = 0;
            layoutElement1.maxWidth = 300;
            layoutElement1.maxHeight = 300;

            var element2GO = new GameObject("Element2", typeof(RectTransform), typeof(LayoutElement));
            element2GO.transform.SetParent(groupGO.transform);
            var layoutElement2 = element2GO.GetComponent<LayoutElement>();
            layoutElement2.minWidth = 10;
            layoutElement2.minHeight = 5;
            layoutElement2.preferredWidth = -1;
            layoutElement2.preferredHeight = -1;
            layoutElement2.flexibleWidth = 0;
            layoutElement2.flexibleHeight = 0;
            layoutElement2.maxWidth = 400;
            layoutElement2.maxHeight = 400;

            var element3GO = new GameObject("Element3", typeof(RectTransform), typeof(LayoutElement));
            element3GO.transform.SetParent(groupGO.transform);
            var layoutElement3 = element3GO.GetComponent<LayoutElement>();
            layoutElement3.minWidth = 25;
            layoutElement3.minHeight = 15;
            layoutElement3.preferredWidth = 200;
            layoutElement3.preferredHeight = 80;
            layoutElement3.flexibleWidth = 1;
            layoutElement3.flexibleHeight = 1;
            layoutElement3.maxWidth = 500;
            layoutElement3.maxHeight = 500;

            if (!Directory.Exists("Assets/Resources/"))
                Directory.CreateDirectory("Assets/Resources/");

            PrefabUtility.SaveAsPrefabAsset(rootGO, kPrefabPath);
            GameObject.DestroyImmediate(rootGO);
#endif
        }

        [SetUp]
        public void TestSetup()
        {
            m_PrefabRoot = Object.Instantiate(Resources.Load("HorizontalLayoutGroupPrefab")) as GameObject;
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(m_PrefabRoot);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
#if UNITY_EDITOR
            AssetDatabase.DeleteAsset(kPrefabPath);
#endif
        }

        [Test]
        public void TestCalculateLayoutInputHorizontal()
        {
            HorizontalLayoutGroup layoutGroup = m_PrefabRoot.GetComponentInChildren<HorizontalLayoutGroup>();
            layoutGroup.CalculateLayoutInputHorizontal();
            layoutGroup.SetLayoutHorizontal();
            layoutGroup.CalculateLayoutInputVertical();
            layoutGroup.SetLayoutVertical();

            Assert.AreEqual(48, layoutGroup.minWidth);
            Assert.AreEqual(318, layoutGroup.preferredWidth);
            Assert.AreEqual(1, layoutGroup.flexibleWidth);
            Assert.AreEqual(1208, layoutGroup.maxWidth);
        }

        [Test]
        public void TestCalculateLayoutInputVertical()
        {
            HorizontalLayoutGroup layoutGroup = m_PrefabRoot.GetComponentInChildren<HorizontalLayoutGroup>();
            layoutGroup.CalculateLayoutInputHorizontal();
            layoutGroup.SetLayoutHorizontal();
            layoutGroup.CalculateLayoutInputVertical();
            layoutGroup.SetLayoutVertical();

            Assert.AreEqual(23, layoutGroup.minHeight);
            Assert.AreEqual(88, layoutGroup.preferredHeight);
            Assert.AreEqual(1, layoutGroup.flexibleHeight);
            Assert.AreEqual(1, layoutGroup.flexibleHeight);
            Assert.AreEqual(308, layoutGroup.maxHeight);
        }

        [Test]
        public void TestCalculateLayoutHorizontal()
        {
            var parentGO = m_PrefabRoot.transform.GetChild(0).GetChild(0);
            var element1GO = parentGO.GetChild(0);
            var element1Trans = element1GO.GetComponent<RectTransform>();
            var element2GO = parentGO.GetChild(1);
            var element2Trans = element2GO.GetComponent<RectTransform>();
            var element3GO = parentGO.GetChild(2);
            var element3Trans = element3GO.GetComponent<RectTransform>();

            HorizontalLayoutGroup layoutGroup = m_PrefabRoot.GetComponentInChildren<HorizontalLayoutGroup>();
            layoutGroup.CalculateLayoutInputHorizontal();
            layoutGroup.SetLayoutHorizontal();
            layoutGroup.CalculateLayoutInputVertical();
            layoutGroup.SetLayoutVertical();

            Assert.AreEqual(13.6f, element1Trans.anchoredPosition.x, 0.1f);
            Assert.AreEqual(31.3f, element2Trans.anchoredPosition.x, 0.1f);
            Assert.AreEqual(66.6f, element3Trans.anchoredPosition.x, 0.1f);
        }

        [Test]
        public void TestCalculateLayoutHorizontalReversed()
        {
            var parentGO = m_PrefabRoot.transform.GetChild(0).GetChild(0);
            var element1GO = parentGO.GetChild(0);
            var element1Trans = element1GO.GetComponent<RectTransform>();
            var element2GO = parentGO.GetChild(1);
            var element2Trans = element2GO.GetComponent<RectTransform>();
            var element3GO = parentGO.GetChild(2);
            var element3Trans = element3GO.GetComponent<RectTransform>();

            HorizontalLayoutGroup layoutGroup = m_PrefabRoot.GetComponentInChildren<HorizontalLayoutGroup>();
            layoutGroup.reverseArrangement = true;
            layoutGroup.CalculateLayoutInputHorizontal();
            layoutGroup.SetLayoutHorizontal();
            layoutGroup.CalculateLayoutInputVertical();
            layoutGroup.SetLayoutVertical();

            Assert.AreEqual(84.4f, element1Trans.anchoredPosition.x, 0.1f);
            Assert.AreEqual(66.7f, element2Trans.anchoredPosition.x, 0.1f);
            Assert.AreEqual(31.4f, element3Trans.anchoredPosition.x, 0.1f);
        }

        [Test]
        public void TestCalculateLayoutInputMax_UnconstrainedChildren()
        {
            HorizontalLayoutGroup layoutGroup = m_PrefabRoot.GetComponentInChildren<HorizontalLayoutGroup>();

            // The shared setup assigns maxes; clear them so no child constrains the group.
            foreach (var element in layoutGroup.GetComponentsInChildren<LayoutElement>())
            {
                element.maxWidth = -1;
                element.maxHeight = -1;
            }

            layoutGroup.CalculateLayoutInputHorizontal();
            layoutGroup.SetLayoutHorizontal();
            layoutGroup.CalculateLayoutInputVertical();
            layoutGroup.SetLayoutVertical();

            // No child sets a max, so the group is unconstrained on both axes.
            Assert.AreEqual(LayoutUtility.DefaultMaxSize, layoutGroup.maxWidth);
            Assert.AreEqual(LayoutUtility.DefaultMaxSize, layoutGroup.maxHeight);
        }

        [Test]
        public void TestCalculateLayoutInputMax()
        {
            HorizontalLayoutGroup layoutGroup = m_PrefabRoot.GetComponentInChildren<HorizontalLayoutGroup>();

            LayoutElement element1 = layoutGroup.transform.GetChild(0).GetComponent<LayoutElement>();
            element1.maxWidth = 30;
            element1.maxHeight = 45;
            LayoutElement element2 = layoutGroup.transform.GetChild(1).GetComponent<LayoutElement>();
            element2.maxWidth = 40;
            element2.maxHeight = 60;
            LayoutElement element3 = layoutGroup.transform.GetChild(2).GetComponent<LayoutElement>();
            element3.maxWidth = 50;
            element3.maxHeight = 70;

            layoutGroup.CalculateLayoutInputHorizontal();
            layoutGroup.SetLayoutHorizontal();
            layoutGroup.CalculateLayoutInputVertical();
            layoutGroup.SetLayoutVertical();

            // Main axis (width): horizontal padding + sum of child max widths + spacing between children = 128.
            Assert.AreEqual(128, layoutGroup.maxWidth);
            // Cross axis (height): smallest child maxHeight + vertical padding = 45 + 8 = 53.
            Assert.AreEqual(53, layoutGroup.maxHeight);
        }

        [Test]
        public void TestCalculateLayoutInputMax_CrossAxisChildIsUnconstrained()
        {
            HorizontalLayoutGroup layoutGroup = m_PrefabRoot.GetComponentInChildren<HorizontalLayoutGroup>();

            LayoutElement element1 = layoutGroup.transform.GetChild(0).GetComponent<LayoutElement>();
            element1.maxWidth = 30;
            element1.maxHeight = -1;
            LayoutElement element2 = layoutGroup.transform.GetChild(1).GetComponent<LayoutElement>();
            element2.maxWidth = 40;
            element2.maxHeight = 60;
            LayoutElement element3 = layoutGroup.transform.GetChild(2).GetComponent<LayoutElement>();
            element3.maxWidth = 50;
            element3.maxHeight = 70;

            layoutGroup.CalculateLayoutInputHorizontal();
            layoutGroup.SetLayoutHorizontal();
            layoutGroup.CalculateLayoutInputVertical();
            layoutGroup.SetLayoutVertical();

            // Main axis (width): all child max widths are set, so it sums to 128.
            Assert.AreEqual(128, layoutGroup.maxWidth);
            // Cross axis (height): smallest child maxHeight + vertical padding = 60 + 8 = 68. Infinity should not be used.
            Assert.AreEqual(68, layoutGroup.maxHeight);
        }
    }
}
