using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(RectMask2D), true)]
    [CanEditMultipleObjects]
    /// <summary>
    /// Custom editor for the RectMask2d component.
    /// Extend this class to write a custom editor for a component derived from Mask.
    /// </summary>
    public class RectMask2DEditor : Editor
    {
        SerializedProperty m_Padding;
        SerializedProperty m_Softness;
        GUIContent m_PaddingContent;
        GUIContent m_SoftnessContent;
        GUIContent m_LeftContent;
        GUIContent m_RightContent;
        GUIContent m_TopContent;
        GUIContent m_BottomContent;

        static private bool m_ShowOffsets = false;
        static private bool m_ShowSoftness = true;

        protected virtual void OnEnable()
        {
            m_PaddingContent = L10n.TextContent("Padding", null, null, null);
            m_SoftnessContent = L10n.TextContent("Softness", null, null, null);
            m_LeftContent = L10n.TextContent("Left", null, null, null);
            m_RightContent = L10n.TextContent("Right", null, null, null);
            m_TopContent = L10n.TextContent("Top", null, null, null);
            m_BottomContent = L10n.TextContent("Bottom", null, null, null);
            m_Padding = serializedObject.FindProperty("m_Padding");
            m_Softness = serializedObject.FindProperty("m_Softness");
        }

        public override void OnInspectorGUI()
        {
            m_ShowOffsets = EditorGUILayout.Foldout(m_ShowOffsets, m_PaddingContent, true);

            if (m_ShowOffsets)
                OffsetGUI();

            m_ShowSoftness = EditorGUILayout.Foldout(m_ShowSoftness, m_SoftnessContent, true);

            if (m_ShowSoftness)
                SoftnessGUI();

            serializedObject.ApplyModifiedProperties();
        }

        void SoftnessGUI()
        {
            EditorGUI.indentLevel++;
            EdgeField("x", 0, m_LeftContent);
            EdgeField("z", 2, m_RightContent);
            EdgeField("w", 3, m_TopContent);
            EdgeField("y", 1, m_BottomContent);
            EditorGUI.indentLevel--;
        }

        // Shows one edge through edgeSoftness, so never-edited data shows Right/Top as Left/Bottom. An edit writes all four edges
        // and the version through each target's own SerializedObject, so undo, prefab overrides and OnValidate run as usual.
        void EdgeField(string component, int edge, GUIContent label)
        {
            var masks = targets;
            float value = ((RectMask2D)masks[0]).edgeSoftness[edge];
            bool mixed = false;
            for (int i = 1; i < masks.Length; i++)
                mixed |= ((RectMask2D)masks[i]).edgeSoftness[edge] != value;

            var rect = EditorGUILayout.GetControlRect();
            label = EditorGUI.BeginProperty(rect, label, m_Softness.FindPropertyRelative(component));
            EditorGUI.showMixedValue = mixed;
            EditorGUI.BeginChangeCheck();
            float newValue = EditorGUI.FloatField(rect, label, value);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                foreach (RectMask2D mask in masks)
                {
                    var edges = mask.edgeSoftness;
                    edges[edge] = newValue;
                    var maskObject = new SerializedObject(mask);
                    maskObject.FindProperty("m_Softness").vector4Value = edges;
                    maskObject.FindProperty("m_Version").intValue = RectMask2D.k_EdgeSoftnessVersion;
                    maskObject.ApplyModifiedProperties();
                }
                serializedObject.Update();
            }
            EditorGUI.EndProperty();
        }

        void OffsetGUI()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.indentLevel++;
                Vector4 newPadding = m_Padding.vector4Value;

                newPadding.x = EditorGUILayout.FloatField(m_LeftContent, newPadding.x);
                newPadding.z = EditorGUILayout.FloatField(m_RightContent, newPadding.z);
                newPadding.w = EditorGUILayout.FloatField(m_TopContent, newPadding.w);
                newPadding.y = EditorGUILayout.FloatField(m_BottomContent, newPadding.y);

                if (check.changed)
                {
                    m_Padding.vector4Value = newPadding;
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}
