using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using UnityEngine.TextCore.LowLevel;

namespace TMPro
{
    [Category("Text Parsing & Layout")]
    internal class FontEngineTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            // Check if "TextMesh Pro" folder is present in project
            string folderPath = AssetDatabase.GUIDToAssetPath("f54d1bd14bd3ca042bd867b519fee8cc");

            if (string.IsNullOrEmpty(folderPath))
            {
                // Import TMP Essential Resources and TMP Examples & Extras
                TMP_PackageResourceImporter.ImportResources(true, true, false);
            }
        }

        [TestCase("e3265ab4bf004d28a9537516768c1c75", "Liberation Sans", "Regular")]
        [TestCase("4beb055f07aaff244873dec698d0363e", "Roboto", "Bold")]
        [TestCase("997a43b767814dd0a7642ec9b78cba41", "Anton", "Regular")]
        public void CreateFontAsset_from_FilePath(string fontFileGUID, string familyName, string styleName)
        {
            string filePath = AssetDatabase.GUIDToAssetPath(fontFileGUID);

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(filePath, 0, 90, 9, GlyphRenderMode.SDFAA, 512, 512);

            Assert.NotNull(fontAsset);

            Assert.AreEqual(fontAsset.faceInfo.familyName, familyName);
            Assert.AreEqual(fontAsset.faceInfo.styleName, styleName);
        }

        [TestCase("e3265ab4bf004d28a9537516768c1c75", "Liberation Sans", "Regular")]
        [TestCase("4beb055f07aaff244873dec698d0363e", "Roboto", "Bold")]
        [TestCase("997a43b767814dd0a7642ec9b78cba41", "Anton", "Regular")]
        public void CreateFontAsset_from_FontObject(string fontFileGUID, string familyName, string styleName)
        {
            string filePath = AssetDatabase.GUIDToAssetPath(fontFileGUID);

            Font font = AssetDatabase.LoadAssetAtPath<Font>(filePath);

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 512, 512);

            Assert.NotNull(fontAsset);

            Assert.AreEqual(fontAsset.faceInfo.familyName, familyName);
            Assert.AreEqual(fontAsset.faceInfo.styleName, styleName);
        }

        [TestCase("e3265ab4bf004d28a9537516768c1c75", "Liberation Sans", "Regular")]
        public void TryAddCharacters_SanityCheck(string fontFileGUID, string familyName, string styleName)
        {
            string filePath = AssetDatabase.GUIDToAssetPath(fontFileGUID);

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(filePath, 0, 90, 9, GlyphRenderMode.SDFAA, 512, 512);

            Assert.NotNull(fontAsset);

            fontAsset.TryAddCharacters("abc");
            Assert.IsTrue(fontAsset.HasCharacters("abc"));
        }

        // =============================================
        // FONT ENGINE - OPENTYPE TESTS
        // =============================================

        #if TEXTCORE_FONT_ENGINE_1_5_OR_NEWER
        [TestCase("e3265ab4bf004d28a9537516768c1c75", 1)]
        [TestCase("4beb055f07aaff244873dec698d0363e", 623)]
        [TestCase("24007ea0bd4d6b2418f4caf1b06e2cb4", 43)]
        public void GetSingleSubstitutionRecords(string fontFileGUID, int recordCount)
        {
            string filePath = AssetDatabase.GUIDToAssetPath(fontFileGUID);
            if (FontEngine.LoadFontFace(filePath) != FontEngineError.Success)
                return;

            UnityEngine.TextCore.LowLevel.SingleSubstitutionRecord[] records = FontEngine.GetAllSingleSubstitutionRecords();

            if (records == null)
                return;

            Assert.AreEqual(recordCount, records.Length);
        }

        [TestCase("47a9b34e6f77bbd4d94f512d266bcd0c", 77)] // Inter - Regular
        public void GetAlternateSubstitutionRecords(string fontFileGUID, int recordCount)
        {
            string filePath = AssetDatabase.GUIDToAssetPath(fontFileGUID);
            if (FontEngine.LoadFontFace(filePath) != FontEngineError.Success)
                return;

            UnityEngine.TextCore.LowLevel.AlternateSubstitutionRecord[] records = FontEngine.GetAllAlternateSubstitutionRecords();

            if (records == null)
                return;

            Assert.AreEqual(recordCount, records.Length);
        }

        [TestCase("e3265ab4bf004d28a9537516768c1c75", 184)]
        [TestCase("4beb055f07aaff244873dec698d0363e", 177)]
        [TestCase("c9f6d0e7bc8541498c9a4799ba184ede", 5)]
        public void GetLigatures(string fontFileGUID, int recordCount)
        {
            string filePath = AssetDatabase.GUIDToAssetPath(fontFileGUID);
            if (FontEngine.LoadFontFace(filePath) != FontEngineError.Success)
                return;

            UnityEngine.TextCore.LowLevel.LigatureSubstitutionRecord[] records = FontEngine.GetAllLigatureSubstitutionRecords();

            if (records == null)
                return;

            Assert.AreEqual(recordCount, records.Length);
        }

        [TestCase("e3265ab4bf004d28a9537516768c1c75", 2016)]
        [TestCase("4beb055f07aaff244873dec698d0363e", 70027)]
        [TestCase("c9f6d0e7bc8541498c9a4799ba184ede", 1940)]
        public void GetPairAdjustmentRecords(string fontFileGUID, int recordCount)
        {
            string filePath = AssetDatabase.GUIDToAssetPath(fontFileGUID);
            if (FontEngine.LoadFontFace(filePath) != FontEngineError.Success)
                return;

            GlyphPairAdjustmentRecord[] records = FontEngine.GetAllPairAdjustmentRecords();

            if (records == null)
                return;

            Assert.AreEqual(recordCount, records.Length);
        }

        [TestCase("e3265ab4bf004d28a9537516768c1c75", 8911)]
        [TestCase("4beb055f07aaff244873dec698d0363e", 0)]
        [TestCase("24007ea0bd4d6b2418f4caf1b06e2cb4", 432)]
        public void GetMarkToBaseAdjustmentRecords(string fontFileGUID, int recordCount)
        {
            string filePath = AssetDatabase.GUIDToAssetPath(fontFileGUID);
            if (FontEngine.LoadFontFace(filePath) != FontEngineError.Success)
                return;

            UnityEngine.TextCore.LowLevel.MarkToBaseAdjustmentRecord[] records = FontEngine.GetAllMarkToBaseAdjustmentRecords();

            if (records == null)
                return;

            Assert.AreEqual(recordCount, records.Length);
        }

        [TestCase("e3265ab4bf004d28a9537516768c1c75", 0)]
        [TestCase("4beb055f07aaff244873dec698d0363e", 0)]
        [TestCase("24007ea0bd4d6b2418f4caf1b06e2cb4", 324)]
        public void GetMarkToMarkAdjustmentRecords(string fontFileGUID, int recordCount)
        {
            string filePath = AssetDatabase.GUIDToAssetPath(fontFileGUID);
            if (FontEngine.LoadFontFace(filePath) != FontEngineError.Success)
                return;

            UnityEngine.TextCore.LowLevel.MarkToMarkAdjustmentRecord[] records = FontEngine.GetAllMarkToMarkAdjustmentRecords();

            if (records == null)
                return;

            Assert.AreEqual(recordCount, records.Length);
        }

        // GetOpenTypeFontFeatureList throws NotImplementedException with new FontEngine changes to support FontFeature (TEXTCORE_FONT_ENGINE_1_5_OR_NEWER)
        /*
        [TestCase("e3265ab4bf004d28a9537516768c1c75", 0)]
        [TestCase("4beb055f07aaff244873dec698d0363e", 0)]
        [TestCase("24007ea0bd4d6b2418f4caf1b06e2cb4", 324)]
        [TestCase("47a9b34e6f77bbd4d94f512d266bcd0c", 324)]
        [TestCase("d01f31227e1b4cd49bc293f44aab2253", 324)]
        public void GetFontFeatureList(string fontFileGUID, int recordCount)
        {
            string filePath = AssetDatabase.GUIDToAssetPath(fontFileGUID);
            if (FontEngine.LoadFontFace(filePath) != FontEngineError.Success)
                return;

            OpenTypeFeature[] fontFeatureList = FontEngine.GetOpenTypeFontFeatureList();

            if (fontFeatureList == null)
                return;

            Assert.AreEqual(recordCount, fontFeatureList.Length);
        } */
        #endif
    }
}
