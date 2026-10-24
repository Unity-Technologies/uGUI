using System.Collections;
using System.Collections.Generic;
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
        const string k_MixedValueText = "\u2014";

        static readonly string[] k_FloatFieldLabels =
        {
            "Min Width", "Min Height",
            "Max Width", "Max Height",
            "Preferred Width", "Preferred Height",
            "Flexible Width", "Flexible Height"
        };

        // Max fields use +Infinity as their disabled value, so infinity is only invalid input for these.
        static readonly string[] k_UnboundedFloatFieldLabels =
        {
            "Min Width", "Min Height",
            "Preferred Width", "Preferred Height",
            "Flexible Width", "Flexible Height"
        };

        GameObject m_GameObject;
        LayoutElement m_LayoutElement;
        Editor m_Editor;
        EditorWindow m_Window;
        VisualElement m_Root;

        // Objects created by the multi-object tests, destroyed in TearDown so a failed assertion cannot leak them.
        readonly List<UnityEngine.Object> m_MultiObjectCleanup = new List<UnityEngine.Object>();

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
                UnityEngine.Object.DestroyImmediate(m_Editor);
            if (m_GameObject != null)
                UnityEngine.Object.DestroyImmediate(m_GameObject);

            foreach (var obj in m_MultiObjectCleanup)
            {
                if (obj != null)
                    UnityEngine.Object.DestroyImmediate(obj);
            }
            m_MultiObjectCleanup.Clear();
        }

        (Toggle toggle, FloatField field, Label labelElement) RowFor(string label)
        {
            return RowFor(m_Root, label);
        }

        static (Toggle toggle, FloatField field, Label labelElement) RowFor(VisualElement root, string label)
        {
            var labelElement = root.Query<Label>().Where(l => l.text == label).First();
            var row = labelElement.parent;
            return (row.Q<Toggle>(), row.Q<FloatField>(), labelElement);
        }

        LayoutElement CreateLayoutElement(string name)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            m_MultiObjectCleanup.Add(gameObject);
            return gameObject.AddComponent<LayoutElement>();
        }

        // Builds a multi-object inspector in the test window; the window's teardown removes the root.
        VisualElement CreateInspectorFor(params LayoutElement[] elements)
        {
            var targets = new UnityEngine.Object[elements.Length];
            for (int i = 0; i < elements.Length; i++)
            {
                targets[i] = elements[i];
            }

            var editor = Editor.CreateEditor(targets);
            m_MultiObjectCleanup.Add(editor);
            var root = editor.CreateInspectorGUI();
            m_Window.rootVisualElement.Add(root);
            return root;
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
            Assert.That(LayoutElementEditor.IsEnabledValue(value), Is.False,
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

        // The field parses "nan" as float.NaN. The runtime only ignores negative values, so a stored NaN
        // would be used for layout while the inspector shows the field as disabled.
        [UnityTest]
        public IEnumerator NaNValue_LeavesFieldEnabledWithValidValue([ValueSource(nameof(k_FloatFieldLabels))] string label)
        {
            var (toggle, field, _) = RowFor(label);

            toggle.value = true;
            yield return null;
            field.Focus();
            yield return null;

            field.value = float.NaN;
            yield return null;

            float value = PersistedValue(label);
            Assert.That(LayoutElementEditor.IsEnabledValue(value), Is.True,
                $"Entering NaN in '{label}' should not store an invalid value, but it stored {value}.");
            Assert.That(toggle.value, Is.True,
                $"Entering NaN in '{label}' should not make the field look disabled.");
        }

        [UnityTest]
        public IEnumerator InfiniteValue_OnUnboundedField_LeavesFieldEnabledWithValidValue([ValueSource(nameof(k_UnboundedFloatFieldLabels))] string label)
        {
            var (toggle, field, _) = RowFor(label);

            toggle.value = true;
            yield return null;
            field.Focus();
            yield return null;

            field.value = float.PositiveInfinity;
            yield return null;

            float value = PersistedValue(label);
            Assert.That(LayoutElementEditor.IsEnabledValue(value), Is.True,
                $"Entering infinity in '{label}' should not store an invalid value, but it stored {value}.");
            Assert.That(toggle.value, Is.True,
                $"Entering infinity in '{label}' should not make the field look disabled.");
        }

        [UnityTest]
        public IEnumerator ReenablingField_RestoresPreviousValue([ValueSource(nameof(k_FloatFieldLabels))] string label)
        {
            var (toggle, field, _) = RowFor(label);

            toggle.value = true;
            field.value = 42f;
            yield return null;
            Assume.That(PersistedValue(label), Is.EqualTo(42f));

            toggle.value = false;
            yield return null;
            Assume.That(LayoutElementEditor.IsEnabledValue(PersistedValue(label)), Is.False);

            toggle.value = true;
            yield return null;

            Assert.That(PersistedValue(label), Is.EqualTo(42f),
                $"Re-enabling '{label}' should restore the value it had when it was disabled.");
            Assert.That(field.value, Is.EqualTo(42f),
                $"The '{label}' field should display the restored value.");
        }

        [UnityTest]
        public IEnumerator EnablingField_WithoutPreviousValue_UsesDefault()
        {
            var (toggle, _, _) = RowFor("Min Width");
            Assume.That(m_LayoutElement.minWidth, Is.EqualTo(-1f));

            toggle.value = true;
            yield return null;

            Assert.That(m_LayoutElement.minWidth, Is.EqualTo(0f),
                "A field that was never enabled in the inspector should take its default when enabled.");
        }

        [UnityTest]
        public IEnumerator ReenablingField_AfterInspectorRecreated_RestoresPreviousValue()
        {
            var (toggle, field, _) = RowFor("Min Height");

            toggle.value = true;
            field.value = 42f;
            yield return null;
            toggle.value = false;
            yield return null;
            Assume.That(LayoutElementEditor.IsEnabledValue(m_LayoutElement.minHeight), Is.False);

            // Deselecting and reselecting the component creates a new Editor and inspector.
            m_Root.RemoveFromHierarchy();
            UnityEngine.Object.DestroyImmediate(m_Editor);
            m_Editor = Editor.CreateEditor(m_LayoutElement);
            m_Root = m_Editor.CreateInspectorGUI();
            m_Window.rootVisualElement.Add(m_Root);
            yield return null;

            var (newToggle, _, _) = RowFor("Min Height");
            newToggle.value = true;
            yield return null;

            Assert.That(m_LayoutElement.minHeight, Is.EqualTo(42f),
                "The remembered value should survive the inspector being recreated.");
        }

        [UnityTest]
        public IEnumerator MultiObject_ReenablingField_RestoresEachTargetIndividually()
        {
            var layoutA = CreateLayoutElement("LayoutElementA");
            var layoutB = CreateLayoutElement("LayoutElementB");
            layoutA.minWidth = 10f;
            layoutB.minWidth = 20f;

            var root = CreateInspectorFor(layoutA, layoutB);
            yield return null;

            var (toggle, field, _) = RowFor(root, "Min Width");
            Assume.That(toggle.value, Is.True);

            toggle.value = false;
            yield return null;
            Assume.That(LayoutElementEditor.IsEnabledValue(layoutA.minWidth), Is.False);
            Assume.That(LayoutElementEditor.IsEnabledValue(layoutB.minWidth), Is.False);

            toggle.value = true;
            yield return null;

            Assert.That(layoutA.minWidth, Is.EqualTo(10f), "Each selected object should get its own value back.");
            Assert.That(layoutB.minWidth, Is.EqualTo(20f), "Each selected object should get its own value back.");
            Assert.That(field.text, Is.EqualTo(k_MixedValueText), "Restoring different values should leave the field showing mixed.");
        }

        [UnityTest]
        public IEnumerator MultiObject_EnablingMixedField_KeepsAlreadyEnabledValues()
        {
            var disabled = CreateLayoutElement("LayoutElementA");
            var enabled = CreateLayoutElement("LayoutElementB");
            disabled.preferredWidth = -1f;
            enabled.preferredWidth = 30f;
            float defaultWidth = ((RectTransform)disabled.transform).rect.width;
            Assume.That(defaultWidth, Is.Not.EqualTo(30f));

            var root = CreateInspectorFor(disabled, enabled);
            yield return null;

            var (toggle, field, _) = RowFor(root, "Preferred Width");
            Assume.That(toggle.showMixedValue, Is.True);
            Assume.That(toggle.value, Is.False);

            toggle.value = true;
            yield return null;

            Assert.That(disabled.preferredWidth, Is.EqualTo(defaultWidth), "The disabled object should take the default when enabled.");
            Assert.That(enabled.preferredWidth, Is.EqualTo(30f), "Enabling a mixed field must not overwrite an object that was already enabled.");
            Assert.That(field.text, Is.EqualTo(k_MixedValueText), "Objects left with different values should show the field as mixed.");
        }

        // NaN typed over a mixed field whose shown value is disabled falls back to the field's enabled default, not 0.
        [UnityTest]
        public IEnumerator MultiObject_NaNValueOverDisabledValue_UsesFieldDefault()
        {
            var disabled = CreateLayoutElement("LayoutElementA");
            var enabled = CreateLayoutElement("LayoutElementB");
            disabled.flexibleWidth = -1f;
            enabled.flexibleWidth = 5f;

            var root = CreateInspectorFor(disabled, enabled);
            yield return null;

            var (toggle, field, _) = RowFor(root, "Flexible Width");
            Assume.That(toggle.showMixedValue, Is.True);
            field.Focus();
            yield return null;

            field.value = float.NaN;
            yield return null;

            Assert.That(disabled.flexibleWidth, Is.EqualTo(1f),
                "Rejected input with no valid previous value should take the Flexible Width default.");
            Assert.That(enabled.flexibleWidth, Is.EqualTo(1f),
                "Every selected object should receive the same fallback value.");
        }

        // When multiple objects with differing field values are selected, the row's enable toggle and field
        // show the mixed-value indicator rather than a single object's value.
        [UnityTest]
        public IEnumerator MultiObject_DivergentField_ShowsMixedValue()
        {
            var layoutA = CreateLayoutElement("LayoutElementA");
            var layoutB = CreateLayoutElement("LayoutElementB");

            layoutA.minWidth = 100f; // enabled
            layoutB.minWidth = -1f;  // disabled sentinel -> the two selections diverge

            var root = CreateInspectorFor(layoutA, layoutB);
            yield return null;
            yield return null;

            var (toggle, field, _) = RowFor(root, "Min Width");

            Assert.That(toggle.showMixedValue, Is.True,
                "Multi-selecting LayoutElements with a divergent 'Min Width' should show the mixed-value indicator on the toggle.");
            Assert.That(field.showMixedValue, Is.True,
                "A divergent 'Min Width' should also display the mixed-value indicator on the field.");
        }

        // When selected objects share the enabled/disabled state but hold different numbers (both enabled,
        // 100 vs 200), the enable toggle controls something they agree on, so it must NOT be mixed - only the
        // field, which shows the raw number, is mixed.
        [UnityTest]
        public IEnumerator MultiObject_SameEnabledStateDifferentValues_MixedOnFieldOnly()
        {
            var layoutA = CreateLayoutElement("LayoutElementA");
            var layoutB = CreateLayoutElement("LayoutElementB");

            layoutA.minWidth = 100f; // enabled
            layoutB.minWidth = 200f; // enabled, different value -> only the number diverges

            var root = CreateInspectorFor(layoutA, layoutB);
            yield return null;
            yield return null;

            var (toggle, field, _) = RowFor(root, "Min Width");

            Assert.That(toggle.showMixedValue, Is.False,
                "Objects that are all enabled should not show the toggle as mixed, even if their values differ.");
            Assert.That(toggle.value, Is.True,
                "The toggle should read enabled when every selected object is enabled.");
            Assert.That(field.showMixedValue, Is.True,
                "Differing 'Min Width' values should still show the mixed-value indicator on the field.");
        }
    }
}
