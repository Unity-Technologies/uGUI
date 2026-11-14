using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEditor.EventSystems
{
    /// <summary>
    /// Custom Editor for the EventSystem Component.
    /// Extend this class to write a custom editor for a component derived from EventSystem.
    /// </summary>
    /// <remarks>
    /// This editor draws the default Inspector for the <see cref="EventSystem"/> component.
    /// If the GameObject has no input module, the Inspector displays an **Add Default Input Modules** button.
    /// In Play mode, the preview area displays the current state of the <see cref="EventSystem"/>.
    /// </remarks>
    [CustomEditor(typeof(EventSystem), true)]
    public class EventSystemEditor : Editor
    {
        /// <summary>
        /// Draws the Inspector for the <see cref="EventSystem"/> component.
        /// </summary>
        /// <remarks>
        /// Draws the default Inspector, followed by an information box if the <see cref="EventSystem"/> drives UI Toolkit input.
        /// If the GameObject has no <see cref="BaseInputModule"/>, it also draws a button to add the default input module.
        /// </remarks>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var eventSystem = target as EventSystem;
            if (eventSystem == null)
                return;

            if (eventSystem.isOverridingUIToolkitEvents)
            {
                EditorGUILayout.HelpBox(L10n.Tr(
                    "This EventSystem will be used to drive UI Toolkit input.\nYou can use the Panel Input Configuration component " +
                    "to configure how UI Toolkit will interact with this EventSystem.", null), MessageType.Info);
            }

            if (eventSystem.GetComponent<BaseInputModule>() != null)
                return;

            // no input modules :(
            if (GUILayout.Button("Add Default Input Modules"))
            {
                InputModuleComponentFactory.AddInputModule(eventSystem.gameObject);
                Undo.RegisterCreatedObjectUndo(eventSystem.gameObject, "Add Default Input Modules");
            }
        }

        /// <summary>
        /// Checks whether the Inspector displays a preview for the <see cref="EventSystem"/>.
        /// </summary>
        /// <returns><c>true</c> when the Editor is in Play mode.</returns>
        public override bool HasPreviewGUI()
        {
            return Application.isPlaying;
        }

        private GUIStyle m_PreviewLabelStyle;

        /// <summary>
        /// The style of the label that displays the <see cref="EventSystem"/> state in the preview area.
        /// </summary>
        protected GUIStyle previewLabelStyle
        {
            get
            {
                if (m_PreviewLabelStyle == null)
                {
                    m_PreviewLabelStyle = new GUIStyle("PreOverlayLabel")
                    {
                        richText = true,
                        alignment = TextAnchor.UpperLeft,
                        fontStyle = FontStyle.Normal
                    };
                }

                return m_PreviewLabelStyle;
            }
        }

        /// <summary>
        /// Checks if this editor requires constant repaints.
        /// </summary>
        /// <returns><c>true</c> when the Editor is in Play mode.</returns>
        public override bool RequiresConstantRepaint()
        {
            return Application.isPlaying;
        }

        /// <summary>
        /// Draws the current state of the <see cref="EventSystem"/> in the preview area.
        /// </summary>
        /// <param name="rect">Rectangle in which to draw the preview.</param>
        /// <param name="background">Background style of the preview area.</param>
        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            var system = target as EventSystem;
            if (system == null)
                return;

            GUI.Label(rect, system.ToString(), previewLabelStyle);
        }
    }
}
