using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomPropertyDrawer(typeof(Navigation), true)]
    public class NavigationDrawer : PropertyDrawer
    {
        private class Styles
        {
            readonly public GUIContent navigationContent;

            public Styles()
            {
                navigationContent = new GUIContent("Navigation");
            }
        }

        private static Styles s_Styles = null;

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            if (s_Styles == null)
                s_Styles = new Styles();

            Rect drawRect = pos;
            drawRect.height = EditorGUIUtility.singleLineHeight;

            SerializedProperty navigation = prop.FindPropertyRelative("m_Mode");
            Navigation.Mode navMode = GetNavigationMode(navigation);

            EditorGUI.PropertyField(drawRect, navigation, s_Styles.navigationContent);

            ++EditorGUI.indentLevel;

            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            switch (navMode)
            {
                case Navigation.Mode.Explicit:
                {
                    SerializedProperty selectOnUp = prop.FindPropertyRelative("m_SelectOnUp");
                    SerializedProperty selectOnDown = prop.FindPropertyRelative("m_SelectOnDown");
                    SerializedProperty selectOnLeft = prop.FindPropertyRelative("m_SelectOnLeft");
                    SerializedProperty selectOnRight = prop.FindPropertyRelative("m_SelectOnRight");

                    EditorGUI.PropertyField(drawRect, selectOnUp);
                    drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    EditorGUI.PropertyField(drawRect, selectOnDown);
                    drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    EditorGUI.PropertyField(drawRect, selectOnLeft);
                    drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    EditorGUI.PropertyField(drawRect, selectOnRight);
                    drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
                break;
            }

            --EditorGUI.indentLevel;
        }

        static Navigation.Mode GetNavigationMode(SerializedProperty navigation)
        {
            return (Navigation.Mode)navigation.enumValueIndex;
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            SerializedProperty navigation = prop.FindPropertyRelative("m_Mode");
            if (navigation == null)
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            Navigation.Mode navMode = GetNavigationMode(navigation);

            switch (navMode)
            {
                case Navigation.Mode.None: return EditorGUIUtility.singleLineHeight;
                case Navigation.Mode.Explicit: return 5 * EditorGUIUtility.singleLineHeight + 5 * EditorGUIUtility.standardVerticalSpacing;
                default: return EditorGUIUtility.singleLineHeight + 1 * EditorGUIUtility.standardVerticalSpacing;
            }
        }
    }
}
