using System.Collections;
using NUnit.Framework;
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

        // When multiple objects with differing field values are selected, the row's enable toggle and field
        // show the mixed-value indicator rather than a single object's value.
        [UnityTest]
        public IEnumerator MultiObject_DivergentField_ShowsMixedValue()
        {
            var goA = new GameObject("LayoutElementA", typeof(RectTransform));
            var goB = new GameObject("LayoutElementB", typeof(RectTransform));
            var layoutA = goA.AddComponent<LayoutElement>();
            var layoutB = goB.AddComponent<LayoutElement>();

            layoutA.minWidth = 100f; // enabled
            layoutB.minWidth = -1f;  // disabled sentinel -> the two selections diverge

            var editor = Editor.CreateEditor(new Object[] { layoutA, layoutB });
            var root = editor.CreateInspectorGUI();
            m_Window.rootVisualElement.Add(root);
            yield return null;
            yield return null;

            var labelElement = root.Query<Label>().Where(l => l.text == "Min Width").First();
            var row = labelElement.parent;
            var toggle = row.Q<Toggle>();
            var field = row.Q<FloatField>();

            Assert.That(toggle.showMixedValue, Is.True,
                "Multi-selecting LayoutElements with a divergent 'Min Width' should show the mixed-value indicator on the toggle.");
            Assert.That(field.showMixedValue, Is.True,
                "A divergent 'Min Width' should also display the mixed-value indicator on the field.");

            root.RemoveFromHierarchy();
            Object.DestroyImmediate(editor);
            Object.DestroyImmediate(goA);
            Object.DestroyImmediate(goB);
        }

        // When selected objects share the enabled/disabled state but hold different numbers (both enabled,
        // 100 vs 200), the enable toggle controls something they agree on, so it must NOT be mixed - only the
        // field, which shows the raw number, is mixed.
        [UnityTest]
        public IEnumerator MultiObject_SameEnabledStateDifferentValues_MixedOnFieldOnly()
        {
            var goA = new GameObject("LayoutElementA", typeof(RectTransform));
            var goB = new GameObject("LayoutElementB", typeof(RectTransform));
            var layoutA = goA.AddComponent<LayoutElement>();
            var layoutB = goB.AddComponent<LayoutElement>();

            layoutA.minWidth = 100f; // enabled
            layoutB.minWidth = 200f; // enabled, different value -> only the number diverges

            var editor = Editor.CreateEditor(new Object[] { layoutA, layoutB });
            var root = editor.CreateInspectorGUI();
            m_Window.rootVisualElement.Add(root);
            yield return null;
            yield return null;

            var labelElement = root.Query<Label>().Where(l => l.text == "Min Width").First();
            var row = labelElement.parent;
            var toggle = row.Q<Toggle>();
            var field = row.Q<FloatField>();

            Assert.That(toggle.showMixedValue, Is.False,
                "Objects that are all enabled should not show the toggle as mixed, even if their values differ.");
            Assert.That(toggle.value, Is.True,
                "The toggle should read enabled when every selected object is enabled.");
            Assert.That(field.showMixedValue, Is.True,
                "Differing 'Min Width' values should still show the mixed-value indicator on the field.");

            root.RemoveFromHierarchy();
            Object.DestroyImmediate(editor);
            Object.DestroyImmediate(goA);
            Object.DestroyImmediate(goB);
        }
    }
}
