using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(RectMask2D), true)]
    [CanEditMultipleObjects]
    /// <summary>
    ///   Custom editor for the RectMask2d component.
    ///   Extend this class to write a custom editor for a Mask-derived component.
    /// </summary>
    public class RectMask2DEditor : Editor
    {
        public override void OnInspectorGUI()
        {
        }
    }
}
