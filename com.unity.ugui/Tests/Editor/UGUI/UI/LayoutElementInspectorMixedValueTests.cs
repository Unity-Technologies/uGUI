using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Toggle = UnityEngine.UIElements.Toggle;

namespace UnityEditor.UI.Tests
{
    // These assert the state the row is built with, which the inspector applies synchronously - hence no frames and
    // no EditorWindow. LayoutElementEditorTests covers the binding-driven re-sync path.
    internal class LayoutElementInspectorMixedValueTests
    {
        const string k_MinWidth = "Min Width";
        const string k_MaxWidth = "Max Width";

        readonly List<GameObject> m_GameObjects = new List<GameObject>();
        Editor m_Editor;
        VisualElement m_Root;

        [TearDown]
        public void TearDown()
        {
            if (m_Editor != null)
                Object.DestroyImmediate(m_Editor);
            m_Editor = null;
            m_Root = null;

            foreach (var gameObject in m_GameObjects)
            {
                if (gameObject != null)
                    Object.DestroyImmediate(gameObject);
            }
            m_GameObjects.Clear();
        }

        LayoutElement CreateLayoutElement()
        {
            var gameObject = new GameObject("LayoutElement", typeof(RectTransform));
            m_GameObjects.Add(gameObject);
            return gameObject.AddComponent<LayoutElement>();
        }

        void CreateInspectorFor(params LayoutElement[] elements)
        {
            var targets = new Object[elements.Length];
            for (int i = 0; i < elements.Length; i++)
                targets[i] = elements[i];

            m_Editor = Editor.CreateEditor(targets);
            Assume.That(m_Editor, Is.Not.Null, "Failed to create an editor for the LayoutElement selection.");

            m_Root = m_Editor.CreateInspectorGUI();
            Assume.That(m_Root, Is.Not.Null, "The LayoutElement inspector should be built with UI Toolkit.");
        }

        (Toggle toggle, FloatField field) RowFor(string label)
        {
            var labelElement = m_Root.Query<Label>().Where(l => l.text == label).First();
            Assume.That(labelElement, Is.Not.Null, $"No '{label}' row found in the LayoutElement inspector.");

            var row = labelElement.parent;
            return (row.Q<Toggle>(), row.Q<FloatField>());
        }

        [Test]
        public void SingleObject_EnabledField_ShowsFieldWithoutMixedIndicator()
        {
            var element = CreateLayoutElement();
            element.minWidth = 100f;

            CreateInspectorFor(element);
            var (toggle, field) = RowFor(k_MinWidth);

            Assert.That(toggle.value, Is.True, "A LayoutElement with a usable 'Min Width' should show the row as enabled.");
            Assert.That(toggle.showMixedValue, Is.False, "A single selected object can never be mixed.");
            Assert.That(field.showMixedValue, Is.False, "A single selected object can never be mixed.");
            Assert.That(field.style.display.value, Is.EqualTo(DisplayStyle.Flex), "An enabled 'Min Width' should show its float field.");
        }

        [Test]
        public void SingleObject_DisabledField_HidesFieldWithoutMixedIndicator()
        {
            var element = CreateLayoutElement();
            element.minWidth = -1f;

            CreateInspectorFor(element);
            var (toggle, field) = RowFor(k_MinWidth);

            Assert.That(toggle.value, Is.False, "The disabled sentinel should show the row as disabled.");
            Assert.That(toggle.showMixedValue, Is.False, "A single selected object can never be mixed.");
            Assert.That(field.showMixedValue, Is.False, "A single selected object can never be mixed.");
            Assert.That(field.style.display.value, Is.EqualTo(DisplayStyle.None), "A disabled 'Min Width' should hide its float field.");
        }

        [Test]
        public void MultiObject_DivergentEnabledState_ShowsMixedToggleAndKeepsFieldVisible()
        {
            var disabled = CreateLayoutElement();
            var enabled = CreateLayoutElement();
            disabled.minWidth = -1f;
            enabled.minWidth = 100f;

            CreateInspectorFor(disabled, enabled);
            var (toggle, field) = RowFor(k_MinWidth);

            Assert.That(toggle.showMixedValue, Is.True,
                "Selecting an enabled and a disabled 'Min Width' should mark the toggle as mixed.");
            Assert.That(field.showMixedValue, Is.True,
                "Divergent 'Min Width' values should mark the field as mixed.");
            Assert.That(field.style.display.value, Is.EqualTo(DisplayStyle.Flex),
                "The field must stay visible when the enabled state is mixed, otherwise the mixed indicator is hidden from the user.");
        }

        [Test]
        public void MultiObject_AllDisabled_HidesFieldWithoutMixedIndicator()
        {
            var first = CreateLayoutElement();
            var second = CreateLayoutElement();
            first.minWidth = -1f;
            second.minWidth = -1f;

            CreateInspectorFor(first, second);
            var (toggle, field) = RowFor(k_MinWidth);

            Assert.That(toggle.value, Is.False, "Objects that are all disabled should read as disabled.");
            Assert.That(toggle.showMixedValue, Is.False, "Objects that agree on being disabled are not mixed.");
            Assert.That(field.showMixedValue, Is.False, "Objects holding the same value are not mixed.");
            Assert.That(field.style.display.value, Is.EqualTo(DisplayStyle.None),
                "A field that is disabled across the whole selection should stay hidden.");
        }

        [Test]
        public void MultiObject_DivergentEnabledStateOnMaxField_ShowsMixedToggle()
        {
            var disabled = CreateLayoutElement();
            var enabled = CreateLayoutElement();
            disabled.maxWidth = LayoutUtility.DefaultMaxSize;
            enabled.maxWidth = 250f;

            CreateInspectorFor(disabled, enabled);
            var (toggle, field) = RowFor(k_MaxWidth);

            Assert.That(toggle.showMixedValue, Is.True,
                "An unbounded and a bounded 'Max Width' disagree about the enabled state and should mark the toggle as mixed.");
            Assert.That(field.style.display.value, Is.EqualTo(DisplayStyle.Flex),
                "The field must stay visible when the enabled state is mixed.");
        }

        [Test]
        public void MultiObject_DivergenceInThirdObject_ShowsMixedToggle()
        {
            var first = CreateLayoutElement();
            var second = CreateLayoutElement();
            var third = CreateLayoutElement();
            first.minWidth = 100f;
            second.minWidth = 200f;
            third.minWidth = -1f;

            CreateInspectorFor(first, second, third);
            var (toggle, field) = RowFor(k_MinWidth);

            Assert.That(toggle.showMixedValue, Is.True,
                "A divergent enabled state anywhere in the selection should mark the toggle as mixed.");
            Assert.That(field.showMixedValue, Is.True,
                "Divergent 'Min Width' values should mark the field as mixed.");
        }

        [Test]
        public void MultiObject_SameEnabledStateDifferentValues_MixesFieldOnly()
        {
            var first = CreateLayoutElement();
            var second = CreateLayoutElement();
            first.minWidth = 100f;
            second.minWidth = 200f;

            CreateInspectorFor(first, second);
            var (toggle, field) = RowFor(k_MinWidth);

            Assert.That(toggle.showMixedValue, Is.False,
                "Objects that are all enabled agree about the toggle, even when their values differ.");
            Assert.That(toggle.value, Is.True, "The toggle should read enabled when every selected object is enabled.");
            Assert.That(field.showMixedValue, Is.True, "Differing 'Min Width' values should mark the field as mixed.");
            Assert.That(field.style.display.value, Is.EqualTo(DisplayStyle.Flex), "An enabled field stays visible.");
        }
    }
}
