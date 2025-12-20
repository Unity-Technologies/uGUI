using System;
using TMPro;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UnityEditor.UI.Analytics
{
    internal class UGUIBuildAnalytics : IPreprocessBuildWithReport, IProcessSceneWithReport, IPostprocessBuildWithReport
    {
        static UGUIBuildEvent buildEvent;

        public int callbackOrder { get; }

        public void OnPreprocessBuild (BuildReport report)
        {
            buildEvent = new UGUIBuildEvent();
        }

        public void OnPostprocessBuild (BuildReport report)
        {
            buildEvent.buildGuid = report.summary.guid.ToString();
            buildEvent.buildType = report.summary.buildType;
            UGUIAnalytics.Send(buildEvent);
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            //Not currently used in AssetBundle builds
            if (buildEvent == null) { return;  }
            var gameObjects = scene.GetRootGameObjects();
            foreach (var gameObject in gameObjects)
            {
                var allCanvas = gameObject.GetComponentsInChildren<Canvas>();
                foreach (var canvas in allCanvas)
                {
                    if (canvas.renderMode == RenderMode.WorldSpace)
                        buildEvent.counter.WorldSpaceCanvas++;
                    else if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                        buildEvent.counter.OverlayCanvas++;
                    else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                        buildEvent.counter.ScreenSpaceCanvas++;
                }

                var allUI = gameObject.GetComponentsInChildren<UIBehaviour>();
                foreach (var ui in allUI)
                {
                    if (ui is TMP_Text or TMP_Dropdown or TMP_InputField or TMP_SubMeshUI or TMP_SelectionCaret)
                        buildEvent.counter.TMPElements++;
                }
            }
        }
    }

}
