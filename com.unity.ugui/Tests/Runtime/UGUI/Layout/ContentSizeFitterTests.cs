using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.UI.Tests;

namespace LayoutTests
{
    class ContentSizeFitterTests : IPrebuildSetup
    {
        const string kPrefabPath = "Assets/Resources/ContentSizeFitterTests.prefab";

        private GameObject m_PrefabRoot;
        private ContentSizeFitter m_ContentSizeFitter;
        private RectTransform m_RectTransform;

        public void Setup()
        {
#if UNITY_EDITOR
            var rootGO = new GameObject("rootGo");
            GameObject canvasGO = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            canvasGO.transform.SetParent(rootGO.transform);

            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.referencePixelsPerUnit = 100;

            var testGO = new GameObject("TestObject", typeof(RectTransform), typeof(ContentSizeFitter));
            testGO.transform.SetParent(canvasGO.transform);

            if (!Directory.Exists("Assets/Resources/"))
                Directory.CreateDirectory("Assets/Resources/");

            PrefabUtility.SaveAsPrefabAsset(rootGO, kPrefabPath);
            GameObject.DestroyImmediate(rootGO);
#endif
        }

        [SetUp]
        public void TestSetup()
        {
            m_PrefabRoot = Object.Instantiate(Resources.Load("ContentSizeFitterTests")) as GameObject;
            m_ContentSizeFitter = m_PrefabRoot.GetComponentInChildren<ContentSizeFitter>();

            m_ContentSizeFitter.enabled = true;

            m_RectTransform = m_ContentSizeFitter.GetComponent<RectTransform>();
            m_RectTransform.sizeDelta = new Vector2(50, 50);

            GameObject testObject = m_ContentSizeFitter.gameObject;
            // set up components
            var componentA = testObject.AddComponent<LayoutElement>();
            componentA.minWidth = 5;
            componentA.minHeight = 10;
            componentA.preferredWidth = 100;
            componentA.preferredHeight = 105;
            componentA.flexibleWidth = 0;
            componentA.flexibleHeight = 0;
            componentA.maxWidth = 300;
            componentA.maxHeight = 300;
            componentA.enabled = true;

            var componentB = testObject.AddComponent<LayoutElement>();
            componentB.minWidth = 15;
            componentB.minHeight = 20;
            componentB.preferredWidth = 110;
            componentB.preferredHeight = 115;
            componentB.flexibleWidth = 0;
            componentB.flexibleHeight = 0;
            componentB.maxWidth = 300;
            componentB.maxHeight = 300;
            componentB.enabled = true;

            var componentC = testObject.AddComponent<LayoutElement>();
            componentC.minWidth = 25;
            componentC.minHeight = 30;
            componentC.preferredWidth = 120;
            componentC.preferredHeight = 125;
            componentC.flexibleWidth = 0;
            componentC.flexibleHeight = 0;
            componentC.maxWidth = 300;
            componentC.maxHeight = 300;
            componentC.enabled = true;
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(m_PrefabRoot);
            m_PrefabRoot = null;
            m_ContentSizeFitter = null;
            m_RectTransform = null;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
#if UNITY_EDITOR
            AssetDatabase.DeleteAsset(kPrefabPath);
#endif
        }

        [Test]
        public void TestFitModeUnconstrained()
        {
            m_ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            m_ContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            m_ContentSizeFitter.SetLayoutHorizontal();
            m_ContentSizeFitter.SetLayoutVertical();

            Assert.AreEqual(50, m_RectTransform.rect.width);
            Assert.AreEqual(50, m_RectTransform.rect.height);
        }

        [Test]
        public void TestFitModeMinSize()
        {
            m_ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;
            m_ContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

            m_ContentSizeFitter.SetLayoutHorizontal();
            m_ContentSizeFitter.SetLayoutVertical();

            Assert.AreEqual(25, m_RectTransform.rect.width);
            Assert.AreEqual(30, m_RectTransform.rect.height);
        }

        [Test]
        public void TestFitModePreferredSize()
        {
            m_ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            m_ContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            m_ContentSizeFitter.SetLayoutHorizontal();
            m_ContentSizeFitter.SetLayoutVertical();

            Assert.AreEqual(120, m_RectTransform.rect.width);
            Assert.AreEqual(125, m_RectTransform.rect.height);
        }

        [Test]
        public void TestFitModePreferredSize_ClampedToMax()
        {
            // Preferred is 120w,125h. cap it with a smaller max so the preferred path clamps down.
            SetMaxSizeOnAllElements(80, 85);
            m_ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            m_ContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            m_ContentSizeFitter.SetLayoutHorizontal();
            m_ContentSizeFitter.SetLayoutVertical();

            Assert.AreEqual(80, m_RectTransform.rect.width);
            Assert.AreEqual(85, m_RectTransform.rect.height);
        }

        [Test]
        public void TestFitModeClamped_ClampsDownToMax()
        {
            // Current size (50w,50h) is clamped down to the smallest max across all LayoutElements (40w, 45h).
            // Distinct per-element values verify max uses smallest, opposite of min/preferred.
            var elements = m_ContentSizeFitter.GetComponents<LayoutElement>();
            elements[0].maxWidth = 60; elements[0].maxHeight = 70;
            elements[1].maxWidth = 40; elements[1].maxHeight = 90;
            elements[2].maxWidth = 80; elements[2].maxHeight = 45;

            m_ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Clamped;
            m_ContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Clamped;

            m_ContentSizeFitter.SetLayoutHorizontal();
            m_ContentSizeFitter.SetLayoutVertical();

            Assert.AreEqual(40, m_RectTransform.rect.width);
            Assert.AreEqual(45, m_RectTransform.rect.height);
        }

        [Test]
        public void TestFitModeClamped_ClampsUpToMin()
        {
            // Current size below the effective min (25w,30h) with no max constraint.
            SetMaxSizeOnAllElements(-1, -1);
            m_RectTransform.sizeDelta = new Vector2(10, 10);
            m_ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Clamped;
            m_ContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Clamped;

            m_ContentSizeFitter.SetLayoutHorizontal();
            m_ContentSizeFitter.SetLayoutVertical();

            Assert.AreEqual(25, m_RectTransform.rect.width);
            Assert.AreEqual(30, m_RectTransform.rect.height);
        }

        [Test]
        public void TestFitModeClamped_KeepsSizeIfInRange()
        {
            // Current 50x50 sits between min (25w,30h) and max (200w, 200h).
            SetMaxSizeOnAllElements(200, 200);
            m_ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Clamped;
            m_ContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Clamped;

            m_ContentSizeFitter.SetLayoutHorizontal();
            m_ContentSizeFitter.SetLayoutVertical();

            Assert.AreEqual(50, m_RectTransform.rect.width);
            Assert.AreEqual(50, m_RectTransform.rect.height);
        }

        [Test]
        public void TestFitModeClamped_MinGreaterThanMax_MinWins()
        {
            // Max is configured below the effective min (25w, 30h), an invalid min > max state.
            // Current size (50w, 50h) sits above min. The result must honor min, not the invalid max.
            SetMaxSizeOnAllElements(20, 25); // max(20) < min(25) width, max(25) < min(30) height

            m_ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Clamped;
            m_ContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Clamped;

            m_ContentSizeFitter.SetLayoutHorizontal();
            m_ContentSizeFitter.SetLayoutVertical();

            Assert.AreEqual(25, m_RectTransform.rect.width);
            Assert.AreEqual(30, m_RectTransform.rect.height);
        }

        [TestCase(10f, 10f)]   // below min  -> old Clamp already returned min
        [TestCase(100f, 100f)] // above min  -> old Clamp returned the smaller (invalid) max
        public void TestFitModeClamped_MinGreaterThanMax_ConsistentRegardlessOfCurrentSize(float startWidth, float startHeight)
        {
            m_RectTransform.sizeDelta = new Vector2(startWidth, startHeight);
            SetMaxSizeOnAllElements(20, 25); // min (25w, 30h) > max (20w, 25h)

            m_ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Clamped;
            m_ContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Clamped;

            m_ContentSizeFitter.SetLayoutHorizontal();
            m_ContentSizeFitter.SetLayoutVertical();

            Assert.AreEqual(25, m_RectTransform.rect.width);
            Assert.AreEqual(30, m_RectTransform.rect.height);
        }

        // Helper function for setting max width/height on layout elements
        private void SetMaxSizeOnAllElements(float maxWidth, float maxHeight)
        {
            foreach (var element in m_ContentSizeFitter.GetComponents<LayoutElement>())
            {
                element.maxWidth = maxWidth;
                element.maxHeight = maxHeight;
            }
        }
    }
}
