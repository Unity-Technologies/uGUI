using System.Threading;
using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;

namespace TMPro
{
    [Category("Text Parsing & Layout")]
    [Ignore("Unstable tests // UUM-133195")]
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
            if (FontEngine.LoadFontFace(filePath, 0, 0, out FontFaceHandle faceHandle) != FontEngineError.Success)
                return;

            UnityEngine.TextCore.LowLevel.SingleSubstitutionRecord[] records = FontEngine.GetAllSingleSubstitutionRecords(faceHandle);

            if (records == null)
                return;

            Assert.AreEqual(recordCount, records.Length);
        }

        [TestCase("47a9b34e6f77bbd4d94f512d266bcd0c", 77)] // Inter - Regular
        public void GetAlternateSubstitutionRecords(string fontFileGUID, int recordCount)
        {
            string filePath = AssetDatabase.GUIDToAssetPath(fontFileGUID);
            if (FontEngine.LoadFontFace(filePath, 0, 0, out FontFaceHandle faceHandle) != FontEngineError.Success)
                return;

            UnityEngine.TextCore.LowLevel.AlternateSubstitutionRecord[] records = FontEngine.GetAllAlternateSubstitutionRecords(faceHandle);

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
            if (FontEngine.LoadFontFace(filePath, 0, 0, out FontFaceHandle faceHandle) != FontEngineError.Success)
                return;

            UnityEngine.TextCore.LowLevel.LigatureSubstitutionRecord[] records = FontEngine.GetAllLigatureSubstitutionRecords(faceHandle);

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
            if (FontEngine.LoadFontFace(filePath, 0, 0, out FontFaceHandle faceHandle) != FontEngineError.Success)
                return;

            GlyphPairAdjustmentRecord[] records = FontEngine.GetAllPairAdjustmentRecords(faceHandle);

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
            if (FontEngine.LoadFontFace(filePath, 0, 0, out FontFaceHandle faceHandle) != FontEngineError.Success)
                return;

            UnityEngine.TextCore.LowLevel.MarkToBaseAdjustmentRecord[] records = FontEngine.GetAllMarkToBaseAdjustmentRecords(faceHandle);

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
            if (FontEngine.LoadFontFace(filePath, 0, 0, out FontFaceHandle faceHandle) != FontEngineError.Success)
                return;

            UnityEngine.TextCore.LowLevel.MarkToMarkAdjustmentRecord[] records = FontEngine.GetAllMarkToMarkAdjustmentRecords(faceHandle);

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

#if TEXTCORE_FONT_ENGINE_1_6_OR_NEWER
    [Category("Text Parsing & Layout")]
    internal class FontFaceHandleTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            // These tests load font assets shipped with the TMP Essential Resources. Import them if
            // the package folder is not already present (mirrors FontEngineTests).
            string essentialResourcesPath = AssetDatabase.GUIDToAssetPath("f54d1bd14bd3ca042bd867b519fee8cc");
            if (string.IsNullOrEmpty(essentialResourcesPath))
                TMP_PackageResourceImporter.ImportResources(true, true, false);

            // If the font files these tests depend on still cannot be resolved (resources unavailable
            // in this environment), skip the whole fixture rather than failing — but log why so the
            // skip is visible in the test output.
            if (string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath("e3265ab4bf004d28a9537516768c1c75")))
            {
                Debug.LogWarning("FontFaceHandleTests skipped: required TMP font resources are not present in the project.");
                Assert.Ignore("Required TMP font resources are not present; skipping FontFaceHandle tests.");
            }
        }

        [TestCase("e3265ab4bf004d28a9537516768c1c75")]
        [TestCase("4beb055f07aaff244873dec698d0363e")]
        public void LoadFontFace_ReturnsValidHandle(string fontFileGUID)
        {
            string filePath = AssetDatabase.GUIDToAssetPath(fontFileGUID);

            FontEngineError error = FontEngine.LoadFontFace(filePath, 90, 0, out FontFaceHandle faceHandle);

            Assert.AreEqual(FontEngineError.Success, error);
            Assert.IsTrue(faceHandle.IsValid);
        }

        [Test]
        public void LoadFontFace_WithInvalidPath_ReturnsErrorAndDefaultHandle()
        {
            FontEngineError error = FontEngine.LoadFontFace("invalid/path/does_not_exist.ttf", 90, 0, out FontFaceHandle faceHandle);

            Assert.AreNotEqual(FontEngineError.Success, error);
            Assert.IsFalse(faceHandle.IsValid);
        }

        [TestCase("e3265ab4bf004d28a9537516768c1c75", "Liberation Sans", "Regular")]
        [TestCase("4beb055f07aaff244873dec698d0363e", "Roboto", "Bold")]
        public void GetFaceInfo_via_Handle_MatchesGlobalGetFaceInfo(string fontFileGUID, string familyName, string styleName)
        {
            string filePath = AssetDatabase.GUIDToAssetPath(fontFileGUID);

            FontEngineError error = FontEngine.LoadFontFace(filePath, 90, 0, out FontFaceHandle faceHandle);
            Assert.AreEqual(FontEngineError.Success, error);

            FaceInfo infoViaHandle = FontEngine.GetFaceInfo(faceHandle);
            FaceInfo infoGlobal   = FontEngine.GetFaceInfo();

            Assert.AreEqual(familyName, infoViaHandle.familyName);
            Assert.AreEqual(styleName,  infoViaHandle.styleName);
            Assert.AreEqual(infoGlobal.familyName,  infoViaHandle.familyName);
            Assert.AreEqual(infoGlobal.pointSize,    infoViaHandle.pointSize);
        }

        // Loads two fonts, stashes their handles, then uses each handle to restore the correct face
        // without a fresh LoadFontFace call — the core correctness guarantee of FontFaceHandle.
        [TestCase("e3265ab4bf004d28a9537516768c1c75", "Liberation Sans", "4beb055f07aaff244873dec698d0363e", "Roboto")]
        public void FontFaceHandle_RestoresCorrectFace(string guidA, string familyA, string guidB, string familyB)
        {
            string pathA = AssetDatabase.GUIDToAssetPath(guidA);
            string pathB = AssetDatabase.GUIDToAssetPath(guidB);

            Assert.AreEqual(FontEngineError.Success, FontEngine.LoadFontFace(pathA, 90, 0, out FontFaceHandle handleA));
            Assert.AreEqual(FontEngineError.Success, FontEngine.LoadFontFace(pathB, 72, 0, out FontFaceHandle handleB));

            // Active face is now B — restore A via its handle.
            FaceInfo infoA = FontEngine.GetFaceInfo(handleA);
            Assert.AreEqual(familyA, infoA.familyName);
            Assert.AreEqual(90, infoA.pointSize);

            // Restore B via its handle.
            FaceInfo infoB = FontEngine.GetFaceInfo(handleB);
            Assert.AreEqual(familyB, infoB.familyName);
            Assert.AreEqual(72, infoB.pointSize);
        }

        [TestCase("e3265ab4bf004d28a9537516768c1c75", 2016)]
        [TestCase("4beb055f07aaff244873dec698d0363e", 70027)]
        public void GetPairAdjustmentRecords_via_Handle(string fontFileGUID, int recordCount)
        {
            string filePath = AssetDatabase.GUIDToAssetPath(fontFileGUID);

            FontEngineError error = FontEngine.LoadFontFace(filePath, 90, 0, out FontFaceHandle faceHandle);
            Assert.AreEqual(FontEngineError.Success, error);

            GlyphPairAdjustmentRecord[] records = FontEngine.GetAllPairAdjustmentRecords(faceHandle);

            Assert.IsNotNull(records);
            Assert.AreEqual(recordCount, records.Length);
        }

        // Exercises the single-pair GPOS lookup whose loop previously used end() as its condition
        // (iterating past the end of the records vector) and indexed the cached map via operator[]
        // (default-inserting a missing key). Derives a real pair from the font's own records, then
        // probes a first glyph against an impossible partner to drive the no-match path that ran
        // past the end of the vector.
        [TestCase("e3265ab4bf004d28a9537516768c1c75")] // Liberation Sans (has kerning)
        public void GetPairAdjustmentRecord_via_Handle(string fontFileGUID)
        {
            string filePath = AssetDatabase.GUIDToAssetPath(fontFileGUID);
            Assert.AreEqual(FontEngineError.Success, FontEngine.LoadFontFace(filePath, 90, 0, out FontFaceHandle faceHandle));

            GlyphPairAdjustmentRecord[] all = FontEngine.GetAllPairAdjustmentRecords(faceHandle);
            Assert.IsNotNull(all);
            Assert.Greater(all.Length, 0);

            // A real (first, second) pair taken from the font's own data must resolve to itself.
            uint first = all[0].firstAdjustmentRecord.glyphIndex;
            uint second = all[0].secondAdjustmentRecord.glyphIndex;

            GlyphPairAdjustmentRecord match = FontEngine.GetPairAdjustmentRecord(faceHandle, first, second);
            Assert.AreEqual(first, match.firstAdjustmentRecord.glyphIndex);
            Assert.AreEqual(second, match.secondAdjustmentRecord.glyphIndex);

            // A first glyph that has records, paired with an impossible partner, must scan to the end
            // and return an empty record rather than reading past it.
            GlyphPairAdjustmentRecord noMatch = FontEngine.GetPairAdjustmentRecord(faceHandle, first, 0xFFFFFFFE);
            Assert.AreEqual(0u, noMatch.secondAdjustmentRecord.glyphIndex);
        }

        [TestCase("e3265ab4bf004d28a9537516768c1c75")]
        public void UnloadFontFace_via_Handle_InvalidatesHandle(string fontFileGUID)
        {
            string filePath = AssetDatabase.GUIDToAssetPath(fontFileGUID);

            Assert.AreEqual(FontEngineError.Success, FontEngine.LoadFontFace(filePath, 90, 0, out FontFaceHandle faceHandle));
            Assert.IsTrue(faceHandle.IsValid);

            Assert.AreEqual(FontEngineError.Success, FontEngine.UnloadFontFace(faceHandle));

            // After unloading, operations using the stale handle should fail with InvalidFontFaceHandle.
            // The native layer logs a warning when it cannot resolve the stale handle — that is expected here.
            LogAssert.Expect(LogType.Warning, "FontEngine: Font face handle no longer resolves to a cached font face.");
            FontEngineError result = FontEngine.SetFaceSize(ref faceHandle, 90);
            Assert.AreEqual(FontEngineError.Invalid_FontFaceHandle, result);
        }

        [TestCase("e3265ab4bf004d28a9537516768c1c75")]
        public void StaleHandle_OperationsReturnGracefully(string fontFileGUID)
        {
            string filePath = AssetDatabase.GUIDToAssetPath(fontFileGUID);

            Assert.AreEqual(FontEngineError.Success, FontEngine.LoadFontFace(filePath, 90, 0, out FontFaceHandle faceHandle));
            Assert.AreEqual(FontEngineError.Success, FontEngine.UnloadFontFace(faceHandle));

            // GetFaceInfo — returns a zeroed FaceInfo rather than throwing.
            LogAssert.Expect(LogType.Warning, "FontEngine: Font face handle no longer resolves to a cached font face.");
            FaceInfo faceInfo = FontEngine.GetFaceInfo(faceHandle);
            Assert.AreEqual(0, faceInfo.pointSize);

            // TryGetGlyphIndex — returns false rather than throwing.
            LogAssert.Expect(LogType.Warning, "FontEngine: Font face handle no longer resolves to a cached font face.");
            bool found = FontEngine.TryGetGlyphIndex(faceHandle, 65, out uint glyphIndex);
            Assert.IsFalse(found);
            Assert.AreEqual(0u, glyphIndex);

            // GetGlyphIndex — returns 0 rather than throwing.
            LogAssert.Expect(LogType.Warning, "FontEngine: Font face handle no longer resolves to a cached font face.");
            uint index = FontEngine.GetGlyphIndex(faceHandle, 65);
            Assert.AreEqual(0u, index);
        }

        // A scalable font requested at two different point sizes resolves to the SAME cached FT_Face
        // (MakeFontID excludes pixel size for scalable fonts), so both handles share one face — and
        // one per-face lock. Each handle must still operate at, and report, its own size: the handle
        // path resolves the face locally and re-applies the requested size under the lock rather than
        // relying on whatever size the previous caller left on the shared face.
        [TestCase("e3265ab4bf004d28a9537516768c1c75", "Liberation Sans")]
        public void SameScalableFont_DifferentSizes_TrackSizePerHandle(string fontFileGUID, string familyName)
        {
            string filePath = AssetDatabase.GUIDToAssetPath(fontFileGUID);

            Assert.AreEqual(FontEngineError.Success, FontEngine.LoadFontFace(filePath, 90, 0, out FontFaceHandle handle90));
            Assert.AreEqual(FontEngineError.Success, FontEngine.LoadFontFace(filePath, 30, 0, out FontFaceHandle handle30));

            // Interleave repeatedly; each handle must re-assert its own size every time.
            for (int i = 0; i < 4; i++)
            {
                FaceInfo info90 = FontEngine.GetFaceInfo(handle90);
                Assert.AreEqual(familyName, info90.familyName);
                Assert.AreEqual(90, info90.pointSize, "handle90 should report size 90 regardless of interleaving");

                FaceInfo info30 = FontEngine.GetFaceInfo(handle30);
                Assert.AreEqual(familyName, info30.familyName);
                Assert.AreEqual(30, info30.pointSize, "handle30 should report size 30 regardless of interleaving");
            }
        }

        // Two distinct faces accessed concurrently must not corrupt each other: with face-threading
        // the handle path never touches a shared global current-face, so each thread keeps seeing its
        // own font/size. (Distinct FT_Faces are independently safe per the FreeType contract.)
        // Faces are loaded on the main thread up front (face creation is not yet concurrency-safe);
        // the workers only read through their already-valid handles, which the per-face lock guards.
        [TestCase("e3265ab4bf004d28a9537516768c1c75", "Liberation Sans", "4beb055f07aaff244873dec698d0363e", "Roboto")]
        public void ConcurrentHandleAccess_DistinctFaces_RemainCorrect(string guidA, string familyA, string guidB, string familyB)
        {
            string pathA = AssetDatabase.GUIDToAssetPath(guidA);
            string pathB = AssetDatabase.GUIDToAssetPath(guidB);

            Assert.AreEqual(FontEngineError.Success, FontEngine.LoadFontFace(pathA, 90, 0, out FontFaceHandle handleA));
            Assert.AreEqual(FontEngineError.Success, FontEngine.LoadFontFace(pathB, 72, 0, out FontFaceHandle handleB));

            const int iterations = 200;
            string failureA = null;
            string failureB = null;

            var tA = new Thread(() => { failureA = HammerFaceInfo(handleA, familyA, 90, iterations); });
            var tB = new Thread(() => { failureB = HammerFaceInfo(handleB, familyB, 72, iterations); });

            tA.Start();
            tB.Start();
            tA.Join();
            tB.Join();

            Assert.IsNull(failureA, failureA);
            Assert.IsNull(failureB, failureB);
        }

        // Two handles to the SAME scalable font at different sizes contend on one shared FT_Face.
        // The per-face lock plus per-handle size re-application must keep each thread's view at its
        // own size; without the lock the threads would stomp the shared face's active size and read
        // each other's metrics.
        [TestCase("e3265ab4bf004d28a9537516768c1c75", "Liberation Sans")]
        public void ConcurrentHandleAccess_SameFontDifferentSizes_RemainCorrect(string fontFileGUID, string familyName)
        {
            string filePath = AssetDatabase.GUIDToAssetPath(fontFileGUID);

            Assert.AreEqual(FontEngineError.Success, FontEngine.LoadFontFace(filePath, 90, 0, out FontFaceHandle handle90));
            Assert.AreEqual(FontEngineError.Success, FontEngine.LoadFontFace(filePath, 30, 0, out FontFaceHandle handle30));

            const int iterations = 200;
            string failure90 = null;
            string failure30 = null;

            var t90 = new Thread(() => { failure90 = HammerFaceInfo(handle90, familyName, 90, iterations); });
            var t30 = new Thread(() => { failure30 = HammerFaceInfo(handle30, familyName, 30, iterations); });

            t90.Start();
            t30.Start();
            t90.Join();
            t30.Join();

            Assert.IsNull(failure90, failure90);
            Assert.IsNull(failure30, failure30);
        }

        // Exercises FontFaceCacheLock: a worker thread repeatedly resolves a stable handle while the
        // main thread churns the font face cache by loading and unloading a DIFFERENT face (insert /
        // erase, and the rehash they can trigger). Without the cache lock, the worker's lookup inside
        // ResolveFaceLocked would race the main-thread structural mutation of the shared cache. The
        // stable face is never unloaded, so the worker must keep reading its correct family.
        [TestCase("e3265ab4bf004d28a9537516768c1c75", "Liberation Sans", "4beb055f07aaff244873dec698d0363e")]
        public void ConcurrentResolve_WhileCacheChurns_RemainsStable(string stableGuid, string stableFamily, string churnGuid)
        {
            string stablePath = AssetDatabase.GUIDToAssetPath(stableGuid);
            string churnPath = AssetDatabase.GUIDToAssetPath(churnGuid);

            Assert.AreEqual(FontEngineError.Success, FontEngine.LoadFontFace(stablePath, 90, 0, out FontFaceHandle stableHandle));

            const int iterations = 200;
            string failure = null;

            var reader = new Thread(() =>
            {
                for (int i = 0; i < iterations && failure == null; i++)
                {
                    FaceInfo info = FontEngine.GetFaceInfo(stableHandle);
                    if (info.familyName != stableFamily)
                    {
                        failure = $"Expected {stableFamily}, got '{info.familyName}' on iteration {i}";
                        return;
                    }
                }
            });

            reader.Start();

            // Main-thread cache churn: load + unload another face repeatedly (writers take the cache lock).
            for (int i = 0; i < iterations; i++)
            {
                if (FontEngine.LoadFontFace(churnPath, 72, 0, out FontFaceHandle churnHandle) == FontEngineError.Success)
                    FontEngine.UnloadFontFace(churnHandle);
            }

            reader.Join();
            Assert.IsNull(failure, failure);
        }

        // Repeatedly resolves the handle and checks the face reports the expected family/size.
        // Returns a description of the first mismatch, or null on success (asserted on the main
        // thread after Join — NUnit assertions don't propagate from worker threads).
        static string HammerFaceInfo(FontFaceHandle handle, string expectedFamily, int expectedSize, int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                FaceInfo info = FontEngine.GetFaceInfo(handle);
                if (info.familyName != expectedFamily || info.pointSize != expectedSize)
                    return $"Expected {expectedFamily}@{expectedSize}, got '{info.familyName}'@{info.pointSize} on iteration {i}";
            }

            return null;
        }
    }
#endif
}
