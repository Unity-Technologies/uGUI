using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TMPro
{
    /// <summary>
    /// Imports the TMP Essential Resources into the test project before the test run starts, so that
    /// fixtures relying on TMP_Settings and the default font asset have them available.
    /// </summary>
    /// <remarks>
    /// This runs in the Editor for Edit Mode and Play Mode runs alike, and ahead of a player build,
    /// which is why it lives in the Play Mode assembly with an editor-only body.
    ///
    /// To reduce boilerplate derive from <see cref="TMPTestSuiteWithEssentialResources"/> rather than referencing this
    /// directly.
    /// </remarks>
    internal class TMPEssentialResourcesImporter : IPrebuildSetup
    {
        /// <summary>
        /// GUID of the "TextMesh Pro" folder that importing the Essential Resources creates.
        /// </summary>
        const string k_EssentialResourcesFolderGUID = "f54d1bd14bd3ca042bd867b519fee8cc";

        /// <summary>
        /// GUID for Roboto-Bold.ttf, which ships in Examples and Extras rather than the Essential Resources.
        /// </summary>
        internal const string k_ExampleFontGUID = "4beb055f07aaff244873dec698d0363e";

        public virtual void Setup()
        {
            Import(essentials: true, examples: false);
        }

        protected static void Import(bool essentials, bool examples)
        {
#if UNITY_EDITOR
            var importEssentials = essentials
                && string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(k_EssentialResourcesFolderGUID));
            var importExamples = examples
                && string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(k_ExampleFontGUID));

            if (!importEssentials && !importExamples)
                return;

            TMP_PackageResourceImporter.ImportResources(importEssentials, importExamples, false);
            AssetDatabase.Refresh();
#endif
        }
    }
}
