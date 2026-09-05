using System.Collections.Generic;
using UnityEditor;
using UnityEditor.TextCore.Text;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;

namespace TMPro
{
    // Surfaces static TMP_FontAssets in the shared Font Asset Migration window. TMP entries never
    // trigger the project-open intro prompt: the window only reaches this provider through the
    // inspector redirect, itself gated on TMP_Settings.useAdvancedText.
    internal sealed class TMP_FontAssetMigrationProvider : FontAssetMigrationProvider<TMP_FontAsset>
    {
        [InitializeOnLoadMethod]
        static void Register()
        {
            RegisterProvider(new TMP_FontAssetMigrationProvider());
        }

        [MenuItem("Window/TextMeshPro/Font Asset Migration", false, 2027)]
        static void ShowMigrationWindow()
        {
            FontAssetMigrationWindow.ShowWindow(FindProvider(typeof(TMP_FontAssetMigrationProvider).FullName));
        }

        internal override string DocsUrl => "https://docs.unity3d.com/Packages/com.unity.ugui@latest/index.html?subfolder=/manual/TextMeshPro/FontAssetsMigration.html";

        protected override bool IsStatic(TMP_FontAsset fontAsset) =>
            fontAsset.atlasPopulationMode != AtlasPopulationMode.Dynamic && fontAsset.atlasPopulationMode != AtlasPopulationMode.DynamicOS;

        protected override Font GetSourceFont(TMP_FontAsset fontAsset) => fontAsset.SourceFont_EditorRef;

        protected override int GetFaceIndex(TMP_FontAsset fontAsset) => fontAsset.faceInfo.faceIndex;

        protected override List<Glyph> GetGlyphTable(TMP_FontAsset fontAsset) => fontAsset.m_GlyphTable;

        protected override IEnumerable<(uint unicode, uint glyphIndex)> GetBakedCharacters(TMP_FontAsset fontAsset)
        {
            List<TMP_Character> characterTable = fontAsset.m_CharacterTable;
            if (characterTable == null)
                yield break;
            foreach (var character in characterTable)
                yield return (character.unicode, character.glyphIndex);
        }

        protected override GlyphRenderMode GetAtlasRenderMode(TMP_FontAsset fontAsset) => fontAsset.atlasRenderMode;

        protected override Vector2Int GetAtlasSize(TMP_FontAsset fontAsset) => new Vector2Int(fontAsset.atlasWidth, fontAsset.atlasHeight);

        protected override Texture2D[] GetAtlasTextures(TMP_FontAsset fontAsset) => fontAsset.atlasTextures;

        protected override void SetAtlasPopulationMode(TMP_FontAsset fontAsset, bool dynamic) =>
            fontAsset.atlasPopulationMode = dynamic ? AtlasPopulationMode.Dynamic : AtlasPopulationMode.Static;

        protected override void SetClearDynamicDataOnBuild(TMP_FontAsset fontAsset, bool clear) => fontAsset.clearDynamicDataOnBuild = clear;

        protected override void ClearStandardPipelineTables(TMP_FontAsset fontAsset)
        {
            fontAsset.m_CharacterTable.Clear();
            fontAsset.m_FontFeatureTable = new TMP_FontFeatureTable();
        }

        protected override void ClearFontAssetData(TMP_FontAsset fontAsset) => fontAsset.ClearFontAssetData(setAtlasSizeToZero: true);

        protected override void UpdateSourceFontFile(TMP_FontAsset fontAsset) => fontAsset.UpdateSourceFontFile();

        protected override bool ApplySubset(TMP_FontAsset fontAsset, string ranges, out string error) =>
            TMP_FontSubsetter.ApplySubset(fontAsset, ranges, out error);

        protected override bool ApplySubsetKeepingBakedData(TMP_FontAsset fontAsset, string ranges, out string error) =>
            TMP_FontSubsetter.ApplySubsetKeepingBakedData(fontAsset, ranges, out error);
    }
}
