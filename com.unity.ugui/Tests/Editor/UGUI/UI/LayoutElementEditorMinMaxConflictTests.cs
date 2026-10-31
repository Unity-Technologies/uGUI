using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace UnityEditor.UI.Tests
{
    internal class LayoutElementEditorMinMaxConflictTests
    {
        const string k_HelpBoxName = "MinMaxConflictHelpBox";

        GameObject m_GameObject;
        LayoutElement m_LayoutElement;
        Editor m_Editor;
        EditorWindow m_Window;
        VisualElement m_Root;

        [SetUp]
        public void SetUp()
        {
            m_GameObject = new GameObject("Element", typeof(RectTransform));
            m_LayoutElement = m_GameObject.AddComponent<LayoutElement>();
        }

        [TearDown]
        public void TearDown()
        {
            if (m_Window != null)
                m_Window.Close();
            if (m_Editor != null)
                Object.DestroyImmediate(m_Editor);
            Object.DestroyImmediate(m_GameObject);
        }

        [Test]
        public void NoConflictHidesTheWarning()
        {
            OpenInspector(m_LayoutElement);

            Assert.AreEqual(DisplayStyle.None, ConflictHelpBox().style.display.value);
        }

        [Test]
        public void MinAboveMaxShowsTheWarning()
        {
            m_LayoutElement.minWidth = 90;
            m_LayoutElement.maxWidth = 1;

            OpenInspector(m_LayoutElement);

            var helpBox = ConflictHelpBox();
            Assert.AreEqual(DisplayStyle.Flex, helpBox.style.display.value, "A minimum above the maximum was not flagged.");
            StringAssert.Contains("'Element' has a minimum width of 90 (from LayoutElement) greater than its maximum width of 1 (from LayoutElement).", helpBox.text);
            StringAssert.Contains("This will result in undefined behaviour.", helpBox.text);
            Assert.Greater(helpBox.style.marginBottom.value.value, 0f, "The warning has no spacing before the fields below it.");
        }

        // The minimum and the maximum may come from different components on the GameObject.
        [Test]
        public void ConflictAcrossTwoLayoutElementsShowsTheWarning()
        {
            m_LayoutElement.minWidth = 90;
            m_GameObject.AddComponent<LayoutElement>().maxWidth = 1;

            OpenInspector(m_LayoutElement);

            Assert.AreEqual(DisplayStyle.Flex, ConflictHelpBox().style.display.value, "A conflict across two components was not flagged.");
        }

        // With several LayoutElements selected, a conflict on any one of them is flagged.
        [Test]
        public void ConflictOnSecondSelectedElementShowsTheWarning()
        {
            var second = new GameObject("Second", typeof(RectTransform)).AddComponent<LayoutElement>();
            try
            {
                second.minHeight = 90;
                second.maxHeight = 1;

                OpenInspector(m_LayoutElement, second);

                Assert.AreEqual(DisplayStyle.Flex, ConflictHelpBox().style.display.value, "A conflict on the second selection was not flagged.");
            }
            finally
            {
                Object.DestroyImmediate(second.gameObject);
            }
        }

        // The per-component message is long; with several conflicting components the box summarizes instead.
        [Test]
        public void MultipleConflictingComponentsSummarizeInsteadOfListingEach()
        {
            m_LayoutElement.minWidth = 90;
            m_LayoutElement.maxWidth = 1;
            var second = new GameObject("Second", typeof(RectTransform)).AddComponent<LayoutElement>();
            try
            {
                second.minHeight = 90;
                second.maxHeight = 1;

                OpenInspector(m_LayoutElement, second);

                var helpBox = ConflictHelpBox();
                Assert.AreEqual(DisplayStyle.Flex, helpBox.style.display.value, "Conflicts on the selection were not flagged.");
                StringAssert.Contains("2", helpBox.text, "The summary does not count the conflicting components.");
                Assert.IsFalse(helpBox.text.Contains("has a minimum"), "Every conflict is listed in full.");
            }
            finally
            {
                Object.DestroyImmediate(second.gameObject);
            }
        }

        // Breaking the minimum and maximum while the Inspector is open flags it without reopening.
        [UnityTest]
        public IEnumerator WarningAppearsWhenValuesChangeWhileOpen()
        {
            OpenInspector(m_LayoutElement);
            m_LayoutElement.minWidth = 90;
            m_LayoutElement.maxWidth = 1;

            double deadline = EditorApplication.timeSinceStartup + 2;
            while (ConflictHelpBox().style.display.value != DisplayStyle.Flex && EditorApplication.timeSinceStartup < deadline)
                yield return null;

            Assert.AreEqual(DisplayStyle.Flex, ConflictHelpBox().style.display.value, "The warning did not appear after the values changed.");
        }

        void OpenInspector(params Object[] targets)
        {
            m_Editor = Editor.CreateEditor(targets);
            m_Root = m_Editor.CreateInspectorGUI();
            m_Window = EditorWindow.CreateWindow<EditorWindow>();
            m_Window.rootVisualElement.Add(m_Root);
            m_Window.Show();
        }

        HelpBox ConflictHelpBox()
        {
            var helpBox = m_Root.Q<HelpBox>(k_HelpBoxName);
            Assert.IsNotNull(helpBox, "The LayoutElement Inspector has no minimum/maximum conflict warning.");
            return helpBox;
        }
    }
}
