using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomPropertyDrawer(typeof(AnimationTriggers), true)]
    public class AnimationTriggersDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
        {
            Rect drawRect = rect;
            drawRect.height = EditorGUIUtility.singleLineHeight;

            SerializedProperty normalTrigger = prop.FindPropertyRelative("m_NormalTrigger");
            SerializedProperty higlightedTrigger = prop.FindPropertyRelative("m_HighlightedTrigger");
            SerializedProperty pressedTrigger = prop.FindPropertyRelative("m_PressedTrigger");
            SerializedProperty disabledTrigger = prop.FindPropertyRelative("m_DisabledTrigger");

            EditorGUI.PropertyField(drawRect, normalTrigger);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(drawRect, higlightedTrigger);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(drawRect, pressedTrigger);
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(drawRect, disabledTrigger);
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            return 4 * EditorGUIUtility.singleLineHeight + 3 * EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
