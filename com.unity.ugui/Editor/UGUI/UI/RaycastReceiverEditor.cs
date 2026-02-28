using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace UnityEditor.UI
{
    /// <summary>
    /// Editor class used to edit RaycastReceiver components.
    /// Extend this class to write your own raycast receiver editor.
    /// </summary>
    [CustomEditor(typeof(RaycastReceiver), false)]
    [CanEditMultipleObjects]
    public class RaycastReceiverEditor : Editor
    {
        /// <summary>
        /// Serialized property representing the script reference.
        /// </summary>
        protected SerializedProperty m_Script;

        /// <summary>
        /// Serialized property for the target graphic component used for raycasting.
        /// </summary>
        protected SerializedProperty m_RaycastTarget;

        /// <summary>
        /// Serialized property for the padding (Vector4) applied to the raycast target.
        /// </summary>
        protected SerializedProperty m_RaycastPadding;

        /// <summary>
        /// Called when the object is loaded.
        /// Initializes serialized properties and subscribes to the SceneView.duringSceneGui event.
        /// </summary>
        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
            m_RaycastTarget = serializedObject.FindProperty("m_RaycastTarget");
            m_RaycastPadding = serializedObject.FindProperty("m_RaycastPadding");

            SceneView.duringSceneGui += DrawAnchorsOnSceneView;
        }

        /// <summary>
        /// Called when the object goes out of scope.
        /// Unsubscribes from the SceneView.duringSceneGui event.
        /// </summary>
        protected virtual void OnDisable()
        {
            SceneView.duringSceneGui -= DrawAnchorsOnSceneView;
        }

        /// <summary>
        /// Implementation of Editor.CreateInspectorGUI to create a UIElements-based inspector.
        /// </summary>
        /// <returns>The root VisualElement containing the custom inspector UI.</returns>
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            //Draw the Script field (standard practice is to disable it)
            var scriptField = new PropertyField(m_Script);
            scriptField.SetEnabled(false);
            root.Add(scriptField);

            //Draw Raycast Target
            var raycastTargetField = new PropertyField(m_RaycastTarget);
            raycastTargetField.RegisterValueChangeCallback(evt =>
            {
                foreach (var obj in targets)
                {
                    if (obj is RaycastReceiver receiver)
                    {
                        receiver.SetRaycastDirty();
                    }
                }
            });
            root.Add(raycastTargetField);

            //Draw Raycast Padding Foldout
            var paddingFoldout = new Foldout
            {
                text = "Raycast Padding",
                viewDataKey = "RaycastReceiver_PaddingFoldout", // Use to have the foldout's state persist
                value = false // Default to closed (false)
            };

            //We need to bind specific float fields to the Vector4 sub-properties
            //to maintain the specific labels (Left, Bottom, Right, Top)
            //and order (x, y, z, w) from the original IMGUI code.
            CreatePaddingField(paddingFoldout, m_RaycastPadding.FindPropertyRelative("x"), "Left");
            CreatePaddingField(paddingFoldout, m_RaycastPadding.FindPropertyRelative("y"), "Bottom");
            CreatePaddingField(paddingFoldout, m_RaycastPadding.FindPropertyRelative("z"), "Right");
            CreatePaddingField(paddingFoldout, m_RaycastPadding.FindPropertyRelative("w"), "Top");
            root.Add(paddingFoldout);

            //Ensure SceneView repaints when padding changes
            root.TrackPropertyValue(m_RaycastPadding, prop => SceneView.RepaintAll());
            return root;
        }

        private void CreatePaddingField(VisualElement parent, SerializedProperty prop, string label)
        {
            var field = new FloatField(label);
            field.BindProperty(prop);
            parent.Add(field);
        }

        // SceneView logic remains largely unchanged as Handles are still used for 3D/2D scene drawing
        private void DrawAnchorsOnSceneView(SceneView sceneView)
        {
            if (!target || targets.Length > 1)
                return;

            if (!sceneView.drawGizmos || !EditorGUIUtility.IsGizmosAllowedForObject(target))
                return;

            Graphic graphic = target as Graphic;

            if (graphic == null)
                return;

            RectTransform gui = graphic.rectTransform;
            Transform ownSpace = gui.transform;
            Rect rectInOwnSpace = gui.rect;

            //Draw Raycast Padding
            Handles.color = Handles.UIColliderHandleColor;
            DrawRect(rectInOwnSpace, ownSpace, graphic.raycastPadding);
        }

        private void DrawRect(Rect rect, Transform space, Vector4 offset)
        {
            Span<Vector3> points = stackalloc Vector3[4];

            //Originally had a Vector2 which would convert to Vector3 automatically (with z value set to 0)
            points[0] = new Vector3(rect.x + offset.x, rect.y + offset.y, 0f);
            points[1] = new Vector3(rect.x + offset.x, rect.yMax - offset.w, 0f);
            points[2] = new Vector3(rect.xMax - offset.z, rect.yMax - offset.w, 0f);
            points[3] = new Vector3(rect.xMax - offset.z, rect.y + offset.y, 0f);

            //Convert to Worldspace
            space.TransformPoints(points);

            //Draw
            Handles.DrawLine(points[0], points[1]);
            Handles.DrawLine(points[1], points[2]);
            Handles.DrawLine(points[2], points[3]);
            Handles.DrawLine(points[3], points[0]);
        }
    }
}
