﻿using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.TextCore.Text;

namespace TMPro.EditorUtilities
{

    public class TMP_ContextMenus : Editor
    {

        private static Texture m_copiedTexture;

        private static Material m_copiedProperties;
        private static Material m_copiedAtlasProperties;


        // Add a Context Menu to the Texture Editor Panel to allow Copy / Paste of Texture.
        #if !TEXTCORE_1_0_OR_NEWER
        [MenuItem("CONTEXT/Texture/Copy", false, 2000)]
        static void CopyTexture(MenuCommand command)
        {
            m_copiedTexture = command.context as Texture;
        }


        // Select the currently assigned material or material preset.
        [MenuItem("CONTEXT/Material/Select Material", false, 500)]
        static void SelectMaterial(MenuCommand command)
        {
            Material mat = command.context as Material;

            // Select current material
            EditorUtility.FocusProjectWindow();
            EditorGUIUtility.PingObject(mat);
        }
        #endif


        // Add a Context Menu to allow easy duplication of the Material.
        [MenuItem("CONTEXT/Material/Create Material Preset", false)]
        static void DuplicateMaterial(MenuCommand command)
        {
            // Get the type of text object
            // If material is not a base material, we get material leaks...

            Material source_Mat = (Material)command.context;
            if (!EditorUtility.IsPersistent(source_Mat))
            {
                Debug.LogWarning("Material is an instance and cannot be converted into a persistent asset.");
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(source_Mat).Split('.')[0];

            if (assetPath.IndexOf("Assets/", System.StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                Debug.LogWarning("Material Preset cannot be created from a material that is located outside the project.");
                return;
            }

            Material duplicate = new Material(source_Mat);

            // Need to manually copy the shader keywords
            duplicate.shaderKeywords = source_Mat.shaderKeywords;

            AssetDatabase.CreateAsset(duplicate, AssetDatabase.GenerateUniqueAssetPath(assetPath + ".mat"));

            GameObject[] selectedObjects = Selection.gameObjects;

            // Assign new Material Preset to selected text objects.
            for (int i = 0; i < selectedObjects.Length; i++)
            {
                TMP_Text textObject = selectedObjects[i].GetComponent<TMP_Text>();

                if (textObject != null)
                {
                    textObject.fontSharedMaterial = duplicate;
                }
                else
                {
                    TMP_SubMesh subMeshObject = selectedObjects[i].GetComponent<TMP_SubMesh>();

                    if (subMeshObject != null)
                        subMeshObject.sharedMaterial = duplicate;
                    else
                    {
                        TMP_SubMeshUI subMeshUIObject = selectedObjects[i].GetComponent<TMP_SubMeshUI>();

                        if (subMeshUIObject != null)
                            subMeshUIObject.sharedMaterial = duplicate;
                    }
                }
            }

            // Ping newly created Material Preset.
            EditorUtility.FocusProjectWindow();
            EditorGUIUtility.PingObject(duplicate);
        }


        // COPY MATERIAL PROPERTIES
        #if !TEXTCORE_1_0_OR_NEWER
        [MenuItem("CONTEXT/Material/Copy Material Properties", false)]
        static void CopyMaterialProperties(MenuCommand command)
        {
            Material mat = null;
            if (command.context.GetType() == typeof(Material))
                mat = (Material)command.context;
            else
            {
                mat = Selection.activeGameObject.GetComponent<CanvasRenderer>().GetMaterial();
            }

            m_copiedProperties = new Material(mat);

            m_copiedProperties.shaderKeywords = mat.shaderKeywords;

            m_copiedProperties.hideFlags = HideFlags.DontSave;
        }


        // PASTE MATERIAL PROPERTIES
        [MenuItem("CONTEXT/Material/Paste Material Properties", true)]
        static bool PasteMaterialPropertiesValidate(MenuCommand command)
        {
            if (m_copiedProperties == null)
                return false;

            return AssetDatabase.IsOpenForEdit(command.context);
        }

        [MenuItem("CONTEXT/Material/Paste Material Properties", false)]
        static void PasteMaterialProperties(MenuCommand command)
        {
            if (m_copiedProperties == null)
            {
                Debug.LogWarning("No Material Properties to Paste. Use Copy Material Properties first.");
                return;
            }

            Material mat = null;
            if (command.context.GetType() == typeof(Material))
                mat = (Material)command.context;
            else
            {
                mat = Selection.activeGameObject.GetComponent<CanvasRenderer>().GetMaterial();
            }

            Undo.RecordObject(mat, "Paste Material");

            ShaderUtilities.GetShaderPropertyIDs(); // Make sure we have valid Property IDs
            if (mat.HasProperty(ShaderUtilities.ID_GradientScale))
            {
                // Preserve unique SDF properties from destination material.
                m_copiedProperties.SetTexture(ShaderUtilities.ID_MainTex, mat.GetTexture(ShaderUtilities.ID_MainTex));
                m_copiedProperties.SetFloat(ShaderUtilities.ID_GradientScale, mat.GetFloat(ShaderUtilities.ID_GradientScale));
                m_copiedProperties.SetFloat(ShaderUtilities.ID_TextureWidth, mat.GetFloat(ShaderUtilities.ID_TextureWidth));
                m_copiedProperties.SetFloat(ShaderUtilities.ID_TextureHeight, mat.GetFloat(ShaderUtilities.ID_TextureHeight));
            }

            EditorShaderUtilities.CopyMaterialProperties(m_copiedProperties, mat);

            // Copy ShaderKeywords from one material to the other.
            mat.shaderKeywords = m_copiedProperties.shaderKeywords;

            // Let TextMeshPro Objects that this mat has changed.
            TMPro_EventManager.ON_MATERIAL_PROPERTY_CHANGED(true, mat);
        }


        // Enable Resetting of Material properties without losing unique properties of the font atlas.
        [MenuItem("CONTEXT/Material/Reset", true, 2100)]
        static bool ResetSettingsValidate(MenuCommand command)
        {
            return AssetDatabase.IsOpenForEdit(command.context);
        }

        [MenuItem("CONTEXT/Material/Reset", false, 2100)]
        static void ResetSettings(MenuCommand command)
        {
            Material mat = null;
            if (command.context.GetType() == typeof(Material))
                mat = (Material)command.context;
            else
            {
                mat = Selection.activeGameObject.GetComponent<CanvasRenderer>().GetMaterial();
            }

            Undo.RecordObject(mat, "Reset Material");

            ShaderUtilities.GetShaderPropertyIDs(); // Make sure we have valid Property IDs
            if (mat.HasProperty(ShaderUtilities.ID_GradientScale))
            {
                bool isSRPShader = mat.HasProperty(ShaderUtilities.ID_IsoPerimeter);

                // Copy unique properties of the SDF Material
                var texture = mat.GetTexture(ShaderUtilities.ID_MainTex);
                var gradientScale = mat.GetFloat(ShaderUtilities.ID_GradientScale);

                float texWidth = 0, texHeight = 0;
                float normalWeight = 0, boldWeight = 0;

                if (!isSRPShader)
                {
                    texWidth = mat.GetFloat(ShaderUtilities.ID_TextureWidth);
                    texHeight = mat.GetFloat(ShaderUtilities.ID_TextureHeight);
                    normalWeight = mat.GetFloat(ShaderUtilities.ID_WeightNormal);
                    boldWeight = mat.GetFloat(ShaderUtilities.ID_WeightBold);
                }

                var stencilId = 0.0f;
                var stencilComp = 0.0f;

                if (mat.HasProperty(ShaderUtilities.ID_StencilID))
                {
                    stencilId = mat.GetFloat(ShaderUtilities.ID_StencilID);
                    stencilComp = mat.GetFloat(ShaderUtilities.ID_StencilComp);
                }

                // Reset the material
                Unsupported.SmartReset(mat);

                // Reset ShaderKeywords
                mat.shaderKeywords = new string[0]; // { "BEVEL_OFF", "GLOW_OFF", "UNDERLAY_OFF" };

                // Copy unique material properties back to the material.
                mat.SetTexture(ShaderUtilities.ID_MainTex, texture);
                mat.SetFloat(ShaderUtilities.ID_GradientScale, gradientScale);

                if (!isSRPShader)
                {
                    mat.SetFloat(ShaderUtilities.ID_TextureWidth, texWidth);
                    mat.SetFloat(ShaderUtilities.ID_TextureHeight, texHeight);
                    mat.SetFloat(ShaderUtilities.ID_WeightNormal, normalWeight);
                    mat.SetFloat(ShaderUtilities.ID_WeightBold, boldWeight);
                }

                if (mat.HasProperty(ShaderUtilities.ID_StencilID))
                {
                    mat.SetFloat(ShaderUtilities.ID_StencilID, stencilId);
                    mat.SetFloat(ShaderUtilities.ID_StencilComp, stencilComp);
                }
            }
            else
            {
                Unsupported.SmartReset(mat);
            }

            TMPro_EventManager.ON_MATERIAL_PROPERTY_CHANGED(true, mat);
        }


        //This function is used for debugging and fixing potentially broken font atlas links.
        [MenuItem("CONTEXT/Material/Copy Atlas", false, 2000)]
        static void CopyAtlas(MenuCommand command)
        {
            Material mat = command.context as Material;

            m_copiedAtlasProperties = new Material(mat);
            m_copiedAtlasProperties.hideFlags = HideFlags.DontSave;
        }


        // This function is used for debugging and fixing potentially broken font atlas links
        [MenuItem("CONTEXT/Material/Paste Atlas", true, 2001)]
        static bool PasteAtlasValidate(MenuCommand command)
        {
            if (m_copiedAtlasProperties == null && m_copiedTexture == null)
                return false;

            return AssetDatabase.IsOpenForEdit(command.context);
        }

        [MenuItem("CONTEXT/Material/Paste Atlas", false, 2001)]
        static void PasteAtlas(MenuCommand command)
        {
            Material mat = command.context as Material;

            if (mat == null)
                return;

            if (m_copiedAtlasProperties != null)
            {
                Undo.RecordObject(mat, "Paste Texture");

                ShaderUtilities.GetShaderPropertyIDs(); // Make sure we have valid Property IDs

                if (m_copiedAtlasProperties.HasProperty(ShaderUtilities.ID_MainTex))
                    mat.SetTexture(ShaderUtilities.ID_MainTex, m_copiedAtlasProperties.GetTexture(ShaderUtilities.ID_MainTex));

                if (m_copiedAtlasProperties.HasProperty(ShaderUtilities.ID_GradientScale))
                {
                    mat.SetFloat(ShaderUtilities.ID_GradientScale, m_copiedAtlasProperties.GetFloat(ShaderUtilities.ID_GradientScale));
                    mat.SetFloat(ShaderUtilities.ID_TextureWidth, m_copiedAtlasProperties.GetFloat(ShaderUtilities.ID_TextureWidth));
                    mat.SetFloat(ShaderUtilities.ID_TextureHeight, m_copiedAtlasProperties.GetFloat(ShaderUtilities.ID_TextureHeight));
                }
            }
            else if (m_copiedTexture != null)
            {
                Undo.RecordObject(mat, "Paste Texture");

                mat.SetTexture(ShaderUtilities.ID_MainTex, m_copiedTexture);
            }
        }
        #endif


        // Context Menus for TMPro Font Assets
        //This function is used for debugging and fixing potentially broken font atlas links.
        [MenuItem("CONTEXT/TMP_FontAsset/Extract Atlas", false, 2200)]
        static void ExtractAtlas(MenuCommand command)
        {
            TMP_FontAsset font = command.context as TMP_FontAsset;

            string fontPath = AssetDatabase.GetAssetPath(font);
            string texPath = Path.GetDirectoryName(fontPath) + "/" + Path.GetFileNameWithoutExtension(fontPath) + " Atlas.png";

            // Create a Serialized Object of the texture to allow us to make it readable.
            SerializedObject texprop = new SerializedObject(font.material.GetTexture(ShaderUtilities.ID_MainTex));
            texprop.FindProperty("m_IsReadable").boolValue = true;
            texprop.ApplyModifiedProperties();

            // Create a copy of the texture.
            Texture2D tex = Instantiate(font.material.GetTexture(ShaderUtilities.ID_MainTex)) as Texture2D;

            // Set the texture to not readable again.
            texprop.FindProperty("m_IsReadable").boolValue = false;
            texprop.ApplyModifiedProperties();

            Debug.Log(texPath);
            // Saving File for Debug
            var pngData = tex.EncodeToPNG();
            File.WriteAllBytes(texPath, pngData);

            AssetDatabase.Refresh();
            DestroyImmediate(tex);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="command"></param>
        [MenuItem("CONTEXT/TMP_FontAsset/Update Atlas Texture...", false, 2000)]
        static void RegenerateFontAsset(MenuCommand command)
        {
            TMP_FontAsset fontAsset = command.context as TMP_FontAsset;

            if (fontAsset != null)
            {
                TMPro_FontAssetCreatorWindow.ShowFontAtlasCreatorWindow(fontAsset);
            }
        }

        /*[MenuItem("CONTEXT/TMP_FontAsset/Force Upgrade To Version 1.1.0...", false, 2020)]
        static void ForceFontAssetUpgrade(MenuCommand command)
        {
            TMP_FontAsset fontAsset = command.context as TMP_FontAsset;

            if (fontAsset != null)
            {
                fontAsset.UpgradeFontAsset();
                TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, fontAsset);
            }
        }*/


        /// <summary>
        /// Clear Dynamic Font Asset data such as glyph, character and font features.
        /// </summary>
        /// <param name="command"></param>
        [MenuItem("CONTEXT/TMP_FontAsset/Reset", true, 100)]
        static bool ClearFontAssetDataValidate(MenuCommand command)
        {
            return AssetDatabase.IsOpenForEdit(command.context);
        }

        [MenuItem("CONTEXT/TMP_FontAsset/Reset", false, 100)]
        static void ClearFontAssetData(MenuCommand command)
        {
            TMP_FontAsset fontAsset = command.context as TMP_FontAsset;

            if (fontAsset == null)
                return;

            if (Selection.activeObject != fontAsset)
                Selection.activeObject = fontAsset;

            fontAsset.ClearFontAssetData(true);

            TMP_ResourceManager.RebuildFontAssetCache();

            TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, fontAsset);
        }

        /// <summary>
        /// Clear Character and Glyph data (only).
        /// </summary>
        /// <param name="command"></param>
        [MenuItem("CONTEXT/TMP_FontAsset/Clear Dynamic Data", true, 2100)]
        static bool ClearFontCharacterDataValidate(MenuCommand command)
        {
            return AssetDatabase.IsOpenForEdit(command.context);
        }

        [MenuItem("CONTEXT/TMP_FontAsset/Clear Dynamic Data", false, 2100)]
        static void ClearFontCharacterData(MenuCommand command)
        {
            TMP_FontAsset fontAsset = command.context as TMP_FontAsset;

            if (fontAsset == null)
                return;

            if (Selection.activeObject != fontAsset)
                Selection.activeObject = fontAsset;

            fontAsset.ClearCharacterAndGlyphTablesInternal();

            TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, fontAsset);
        }

        [MenuItem("CONTEXT/TMP_FontAsset/Reset FaceInfo", priority = 101)]
        static void ResetFaceInfo(MenuCommand command)
        {
            TMP_FontAsset fontAsset = command.context as TMP_FontAsset;

            if (fontAsset == null)
                return;

            if (Selection.activeObject != fontAsset)
                Selection.activeObject = fontAsset;

            if (fontAsset.LoadFontFace() != FontEngineError.Success)
                return;

            fontAsset.faceInfo = FontEngine.GetFaceInfo();
            TextResourceManager.RebuildFontAssetCache();
            TextEventManager.ON_FONT_PROPERTY_CHANGED(true, fontAsset);

            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssetIfDirty(fontAsset);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Import all font features
        /// </summary>
        /// <param name="command"></param>
        #if TEXTCORE_FONT_ENGINE_1_5_OR_NEWER
        [MenuItem("CONTEXT/TMP_FontAsset/Import Font Features", true, 2110)]
        static bool ReimportFontFeaturesValidate(MenuCommand command)
        {
            return AssetDatabase.IsOpenForEdit(command.context);
        }

        [MenuItem("CONTEXT/TMP_FontAsset/Import Font Features", false, 2110)]
        static void ReimportFontFeatures(MenuCommand command)
        {
            TMP_FontAsset fontAsset = command.context as TMP_FontAsset;

            if (fontAsset == null)
                return;

            if (Selection.activeObject != fontAsset)
                Selection.activeObject = fontAsset;

            fontAsset.ImportFontFeatures();

            TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, fontAsset);
        }
        #endif

        /// <summary>
        ///
        /// </summary>
        /// <param name="command"></param>
        [MenuItem("CONTEXT/TrueTypeFontImporter/Create TMP Font Asset...", false, 200)]
        static void CreateFontAsset(MenuCommand command)
        {
            TrueTypeFontImporter importer = command.context as TrueTypeFontImporter;

            if (importer != null)
            {
                Font sourceFontFile = AssetDatabase.LoadAssetAtPath<Font>(importer.assetPath);

                if (sourceFontFile)
                    TMPro_FontAssetCreatorWindow.ShowFontAtlasCreatorWindow(sourceFontFile);
            }
        }
    }
}
