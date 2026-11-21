using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Build;
using UnityEditor.Build.Content;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UnityEditor.UI.Analytics
{
    internal class UGUIBuildAnalytics : AssetPostprocessor, IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        static UGUIBuildEvent buildEvent;

        static readonly Type[] k_TMPComponentTypes =
        {
            typeof(TMP_Text),
            typeof(TMP_Dropdown),
            typeof(TMP_InputField),
            typeof(TMP_SubMeshUI),
            typeof(TMP_SelectionCaret),
        };

        public int callbackOrder { get; }

        public void OnPreprocessBuild (BuildReport report)
        {
            buildEvent = new UGUIBuildEvent();
        }

        public void OnPostprocessBuild (BuildReport report)
        {
            buildEvent.buildGuid = report.summary.guid.ToString();
            buildEvent.buildType = report.summary.buildType;

            // OnProcessScene never sees prefabs that ship as assets, such as those in Resources.
            if (EditorAnalytics.enabled && HasPrefabCanvas(GetPackedCanvasSourceGuids(report)))
                MarkPrefabCanvasPresent(ref buildEvent.counter);

            UGUIAnalytics.Send(buildEvent);
        }

        // Canvas is a native type, so the report lists it without any prefab being loaded.
        static IEnumerable<GUID> GetPackedCanvasSourceGuids(BuildReport report)
        {
            foreach (var packedAssets in report.packedAssets)
            {
                foreach (var info in packedAssets.contents)
                {
                    if (info.type == typeof(Canvas))
                        yield return info.sourceAssetGUID;
                }
            }
        }

        // Scene canvases are attributed to the scene asset, so they fail the GameObject check and are left to OnProcessScene.
        internal static bool HasPrefabCanvas(IEnumerable<GUID> canvasSourceGuids)
        {
            foreach (var guid in canvasSourceGuids)
            {
                if (AssetDatabase.GetMainAssetTypeFromGUID(guid) == typeof(GameObject))
                    return true;
            }
            return false;
        }

        // Prefab contents are unknown without loading them, so every field only records that the UI may be present.
        internal static void MarkPrefabCanvasPresent(ref UGUIBuildEvent.Counter counter)
        {
            counter.WorldSpaceCanvas = Math.Max(counter.WorldSpaceCanvas, 1);
            counter.ScreenSpaceCanvas = Math.Max(counter.ScreenSpaceCanvas, 1);
            counter.OverlayCanvas = Math.Max(counter.OverlayCanvas, 1);
            counter.TMPElements = Math.Max(counter.TMPElements, 1);
        }

        // Inactive objects are counted because UI such as menus and popups is often enabled at runtime.
        internal static void AddCounts(GameObject root, ref UGUIBuildEvent.Counter counter)
        {
            foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
            {
                if (canvas.renderMode == RenderMode.WorldSpace)
                    counter.WorldSpaceCanvas++;
                else if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    counter.OverlayCanvas++;
                else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                    counter.ScreenSpaceCanvas++;
            }

            foreach (var ui in root.GetComponentsInChildren<UIBehaviour>(true))
            {
                if (IsTMPComponent(ui.GetType()))
                    counter.TMPElements++;
            }
        }

        internal static bool IsTMPComponent(Type type)
        {
            foreach (var tmpType in k_TMPComponentTypes)
            {
                if (tmpType.IsAssignableFrom(type))
                    return true;
            }
            return false;
        }

        public void OnProcessScene(Scene scene, SceneImportContext sceneContext)
        {
            //Not currently used in AssetBundle builds
            if (buildEvent == null)
                return;

            foreach (var gameObject in scene.GetRootGameObjects())
            {
                AddCounts(gameObject, ref buildEvent.counter);
            }
        }
    }

}
