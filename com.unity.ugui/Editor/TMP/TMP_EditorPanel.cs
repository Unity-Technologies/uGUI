using UnityEngine;
using UnityEditor;


namespace TMPro.EditorUtilities
{

    [CustomEditor(typeof(TextMeshPro), true), CanEditMultipleObjects]
    public class TMP_EditorPanel : TMP_BaseEditorPanel
    {
        static readonly GUIContent k_SortingLayerLabel = new GUIContent("Sorting Layer", "Name of the Renderer's sorting layer.");
        static readonly GUIContent k_OrderInLayerLabel = new GUIContent("Order in Layer", "Renderer's order within a sorting layer.");
        static readonly GUIContent k_OrthographicLabel = new GUIContent("Orthographic Mode", "Should be enabled when using an orthographic camera. Instructs the shader to not perform any perspective correction.");
        static readonly GUIContent k_VolumetricLabel = new GUIContent("Volumetric Setup", "Use cubes rather than quads to render the text. Allows for volumetric rendering when combined with a compatible shader.");

        private static string[] k_SortingLayerNames;
        bool IsPreset;

        SerializedProperty m_IsVolumetricTextProp;
        SerializedProperty m_IsOrthographicProp;
        Object[] m_Renderers;

        SerializedObject m_RendererSerializedObject;
        SerializedProperty m_RendererSortingLayerProp;
        SerializedProperty m_RendererSortingLayerIDProp;
        SerializedProperty m_RendererSortingOrderProp;

        SerializedProperty m_TextSortingLayerProp;
        SerializedProperty m_TextSortingLayerIDProp;
        SerializedProperty m_TextSortingOrderProp;


        protected override void OnEnable()
        {
            base.OnEnable();

            // Determine if the inspected object is a Preset
            IsPreset = (int)(target as Component).gameObject.hideFlags == 93;

            m_IsOrthographicProp = serializedObject.FindProperty("m_isOrthographic");

            m_IsVolumetricTextProp = serializedObject.FindProperty("m_isVolumetricText");

            m_Renderers = new Object[targets.Length];
            for (int i = 0; i < m_Renderers.Length; i++)
                m_Renderers[i] = (targets[i] as TextMeshPro)?.GetComponent<Renderer>();

            m_RendererSerializedObject = new SerializedObject(m_Renderers);
            m_RendererSortingLayerProp = m_RendererSerializedObject.FindProperty("m_SortingLayer");
            m_RendererSortingLayerIDProp = m_RendererSerializedObject.FindProperty("m_SortingLayerID");
            m_RendererSortingOrderProp = m_RendererSerializedObject.FindProperty("m_SortingOrder");

            m_TextSortingLayerProp = serializedObject.FindProperty("_SortingLayer");
            m_TextSortingLayerIDProp = serializedObject.FindProperty("_SortingLayerID");
            m_TextSortingOrderProp = serializedObject.FindProperty("_SortingOrder");

            // Populate Sorting Layer Names
            k_SortingLayerNames = SortingLayerHelper.sortingLayerNames;
        }

        protected override void DrawExtraSettings()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 24);

            if (GUI.Button(rect, new GUIContent("<b>Extra Settings</b>"), TMP_UIStyleManager.sectionHeader))
                Foldout.extraSettings = !Foldout.extraSettings;

            GUI.Label(rect, (Foldout.extraSettings ? "" : k_UiStateLabel[1]), TMP_UIStyleManager.rightLabel);

            if (Foldout.extraSettings)
            {

                DrawMargins();

                DrawSortingLayer();

                DrawGeometrySorting();

                DrawIsTextObjectScaleStatic();

                DrawOrthographicMode();

                DrawRichText();

                DrawParsing();

                DrawEmojiFallbackSupport();

                DrawSpriteAsset();

                DrawStyleSheet();

                DrawFontFeatures();

                DrawPadding();
                
            }
        }

        private void DrawSortingLayer()
        {
            m_RendererSerializedObject.Update();

            Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);

            // Special handling for Presets where the sorting layer, id and order is serialized with the text object instead of on the MeshRenderer.
            SerializedProperty sortingLayerProp = IsPreset ? m_TextSortingLayerProp : m_RendererSortingLayerProp;
            SerializedProperty sortingLayerIDProp = IsPreset ? m_TextSortingLayerIDProp : m_RendererSortingLayerIDProp;

            EditorGUI.BeginProperty(rect, k_SortingLayerLabel, sortingLayerIDProp);
            EditorGUI.BeginChangeCheck();

            int currentLayerIndex = SortingLayerHelper.GetSortingLayerIndexFromSortingLayerID(sortingLayerIDProp.intValue);
            int newLayerIndex = EditorGUI.Popup(rect, k_SortingLayerLabel, currentLayerIndex, k_SortingLayerNames);

            if (EditorGUI.EndChangeCheck())
            {
                sortingLayerIDProp.intValue = SortingLayer.NameToID(k_SortingLayerNames[newLayerIndex]);
                sortingLayerProp.intValue = SortingLayer.GetLayerValueFromName(k_SortingLayerNames[newLayerIndex]);
                m_HavePropertiesChanged = true;

                // Sync Sorting Layer ID change on potential sub text object.
                TextMeshPro textComponent = m_TextComponent as TextMeshPro;
                textComponent.UpdateSubMeshSortingLayerID(sortingLayerIDProp.intValue);
            }

            EditorGUI.EndProperty();

            // Sorting Order
            SerializedProperty sortingOrderLayerProp = IsPreset ? m_TextSortingOrderProp : m_RendererSortingOrderProp;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(sortingOrderLayerProp, k_OrderInLayerLabel);

            if (EditorGUI.EndChangeCheck())
            {
                m_HavePropertiesChanged = true;

                TextMeshPro textComponent = m_TextComponent as TextMeshPro;
                textComponent.UpdateSubMeshSortingOrder(sortingOrderLayerProp.intValue);
            }

            m_RendererSerializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
        }

        private void DrawOrthographicMode()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_IsOrthographicProp, k_OrthographicLabel);
            if (EditorGUI.EndChangeCheck())
                m_HavePropertiesChanged = true;
        }

        protected void DrawVolumetricSetup()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_IsVolumetricTextProp, k_VolumetricLabel);
            if (EditorGUI.EndChangeCheck())
            {
                m_HavePropertiesChanged = true;
                m_TextComponent.textInfo.ResetVertexLayout(m_IsVolumetricTextProp.boolValue);
            }

            EditorGUILayout.Space();
        }

        // Method to handle multi object selection
        protected override bool IsMixSelectionTypes()
        {
            GameObject[] objects = Selection.gameObjects;
            if (objects.Length > 1)
            {
                for (int i = 0; i < objects.Length; i++)
                {
                    if (objects[i].GetComponent<TextMeshPro>() == null)
                        return true;
                }
            }
            return false;
        }

        protected override void OnUndoRedo()
        {
            int undoEventId = Undo.GetCurrentGroup();
            int lastUndoEventId = s_EventId;

            if (undoEventId != lastUndoEventId)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    //Debug.Log("Undo & Redo Performed detected in Editor Panel. Event ID:" + Undo.GetCurrentGroup());
                    TMPro_EventManager.ON_TEXTMESHPRO_PROPERTY_CHANGED(true, targets[i] as TextMeshPro);
                    s_EventId = undoEventId;
                }
            }
        }

    }
}
