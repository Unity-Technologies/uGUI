using System;
using UnityEngine;
using UnityEditor;


namespace TMPro.EditorUtilities
{
    /// <summary>
    /// Asset post processor used to handle text assets changes.
    /// This includes tracking of changes to textures used by sprite assets as well as font assets potentially getting updated outside of the Unity editor.
    /// </summary>
    internal class TMPro_TexturePostProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // Only run post processor after the Editor has been fully loaded.
            if (Time.frameCount == 0)
                return;

            bool textureImported = false;

            foreach (var asset in importedAssets)
            {
                // Return if imported asset path is outside of the project.
                if (asset.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) == false)
                    continue;

                Type assetType = AssetDatabase.GetMainAssetTypeAtPath(asset);

                if (assetType == typeof(TMP_FontAsset))
                {
                    TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath(asset, typeof(TMP_FontAsset)) as TMP_FontAsset;

                    // Only refresh font asset definition if font asset was previously initialized.
                    if (fontAsset != null && fontAsset.m_CharacterLookupDictionary != null)
                        TMP_EditorResourceManager.RegisterFontAssetForDefinitionRefresh(fontAsset);

                    continue;
                }

                if (assetType == typeof(Texture2D))
                    textureImported = true;
            }

            // If textures were imported, issue callback to any potential text objects that might require updating.
            if (textureImported)
                TMPro_EventManager.ON_SPRITE_ASSET_PROPERTY_CHANGED(true, null);
        }
    }

    internal class TMP_FontAssetPostProcessor : UnityEditor.AssetModificationProcessor
    {
        static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions opt)
        {
            if (AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(TMP_FontAsset))
                TMP_ResourceManager.RebuildFontAssetCache();

            return AssetDeleteResult.DidNotDelete;
        }
    }
}
