﻿using UnityEngine;
using Unity.Profiling;
using UnityEngine.UI;
using System.Collections.Generic;


namespace TMPro
{

    public class TMP_UpdateManager
    {
        private static TMP_UpdateManager s_Instance;

        private readonly HashSet<int> m_LayoutQueueLookup = new HashSet<int>();
        private readonly List<TMP_Text> m_LayoutRebuildQueue = new List<TMP_Text>();

        private readonly HashSet<int> m_GraphicQueueLookup = new HashSet<int>();
        private readonly List<TMP_Text> m_GraphicRebuildQueue = new List<TMP_Text>();

        private readonly HashSet<int> m_InternalUpdateLookup = new HashSet<int>();
        private readonly List<TMP_Text> m_InternalUpdateQueue = new List<TMP_Text>();

        private readonly HashSet<int> m_CullingUpdateLookup = new HashSet<int>();
        private readonly List<TMP_Text> m_CullingUpdateQueue = new List<TMP_Text>();

        // Profiler Marker declarations
        private static ProfilerMarker k_RegisterTextObjectForUpdateMarker = new ProfilerMarker("TMP.RegisterTextObjectForUpdate");
        private static ProfilerMarker k_RegisterTextElementForGraphicRebuildMarker = new ProfilerMarker("TMP.RegisterTextElementForGraphicRebuild");
        private static ProfilerMarker k_RegisterTextElementForCullingUpdateMarker = new ProfilerMarker("TMP.RegisterTextElementForCullingUpdate");
        private static ProfilerMarker k_UnregisterTextObjectForUpdateMarker = new ProfilerMarker("TMP.UnregisterTextObjectForUpdate");
        private static ProfilerMarker k_UnregisterTextElementForGraphicRebuildMarker = new ProfilerMarker("TMP.UnregisterTextElementForGraphicRebuild");

        /// <summary>
        /// Get a singleton instance of the registry
        /// </summary>
        static TMP_UpdateManager instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = new TMP_UpdateManager();

                return s_Instance;
            }
        }

        /// <summary>
        /// Register to receive rendering callbacks.
        /// </summary>
        TMP_UpdateManager()
        {
            Canvas.willRenderCanvases += DoRebuilds;
        }

        /// <summary>
        /// Function used as a replacement for LateUpdate() to handle SDF Scale updates and Legacy Animation updates.
        /// </summary>
        /// <param name="textObject"></param>
        internal static void RegisterTextObjectForUpdate(TMP_Text textObject)
        {
            k_RegisterTextObjectForUpdateMarker.Begin();

            instance.InternalRegisterTextObjectForUpdate(textObject);

            k_RegisterTextObjectForUpdateMarker.End();
        }

        private void InternalRegisterTextObjectForUpdate(TMP_Text textObject)
        {
            int id = textObject.GetInstanceID();

            if (m_InternalUpdateLookup.Contains(id))
                return;

            m_InternalUpdateLookup.Add(id);
            m_InternalUpdateQueue.Add(textObject);
        }

        /// <summary>
        /// Function to register elements which require a layout rebuild.
        /// </summary>
        /// <param name="element"></param>
        public static void RegisterTextElementForLayoutRebuild(TMP_Text element)
        {
            instance.InternalRegisterTextElementForLayoutRebuild(element);
        }

        private void InternalRegisterTextElementForLayoutRebuild(TMP_Text element)
        {
            int id = element.GetInstanceID();

            if (m_LayoutQueueLookup.Contains(id))
                return;

            m_LayoutQueueLookup.Add(id);
            m_LayoutRebuildQueue.Add(element);
        }

        /// <summary>
        /// Function to register elements which require a layout rebuild.
        /// </summary>
        /// <param name="element"></param>
        public static void RegisterTextElementForGraphicRebuild(TMP_Text element)
        {
            k_RegisterTextElementForGraphicRebuildMarker.Begin();

            instance.InternalRegisterTextElementForGraphicRebuild(element);

            k_RegisterTextElementForGraphicRebuildMarker.End();
        }

        private void InternalRegisterTextElementForGraphicRebuild(TMP_Text element)
        {
            int id = element.GetInstanceID();

            if (m_GraphicQueueLookup.Contains(id))
                return;

            m_GraphicQueueLookup.Add(id);
            m_GraphicRebuildQueue.Add(element);
        }

        public static void RegisterTextElementForCullingUpdate(TMP_Text element)
        {
            k_RegisterTextElementForCullingUpdateMarker.Begin();

            instance.InternalRegisterTextElementForCullingUpdate(element);

            k_RegisterTextElementForCullingUpdateMarker.End();
        }

        private void InternalRegisterTextElementForCullingUpdate(TMP_Text element)
        {
            int id = element.GetInstanceID();

            if (m_CullingUpdateLookup.Contains(id))
                return;

            m_CullingUpdateLookup.Add(id);
            m_CullingUpdateQueue.Add(element);
        }

        /// <summary>
        /// Callback which occurs just before the cam is rendered.
        /// </summary>
        void OnCameraPreCull()
        {
            DoRebuilds();
        }

        /// <summary>
        /// Process the rebuild requests in the rebuild queues.
        /// </summary>
        void DoRebuilds()
        {
            // Handle text objects the require an update either as a result of scale changes or legacy animation.
            for (int i = 0; i < m_InternalUpdateQueue.Count; i++)
            {
                m_InternalUpdateQueue[i].InternalUpdate();
            }

            // Handle Layout Rebuild Phase
            for (int i = 0; i < m_LayoutRebuildQueue.Count; i++)
            {
                m_LayoutRebuildQueue[i].Rebuild(CanvasUpdate.Prelayout);
            }

            if (m_LayoutRebuildQueue.Count > 0)
            {
                m_LayoutRebuildQueue.Clear();
                m_LayoutQueueLookup.Clear();
            }

            // Handle Graphic Rebuild Phase
            for (int i = 0; i < m_GraphicRebuildQueue.Count; i++)
            {
                m_GraphicRebuildQueue[i].Rebuild(CanvasUpdate.PreRender);
            }

            // If there are no objects in the queue, we don't need to clear the lists again.
            if (m_GraphicRebuildQueue.Count > 0)
            {
                m_GraphicRebuildQueue.Clear();
                m_GraphicQueueLookup.Clear();
            }

            // Handle Culling Update
            for (int i = 0; i < m_CullingUpdateQueue.Count; i++)
            {
                m_CullingUpdateQueue[i].UpdateCulling();
            }

            // If there are no objects in the queue, we don't need to clear the lists again.
            if (m_CullingUpdateQueue.Count > 0)
            {
                m_CullingUpdateQueue.Clear();
                m_CullingUpdateLookup.Clear();
            }
        }

        internal static void UnRegisterTextObjectForUpdate(TMP_Text textObject)
        {
            k_UnregisterTextObjectForUpdateMarker.Begin();

            instance.InternalUnRegisterTextObjectForUpdate(textObject);

            k_UnregisterTextObjectForUpdateMarker.End();
        }

        /// <summary>
        /// Function to unregister elements which no longer require a rebuild.
        /// </summary>
        /// <param name="element"></param>
        public static void UnRegisterTextElementForRebuild(TMP_Text element)
        {
            instance.InternalUnRegisterTextElementForGraphicRebuild(element);
            instance.InternalUnRegisterTextElementForLayoutRebuild(element);
            instance.InternalUnRegisterTextObjectForUpdate(element);
        }

        private void InternalUnRegisterTextElementForGraphicRebuild(TMP_Text element)
        {
            k_UnregisterTextElementForGraphicRebuildMarker.Begin();

            int id = element.GetInstanceID();

            m_GraphicRebuildQueue.Remove(element);
            m_GraphicQueueLookup.Remove(id);

            k_UnregisterTextElementForGraphicRebuildMarker.End();
        }

        private void InternalUnRegisterTextElementForLayoutRebuild(TMP_Text element)
        {
            int id = element.GetInstanceID();

            m_LayoutRebuildQueue.Remove(element);
            m_LayoutQueueLookup.Remove(id);
        }

        private void InternalUnRegisterTextObjectForUpdate(TMP_Text textObject)
        {
            int id = textObject.GetInstanceID();

            m_InternalUpdateQueue.Remove(textObject);
            m_InternalUpdateLookup.Remove(id);
        }
    }
}
