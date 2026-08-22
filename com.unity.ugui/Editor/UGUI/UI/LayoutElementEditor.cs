using UnityEngine;
using UnityEngine.UI;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(LayoutElement), true)]
    [CanEditMultipleObjects]
    /// <summary>
    ///   Custom editor for the LayoutElement component.
    ///   Extend this class to write a custom editor for a component derived from LayoutElement.
    /// </summary>
    public class LayoutElementEditor : Editor
    {
        //Shared layout constants: defined once so every row's layout is identical.
        private const float k_LabelWidth     = 130f;
        private const float k_RowMarginBottom = 2f;
        private const float k_ToggleMarginRight = 4f;

        SerializedProperty m_IgnoreLayout;
        SerializedProperty m_MinWidth;
        SerializedProperty m_MinHeight;
        SerializedProperty m_MaxWidth;
        SerializedProperty m_MaxHeight;
        SerializedProperty m_PreferredWidth;
        SerializedProperty m_PreferredHeight;
        SerializedProperty m_FlexibleWidth;
        SerializedProperty m_FlexibleHeight;
        SerializedProperty m_LayoutPriority;

        protected virtual void OnEnable()
        {
            m_IgnoreLayout = serializedObject.FindProperty("m_IgnoreLayout");
            m_MinWidth = serializedObject.FindProperty("m_MinWidth");
            m_MinHeight = serializedObject.FindProperty("m_MinHeight");
            m_MaxWidth = serializedObject.FindProperty("m_MaxWidth");
            m_MaxHeight = serializedObject.FindProperty("m_MaxHeight");
            m_PreferredWidth = serializedObject.FindProperty("m_PreferredWidth");
            m_PreferredHeight = serializedObject.FindProperty("m_PreferredHeight");
            m_FlexibleWidth = serializedObject.FindProperty("m_FlexibleWidth");
            m_FlexibleHeight = serializedObject.FindProperty("m_FlexibleHeight");
            m_LayoutPriority = serializedObject.FindProperty("m_LayoutPriority");
        }

        /// <summary>
        /// Creates the Inspector for LayoutElement component.
        /// </summary>
        /// <returns>The root VisualElement containing the custom inspector UI.</returns>
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            // Show Ignore Layout as a uniform bool row to maintain formatting
            var ignoreLayoutRow = CreateBoolField(m_IgnoreLayout, "Ignore Layout");
            root.Add(ignoreLayoutRow);

            // Create a container for the layout fields
            var layoutFieldsContainer = new VisualElement();
            layoutFieldsContainer.style.marginTop = 8f;

            var rectTransform = (target as LayoutElement).transform as RectTransform;

            layoutFieldsContainer.Add(CreateLayoutElementField(m_MinWidth, "Min Width", 0, -1));
            layoutFieldsContainer.Add(CreateLayoutElementField(m_MinHeight, "Min Height", 0, -1));
            layoutFieldsContainer.Add(CreateLayoutElementField(m_MaxWidth, "Max Width", rectTransform.rect.width, LayoutUtility.DefaultMaxSize));
            layoutFieldsContainer.Add(CreateLayoutElementField(m_MaxHeight, "Max Height", rectTransform.rect.height, LayoutUtility.DefaultMaxSize));
            layoutFieldsContainer.Add(CreateLayoutElementField(m_PreferredWidth, "Preferred Width", rectTransform.rect.width, -1));
            layoutFieldsContainer.Add(CreateLayoutElementField(m_PreferredHeight, "Preferred Height", rectTransform.rect.height, -1));
            layoutFieldsContainer.Add(CreateLayoutElementField(m_FlexibleWidth, "Flexible Width", 1, -1));
            layoutFieldsContainer.Add(CreateLayoutElementField(m_FlexibleHeight, "Flexible Height", 1, -1));

            root.Add(layoutFieldsContainer);

            // Show Layout Priority as a uniform int row to maintain formatting
            var layoutPriorityRow = CreateIntField(m_LayoutPriority, "Layout Priority");
            layoutPriorityRow.style.marginTop = 4f;
            root.Add(layoutPriorityRow);

            // Initial visibility of layout fields
            layoutFieldsContainer.style.display = m_IgnoreLayout.boolValue ? DisplayStyle.None : DisplayStyle.Flex;

            // We pull the Toggle out of the row we just built so we can
            // register a callback on it to show/hide the layout fields.
            var ignoreToggle = ignoreLayoutRow.Q<UnityEngine.UIElements.Toggle>();
            ignoreToggle.RegisterValueChangedCallback(evt =>
            {
                layoutFieldsContainer.style.display = evt.newValue ? DisplayStyle.None : DisplayStyle.Flex;
            });

            root.Bind(serializedObject);
            return root;
        }

        /// <summary>
        /// Creates a row container with a fixed-width label, matching every other
        /// row in the inspector. All three field types call this first.
        /// </summary>
        private (VisualElement row, Label labelElement) CreateRow(string label)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems    = Align.Center;
            row.style.marginBottom  = k_RowMarginBottom;

            var labelElement = new Label(label);
            labelElement.style.width    = k_LabelWidth;
            labelElement.style.minWidth = k_LabelWidth;
            row.Add(labelElement);

            return (row, labelElement);
        }

        /// <summary>
        /// Creates a uniform bool row for a SerializedProperty of type bool.
        /// Layout: [Label 130px][Toggle flex]
        /// Matches the same structure as the float and int rows.
        /// </summary>
        private VisualElement CreateBoolField(SerializedProperty property, string label)
        {
            var (row, _) = CreateRow(label);

            var toggle = new UnityEngine.UIElements.Toggle();
            toggle.style.flexGrow = 1f;
            toggle.BindProperty(property);

            toggle.RegisterValueChangedCallback(evt =>
            {
                serializedObject.Update();
                property.boolValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
            });

            row.Add(toggle);
            return row;
        }

        /// <summary>
        /// Creates a uniform int row for a SerializedProperty of type int,
        /// with a FieldMouseDragger wired to the label.
        /// Layout: [Label/DragZone 130px][IntegerField flex]
        /// </summary>
        private VisualElement CreateIntField(SerializedProperty property, string label)
        {
            var (row, labelElement) = CreateRow(label);

            var intField = new IntegerField();
            intField.style.flexGrow = 1f;
            intField.BindProperty(property);

            intField.SetupDragger(labelElement);

            intField.RegisterValueChangedCallback(evt =>
            {
                serializedObject.Update();
                property.intValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
            });

            row.Add(intField);
            return row;
        }

        /// <summary>
        /// Creates a uniform float row with a label/drag-zone, enable toggle,
        /// and a clamped FloatField with FieldMouseDragger.
        /// Layout: [Label/DragZone 130px][Toggle][FloatField flex]
        /// </summary>
        private VisualElement CreateLayoutElementField(SerializedProperty property, string label, float defaultEnabledValue, float defaultDisabledValue)
        {
            var (row, labelElement) = CreateRow(label);

            // State for clamping only the user's own edits (typing or dragging); binding-driven changes are left alone.
            bool fieldHasFocus = false;
            bool labelIsDragging = false;
            int undoGroupAtEditStart = 0;

            // Enable/Disable the property for editing
            var toggle = new UnityEngine.UIElements.Toggle();
            toggle.style.marginRight = k_ToggleMarginRight;
            row.Add(toggle);

            // Float property area
            var floatField = new FloatField();
            floatField.style.flexGrow = 1f;
            floatField.BindProperty(property);
            row.Add(floatField);

            floatField.SetupDragger(labelElement, () => floatField.style.display == DisplayStyle.Flex);

            // Initialise the row's toggle, visibility and mixed-value state from the property.
            ApplyState(property, property.floatValue, toggle, floatField);

            // Re-sync the row when the property changes outside this field (multi-selection, another
            // Inspector, undo/redo, script) - including binding updates that raise no change event.
            floatField.TrackPropertyValue(property, prop => ApplyState(prop, prop.floatValue, toggle, floatField));

            // Toggle callback - set the value to default value
            toggle.RegisterValueChangedCallback(evt =>
            {
                serializedObject.Update();
                property.floatValue = evt.newValue ? defaultEnabledValue : defaultDisabledValue;
                serializedObject.ApplyModifiedProperties();

                toggle.showMixedValue = false;
                floatField.showMixedValue = false;
                floatField.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                if (evt.newValue)
                    floatField.SetValueWithoutNotify(defaultEnabledValue);
            });

            floatField.RegisterCallback<FocusInEvent>(_ => { fieldHasFocus = true; undoGroupAtEditStart = Undo.GetCurrentGroup(); });
            floatField.RegisterCallback<FocusOutEvent>(_ => fieldHasFocus = false);
            labelElement.RegisterCallback<PointerCaptureEvent>(_ => { labelIsDragging = true; undoGroupAtEditStart = Undo.GetCurrentGroup(); });
            labelElement.RegisterCallback<PointerCaptureOutEvent>(_ => labelIsDragging = false);

            floatField.RegisterValueChangedCallback(evt =>
            {
                if (!fieldHasFocus && !labelIsDragging)
                {
                    // Change came from the binding, not user input; mirror it onto the row without clamping.
                    ApplyState(property, evt.newValue, toggle, floatField);
                    return;
                }

                float clampedValue = Mathf.Max(0f, evt.newValue);
                if (!Mathf.Approximately(clampedValue, evt.newValue))
                {
                    serializedObject.Update();
                    property.floatValue = clampedValue;
                    serializedObject.ApplyModifiedProperties();
                    floatField.SetValueWithoutNotify(clampedValue);

                    // Merge the rejected write and this correction into one undo step.
                    Undo.CollapseUndoOperations(undoGroupAtEditStart);
                }
            });

            return row;
        }

        private static void ApplyState(SerializedProperty property, float value, UnityEngine.UIElements.Toggle toggle, FloatField floatField)
        {
            bool valuesDiffer = property.hasMultipleDifferentValues;
            bool enabled = value >= 0f && value < LayoutUtility.DefaultMaxSize;
            bool enabledMixed = valuesDiffer && EnabledStateIsMixed(property);

            toggle.showMixedValue = enabledMixed;
            toggle.SetValueWithoutNotify(enabled);

            floatField.showMixedValue = valuesDiffer;
            floatField.style.display = (enabled || enabledMixed) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static bool EnabledStateIsMixed(SerializedProperty property)
        {
            string propertyPath = property.propertyPath;
            bool firstEnabled = false;
            bool haveFirst = false;
            foreach (var target in property.serializedObject.targetObjects)
            {
                using (var targetObject = new SerializedObject(target))
                {
                    float fieldValue = targetObject.FindProperty(propertyPath).floatValue;
                    bool enabled = fieldValue >= 0f && fieldValue < LayoutUtility.DefaultMaxSize;
                    if (!haveFirst)
                    {
                        firstEnabled = enabled;
                        haveFirst = true;
                    }
                    else if (enabled != firstEnabled)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
