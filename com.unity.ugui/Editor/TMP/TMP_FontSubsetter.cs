using UnityEditor;
using UnityEditor.TextCore.Text;
using UnityEngine;

namespace TMPro
{
    // Routes TMP_FontAsset subsetting through the shared FontSubsetterManager recipe machinery
    // and repoints the font asset at the hidden subset sub-asset.
    internal static class TMP_FontSubsetter
    {
        internal const string NoSourceFontImporterMessage = FontSubsetterManager.NoSourceFontImporterMessage;

        internal static bool TryGetSourceFontImporter(TMP_FontAsset fontAsset, out string fontPath)
        {
            return FontSubsetterManager.TryGetSourceFontImporter(fontAsset.SourceFont_EditorRef, out _, out fontPath);
        }

        internal static bool TryGetActiveRecipe(TMP_FontAsset fontAsset, out FontSubsetterManager.SubsetRecipe recipe)
        {
            return FontSubsetterManager.TryGetActiveRecipe(fontAsset, fontAsset.SourceFont_EditorRef, out recipe);
        }

        internal static bool ApplySubset(TMP_FontAsset fontAsset, string ranges, out string error)
        {
            if (!FontSubsetterManager.TryGenerateSubsetFont(fontAsset, fontAsset.SourceFont_EditorRef, ranges,
                fontAsset.faceInfo.faceIndex, FontSubsetterManager.HintingFlags(fontAsset.atlasRenderMode),
                out Font subsetFont, out error))
                return false;

            // hb-subset emits a single-face font, so a baked non-zero face index no longer applies.
            RepointSourceFont(fontAsset, subsetFont, faceIndex: 0);
            return true;
        }

        internal static void RemoveSubset(TMP_FontAsset fontAsset)
        {
            int faceIndex = fontAsset.faceInfo.faceIndex;
            FontSubsetterManager.RemoveRecipe(fontAsset, fontAsset.SourceFont_EditorRef, ref faceIndex);

            RepointSourceFont(fontAsset, fontAsset.SourceFont_EditorRef, faceIndex);
        }

        internal static Font ResolveDynamicSourceFont(TMP_FontAsset fontAsset)
        {
            var subsetFont = FontSubsetterManager.ResolveSubsetFont(fontAsset, fontAsset.SourceFont_EditorRef);
            return subsetFont != null ? subsetFont : fontAsset.SourceFont_EditorRef;
        }

        internal static long GetSubsetSizePreview(TMP_FontAsset fontAsset, string ranges, int faceIndex)
        {
            return FontSubsetterManager.GetSubsetSizePreview(fontAsset.SourceFont_EditorRef, ranges, faceIndex,
                FontSubsetterManager.HintingFlags(fontAsset.atlasRenderMode));
        }

        internal static string GetMissingCodePoints(TMP_FontAsset fontAsset, string ranges, int faceIndex)
        {
            return FontSubsetterManager.GetMissingCodePoints(fontAsset.SourceFont_EditorRef, ranges, faceIndex);
        }

        internal static bool IsSubsetActive(TMP_FontAsset fontAsset)
        {
            return FontSubsetterManager.IsSubsetActive(fontAsset, fontAsset.sourceFontFile);
        }

        static void RepointSourceFont(TMP_FontAsset fontAsset, Font font, int faceIndex)
        {
            // Face index first: UpdateSourceFontFile rebuilds the native shaping state with it.
            fontAsset.m_FaceInfo.faceIndex = faceIndex;
            fontAsset.sourceFontFile = font;
            fontAsset.UpdateSourceFontFile();
            fontAsset.ClearFontAssetData();
            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssetIfDirty(fontAsset);
        }
    }
}
