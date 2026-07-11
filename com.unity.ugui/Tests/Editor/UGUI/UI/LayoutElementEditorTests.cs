using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Toggle = UnityEngine.UIElements.Toggle;

namespace UnityEditor.UI.Tests
{
    internal class LayoutElementEditorTests
    {
        static readonly string[] k_FloatFieldLabels =
        {
            "Min Width", "Min Height",
            "Max Width", "Max Height",
            "Preferred Width", "Preferred Height",
            "Flexible Width", "Flexible Height"
        };

        GameObject m_GameObject;
        LayoutElement m_LayoutElement;
        Editor m_Editor;
        EditorWindow m_Window;
        VisualElement m_Root;

        [SetUp]
        public void SetUp()
        {
            VisualTreeBindingsUpdater.disableBindingsThrottling = true;

            m_GameObject = new GameObject("LayoutElement", typeof(RectTransform));
            m_LayoutElement = m_GameObject.AddComponent<LayoutElement>();

            m_Editor = Editor.CreateEditor(m_LayoutElement);
            m_Root = m_Editor.CreateInspectorGUI();

            m_Window = EditorWindow.CreateWindow<EditorWindow>();
            m_Window.rootVisualElement.Add(m_Root);
            m_Window.Show();
        }

        [TearDown]
        public void TearDown()
        {
            VisualTreeBindingsUpdater.disableBindingsThrottling = false;

            if (m_Window != null)
                m_Window.Close();
            if (m_Editor != null)
                Object.DestroyImmediate(m_Editor);
            if (m_GameObject != null)
                Object.DestroyImmediate(m_GameObject);
        }

        (Toggle toggle, FloatField field, Label labelElement) RowFor(string label)
        {
            var labelElement = m_Root.Query<Label>().Where(l => l.text == label).First();
            var row = labelElement.parent;
            return (row.Q<Toggle>(), row.Q<FloatField>(), labelElement);
        }

        float PersistedValue(string label)
        {
            switch (label)
            {
                case "Min Width": return m_LayoutElement.minWidth;
                case "Min Height": return m_LayoutElement.minHeight;
                case "Max Width": return m_LayoutElement.maxWidth;
                case "Max Height": return m_LayoutElement.maxHeight;
                case "Preferred Width": return m_LayoutElement.preferredWidth;
                case "Preferred Height": return m_LayoutElement.preferredHeight;
                case "Flexible Width": return m_LayoutElement.flexibleWidth;
                case "Flexible Height": return m_LayoutElement.flexibleHeight;
                default: throw new System.ArgumentException($"Unknown label '{label}'", nameof(label));
            }
        }

        [UnityTest]
        public IEnumerator NegativeValue_IsClampedToZero([ValueSource(nameof(k_FloatFieldLabels))] string label)
        {
            var (toggle, field, _) = RowFor(label);

            toggle.value = true;
            yield return null;
            field.Focus();
            yield return null;

            field.value = -1f;
            yield return null;

            Assert.That(PersistedValue(label), Is.EqualTo(0f),
                $"Negative '{label}' should be clamped to 0 in the LayoutElement.");
            Assert.That(field.value, Is.EqualTo(0f),
                $"The '{label}' field should display the clamped value rather than the rejected one.");
        }

        // Disabling a field stores a sentinel (-1, or +Infinity for Max fields); it should stay
        // disabled rather than be clamped into the enabled range.
        [UnityTest]
        public IEnumerator DisablingField_DoesNotClampToEnabledValue([ValueSource(nameof(k_FloatFieldLabels))] string label)
        {
            var (toggle, field, _) = RowFor(label);

            toggle.value = true;
            field.value = 5f;
            yield return null;
            Assume.That(PersistedValue(label), Is.EqualTo(5f));

            toggle.value = false;
            yield return null;
            yield return null;

            float value = PersistedValue(label);
            bool isEnabledValue = value >= 0f && value < LayoutUtility.DefaultMaxSize;
            Assert.That(isEnabledValue, Is.False,
                $"Disabling '{label}' should leave it disabled, but it was clamped to an enabled value ({value}).");
        }

        [UnityTest]
        public IEnumerator BindingDrivenDisable_SyncsToggleAndVisibility([ValueSource(nameof(k_FloatFieldLabels))] string label)
        {
            var (toggle, field, _) = RowFor(label);

            toggle.value = true;
            yield return null;
            Assume.That(toggle.value, Is.True);

            // Setting the value without focusing the field simulates a binding push (Undo/Reset/external).
            field.value = -1f;
            yield return null;

            Assert.That(toggle.value, Is.False,
                $"A binding-driven disable of '{label}' should uncheck the toggle.");
            Assert.That(field.style.display.value, Is.EqualTo(DisplayStyle.None),
                $"A binding-driven disable of '{label}' should hide the field.");
        }

        [UnityTest]
        public IEnumerator UndoAfterClampingNegative_DoesNotExposeRejectedValue([ValueSource(nameof(k_FloatFieldLabels))] string label)
        {
            var (toggle, field, _) = RowFor(label);

            toggle.value = true;
            yield return null;

            field.Focus();
            yield return null;
            field.value = 5f;
            yield return null;
            field.Blur();
            yield return null;
            Assume.That(PersistedValue(label), Is.EqualTo(5f));

            field.Focus();
            yield return null;
            field.value = -1f;
            yield return null;
            field.Blur();
            yield return null;
            Assume.That(PersistedValue(label), Is.EqualTo(0f));

            Undo.PerformUndo();
            yield return null;
            yield return null;

            Assert.That(PersistedValue(label), Is.GreaterThanOrEqualTo(0f),
                $"Undo after clamping a negative '{label}' must not expose the rejected negative value.");
        }

        [UnityTest]
        public IEnumerator NegativeValueWhileDragging_IsClampedToZero([ValueSource(nameof(k_FloatFieldLabels))] string label)
        {
            var (toggle, field, labelElement) = RowFor(label);

            toggle.value = true;
            yield return null;

            // The dragger captures the pointer on the label for the duration of a drag.
            labelElement.CaptureMouse();
            yield return null;

            field.value = -1f;
            yield return null;

            labelElement.ReleaseMouse();
            yield return null;

            Assert.That(PersistedValue(label), Is.EqualTo(0f),
                $"A negative '{label}' produced while dragging should be clamped to 0.");
            Assert.That(field.value, Is.EqualTo(0f));
        }
    }
}
