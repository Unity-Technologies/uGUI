using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace TMPro
{
    public class TMP_PreBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild(BuildReport report)
        {
            // Find all font assets in the project
            string searchPattern = "t:TMP_FontAsset";
            string[] fontAssetGUIDs = AssetDatabase.FindAssets(searchPattern);

            for (int i = 0; i < fontAssetGUIDs.Length; i++)
            {
                string fontAssetPath = AssetDatabase.GUIDToAssetPath(fontAssetGUIDs[i]);
                TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);

                if (fontAsset != null && (fontAsset.atlasPopulationMode == AtlasPopulationMode.Dynamic || fontAsset.atlasPopulationMode == AtlasPopulationMode.DynamicOS) && fontAsset.clearDynamicDataOnBuild && fontAsset.atlasTexture.width != 0)
                {
                    fontAsset.ClearCharacterAndGlyphTablesInternal();
                }
            }
        }
    }
}
