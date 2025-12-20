using System;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Serialization;

namespace UnityEditor.UI.Analytics
{
    [AnalyticInfo(eventName: "ugui_onBuild", vendorKey: "unity.ugui")]
    internal class UGUIBuildEvent : IAnalytic
    {
        [Serializable]
        internal struct Payload : IAnalytic.IData
        {
            public string build_guid;
            public int build_type;
            public Counter component_count;
        }

        [Serializable]
        internal struct Counter
        {
            public int WorldSpaceCanvas;
            public int ScreenSpaceCanvas;
            public int OverlayCanvas;
            public int TMPElements;
        }

        public string buildGuid;
        public BuildType buildType;
        public Counter counter;

        public bool TryGatherData(out IAnalytic.IData data, out Exception error)
        {
            error = null;
            data = new Payload
            {
                build_guid = buildGuid,
                build_type = (int)buildType,
                component_count = counter,
            };
            return data != null;
        }
    }
}
