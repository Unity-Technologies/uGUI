#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
    public static class GraphicRebuildTracker
    {
        static List<Graphic> m_Tracked = new List<Graphic>();
        static bool s_Initialized;

        public static void TrackGraphic(Graphic g)
        {
            if (!s_Initialized)
            {
                CanvasRenderer.onRequestRebuild += OnRebuildRequested;
                s_Initialized = true;
            }

            for (int i = 0; i < m_Tracked.Count; i++)
            {
                if (m_Tracked[i] == g)
                    return;
            }
            m_Tracked.Add(g);
        }

        public static void UnTrackGraphic(Graphic g)
        {
            m_Tracked.Remove(g);
        }

        static void OnRebuildRequested()
        {
            for (int i = 0; i < m_Tracked.Count; i++)
            {
                m_Tracked[i].OnRebuildRequested();
            }
        }
    }
}
#endif // if UNITY_EDITOR
