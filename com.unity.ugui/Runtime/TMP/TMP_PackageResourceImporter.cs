#if UNITY_EDITOR

using System;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace TMPro
{
    [Serializable]
    public class TMP_PackageResourceImporter
    {
        bool m_EssentialResourcesImported;
        bool m_ExamplesAndExtrasResourcesImported;
        bool m_EssentialResourcesNeedUpdate;
        bool m_ExamplesAndExtrasNeedUpdate;
        bool m_LogErrors;
        internal bool m_IsImportingExamples;

        public TMP_PackageResourceImporter(bool logErrors = true)
        {
            m_LogErrors = logErrors;
            m_EssentialResourcesNeedUpdate = m_ExamplesAndExtrasNeedUpdate = !TMP_Settings.isTMPSettingsNull && TMP_Settings.instance.assetVersion != TMP_Settings.s_CurrentAssetVersion;
        }

        public void OnDestroy()
        {
            if (m_LogErrors && (TMP_Settings.isTMPSettingsNull || TMP_Settings.instance?.assetVersion != TMP_Settings.s_CurrentAssetVersion))
                Debug.LogError("TextMesh Pro Essential Resources are missing, which are crucial for proper functionality. To import them, go to 'Window > Text Mesh Pro > Import TMP Essential Resources' in the menu.");
        }

        public void OnGUI()
        {
            // Check if the resources state has changed.
            m_EssentialResourcesImported = File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset");
            m_ExamplesAndExtrasResourcesImported = Directory.Exists("Assets/TextMesh Pro/Examples & Extras");

            GUILayout.BeginVertical();
            {
                // Display options to import Essential resources
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    GUILayout.Label("TMP Essentials", EditorStyles.boldLabel);
                    if (m_EssentialResourcesImported && m_EssentialResourcesNeedUpdate)
                        GUILayout.Label("It appears that the essential resources for TextMesh Pro have been updated. To ensure proper functionality, you need to reimport these resources into your project. The updated resources will be placed at the root of your project in the \"TextMesh Pro\" folder.", new GUIStyle(EditorStyles.label) { wordWrap = true } );
                    else
                    {
                        GUILayout.Label("This appears to be the first time you access TextMesh Pro, as such we need to add resources to your project that are essential for using TextMesh Pro. These new resources will be placed at the root of your project in the \"TextMesh Pro\" folder.", new GUIStyle(EditorStyles.label) { wordWrap = true } );
                    }
                    GUILayout.Space(5f);

                    GUI.enabled = !m_EssentialResourcesImported || m_EssentialResourcesNeedUpdate;
                    if (GUILayout.Button("Import TMP Essentials"))
                    {
                        m_EssentialResourcesNeedUpdate = false;
                        if (m_EssentialResourcesImported)
                            PreparePackageImport();

                        AssetDatabase.importPackageCompleted += ImportCallback;

                        string packageFullPath = GetPackageFullPath();
                        AssetDatabase.ImportPackage(packageFullPath + "/Package Resources/TMP Essential Resources.unitypackage", false);
                    }

                    GUILayout.Space(5f);
                    GUI.enabled = true;
                }
                GUILayout.EndVertical();

                // Display options to import Examples & Extras
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    GUILayout.Label("TMP Examples & Extras", EditorStyles.boldLabel);
                    if (m_ExamplesAndExtrasResourcesImported && m_ExamplesAndExtrasNeedUpdate)
                        GUILayout.Label("It appears that the Examples & Extras package for TextMesh Pro has been updated. To ensure proper functionality, you need to reimport these updated resources into your project. The updated resources will be placed in the same folder as the TMP essential resources.", new GUIStyle(EditorStyles.label) { wordWrap = true });
                    else
                        GUILayout.Label("The Examples & Extras package contains addition resources and examples that will make discovering and learning about TextMesh Pro's powerful features easier. These additional resources will be placed in the same folder as the TMP essential resources.", new GUIStyle(EditorStyles.label) { wordWrap = true });
                    GUILayout.Space(5f);

                    GUI.enabled = (m_EssentialResourcesImported && !m_ExamplesAndExtrasResourcesImported) || m_ExamplesAndExtrasNeedUpdate;
                    if (GUILayout.Button("Import TMP Examples & Extras"))
                    {
                        // Set flag to get around importing scripts as per of this package which results in an assembly reload which in turn prevents / clears any callbacks.
                        m_IsImportingExamples = true;
                        m_ExamplesAndExtrasNeedUpdate = false;

                        // Disable AssetDatabase refresh until examples have been imported.
                        //AssetDatabase.DisallowAutoRefresh();

                        string packageFullPath = GetPackageFullPath();
                        AssetDatabase.ImportPackage(packageFullPath + "/Package Resources/TMP Examples & Extras.unitypackage", false);
                    }
                    GUILayout.Space(5f);
                    GUI.enabled = true;
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
            GUILayout.Space(5f);
        }

        internal void RegisterResourceImportCallback()
        {
            AssetDatabase.importPackageCompleted += ImportCallback;
        }


        private static string k_SettingsFilePath;
        private static byte[] k_SettingsBackup;

        internal void PreparePackageImport()
        {
            // Check if the TMP Settings asset is already present in the project.
            string[] settings = AssetDatabase.FindAssets("t:TMP_Settings");

            if (settings.Length > 0)
            {
                // Save assets just in case the TMP Setting were modified before import.
                AssetDatabase.SaveAssets();

                // Copy existing TMP Settings asset to a byte[]
                k_SettingsFilePath = AssetDatabase.GUIDToAssetPath(settings[0]);
                k_SettingsBackup = File.ReadAllBytes(k_SettingsFilePath);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="packageName"></param>
        void ImportCallback(string packageName)
        {
            if (packageName == "TMP Essential Resources")
            {
                if (m_EssentialResourcesImported)
                {
                    // Restore backup of TMP Settings from byte[]
                    File.WriteAllBytes(k_SettingsFilePath, k_SettingsBackup);
                    AssetDatabase.Refresh();

                    TMP_Settings.instance.SetAssetVersion();
                    EditorUtility.SetDirty(TMP_Settings.instance);
                    AssetDatabase.SaveAssetIfDirty(TMP_Settings.instance);
                }
                m_EssentialResourcesImported = true;
                TMPro_EventManager.ON_RESOURCES_LOADED();

                #if UNITY_2018_3_OR_NEWER
                SettingsService.NotifySettingsProviderChanged();
                #endif
            }
            else if (packageName == "TMP Examples & Extras")
            {
                m_ExamplesAndExtrasResourcesImported = true;
                m_IsImportingExamples = false;
                //AssetDatabase.AllowAutoRefresh();
            }

            Debug.Log("[" + packageName + "] have been imported.");

            AssetDatabase.importPackageCompleted -= ImportCallback;
        }

        static string GetPackageFullPath()
        {
            // Check for potential UPM package
            string packagePath = Path.GetFullPath("Packages/com.unity.ugui");
            if (Directory.Exists(packagePath))
            {
                return packagePath;
            }

            packagePath = Path.GetFullPath("Assets/..");
            if (Directory.Exists(packagePath))
            {
                // Search default location for development package
                if (Directory.Exists(packagePath + "/Assets/Packages/com.unity.ugui/Editor Resources"))
                {
                    return packagePath + "/Assets/Packages/com.unity.ugui";
                }

                // Search for default location of normal TextMesh Pro AssetStore package
                if (Directory.Exists(packagePath + "/Assets/TextMesh Pro/Editor Resources"))
                {
                    return packagePath + "/Assets/TextMesh Pro";
                }

                // Search for potential alternative locations in the user project
                string[] matchingPaths = Directory.GetDirectories(packagePath, "TextMesh Pro", SearchOption.AllDirectories);
                string path = ValidateLocation(matchingPaths, packagePath);
                if (path != null) return packagePath + path;
            }

            return null;
        }

        static string ValidateLocation(string[] paths, string projectPath)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                // Check if the Editor Resources folder exists.
                if (Directory.Exists(paths[i] + "/Editor Resources"))
                {
                    string folderPath = paths[i].Replace(projectPath, "");
                    folderPath = folderPath.TrimStart('\\', '/');
                    return folderPath;
                }
            }

            return null;
        }

        /// <summary>
        /// Imports the specified TMP resources.
        /// </summary>
        /// <param name="importEssentials">Should import the TMP Essential Resources.</param>
        /// <param name="importExamples">Should import the TMP Examples & Extras.</param>
        /// <param name="interactive">If interactive is true, an import package dialog will be opened, else all assets in the package will be imported into the current project silently.</param>
        public static void ImportResources(bool importEssentials, bool importExamples, bool interactive)
        {
            string packageFullPath = GetPackageFullPath();

            if (importEssentials)
                AssetDatabase.ImportPackage(packageFullPath + "/Package Resources/TMP Essential Resources.unitypackage", interactive);

            if (importExamples)
                AssetDatabase.ImportPackage(packageFullPath + "/Package Resources/TMP Examples & Extras.unitypackage", interactive);
        }
    }

    public class TMP_PackageResourceImporterWindow : EditorWindow
    {
        [SerializeField]
        TMP_PackageResourceImporter m_ResourceImporter;

        static TMP_PackageResourceImporterWindow m_ImporterWindow;

        public static void ShowPackageImporterWindow()
        {
            if (m_ImporterWindow == null)
            {
                m_ImporterWindow = GetWindow<TMP_PackageResourceImporterWindow>();
                m_ImporterWindow.titleContent = new GUIContent("TMP Importer");
                m_ImporterWindow.Focus();
            }
        }

        void OnEnable()
        {
            // Set Editor Window Size
            SetEditorWindowSize();

            if (m_ResourceImporter == null)
                m_ResourceImporter = new TMP_PackageResourceImporter();

            if (m_ResourceImporter.m_IsImportingExamples)
                m_ResourceImporter.RegisterResourceImportCallback();
        }

        void OnDestroy()
        {
            m_ResourceImporter.OnDestroy();
        }

        void OnGUI()
        {
            m_ResourceImporter.OnGUI();
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        /// <summary>
        /// Limits the minimum size of the editor window.
        /// </summary>
        void SetEditorWindowSize()
        {
            EditorWindow editorWindow = this;

            Vector2 windowSize = new Vector2(640, 210);
            editorWindow.minSize = windowSize;
            editorWindow.maxSize = windowSize;
        }
    }


    [Serializable]
    internal class TMP_ShaderPackageImporter
    {
        bool m_ShadersImported;

        const string s_TMPShaderPackageGUID = "e02b76aaf840d38469530d159da03749";

        public void OnDestroy() { }

        public void OnGUI()
        {
            GUILayout.BeginVertical();
            {
                // Display options to import Essential resources
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    //GUILayout.Label("TextMeshPro Resources Update", EditorStyles.boldLabel);
                    GUILayout.Label("This release of the TMP package includes updated resources.\n\nPlease use the menu options located in \"Window\\TextMeshPro\\...\" to import the updated\n\"TMP Essential Resources\" and \"TMP Examples & Extras\".\n\nAs usual please be sure to backup any of the files and resources that you may have added or modified in the \"Assets\\TextMesh Pro\\...\" folders.", new GUIStyle(EditorStyles.label) { wordWrap = true } );
                    GUILayout.Space(5f);

                    GUI.enabled = !m_ShadersImported;
                    // if (GUILayout.Button("Update TMP Shaders"))
                    // {
                    //     string packagePath = AssetDatabase.GUIDToAssetPath(s_TMPShaderPackageGUID);
                    //
                    //     if (string.IsNullOrEmpty(packagePath))
                    //         return;
                    //
                    //     AssetDatabase.importPackageCompleted += ImportCallback;
                    //     AssetDatabase.ImportPackage(packagePath, true);
                    // }
                    GUILayout.Space(5f);
                    GUI.enabled = true;
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
            GUILayout.Space(5f);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="packageName"></param>
        void ImportCallback(string packageName)
        {
            if (packageName == "TMP Shaders")
            {
                m_ShadersImported = true;

                EditorWindow window = TMP_ShaderPackageImporterWindow.importerWindow;

                if (window != null)
                    window.Close();
            }

            Debug.Log("[" + packageName + "] have been imported.");

            AssetDatabase.importPackageCompleted -= ImportCallback;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="interactive"></param>
        internal static void ImportShaders(bool interactive)
        {
            string packagePath = AssetDatabase.GUIDToAssetPath(s_TMPShaderPackageGUID);

            if (string.IsNullOrEmpty(packagePath))
                return;

            AssetDatabase.ImportPackage(packagePath, interactive);
        }
    }


    internal class TMP_ShaderPackageImporterWindow : EditorWindow
    {
        [SerializeField]
        TMP_ShaderPackageImporter m_ResourceImporter;

        internal static TMP_ShaderPackageImporterWindow importerWindow;

        public static void ShowPackageImporterWindow()
        {
            if (importerWindow == null)
            {
                importerWindow = GetWindow<TMP_ShaderPackageImporterWindow>(true, "TextMeshPro Resources Update", true);
            }
        }

        void OnEnable()
        {
            // Set Editor Window Size
            SetEditorWindowSize();

            if (m_ResourceImporter == null)
                m_ResourceImporter = new TMP_ShaderPackageImporter();
        }

        void OnDestroy()
        {
            m_ResourceImporter.OnDestroy();
        }

        void OnGUI()
        {
            Rect p = position;

            if (p.x < 10)
            {
                p.x = 10;
                position = p;
            }

            if (p.y < 190)
            {
                p.y = 190;
                position = p;
            }

            m_ResourceImporter.OnGUI();
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        /// <summary>
        /// Limits the minimum size of the editor window.
        /// </summary>
        void SetEditorWindowSize()
        {
            EditorWindow editorWindow = this;

            Vector2 windowSize = new Vector2(640, 117);
            editorWindow.minSize = windowSize;
            editorWindow.maxSize = windowSize;
        }
    }

}

#endif
