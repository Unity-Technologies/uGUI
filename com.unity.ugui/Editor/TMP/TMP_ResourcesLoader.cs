using System.Collections;
using UnityEditor;
using UnityEngine;


namespace TMPro.EditorUtilities
{
    /*
    [InitializeOnLoad]
    class EssentialResourcesManager
    {
        private const string s_TMPShaderIncludeGUID = "407bc68d299748449bbf7f48ee690f8d";
        const string k_EssentialResourcesShaderVersionCheckKey = "TMP.EssentialResources.ShaderVersionCheck";

        static EssentialResourcesManager()
        {
            bool shaderSearched = SessionState.GetBool(k_EssentialResourcesShaderVersionCheckKey, false);

            if (!EditorApplication.isPlayingOrWillChangePlaymode && !shaderSearched)
                CheckShaderVersions();
        }

        static void CheckShaderVersions()
        {
            // Get path to TMP shader include file.
            string assetPath = AssetDatabase.GUIDToAssetPath(s_TMPShaderIncludeGUID);

            if (string.IsNullOrEmpty(assetPath))
                return;

            AssetImporter importer = AssetImporter.GetAtPath(assetPath);

            if (importer != null && string.IsNullOrEmpty(importer.userData))
            {
                // Show Shader Import Window
                TMP_EditorCoroutine.StartCoroutine(ShowShaderPackageImporterWindow());
            }

            SessionState.SetBool(k_EssentialResourcesShaderVersionCheckKey, true);
        }

        static IEnumerator ShowShaderPackageImporterWindow()
        {
            yield return new WaitForSeconds(5.0f);

            TMP_ShaderPackageImporterWindow.ShowPackageImporterWindow();
        }
    }
    */

    /*
    //[InitializeOnLoad]
    class TMP_ResourcesLoader
    {

        /// <summary>
        /// Function to pre-load the TMP Resources
        /// </summary>
        public static void LoadTextMeshProResources()
        {
            //TMP_Settings.LoadDefaultSettings();
            //TMP_StyleSheet.LoadDefaultStyleSheet();
        }


        static TMP_ResourcesLoader()
        {
            //Debug.Log("Loading TMP Resources...");

            // Get current targetted platform


            //string Settings = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            //TMPro.TMP_Settings.LoadDefaultSettings();
            //TMPro.TMP_StyleSheet.LoadDefaultStyleSheet();
        }


        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        //static void OnBeforeSceneLoaded()
        //{
            //Debug.Log("Before scene is loaded.");

            //    //TMPro.TMP_Settings.LoadDefaultSettings();
            //    //TMPro.TMP_StyleSheet.LoadDefaultStyleSheet();

            //    //ShaderVariantCollection collection = new ShaderVariantCollection();
            //    //Shader s0 = Shader.Find("TextMeshPro/Mobile/Distance Field");
            //    //ShaderVariantCollection.ShaderVariant tmp_Variant = new ShaderVariantCollection.ShaderVariant(s0, UnityEngine.Rendering.PassType.Normal, string.Empty);

            //    //collection.Add(tmp_Variant);
            //    //collection.WarmUp();
        //}

    }

    //static class TMP_ProjectSettings
    //{
    //    [InitializeOnLoadMethod]
    //    static void SetProjectDefineSymbols()
    //    {
    //        string currentBuildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

    //        //Check for and inject TMP_INSTALLED
    //        if (!currentBuildSettings.Contains("TMP_PRESENT"))
    //        {
    //            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currentBuildSettings + ";TMP_PRESENT");
    //        }
    //    }
    //}
    */
}
