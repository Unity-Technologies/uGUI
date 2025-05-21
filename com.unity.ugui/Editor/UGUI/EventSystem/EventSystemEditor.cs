using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEditor.EventSystems
{
    [CustomEditor(typeof(EventSystem), true)]
    /// <summary>
    /// Custom Editor for the EventSystem Component.
    /// Extend this class to write a custom editor for a component derived from EventSystem.
    /// </summary>
    public class EventSystemEditor : Editor
    {
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
                    "to configure how UI Toolkit will interact with this EventSystem."), MessageType.Info);
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

        public override bool HasPreviewGUI()
        {
            return Application.isPlaying;
        }

        private GUIStyle m_PreviewLabelStyle;

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

        public override bool RequiresConstantRepaint()
        {
            return Application.isPlaying;
        }

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            var system = target as EventSystem;
            if (system == null)
                return;

            GUI.Label(rect, system.ToString(), previewLabelStyle);
        }
    }
}
