using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEditor.EventSystems
{
    /// <summary>
    /// Custom Editor for the Physics2DRaycaster component.
    /// Extend this class to write a custom editor for a component derived from Physics2DRaycaster.
    /// </summary>
    /// <remarks>
    /// This editor draws the default Inspector for the <see cref="Physics2DRaycaster"/> component.
    /// If the Physics 2D module isn't present in the project, the Inspector displays a warning.
    /// </remarks>
    [CustomEditor(typeof(Physics2DRaycaster), true)]
    public class Physics2DRaycasterEditor : Editor
    {
        /// <summary>
        /// Draws the Inspector for the <see cref="Physics2DRaycaster"/> component.
        /// </summary>
        /// <remarks>
        /// If the Physics 2D module isn't present, also draws a warning that the raycaster has no effect.
        /// </remarks>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
#if !PACKAGE_PHYSICS2D
            EditorGUILayout.HelpBox("Physics2D module is not present. This Raycaster will have no effect", MessageType.Warning);
#endif
        }
    }
}
