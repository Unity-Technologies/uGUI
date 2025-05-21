using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using UnityEditor;

using Object = UnityEngine.Object;


namespace TMPro
{
    static class TMP_FontAsset_CreationMenu
    {
        [MenuItem("Assets/Create/TextMeshPro/Font Asset/Font Asset Variant", false, 200)]
        static void CreateFontAssetVariant()
        {
            Object target = Selection.activeObject;

            // Make sure the selection is a font file
            if (target == null || target.GetType() != typeof(TMP_FontAsset))
            {
                Debug.LogWarning("A Font file must first be selected in order to create a Font Asset.");
                return;
            }

            // Make sure TMP Essential Resources have been imported in the user project.
            if (TMP_Settings.instance == null)
            {
                Debug.Log("Unable to create font asset. Please import the TMP Essential Resources.");
                return;
            }

            TMP_FontAsset sourceFontAsset = (TMP_FontAsset)target;

            string sourceFontFilePath = AssetDatabase.GetAssetPath(target);

            string folderPath = Path.GetDirectoryName(sourceFontFilePath);
            string assetName = Path.GetFileNameWithoutExtension(sourceFontFilePath);

            string newAssetFilePathWithName = AssetDatabase.GenerateUniqueAssetPath(folderPath + "/" + assetName + " - Variant.asset");

            // Set Texture and Material reference to the source font asset.
            TMP_FontAsset fontAsset = ScriptableObject.Instantiate<TMP_FontAsset>(sourceFontAsset);
            AssetDatabase.CreateAsset(fontAsset, newAssetFilePathWithName);

            fontAsset.atlasPopulationMode = AtlasPopulationMode.Static;

            // Initialize array for the font atlas textures.
            fontAsset.atlasTextures = sourceFontAsset.atlasTextures;
            fontAsset.material = sourceFontAsset.material;

            // Not sure if this is still necessary in newer versions of Unity.
            EditorUtility.SetDirty(fontAsset);

            AssetDatabase.SaveAssets();
        }

        [MenuItem("Assets/Create/TextMeshPro/Font Asset/SDF #%F12", false, 100)]
        //[MenuItem("Assets/Create/TextMeshPro/Font Asset", false, 100)]
        static void CreateFontAssetSDF()
        {
            CreateFontAsset(GlyphRenderMode.SDFAA);
        }

        [MenuItem("Assets/Create/TextMeshPro/Font Asset/Bitmap", false, 105)]
        static void CreateFontAssetBitmap()
        {
            CreateFontAsset(GlyphRenderMode.SMOOTH);
        }

        #if TEXTCORE_FONT_ENGINE_1_5_OR_NEWER
        [MenuItem("Assets/Create/TextMeshPro/Font Asset/Color", false, 110)]
        static void CreateFontAssetColor()
        {
            CreateFontAsset(GlyphRenderMode.COLOR);
        }
        #endif

        static void CreateFontAsset(GlyphRenderMode renderMode)
        {
            Object[] targets = Selection.objects;

            if (targets == null)
            {
                Debug.LogWarning("A Font file must first be selected in order to create a Font Asset.");
                return;
            }

            // Make sure TMP Essential Resources have been imported in the user project.
            if (TMP_Settings.instance == null)
            {
                Debug.Log("Unable to create font asset. Please import the TMP Essential Resources.");

                // Show Window to Import TMP Essential Resources
                return;
            }

            for (int i = 0; i < targets.Length; i++)
            {
                Object target = targets[i];

                // Make sure the selection is a font file
                if (target == null || target.GetType() != typeof(Font))
                {
                    Debug.LogWarning("Selected Object [" + target.name + "] is not a Font file. A Font file must be selected in order to create a Font Asset.", target);
                    continue;
                }

                CreateFontAssetFromSelectedObject(target, renderMode);
            }
        }

        static void CreateFontAssetFromSelectedObject(Object target, GlyphRenderMode renderMode)
        {
            Font font = (Font)target;

            string sourceFontFilePath = AssetDatabase.GetAssetPath(target);

            string folderPath = Path.GetDirectoryName(sourceFontFilePath);
            string assetName = Path.GetFileNameWithoutExtension(sourceFontFilePath);

            string newAssetFilePathWithName;
            ;
            switch (renderMode)
            {
                case GlyphRenderMode.SMOOTH:
                    newAssetFilePathWithName = AssetDatabase.GenerateUniqueAssetPath(folderPath + "/" + assetName + " Bitmap.asset");
                    break;
                #if TEXTCORE_FONT_ENGINE_1_5_OR_NEWER
                case GlyphRenderMode.COLOR:
                    newAssetFilePathWithName = AssetDatabase.GenerateUniqueAssetPath(folderPath + "/" + assetName + " Color.asset");
                    break;
                #endif
                case GlyphRenderMode.SDFAA:
                default:
                    newAssetFilePathWithName = AssetDatabase.GenerateUniqueAssetPath(folderPath + "/" + assetName + " SDF.asset");
                    break;
            }

            // Initialize FontEngine
            FontEngine.InitializeFontEngine();

            // Load Font Face
            if (FontEngine.LoadFontFace(font, 90) != FontEngineError.Success)
            {
                Debug.LogWarning("Unable to load font face for [" + font.name + "]. Make sure \"Include Font Data\" is enabled in the Font Import Settings.", font);
                return;
            }

            // Create new Font Asset
            TMP_FontAsset fontAsset = ScriptableObject.CreateInstance<TMP_FontAsset>();
            AssetDatabase.CreateAsset(fontAsset, newAssetFilePathWithName);

            fontAsset.version = "1.1.0";
            fontAsset.faceInfo = FontEngine.GetFaceInfo();

            // Set font reference and GUID
            fontAsset.sourceFontFile = font;
            fontAsset.m_SourceFontFileGUID = AssetDatabase.AssetPathToGUID(sourceFontFilePath);
            fontAsset.m_SourceFontFile_EditorRef = font;

            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            fontAsset.clearDynamicDataOnBuild = TMP_Settings.clearDynamicDataOnBuild;

            // Get all font features
            //fontAsset.ImportFontFeatures();

            // Default atlas resolution is 1024 x 1024.
            fontAsset.atlasTextures = new Texture2D[1];
            int atlasWidth = fontAsset.atlasWidth = 1024;
            int atlasHeight = fontAsset.atlasHeight = 1024;
            int atlasPadding = fontAsset.atlasPadding = 9;

            Texture2D texture;
            Material mat;
            Shader shader;
            int packingModifier;

            switch (renderMode)
            {
                case GlyphRenderMode.SMOOTH:
                    fontAsset.atlasRenderMode = GlyphRenderMode.SMOOTH;
                    texture = new Texture2D(1, 1, TextureFormat.Alpha8, false);
                    shader = Shader.Find("TextMeshPro/Bitmap");
                    packingModifier = 0;
                    mat = new Material(shader);
                    break;
                #if TEXTCORE_FONT_ENGINE_1_5_OR_NEWER
                case GlyphRenderMode.COLOR:
                    fontAsset.atlasRenderMode = GlyphRenderMode.COLOR;
                    texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    shader = Shader.Find("TextMeshPro/Sprite");
                    packingModifier = 0;
                    mat = new Material(shader);
                    break;
                #endif
                case GlyphRenderMode.SDFAA:
                default:
                    fontAsset.atlasRenderMode = GlyphRenderMode.SDFAA;
                    texture = new Texture2D(1, 1, TextureFormat.Alpha8, false);
                    shader = Shader.Find("TextMeshPro/Distance Field");
                    packingModifier = 1;
                    mat = new Material(shader);

                    mat.SetFloat(ShaderUtilities.ID_GradientScale, atlasPadding + packingModifier);
                    mat.SetFloat(ShaderUtilities.ID_WeightNormal, fontAsset.normalStyle);
                    mat.SetFloat(ShaderUtilities.ID_WeightBold, fontAsset.boldStyle);

                    break;
            }

            texture.name = assetName + " Atlas";
            mat.name = texture.name + " Material";

            fontAsset.atlasTextures[0] = texture;
            AssetDatabase.AddObjectToAsset(texture, fontAsset);

            fontAsset.freeGlyphRects = new List<GlyphRect>() { new GlyphRect(0, 0, atlasWidth - packingModifier, atlasHeight - packingModifier) };
            fontAsset.usedGlyphRects = new List<GlyphRect>();

            mat.SetTexture(ShaderUtilities.ID_MainTex, texture);
            mat.SetFloat(ShaderUtilities.ID_TextureWidth, atlasWidth);
            mat.SetFloat(ShaderUtilities.ID_TextureHeight, atlasHeight);

            fontAsset.material = mat;
            AssetDatabase.AddObjectToAsset(mat, fontAsset);

            // Add Font Asset Creation Settings
            fontAsset.creationSettings = new FontAssetCreationSettings(fontAsset.m_SourceFontFileGUID, (int)fontAsset.faceInfo.pointSize, 0, atlasPadding, 0, 1024, 1024, 7, string.Empty, (int)renderMode);

            // Not sure if this is still necessary in newer versions of Unity.
            //EditorUtility.SetDirty(fontAsset);

            AssetDatabase.SaveAssets();
        }
    }
}
