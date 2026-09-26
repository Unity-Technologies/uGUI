using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TMPro.EditorUtilities
{
    // Preloaded for the duration of the build so the active TMP Settings ship and self-register at player startup.
    class TMP_SettingsBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        static TMP_Settings s_Settings;
        static bool s_RemoveFromPreloadedAssets;
        // Built-in settings found in the preloaded assets while a project asset is active, put back after the build.
        static readonly List<TMP_Settings> s_DisplacedBuiltInSettings = new List<TMP_Settings>();

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            s_RemoveFromPreloadedAssets = false;
            s_DisplacedBuiltInSettings.Clear();
            s_Settings = TMP_Settings.FindSettingsInEditor(out _, out _, logErrors: true);
            if (s_Settings == null)
                return;

            if (s_Settings.isBuiltInSettings && !TMP_EditorProjectSettings.instance.includeBuiltInSettingsInBuilds)
            {
                s_Settings = null;
                return;
            }

            var preloadedAssets = PlayerSettings.GetPreloadedAssets();
            bool changed = false;

            if (!s_Settings.isBuiltInSettings)
            {
                foreach (var asset in preloadedAssets)
                {
                    if (asset is TMP_Settings settings && settings.isBuiltInSettings)
                        s_DisplacedBuiltInSettings.Add(settings);
                }

                foreach (var settings in s_DisplacedBuiltInSettings)
                    ArrayUtility.Remove(ref preloadedAssets, settings);
                changed = s_DisplacedBuiltInSettings.Count > 0;
            }

            if (Array.IndexOf(preloadedAssets, s_Settings) < 0)
            {
                ArrayUtility.Add(ref preloadedAssets, s_Settings);
                s_RemoveFromPreloadedAssets = true;
                changed = true;
            }

            if (!changed)
                return;

            SetPreloadedAssetsWithoutDirtying(preloadedAssets);
            EditorApplication.delayCall += RemoveFromPreloadedAssets;
        }

        public void OnPostprocessBuild(BuildReport report) => RemoveFromPreloadedAssets();

        static void RemoveFromPreloadedAssets()
        {
            if (s_Settings == null || (!s_RemoveFromPreloadedAssets && s_DisplacedBuiltInSettings.Count == 0))
                return;

            var preloadedAssets = PlayerSettings.GetPreloadedAssets();

            if (s_RemoveFromPreloadedAssets)
                ArrayUtility.Remove(ref preloadedAssets, s_Settings);

            foreach (var settings in s_DisplacedBuiltInSettings)
            {
                if (settings != null && Array.IndexOf(preloadedAssets, settings) < 0)
                    ArrayUtility.Add(ref preloadedAssets, settings);
            }

            s_RemoveFromPreloadedAssets = false;
            s_DisplacedBuiltInSettings.Clear();
            s_Settings = null;

            SetPreloadedAssetsWithoutDirtying(preloadedAssets);
        }

        static void SetPreloadedAssetsWithoutDirtying(UnityEngine.Object[] preloadedAssets)
        {
            var playerSettings = GetPlayerSettings();
            var wasDirty = playerSettings != null && EditorUtility.IsDirty(playerSettings);
            PlayerSettings.SetPreloadedAssets(preloadedAssets);

            if (!wasDirty && playerSettings != null)
                EditorUtility.ClearDirty(playerSettings);
        }

        static PlayerSettings GetPlayerSettings()
        {
            var settings = Resources.FindObjectsOfTypeAll<PlayerSettings>();
            return settings != null && settings.Length > 0 ? settings[0] : null;
        }
    }
}
