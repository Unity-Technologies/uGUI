using UnityEngine.EventSystems;

namespace UnityEditor.EventSystems
{
    /// <summary>
    /// Custom Editor for the PhysicsRaycaster component.
    /// Extend this class to write a custom editor for a component derived from PhysicsRaycaster.
    /// </summary>
    /// <remarks>
    /// This editor draws the default Inspector for the <see cref="PhysicsRaycaster"/> component.
    /// If the Physics module isn't present in the project, the Inspector displays a warning.
    /// </remarks>
    [CustomEditor(typeof(PhysicsRaycaster), true)]
    public class PhysicsRaycasterEditor : Editor
    {
        /// <summary>
        /// Draws the Inspector for the <see cref="PhysicsRaycaster"/> component.
        /// </summary>
        /// <remarks>
        /// If the Physics module isn't present, also draws a warning that the raycaster has no effect.
        /// </remarks>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
#if !PACKAGE_PHYSICS
            EditorGUILayout.HelpBox("Physics module is not present. This Raycaster will have no effect", MessageType.Warning);
#endif
        }
    }
}
