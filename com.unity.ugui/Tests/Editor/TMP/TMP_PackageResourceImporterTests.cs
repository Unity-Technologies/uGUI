using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace TMPro
{
    internal class TMP_PackageResourceImporterTests
    {
        const string k_MissingResourcesError = "TextMesh Pro Essential Resources are missing, which are crucial for proper functionality. To import them, go to 'Window > Text Mesh Pro > Import TMP Essential Resources' in the menu.";
        const string k_TempFolderName = "TMP_PackageResourceImporterTests";
        const string k_TempFolder = "Assets/" + k_TempFolderName;

        FieldInfo m_SettingsInstanceField;
        TMP_Settings m_CachedSettings;

        [SetUp]
        public void SetUp()
        {
            m_SettingsInstanceField = typeof(TMP_Settings).GetField("s_Instance", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(m_SettingsInstanceField, "TMP_Settings.s_Instance was not found.");
            m_CachedSettings = (TMP_Settings)m_SettingsInstanceField.GetValue(null);
            AssetDatabase.DeleteAsset(k_TempFolder);
        }

        [TearDown]
        public void TearDown()
        {
            m_SettingsInstanceField.SetValue(null, m_CachedSettings);
            AssetDatabase.DeleteAsset(k_TempFolder);
        }

        [Test]
        public void OnDestroy_WhenImportedSettingsAreNotLoadedYet_DoesNotLogMissingResourcesError()
        {
            // State right after a first import: a current TMP Settings asset is in a Resources folder, but nothing has loaded it yet.
            // The test creates its own asset because this test project does not import the TMP Essential Resources.
            CreateCurrentSettingsAsset();
            m_SettingsInstanceField.SetValue(null, null);
            Assert.IsTrue(TMP_Settings.isTMPSettingsNull, "Setup failed: the TMP Settings cache should be empty.");
            Assert.IsNotNull(Resources.Load<TMP_Settings>("TMP Settings"), "Setup failed: the TMP Settings asset should be in a Resources folder.");

            new TMP_PackageResourceImporter().OnDestroy();

            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void OnDestroy_WhenSettingsAssetVersionIsOutdated_LogsMissingResourcesError()
        {
            var outdatedSettings = ScriptableObject.CreateInstance<TMP_Settings>();
            try
            {
                outdatedSettings.assetVersion = "1";
                m_SettingsInstanceField.SetValue(null, outdatedSettings);
                LogAssert.Expect(LogType.Error, k_MissingResourcesError);

                new TMP_PackageResourceImporter().OnDestroy();
            }
            finally
            {
                Object.DestroyImmediate(outdatedSettings);
            }
        }

        static void CreateCurrentSettingsAsset()
        {
            AssetDatabase.CreateFolder("Assets", k_TempFolderName);
            AssetDatabase.CreateFolder(k_TempFolder, "Resources");

            var settings = ScriptableObject.CreateInstance<TMP_Settings>();
            settings.SetAssetVersion();
            AssetDatabase.CreateAsset(settings, k_TempFolder + "/Resources/TMP Settings.asset");
        }
    }
}
