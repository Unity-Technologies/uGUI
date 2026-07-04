using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UnityEditor.UI
{
    /// <summary>
    /// Custom inspector for CanvasGroup Component using UIElements
    /// </summary>
    [CustomEditor(typeof(CanvasGroup))]
    [CanEditMultipleObjects]
    public class CanvasGroupInspector : Editor
    {
        /// <summary>
        /// Creates the UIElements-based inspector GUI for the CanvasGroup component.
        /// </summary>
        /// <returns>A root VisualElement containing the inspector fields.</returns>
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var alphaProp = serializedObject.FindProperty("m_Alpha");

            var alphaField = new Slider("Alpha", 0f, 1f);
            alphaField.tooltip = "Controls the transparency of the CanvasGroup. 0 = fully transparent, 1 = fully opaque.";
            alphaField.showInputField = true;
            alphaField.BindProperty(alphaProp);
            root.Add(alphaField);

            root.Add(new PropertyField(serializedObject.FindProperty("m_Interactable"), "Interactable")
            {
                tooltip = "Enable this to make UI elements under this CanvasGroup interactable."
            });
            root.Add(new PropertyField(serializedObject.FindProperty("m_BlocksRaycasts"), "Blocks Raycasts")
            {
                tooltip = "Enable this to block raycasts and prevent clicks from passing through."
            });
            root.Add(new PropertyField(serializedObject.FindProperty("m_IgnoreParentGroups"), "Ignore Parent Groups")
            {
                tooltip = "Enable this to ignore settings inherited from parent CanvasGroups."
            });

            return root;
        }
    }
}
