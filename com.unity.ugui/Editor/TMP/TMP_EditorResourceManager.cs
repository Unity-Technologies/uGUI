using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.TextCore.LowLevel;


namespace TMPro
{
    static class EditorEventCallbacks
    {
        [InitializeOnLoadMethod]
        internal static void InitializeFontAssetResourceChangeCallBacks()
        {
            TMP_FontAsset.RegisterResourceForUpdate += TMP_EditorResourceManager.RegisterResourceForUpdate;
            TMP_FontAsset.RegisterResourceForReimport += TMP_EditorResourceManager.RegisterResourceForReimport;
            TMP_FontAsset.OnFontAssetTextureChanged += TMP_EditorResourceManager.AddTextureToAsset;
            TMP_FontAsset.SetAtlasTextureIsReadable +=  FontEngineEditorUtilities.SetAtlasTextureIsReadable;
            TMP_FontAsset.GetSourceFontRef += TMP_EditorResourceManager.GetSourceFontRef;
            TMP_FontAsset.SetSourceFontGUID += TMP_EditorResourceManager.SetSourceFontGUID;

            // Callback to handle clearing dynamic font asset data when closing the Editor
            EditorApplication.quitting += () =>
            {
                // Find all font assets in the project
                string searchPattern = "t:TMP_FontAsset";
                string[] fontAssetGUIDs = AssetDatabase.FindAssets(searchPattern);

                for (int i = 0; i < fontAssetGUIDs.Length; i++)
                {
                    string fontAssetPath = AssetDatabase.GUIDToAssetPath(fontAssetGUIDs[i]);
                    TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);

                    if (fontAsset != null && (fontAsset.atlasPopulationMode == AtlasPopulationMode.Dynamic || fontAsset.atlasPopulationMode == AtlasPopulationMode.DynamicOS) && fontAsset.clearDynamicDataOnBuild && fontAsset.atlasTexture.width > 1)
                    {
                        Debug.Log("Clearing [" + fontAsset.name + "] dynamic font asset data.");
                        fontAsset.ClearCharacterAndGlyphTablesInternal();
                    }
                }
            };
        }
    }

    internal class TMP_EditorResourceManager
    {
        private static TMP_EditorResourceManager s_Instance;

        private readonly List<Object> m_ObjectUpdateQueue = new List<Object>();
        private HashSet<int> m_ObjectUpdateQueueLookup = new HashSet<int>();

        private readonly List<Object> m_ObjectReImportQueue = new List<Object>();
        private HashSet<int> m_ObjectReImportQueueLookup = new HashSet<int>();

        private readonly List<TMP_FontAsset> m_FontAssetDefinitionRefreshQueue = new List<TMP_FontAsset>();
        private HashSet<int> m_FontAssetDefinitionRefreshQueueLookup = new HashSet<int>();

        /// <summary>
        /// Get a singleton instance of the manager.
        /// </summary>
        internal static TMP_EditorResourceManager instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = new TMP_EditorResourceManager();

                return s_Instance;
            }
        }

        /// <summary>
        /// Register to receive rendering callbacks.
        /// </summary>
        private TMP_EditorResourceManager()
        {
            // Register to the appropriate callback for the given render pipeline.
            if (RenderPipelineManager.currentPipeline == null)
                Camera.onPostRender += OnCameraPostRender;
            else
            {
                #if UNITY_2023_3_OR_NEWER
                    RenderPipelineManager.endContextRendering += OnEndOfFrame;
                #else
                    RenderPipelineManager.endFrameRendering += OnEndOfFrame;
                #endif
            }

            Canvas.willRenderCanvases += OnPreRenderCanvases;
        }

        void OnCameraPostRender(Camera cam)
        {
            // Exclude the PreRenderCamera
            if (cam.cameraType != CameraType.SceneView)
                return;

            DoPostRenderUpdates();
        }

        void OnPreRenderCanvases()
        {
            DoPreRenderUpdates();
        }

        #if UNITY_2023_3_OR_NEWER
        void OnEndOfFrame(ScriptableRenderContext renderContext, List<Camera> cameras)
        {
            DoPostRenderUpdates();
        }
        #else
        void OnEndOfFrame(ScriptableRenderContext renderContext, Camera[] cameras)
        {
            DoPostRenderUpdates();
        }
        #endif

        /// <summary>
        /// Register resource for re-import.
        /// </summary>
        /// <param name="obj"></param>
        internal static void RegisterResourceForReimport(Object obj)
        {
            // Return if referenced object is not a persistent asset
            if (!EditorUtility.IsPersistent(obj))
                return;

            instance.InternalRegisterResourceForReimport(obj);
        }

        private void InternalRegisterResourceForReimport(Object obj)
        {
            int id = obj.GetInstanceID();

            if (m_ObjectReImportQueueLookup.Contains(id))
                return;

            m_ObjectReImportQueueLookup.Add(id);
            m_ObjectReImportQueue.Add(obj);
        }

        /// <summary>
        /// Register resource to be updated.
        /// </summary>
        /// <param name="obj"></param>
        internal static void RegisterResourceForUpdate(Object obj)
        {
            // Return if referenced object is not a persistent asset
            if (!EditorUtility.IsPersistent(obj))
                return;

            instance.InternalRegisterResourceForUpdate(obj);
        }

        private void InternalRegisterResourceForUpdate(Object obj)
        {
            int id = obj.GetInstanceID();

            if (m_ObjectUpdateQueueLookup.Contains(id))
                return;

            m_ObjectUpdateQueueLookup.Add(id);
            m_ObjectUpdateQueue.Add(obj);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fontAsset"></param>
        internal static void RegisterFontAssetForDefinitionRefresh(TMP_FontAsset fontAsset)
        {
            instance.InternalRegisterFontAssetForDefinitionRefresh(fontAsset);
        }

        private void InternalRegisterFontAssetForDefinitionRefresh(TMP_FontAsset fontAsset)
        {
            int id = fontAsset.GetInstanceID();

            if (m_FontAssetDefinitionRefreshQueueLookup.Contains(id))
                return;

            m_FontAssetDefinitionRefreshQueueLookup.Add(id);
            m_FontAssetDefinitionRefreshQueue.Add(fontAsset);
        }

        /// <summary>
        /// Add texture as sub asset to the referenced object.
        /// </summary>
        /// <param name="tex">The texture to be added as sub object.</param>
        /// <param name="obj">The object to which this texture sub object will be added.</param>
        internal static void AddTextureToAsset(Texture tex, Object obj)
        {
            // Return if referenced object is not a persistent asset
            if (!EditorUtility.IsPersistent(obj))
                return;

            if (tex != null)
                AssetDatabase.AddObjectToAsset(tex, obj);

            RegisterResourceForReimport(obj);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        internal static Font GetSourceFontRef(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<Font>(path);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="font"></param>
        /// <returns></returns>
        internal static string SetSourceFontGUID(Font font)
        {
            return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(font));
        }

        void DoPostRenderUpdates()
        {
            // Handle objects that need updating
            int objUpdateCount = m_ObjectUpdateQueue.Count;

            for (int i = 0; i < objUpdateCount; i++)
            {
                EditorUtilities.TMP_PropertyDrawerUtilities.s_RefreshGlyphProxyLookup = true;
                #if TEXTCORE_FONT_ENGINE_1_5_OR_NEWER
                UnityEditor.TextCore.Text.TextCorePropertyDrawerUtilities.s_RefreshGlyphProxyLookup = true;
                #endif

                Object obj = m_ObjectUpdateQueue[i];
                if (obj != null)
                {
                    //EditorUtility.SetDirty(obj);
                }
            }

            if (objUpdateCount > 0)
            {
                //Debug.Log("Saving assets");
                //AssetDatabase.SaveAssets();

                m_ObjectUpdateQueue.Clear();
                m_ObjectUpdateQueueLookup.Clear();
            }

            // Handle objects that need re-importing
            int objReImportCount = m_ObjectReImportQueue.Count;

            for (int i = 0; i < objReImportCount; i++)
            {
                Object obj = m_ObjectReImportQueue[i];
                if (obj != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(obj);

                    // Exclude Assets not located in the project
                    if (assetPath.StartsWith("Assets/", System.StringComparison.OrdinalIgnoreCase))
                        AssetDatabase.ImportAsset(assetPath);
                }
            }

            if (objReImportCount > 0)
            {
                m_ObjectReImportQueue.Clear();
                m_ObjectReImportQueueLookup.Clear();
            }
        }

        void DoPreRenderUpdates()
        {
            // Handle Font Asset Definition Refresh
            for (int i = 0; i < m_FontAssetDefinitionRefreshQueue.Count; i++)
            {
                TMP_FontAsset fontAsset = m_FontAssetDefinitionRefreshQueue[i];

                if (fontAsset != null)
                {
                    fontAsset.ReadFontAssetDefinition();
                    TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, fontAsset);
                }
            }

            if (m_FontAssetDefinitionRefreshQueue.Count > 0)
            {
                m_FontAssetDefinitionRefreshQueue.Clear();
                m_FontAssetDefinitionRefreshQueueLookup.Clear();
            }
        }
    }
}
