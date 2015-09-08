using System.Linq;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    /// <summary>
    /// Editor class used to edit UI Graphics.
    /// </summary>

    [CustomEditor(typeof(MaskableGraphic), false)]
    [CanEditMultipleObjects]
    public class GraphicEditor : Editor
    {
        protected SerializedProperty m_Script;
        protected SerializedProperty m_Color;
        protected SerializedProperty m_Material;
        protected SerializedProperty m_RaycastTarget;

        private GUIContent m_CorrectButtonContent;
        protected AnimBool m_ShowNativeSize;

        protected virtual void OnDisable()
        {
            Tools.hidden = false;
            m_ShowNativeSize.valueChanged.RemoveListener(Repaint);
        }

        protected virtual void OnEnable()
        {
            m_CorrectButtonContent = new GUIContent("Set Native Size", "Sets the size to match the content.");

            m_Script = serializedObject.FindProperty("m_Script");
            m_Color = serializedObject.FindProperty("m_Color");
            m_Material = serializedObject.FindProperty("m_Material");
            m_RaycastTarget = serializedObject.FindProperty("m_RaycastTarget");

            m_ShowNativeSize = new AnimBool(false);
            m_ShowNativeSize.valueChanged.AddListener(Repaint);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_Script);
            AppearanceControlsGUI();
            RaycastControlsGUI();
            serializedObject.ApplyModifiedProperties();
        }

        protected void SetShowNativeSize(bool show, bool instant)
        {
            if (instant)
                m_ShowNativeSize.value = show;
            else
                m_ShowNativeSize.target = show;
        }

        protected void NativeSizeButtonGUI()
        {
            if (EditorGUILayout.BeginFadeGroup(m_ShowNativeSize.faded))
            {
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(EditorGUIUtility.labelWidth);
                    if (GUILayout.Button(m_CorrectButtonContent, EditorStyles.miniButton))
                    {
                        foreach (Graphic graphic in targets.Select(obj => obj as Graphic))
                        {
                            Undo.RecordObject(graphic.rectTransform, "Set Native Size");
                            graphic.SetNativeSize();
                            EditorUtility.SetDirty(graphic);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFadeGroup();
        }

        protected void AppearanceControlsGUI()
        {
            EditorGUILayout.PropertyField(m_Color);
            EditorGUILayout.PropertyField(m_Material);
        }

        protected void RaycastControlsGUI()
        {
            EditorGUILayout.PropertyField(m_RaycastTarget);
        }
    }
}
