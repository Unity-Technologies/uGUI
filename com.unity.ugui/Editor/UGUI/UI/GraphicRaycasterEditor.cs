using UnityEditor.UIElements;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace UnityEditor.UI
{
    /// <summary>
    /// Custom Editor for the GraphicRaycaster Component.
    /// Extend this class to write a custom editor for a component derived from GraphicRaycaster.
    /// </summary>
    [CustomEditor(typeof(GraphicRaycaster), true)]
    [CanEditMultipleObjects]
    public class GraphicRaycasterEditor : Editor
    {
        SerializedProperty m_IgnoreReversedGraphics;
        SerializedProperty m_BlockingObjects;
        SerializedProperty m_BlockingMask;

#if !PACKAGE_PHYSICS
        HelpBox m_PhysicsNotPresentWarning;
#endif
#if !PACKAGE_PHYSICS2D
        HelpBox m_Physics2DNotPresentWarning;
#endif

        void OnEnable()
        {
            m_IgnoreReversedGraphics = serializedObject.FindProperty("m_IgnoreReversedGraphics");
            m_BlockingObjects = serializedObject.FindProperty("m_BlockingObjects");
            m_BlockingMask = serializedObject.FindProperty("m_BlockingMask");
        }

        /// <inheritdoc/>
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new PropertyField(m_IgnoreReversedGraphics));

            var blockingObjectsField = new PropertyField(m_BlockingObjects);
            root.Add(blockingObjectsField);

            root.Add(new PropertyField(m_BlockingMask));

#if !PACKAGE_PHYSICS
            m_PhysicsNotPresentWarning = new HelpBox("Physics module is not present. Setting Blocking Objects to ThreeD or All has no effect on 3D colliders.", HelpBoxMessageType.Warning);
            root.Add(m_PhysicsNotPresentWarning);
#endif
#if !PACKAGE_PHYSICS2D
            m_Physics2DNotPresentWarning = new HelpBox("Physics2D module is not present. Setting Blocking Objects to TwoD or All has no effect on 2D colliders.", HelpBoxMessageType.Warning);
            root.Add(m_Physics2DNotPresentWarning);
#endif

#if !PACKAGE_PHYSICS || !PACKAGE_PHYSICS2D
            blockingObjectsField.RegisterValueChangeCallback(evt => UpdateBlockingObjectsWarnings());
            UpdateBlockingObjectsWarnings();
#endif
            return root;
        }

#if !PACKAGE_PHYSICS || !PACKAGE_PHYSICS2D
        void UpdateBlockingObjectsWarnings()
        {
            bool anyThreeDOrAll = false;
            bool anyTwoDOrAll = false;
            foreach (GraphicRaycaster raycaster in targets)
            {
                var blockingObjects = raycaster.blockingObjects;
                anyThreeDOrAll |= blockingObjects == GraphicRaycaster.BlockingObjects.ThreeD || blockingObjects == GraphicRaycaster.BlockingObjects.All;
                anyTwoDOrAll |= blockingObjects == GraphicRaycaster.BlockingObjects.TwoD || blockingObjects == GraphicRaycaster.BlockingObjects.All;
            }

#if !PACKAGE_PHYSICS
            m_PhysicsNotPresentWarning.style.display = anyThreeDOrAll ? DisplayStyle.Flex : DisplayStyle.None;
#endif
#if !PACKAGE_PHYSICS2D
            m_Physics2DNotPresentWarning.style.display = anyTwoDOrAll ? DisplayStyle.Flex : DisplayStyle.None;
#endif
        }
#endif
    }
}
