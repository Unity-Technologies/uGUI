using UnityEditor;

namespace TMPro.EditorUtilities
{
    internal class TMP_SettingsPostProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var path in importedAssets)
            {
                if (AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(TMP_Settings))
                {
                    TMP_Settings.ResetStaticSettings();
                    return;
                }
            }
        }
    }
}
