using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Tests;

namespace LayoutTests
{
    internal class VerticalLayoutGroupTests : IPrebuildSetup
    {
        GameObject m_PrefabRoot;
        const string kPrefabPath = "Assets/Resources/VerticalLayoutGroupPrefab.prefab";

        public void Setup()
        {
#if UNITY_EDITOR
            var rootGO = new GameObject("rootGo");
            GameObject canvasGO = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            canvasGO.transform.SetParent(rootGO.transform);

            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.referencePixelsPerUnit = 100;

            var groupGO = new GameObject("Group", typeof(RectTransform), typeof(VerticalLayoutGroup));
            groupGO.transform.SetParent(canvasGO.transform);

            var element1GO = new GameObject("Element1", typeof(RectTransform), typeof(LayoutElement));
            element1GO.transform.SetParent(groupGO.transform);

            var element2GO = new GameObject("Element2", typeof(RectTransform), typeof(LayoutElement));
            element2GO.transform.SetParent(groupGO.transform);

            var element3GO = new GameObject("Element3", typeof(RectTransform), typeof(LayoutElement));
            element3GO.transform.SetParent(groupGO.transform);

            VerticalLayoutGroup layoutGroup = groupGO.GetComponent<VerticalLayoutGroup>();
            layoutGroup.padding = new RectOffset(2, 4, 3, 5);
            layoutGroup.spacing = 1;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;

            var element1 = element1GO.GetComponent<LayoutElement>();
            element1.minWidth = 5;
            element1.minHeight = 10;
            element1.preferredWidth = 100;
            element1.preferredHeight = 50;
            element1.flexibleWidth = 0;
            element1.flexibleHeight = 0;
            element1.maxWidth = 300;
            element1.maxHeight = 300;
            element1.enabled = true;

            var element2 = element2GO.GetComponent<LayoutElement>();
            element2.minWidth = 10;
            element2.minHeight = 5;
            element2.preferredWidth = -1;
            element2.preferredHeight = -1;
            element2.flexibleWidth = 0;
            element2.flexibleHeight = 0;
            element2.maxWidth = 400;
            element2.maxHeight = 400;
            element2.enabled = true;

            var element3 = element3GO.GetComponent<LayoutElement>();
            element3.minWidth = 25;
            element3.minHeight = 15;
            element3.preferredWidth = 200;
            element3.preferredHeight = 80;
            element3.flexibleWidth = 1;
            element3.flexibleHeight = 1;
            element3.maxWidth = 500;
            element3.maxHeight = 500;
            element3.enabled = true;

            if (!Directory.Exists("Assets/Resources/"))
                Directory.CreateDirectory("Assets/Resources/");

            PrefabUtility.SaveAsPrefabAsset(rootGO, kPrefabPath);
            GameObject.DestroyImmediate(rootGO);
#endif
        }

        [SetUp]
        public void TestSetup()
        {
            m_PrefabRoot = Object.Instantiate(Resources.Load("VerticalLayoutGroupPrefab")) as GameObject;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_PrefabRoot);
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
            var layoutGroup = m_PrefabRoot.GetComponentInChildren<VerticalLayoutGroup>();
            layoutGroup.CalculateLayoutInputHorizontal();
            layoutGroup.SetLayoutHorizontal();
            layoutGroup.CalculateLayoutInputVertical();
            layoutGroup.SetLayoutVertical();

            Assert.AreEqual(31, layoutGroup.minWidth);
            Assert.AreEqual(206, layoutGroup.preferredWidth);
            Assert.AreEqual(1, layoutGroup.flexibleWidth);
            Assert.AreEqual(306, layoutGroup.maxWidth);
        }

        [Test]
        public void TestCalculateLayoutInputVertical()
        {
            var layoutGroup = m_PrefabRoot.GetComponentInChildren<VerticalLayoutGroup>();
            layoutGroup.CalculateLayoutInputHorizontal();
            layoutGroup.SetLayoutHorizontal();
            layoutGroup.CalculateLayoutInputVertical();
            layoutGroup.SetLayoutVertical();

            Assert.AreEqual(40, layoutGroup.minHeight);
            Assert.AreEqual(145, layoutGroup.preferredHeight);
            Assert.AreEqual(1, layoutGroup.flexibleHeight);
            Assert.AreEqual(1210, layoutGroup.maxHeight);
        }

        [Test]
        public void TestCalculateLayoutVertical()
        {
            var parentGO = m_PrefabRoot.transform.GetChild(0).GetChild(0);
            var element1GO = parentGO.GetChild(0);
            var element1Trans = element1GO.GetComponent<RectTransform>();
            var element2GO = parentGO.GetChild(1);
            var element2Trans = element2GO.GetComponent<RectTransform>();
            var element3GO = parentGO.GetChild(2);
            var element3Trans = element3GO.GetComponent<RectTransform>();

            var layoutGroup = m_PrefabRoot.GetComponentInChildren<VerticalLayoutGroup>();
            layoutGroup.CalculateLayoutInputHorizontal();
            layoutGroup.SetLayoutHorizontal();
            layoutGroup.CalculateLayoutInputVertical();
            layoutGroup.SetLayoutVertical();

            Assert.AreEqual(-19.4f, element1Trans.anchoredPosition.y, 0.1f);
            Assert.AreEqual(-39.4f, element2Trans.anchoredPosition.y, 0.1f);
            Assert.AreEqual(-68.9f, element3Trans.anchoredPosition.y, 0.1f);
        }

        [Test]
        public void TestCalculateLayoutVerticalReversed()
        {
            var parentGO = m_PrefabRoot.transform.GetChild(0).GetChild(0);
            var element1GO = parentGO.GetChild(0);
            var element1Trans = element1GO.GetComponent<RectTransform>();
            var element2GO = parentGO.GetChild(1);
            var element2Trans = element2GO.GetComponent<RectTransform>();
            var element3GO = parentGO.GetChild(2);
            var element3Trans = element3GO.GetComponent<RectTransform>();

            var layoutGroup = m_PrefabRoot.GetComponentInChildren<VerticalLayoutGroup>();
            layoutGroup.reverseArrangement = true;
            layoutGroup.CalculateLayoutInputHorizontal();
            layoutGroup.SetLayoutHorizontal();
            layoutGroup.CalculateLayoutInputVertical();
            layoutGroup.SetLayoutVertical();


            //Assert.AreEqual(-78.6f, element1Trans.anchoredPosition.y, 0.1f);
            Assert.AreEqual(-58.6f, element2Trans.anchoredPosition.y, 0.1f);
            Assert.AreEqual(-29.1f, element3Trans.anchoredPosition.y, 0.1f);
        }

        [Test]
        public void TestCalculateLayoutInputMax_UnconstrainedChildren()
        {
            VerticalLayoutGroup layoutGroup = m_PrefabRoot.GetComponentInChildren<VerticalLayoutGroup>();

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
            VerticalLayoutGroup layoutGroup = m_PrefabRoot.GetComponentInChildren<VerticalLayoutGroup>();

            LayoutElement element1 = layoutGroup.transform.GetChild(0).GetComponent<LayoutElement>();
            element1.maxHeight = 30;
            element1.maxWidth = 45;
            LayoutElement element2 = layoutGroup.transform.GetChild(1).GetComponent<LayoutElement>();
            element2.maxHeight = 40;
            element2.maxWidth = 60;
            LayoutElement element3 = layoutGroup.transform.GetChild(2).GetComponent<LayoutElement>();
            element3.maxHeight = 50;
            element3.maxWidth = 70;

            layoutGroup.CalculateLayoutInputHorizontal();
            layoutGroup.SetLayoutHorizontal();
            layoutGroup.CalculateLayoutInputVertical();
            layoutGroup.SetLayoutVertical();

            // Main axis (height): vertical padding + sum of child max heights + spacing between children = 130.
            Assert.AreEqual(130, layoutGroup.maxHeight);
            // Cross axis (width): smallest child maxWidth + horizontal padding = 45 + 6 = 51.
            Assert.AreEqual(51, layoutGroup.maxWidth);
        }

        [Test]
        public void TestCalculateLayoutInputMax_CrossAxisChildIsUnconstrained()
        {
            VerticalLayoutGroup layoutGroup = m_PrefabRoot.GetComponentInChildren<VerticalLayoutGroup>();

            // Element1 leaves maxWidth unset (infinite). On the cross axis max ignores infinity,
            // so element1's unset maxWidth drops out and the remaining finite children decide it.
            LayoutElement element1 = layoutGroup.transform.GetChild(0).GetComponent<LayoutElement>();
            element1.maxHeight = 30;
            element1.maxWidth = -1; // clear the value assigned in the shared setup; leaves maxWidth unset (infinite)
            LayoutElement element2 = layoutGroup.transform.GetChild(1).GetComponent<LayoutElement>();
            element2.maxHeight = 40;
            element2.maxWidth = 60;
            LayoutElement element3 = layoutGroup.transform.GetChild(2).GetComponent<LayoutElement>();
            element3.maxHeight = 50;
            element3.maxWidth = 70;

            layoutGroup.CalculateLayoutInputHorizontal();
            layoutGroup.SetLayoutHorizontal();
            layoutGroup.CalculateLayoutInputVertical();
            layoutGroup.SetLayoutVertical();

            // Main axis (height): all child max heights are set, so it sums to 130.
            Assert.AreEqual(130, layoutGroup.maxHeight);
            // Cross axis (height): smallest child maxHeight + vertical padding = 60 + 6 = 66. Infinity should not be used.
            Assert.AreEqual(66, layoutGroup.maxWidth);
        }
    }
}
