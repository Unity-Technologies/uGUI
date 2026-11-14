using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using ATGMeshInfo = UnityEngine.TextCore.Text.ATGMeshInfo;
using NativeTextInfo = UnityEngine.TextCore.Text.NativeTextInfo;
using OSFontFallbackResolver = UnityEngine.TextCore.Text.OSFontFallbackResolver;
using TextCoreFontAsset = UnityEngine.TextCore.Text.FontAsset;
using TextCoreFontStyles = UnityEngine.TextCore.Text.FontStyles;
using TextCoreStyle = UnityEngine.TextCore.Text.TextStyle;
using TextCoreStyleSheet = UnityEngine.TextCore.Text.TextStyleSheet;
using TextCoreTextSettings = UnityEngine.TextCore.Text.TextSettings;
using TextFontWeight = UnityEngine.TextCore.Text.TextFontWeight;
using TextGenerationInfo = UnityEngine.TextCore.Text.TextGenerationInfo;
using TextHandle = UnityEngine.TextCore.Text.TextHandle;
using TextLib = UnityEngine.TextCore.Text.TextLib;

namespace TMPro
{
    internal enum GenerationOutcome { Rendered = 0, NoFontAsset = 1, EmptyText = 2 }

    /// <summary>
    /// Serves TMP text generation through the Advanced Text Generator (TextLib) when a text
    /// component opts in via <see cref="TMP_Text.enableAdvancedText"/>. Converts the component
    /// state to <see cref="NativeTextGenerationSettings"/> and the native output back into the
    /// component's <see cref="TMP_TextInfo"/>.
    /// </summary>
    internal class TMP_AdvancedTextGenerator
    {
        static TMP_AdvancedTextGenerator s_Generator;
        static TextLib s_TextLib;
        static TextCoreTextSettings s_TextSettings;
        static readonly Dictionary<(EntityId, int), Material> s_BridgeMaterials = new Dictionary<(EntityId, int), Material>();
        static readonly Dictionary<EntityId, HashSet<uint>> s_MissingGlyphsPerFontAsset = new Dictionary<EntityId, HashSet<uint>>();
        static readonly List<uint> s_GlyphsToAdd = new List<uint>();

        internal static TMP_AdvancedTextGenerator GetTextGenerator(TMP_Text textComponent, TMP_TextInfo textInfo)
        {
            if (s_Generator == null)
                s_Generator = new TMP_AdvancedTextGenerator();

            s_Generator.m_TextComponent = textComponent;
            s_Generator.m_TextInfo = textInfo;
            return s_Generator;
        }

        static TextLib GetTextLib()
        {
            if (s_TextLib == null)
            {
                // A missing ICU data asset is not fatal: the native side falls back
                // to minimal text segmentation (basic line breaking rules only).
                var icuAsset = TextHandle.GetICUAssetStaticFalback();
                s_TextLib = new TextLib(icuAsset != null ? icuAsset.bytes : Array.Empty<byte>());
            }
            return s_TextLib;
        }

        static TextCoreTextSettings GetTextSettings()
        {
            if (s_TextSettings == null)
            {
                s_TextSettings = ScriptableObject.CreateInstance<TextCoreTextSettings>();
                s_TextSettings.hideFlags = HideFlags.HideAndDontSave;
                s_TextSettings.externalGlobalFallbackProvider = BuildGlobalFallbackPtrs;
                InstallRichTextAssetHooks();
            }
            return s_TextSettings;
        }

        static readonly List<TMP_FontAsset> s_GlobalFallbackCandidates = new List<TMP_FontAsset>();
        static readonly List<TMP_FontAsset> s_UploadedGlobalFallbacks = new List<TMP_FontAsset>();

        // Mirrors the legacy missing-character search order: the TMP settings fallback list, the
        // emoji fallback font assets, then the default font asset. Provided to the native
        // TextSettings as global fallbacks so ATG resolves the same characters as the legacy path.
        static void UpdateGlobalFallbacks()
        {
            s_GlobalFallbackCandidates.Clear();

            var fallbacks = TMP_Settings.fallbackFontAssets;
            if (fallbacks != null)
            {
                foreach (var fallback in fallbacks)
                {
                    if (fallback != null)
                        s_GlobalFallbackCandidates.Add(fallback);
                }
            }

            var emojiFallbacks = TMP_Settings.emojiFallbackTextAssets;
            if (emojiFallbacks != null)
            {
                foreach (var fallback in emojiFallbacks)
                {
                    if (fallback is TMP_FontAsset fontAsset)
                        s_GlobalFallbackCandidates.Add(fontAsset);
                }
            }

            if (TMP_Settings.defaultFontAsset != null)
                s_GlobalFallbackCandidates.Add(TMP_Settings.defaultFontAsset);

            // Re-uploading is not free; only dirty when the set changed.
            if (CandidatesMatchUploadedFallbacks())
                return;

            s_UploadedGlobalFallbacks.Clear();
            s_UploadedGlobalFallbacks.AddRange(s_GlobalFallbackCandidates);

            s_TextSettings.SetNativeTextSettingsDirty();
        }

        // Invoked by TextSettings at upload time. Built fresh on each call: native peers may have
        // been destroyed and lazily recreated since the last upload, so pointers are never cached.
        static IntPtr[] BuildGlobalFallbackPtrs()
        {
            var nativeFallbacks = new List<IntPtr>();
            foreach (var fallback in s_UploadedGlobalFallbacks)
            {
                if (fallback == null)
                    continue;

                var nativeFallback = fallback.nativeFontAsset;
                if (nativeFallback != IntPtr.Zero)
                    nativeFallbacks.Add(nativeFallback);
                else
                    nativeFallbacks.AddRange(fallback.GetNativeFallbacks());
            }
            return nativeFallbacks.ToArray();
        }

        static bool CandidatesMatchUploadedFallbacks()
        {
            if (s_GlobalFallbackCandidates.Count != s_UploadedGlobalFallbacks.Count)
                return false;

            for (int i = 0; i < s_GlobalFallbackCandidates.Count; i++)
            {
                if (!ReferenceEquals(s_GlobalFallbackCandidates[i], s_UploadedGlobalFallbacks[i]))
                    return false;
            }
            return true;
        }

        static readonly Dictionary<uint, TMP_FontAsset> s_TagFontAssets = new Dictionary<uint, TMP_FontAsset>();
        static readonly Dictionary<uint, TMP_SpriteAsset> s_TagSpriteAssets = new Dictionary<uint, TMP_SpriteAsset>();
        static readonly Dictionary<(TMP_StyleSheet, TMP_StyleSheet), TextCoreStyleSheet> s_ConvertedStyleSheets = new Dictionary<(TMP_StyleSheet, TMP_StyleSheet), TextCoreStyleSheet>();
        static readonly HashSet<TMP_SpriteAsset> s_VisitedSpriteAssets = new HashSet<TMP_SpriteAsset>();
        static TMP_SpriteAsset s_CurrentSpriteAsset;

        // Rich text tag asset resolution (<font>, <sprite>) is served through the registry the
        // native parser calls back into; the hooks self-gate on the ATG TextSettings instance.
        static void InstallRichTextAssetHooks()
        {
            NativeRichTextAssetRegistry.externalFontAssetResolver = ResolveTagFontAsset;
            NativeRichTextAssetRegistry.externalFontAssetLoader = LoadTagFontAsset;
            NativeRichTextAssetRegistry.externalSpriteAssetLoader = LoadTagSpriteAsset;
            NativeRichTextAssetRegistry.externalSpriteResolver = ResolveTagSprite;

            TMPro_EventManager.TEXT_STYLE_PROPERTY_EVENT.Add(OnTextStyleChanged);
        }

        static void OnTextStyleChanged(bool isChanged)
        {
            s_ConvertedStyleSheets.Clear();
        }

        static bool IsAdvancedTextSettings(EntityId textSettingsId)
        {
            return s_TextSettings != null && textSettingsId == s_TextSettings.GetEntityId();
        }

        static IntPtr ResolveTagFontAsset(uint nameHash)
        {
            if (!s_TagFontAssets.TryGetValue(nameHash, out var fontAsset) || fontAsset == null)
                return IntPtr.Zero;

            return fontAsset.nativeFontAsset;
        }

        static void LoadTagFontAsset(EntityId textSettingsId, string name)
        {
            if (!IsAdvancedTextSettings(textSettingsId))
                return;

            uint hash = NativeRichTextAssetRegistry.HashName(name);
            if (s_TagFontAssets.ContainsKey(hash))
                return;

            var fontAsset = Resources.Load<TMP_FontAsset>(TMP_Settings.defaultFontAssetPath + name);
            if (fontAsset == null)
                return;

            fontAsset.EnsureNativeFontAssetIsCreated();
            s_TagFontAssets[hash] = fontAsset;
        }

        static void LoadTagSpriteAsset(EntityId textSettingsId, string name)
        {
            if (!IsAdvancedTextSettings(textSettingsId))
                return;

            uint hash = NativeRichTextAssetRegistry.HashName(name);
            if (s_TagSpriteAssets.ContainsKey(hash))
                return;

            var spriteAsset = Resources.Load<TMP_SpriteAsset>(TMP_Settings.defaultSpriteAssetPath + name);
            if (spriteAsset == null)
                return;

            spriteAsset.UpdateLookupTables();
            s_TagSpriteAssets[hash] = spriteAsset;
        }

        // FNV hash of an empty name, matching NativeRichTextAssetRegistry.HashName.
        const uint k_EmptyNameHash = 2166136261u;

        const char k_SoftHyphen = (char)0xAD;

        static bool ResolveTagSprite(EntityId textSettingsId, uint assetNameHash, uint spriteNameHash, int spriteIndexHint,
            out int spriteIndex, out EntityId spriteAssetId, out GlyphMetrics metrics, out float scale)
        {
            spriteIndex = -1;
            spriteAssetId = default;
            metrics = default;
            scale = 0f;

            if (!IsAdvancedTextSettings(textSettingsId))
                return false;

            TMP_SpriteAsset searchRoot = assetNameHash == k_EmptyNameHash
                ? s_CurrentSpriteAsset
                : (s_TagSpriteAssets.TryGetValue(assetNameHash, out var namedAsset) ? namedAsset : null);

            s_VisitedSpriteAssets.Clear();
            var asset = FindSprite(searchRoot, spriteNameHash, spriteIndexHint, out spriteIndex);

            if (asset == null && assetNameHash == k_EmptyNameHash && !ReferenceEquals(searchRoot, TMP_Settings.defaultSpriteAsset))
                asset = FindSprite(TMP_Settings.defaultSpriteAsset, spriteNameHash, spriteIndexHint, out spriteIndex);

            // Handled either way: the sprite is simply unresolved for this text when null.
            if (asset == null)
                return true;

            var spriteCharacter = asset.spriteCharacterTable[spriteIndex];
            spriteAssetId = asset.GetEntityId();
            metrics = spriteCharacter.glyph != null ? spriteCharacter.glyph.metrics : default;
            float glyphScale = spriteCharacter.glyph != null ? spriteCharacter.glyph.scale : 1f;
            scale = spriteCharacter.scale * glyphScale;
            return true;
        }

        static TMP_SpriteAsset FindSprite(TMP_SpriteAsset asset, uint spriteNameHash, int spriteIndexHint, out int spriteIndex)
        {
            spriteIndex = -1;
            if (asset == null || !s_VisitedSpriteAssets.Add(asset))
                return null;

            var table = asset.spriteCharacterTable;
            int count = table != null ? table.Count : 0;

            if (spriteIndexHint >= 0)
            {
                // Index lookups apply to the specified asset only, never its fallbacks.
                if (spriteIndexHint < count && table[spriteIndexHint] != null)
                {
                    spriteIndex = spriteIndexHint;
                    return asset;
                }
                return null;
            }

            if (spriteNameHash != 0)
            {
                for (int i = 0; i < count; i++)
                {
                    var spriteCharacter = table[i];
                    if (spriteCharacter != null && spriteCharacter.name != null && NativeRichTextAssetRegistry.HashName(spriteCharacter.name) == spriteNameHash)
                    {
                        spriteIndex = i;
                        return asset;
                    }
                }
            }

            var fallbacks = asset.fallbackSpriteAssets;
            if (fallbacks == null)
                return null;

            foreach (var fallback in fallbacks)
            {
                var found = FindSprite(fallback, spriteNameHash, spriteIndexHint, out spriteIndex);
                if (found != null)
                    return found;
            }
            return null;
        }

        static TextCoreStyleSheet GetConvertedStyleSheet(TMP_StyleSheet componentSheet, TMP_StyleSheet defaultSheet)
        {
            if (componentSheet == null && defaultSheet == null)
                return null;

            var key = (componentSheet, defaultSheet);
            if (s_ConvertedStyleSheets.TryGetValue(key, out var converted) && converted != null)
                return converted;

            converted = ScriptableObject.CreateInstance<TextCoreStyleSheet>();
            converted.hideFlags = HideFlags.HideAndDontSave;

            // The component sheet is added first so its styles win over the default sheet's on
            // duplicate names, matching the legacy lookup order.
            AppendStyles(converted, componentSheet);
            AppendStyles(converted, defaultSheet);
            converted.RefreshStyles();

            s_ConvertedStyleSheets[key] = converted;
            return converted;
        }

        static void AppendStyles(TextCoreStyleSheet target, TMP_StyleSheet source)
        {
            if (source == null)
                return;

            foreach (var style in source.styles)
            {
                if (style != null)
                    target.styles.Add(new TextCoreStyle(style.name, style.styleOpeningDefinition, style.styleClosingDefinition));
            }
        }

        static TMP_Style GetTextStyle(TMP_StyleSheet componentSheet, int hashCode)
        {
            TMP_Style style = componentSheet != null ? componentSheet.GetStyle(hashCode) : null;

            if (style == null && TMP_Settings.defaultStyleSheet != null)
                style = TMP_Settings.defaultStyleSheet.GetStyle(hashCode);

            return style;
        }

        TMP_Text m_TextComponent;
        TMP_TextInfo m_TextInfo;
        NativeTextGenerationSettings m_NativeSettings = NativeTextGenerationSettings.Default;

        internal bool m_isTextTruncated;
        internal float m_preferredWidth;
        internal float m_preferredHeight;

        // Glyph position scale from native layout units (font points) to component local units.
        float m_PositionScale;

        internal GenerationOutcome GenerateText()
        {
            var textComponent = m_TextComponent;

            if (!ConvertToNativeGenerationSettings(null))
                return GenerationOutcome.NoFontAsset;

            if (m_NativeSettings.textBufferLength == 0)
                return GenerationOutcome.EmptyText;

            if (textComponent.m_TextGenerationInfo == IntPtr.Zero)
                textComponent.m_TextGenerationInfo = TextGenerationInfo.Create(isPermanent: true);

            TextCoreFontAsset.CreateHbFaceIfNeeded();

            bool wasCached = false;
            var nativeTextInfo = GetTextLib().GenerateText(m_NativeSettings, textComponent.m_TextGenerationInfo, ref wasCached);

            m_isTextTruncated = nativeTextInfo.isElided;
            m_preferredWidth = nativeTextInfo.totalWidth / 64.0f * m_PositionScale;
            m_preferredHeight = nativeTextInfo.totalHeight / 64.0f * m_PositionScale;

            ConvertOutput(nativeTextInfo, textComponent.m_TextGenerationInfo);

            return GenerationOutcome.Rendered;
        }

        /// <summary>
        /// Measures the component's text, or <paramref name="textOverride"/> when provided, with
        /// the component's current settings. Negative width/height measure unconstrained.
        /// </summary>
        internal static Vector2 MeasureText(TMP_Text textComponent, float width, float height, string textOverride = null)
        {
            var generator = GetTextGenerator(textComponent, null);

            if (!generator.ConvertToNativeGenerationSettings(textOverride))
                return Vector2.zero;

            if (generator.m_NativeSettings.textBufferLength == 0)
                return Vector2.zero;

            float positionScale = generator.m_PositionScale;
            generator.m_NativeSettings.screenWidth = ToNativeMeasureSize(width, positionScale);

            if (generator.m_NativeSettings.bestFit)
            {
                generator.m_NativeSettings.screenHeight = ToNativeMeasureSize(height, positionScale);
            }
            else
            {
                generator.m_NativeSettings.screenHeight = TextLib.k_unconstrainedScreenSize;
                generator.m_NativeSettings.overflow = TextOverflow.Clip;
            }

            if (textComponent.m_TextGenerationInfo == IntPtr.Zero)
                textComponent.m_TextGenerationInfo = TextGenerationInfo.Create(isPermanent: true);

            TextCoreFontAsset.CreateHbFaceIfNeeded();

            Vector2 size = GetTextLib().MeasureText(generator.m_NativeSettings, textComponent.m_TextGenerationInfo);
            return size * positionScale;
        }

        const float k_MaxNativeMeasureSize = 0x1000000;

        static int ToNativeMeasureSize(float size, float positionScale)
        {
            if (size < 0 || float.IsNaN(size) || float.IsInfinity(size))
                return TextLib.k_unconstrainedScreenSize;

            float pixels = size / positionScale;
            return pixels >= k_MaxNativeMeasureSize ? TextLib.k_unconstrainedScreenSize : Mathf.RoundToInt(pixels * 64.0f);
        }

        bool ConvertToNativeGenerationSettings(string textOverride)
        {
            var textComponent = m_TextComponent;
            var fontAsset = textComponent.font;
            if (fontAsset == null)
                return false;

            fontAsset.EnsureNativeFontAssetIsCreated();
            if (fontAsset.nativeFontAsset == IntPtr.Zero)
                return false;

            m_NativeSettings.fontAsset = fontAsset.nativeFontAsset;

            var textSettings = GetTextSettings();
            UpdateGlobalFallbacks();
            m_NativeSettings.textSettings = textSettings.nativeTextSettings;

            // Native layout runs in font point units (pixels-per-point 1). Output positions are
            // mapped to local units with m_PositionScale, which matches the 0.1 world scale the
            // legacy generator applies to perspective (non orthographic) text.
            m_PositionScale = textComponent.isOrthographic ? 1.0f : 0.1f;
            m_NativeSettings.pixelsPerPointFixed64 = 64;

            m_NativeSettings.fontSize = Mathf.RoundToInt(textComponent.fontSize * 64.0f);
            m_NativeSettings.bestFit = textComponent.enableAutoSizing;
            m_NativeSettings.maxFontSize = Mathf.RoundToInt(textComponent.fontSizeMax * 64.0f);
            m_NativeSettings.minFontSize = Mathf.RoundToInt(textComponent.fontSizeMin * 64.0f);

            var overflowMode = textComponent.overflowMode;
            var rect = textComponent.rectTransform.rect;
            var margin = textComponent.margin;
            float marginWidth = rect.width - margin.x - margin.z;
            float marginHeight = rect.height - margin.y - margin.w;

            m_NativeSettings.overflow = overflowMode == TextOverflowModes.Ellipsis ? TextOverflow.Ellipsis : TextOverflow.Clip;
            m_NativeSettings.screenWidth = Mathf.RoundToInt(Mathf.Max(0, marginWidth) / m_PositionScale * 64.0f);
            m_NativeSettings.screenHeight = Mathf.RoundToInt(Mathf.Max(0, marginHeight) / m_PositionScale * 64.0f);

            var textWrappingMode = textComponent.textWrappingMode;
            m_NativeSettings.wordWrapEnabled = textWrappingMode == TextWrappingModes.Normal || textWrappingMode == TextWrappingModes.PreserveWhitespace;

            // TMP preserves whitespace; only escape sequence parsing is opted in.
            m_NativeSettings.preProcessFlags = textComponent.parseCtrlCharacters ? PreProcessFlags.ParseEscapeSequences : PreProcessFlags.None;

            m_NativeSettings.horizontalAlignment = GetHorizontalAlignment(textComponent.horizontalAlignment);
            m_NativeSettings.verticalAlignment = GetVerticalAlignment(textComponent.verticalAlignment);
            m_NativeSettings.verticalOffset = Mathf.RoundToInt(GetLegacyVerticalAnchorOffset(textComponent.verticalAlignment, margin) / m_PositionScale * 64.0f);

            m_NativeSettings.applyVisibilityLimits = true;
            m_NativeSettings.maxVisibleCharacters = textComponent.maxVisibleCharacters;
            m_NativeSettings.maxVisibleWords = textComponent.maxVisibleWords;
            m_NativeSettings.maxVisibleLines = textComponent.maxVisibleLines;

            // Bold is carried by the font weight in the native settings.
            var fontStyle = textComponent.fontStyle;
            var fontWeight = textComponent.fontWeight;
            m_NativeSettings.fontStyle = (TextCoreFontStyles)fontStyle & ~TextCoreFontStyles.Bold;
            bool isBold = (fontStyle & FontStyles.Bold) == FontStyles.Bold || fontWeight == FontWeight.Bold;
            m_NativeSettings.fontWeight = isBold ? TextFontWeight.Bold : (TextFontWeight)fontWeight;

            bool richText = textComponent.richText;
            m_NativeSettings.color = textComponent.color;
            m_NativeSettings.richTextEnabled = richText;
            m_NativeSettings.languageDirection = textComponent.isRightToLeftText ? LanguageDirection.RTL : LanguageDirection.LTR;

            float characterSpacing = textComponent.characterSpacing;
            m_NativeSettings.characterSpacing = Mathf.RoundToInt(characterSpacing * 64.0f);
            m_NativeSettings.wordSpacing = Mathf.RoundToInt((textComponent.wordSpacing + characterSpacing) * 64.0f);
            m_NativeSettings.paragraphSpacing = Mathf.RoundToInt(textComponent.paragraphSpacing * 64.0f);
            m_NativeSettings.lineSpacing = Mathf.RoundToInt(textComponent.lineSpacing * 64.0f);

            m_NativeSettings.vertexPadding = Mathf.RoundToInt(textComponent.materialPadding * 64.0f);
            m_NativeSettings.disableAdvancedFontFeatures = false;
            m_NativeSettings.hoveredTag = HoveredTag.None;

            // The component-wide text style wraps the whole text, like the legacy generator.
            string styleOpening = null, styleClosing = null;
            if (richText)
            {
                var styleSheet = textComponent.styleSheet;
                int textStyleHashCode = textComponent.activeTextStyleHashCode;
                if (textStyleHashCode != TMP_Style.NormalStyle.hashCode)
                {
                    var textStyle = GetTextStyle(styleSheet, textStyleHashCode);
                    if (textStyle != null)
                    {
                        styleOpening = textStyle.styleOpeningDefinition;
                        styleClosing = textStyle.styleClosingDefinition;
                    }
                }

                GetTextSettings().defaultStyleSheet = GetConvertedStyleSheet(styleSheet, TMP_Settings.defaultStyleSheet);
                s_CurrentSpriteAsset = textComponent.spriteAsset != null ? textComponent.spriteAsset : TMP_Settings.defaultSpriteAsset;
            }

            ref var textBuffer = ref textComponent.m_ATGTextBuffer;
            textBuffer.length = 0;
            AppendToTextBuffer(ref textBuffer, styleOpening);

            if (textOverride == null && textComponent.TryGetSetTextSource(out var setTextSource))
            {
                AppendToTextBuffer(ref textBuffer, setTextSource);
            }
            else
            {
                string text = textOverride ?? textComponent.text ?? string.Empty;
                var textPreprocessor = textComponent.textPreprocessor;
                if (textPreprocessor != null)
                    text = textPreprocessor.PreprocessText(text) ?? string.Empty;
                AppendToTextBuffer(ref textBuffer, text);
            }

            AppendToTextBuffer(ref textBuffer, styleClosing);
            m_NativeSettings.SetTextBuffer(textBuffer.buffer, textBuffer.length);

            // Load the assets referenced by <font>/<sprite> tags on the main thread before the
            // native parser resolves them.
            if (richText)
                NativeRichTextAssetRegistry.PreloadAssetsFromTags(textComponent.m_ATGTextBuffer, GetTextSettings());

            return true;
        }

        static void AppendToTextBuffer(ref NativeTextBuffer buffer, ReadOnlySpan<char> chars)
        {
            if (chars.Length == 0)
                return;

            int offset = buffer.length;
            buffer.EnsureCapacity(offset + chars.Length, preserveContent: true);
            for (int i = 0; i < chars.Length; i++)
                buffer[offset + i] = chars[i];
            buffer.length = offset + chars.Length;
        }

        // The text backing array stores UTF-16 code units widened to uint.
        static void AppendToTextBuffer(ref NativeTextBuffer buffer, ReadOnlySpan<uint> chars)
        {
            if (chars.Length == 0)
                return;

            int offset = buffer.length;
            buffer.EnsureCapacity(offset + chars.Length, preserveContent: true);
            for (int i = 0; i < chars.Length; i++)
                buffer[offset + i] = (char)chars[i];
            buffer.length = offset + chars.Length;
        }

        static HorizontalAlignment GetHorizontalAlignment(HorizontalAlignmentOptions alignment)
        {
            switch (alignment)
            {
                case HorizontalAlignmentOptions.Center:
                case HorizontalAlignmentOptions.Geometry:
                    return HorizontalAlignment.Center;
                case HorizontalAlignmentOptions.Right:
                    return HorizontalAlignment.Right;
                case HorizontalAlignmentOptions.Justified:
                    return HorizontalAlignment.Justified;
                case HorizontalAlignmentOptions.Flush:
                    return HorizontalAlignment.Flush;
                default:
                    return HorizontalAlignment.Left;
            }
        }

        static void SetStylePadding(MeshGroup group, float normalStyle, float boldStyle)
        {
            var material = group.material;
            if (material == null || !material.HasProperty(ShaderUtilities.ID_GradientScale) || !material.HasProperty(ShaderUtilities.ID_ScaleRatio_A))
                return;

            float gradientScale = material.GetFloat(ShaderUtilities.ID_GradientScale);
            float stylePaddingScale = 0.25f * gradientScale * material.GetFloat(ShaderUtilities.ID_ScaleRatio_A);

            group.gradientScale = gradientScale;
            group.normalStylePadding = normalStyle * stylePaddingScale;
            group.boldStylePadding = boldStyle * stylePaddingScale;
        }

        static void ApplyStylePadding(ref Vector3 bottomLeft, ref Vector3 topLeft, ref Vector3 topRight, ref Vector3 bottomRight, GlyphMetrics metrics, float padding, float glyphPadding, float stylePadding)
        {
            float paddedWidth = metrics.width + 2 * padding;
            float paddedHeight = metrics.height + 2 * padding;
            if (paddedWidth <= 0 || paddedHeight <= 0)
                return;

            Vector3 widthStep = (topRight - topLeft) / paddedWidth;
            Vector3 heightStep = (topLeft - bottomLeft) / paddedHeight;
            Vector3 widthAxis = widthStep.normalized;
            Vector3 shearStep = widthAxis * Vector3.Dot(heightStep, widthAxis);

            Vector3 horizontal = widthStep * (glyphPadding + stylePadding - padding);
            Vector3 vertical = heightStep * (glyphPadding - padding) + shearStep * stylePadding;

            bottomLeft += -horizontal - vertical;
            topLeft += -horizontal + vertical;
            topRight += horizontal + vertical;
            bottomRight += horizontal - vertical;
        }

        static VerticalAlignment GetVerticalAlignment(VerticalAlignmentOptions alignment)
        {
            switch (alignment)
            {
                case VerticalAlignmentOptions.Top:
                    return VerticalAlignment.Top;
                case VerticalAlignmentOptions.Bottom:
                    return VerticalAlignment.Bottom;
                case VerticalAlignmentOptions.Baseline:
                    return VerticalAlignment.Baseline;
                case VerticalAlignmentOptions.Geometry:
                    return VerticalAlignment.Midline;
                case VerticalAlignmentOptions.Capline:
                    return VerticalAlignment.Capline;
                default:
                    return VerticalAlignment.Middle;
            }
        }

        static float GetLegacyVerticalAnchorOffset(VerticalAlignmentOptions alignment, Vector4 margin)
        {
            switch (alignment)
            {
                case VerticalAlignmentOptions.Baseline:
                    return (margin.w - margin.y) * 0.5f;
                case VerticalAlignmentOptions.Capline:
                    return -margin.y;
                default:
                    return 0;
            }
        }

        class MeshGroup
        {
            public Material material;
            public TMP_FontAsset tmpFontAsset;
            public TextCoreFontAsset coreFontAsset;
            public TMP_SpriteAsset tmpSpriteAsset;
            public bool isColorFont;
            public float inverseAtlasWidth;
            public float inverseAtlasHeight;
            public float padding;
            public float gradientScale = float.PositiveInfinity;
            public float normalStylePadding;
            public float boldStylePadding;
            public readonly List<(int mesh, int element)> elements = new List<(int, int)>();
        }

        readonly List<MeshGroup> m_MeshGroups = new List<MeshGroup>();
        RangeInt[] m_Words;
        readonly Dictionary<EntityId, int> m_MaterialToGroupIndex = new Dictionary<EntityId, int>();

        void ConvertOutput(NativeTextInfo nativeTextInfo, IntPtr textGenerationInfo)
        {
            var textInfo = m_TextInfo;
            textInfo.textComponent = m_TextComponent;

            ResolveMissingGlyphs(ref nativeTextInfo);
            BuildMeshGroups(ref nativeTextInfo);

            int materialCount = m_MeshGroups.Count;
            textInfo.materialCount = materialCount;

            if (textInfo.meshInfo.Length < materialCount)
                TMP_TextInfo.Resize(ref textInfo.meshInfo, materialCount, false);

            // Top left corner of the text area in local space. Corners are ordered BL, TL, TR, BR.
            var componentMargin = m_TextComponent.margin;
            Vector3 textAreaTopLeft = m_TextComponent.GetTextContainerLocalCornersInternal()[1] + new Vector3(componentMargin.x, -componentMargin.y, 0);

            Span<ATGMeshInfo> meshInfos = nativeTextInfo.meshInfos;
            float padding = m_NativeSettings.vertexPadding / 64.0f;
            bool convertToLinearSpace = m_TextComponent.GetConvertToLinearSpace();
            var sharedMaterial = m_TextComponent.fontSharedMaterial;
            bool isSdfShader = sharedMaterial != null && sharedMaterial.HasProperty(ShaderUtilities.ID_WeightNormal);

            // Per (mesh, element) mapping to the group index and quad slot, used to fill character info.
            var elementGroup = new int[meshInfos.Length][];
            var elementSlot = new int[meshInfos.Length][];
            for (int i = 0; i < meshInfos.Length; i++)
            {
                int elementCount = meshInfos[i].textElementInfos.Length;
                elementGroup[i] = new int[elementCount];
                elementSlot[i] = new int[elementCount];
                for (int j = 0; j < elementCount; j++)
                    elementGroup[i][j] = -1;
            }

            for (int groupIndex = 0; groupIndex < materialCount; groupIndex++)
            {
                var group = m_MeshGroups[groupIndex];
                int quadCount = Mathf.Min(group.elements.Count, TMP_Math.MAX_QUADS_PER_MESH);

                ref var meshInfo = ref textInfo.meshInfo[groupIndex];
                int requiredCapacity = Mathf.NextPowerOfTwo(Mathf.Max(quadCount, 1));
                if (meshInfo.vertices == null)
                {
                    var mesh = meshInfo.mesh;
                    meshInfo = new TMP_MeshInfo(requiredCapacity);
                    meshInfo.mesh = mesh;
                }
                else if (meshInfo.vertices.Length < quadCount * 4)
                {
                    meshInfo.ResizeMeshInfo(requiredCapacity);
                }

                meshInfo.material = group.material;
                meshInfo.vertexCount = quadCount * 4;

                float xScale = m_TextComponent.fontSize / GetSamplingPointSize(group) * m_PositionScale * m_TextComponent.GetCharacterScaleFactor();

                // Sprite quads are not SDF rendered and carry no padding.
                float groupPadding = group.tmpSpriteAsset != null ? 0 : padding;

                for (int slot = 0; slot < quadCount; slot++)
                {
                    var (meshIndex, elementIndex) = group.elements[slot];
                    ref var element = ref meshInfos[meshIndex].textElementInfos[elementIndex];

                    elementGroup[meshIndex][elementIndex] = groupIndex;
                    elementSlot[meshIndex][elementIndex] = slot;

                    Glyph glyph = GetGlyph(group, (uint)element.glyphID);
                    FillQuad(ref meshInfo, slot, ref element, glyph, group, textAreaTopLeft, groupPadding, xScale, convertToLinearSpace, isSdfShader);
                }

                // Degenerate any stale quads left from a previous, larger generation.
                meshInfo.ClearUnusedVertices(meshInfo.vertexCount);
            }

            FillTextInfo(nativeTextInfo, textGenerationInfo, textAreaTopLeft, elementGroup, elementSlot);
        }

        // Material reference used by the components to create and configure sub text objects.
        internal MaterialReference GetMaterialReference(int index)
        {
            var group = m_MeshGroups[index];
            var materialReference = new MaterialReference();
            materialReference.index = index;
            materialReference.material = group.material;
            materialReference.fontAsset = group.tmpFontAsset;
            materialReference.spriteAsset = group.tmpSpriteAsset;
            materialReference.isDefaultMaterial = true;
            return materialReference;
        }

        float GetSamplingPointSize(MeshGroup group)
        {
            float pointSize = group.tmpFontAsset != null ? group.tmpFontAsset.faceInfo.pointSize : m_TextComponent.font.faceInfo.pointSize;
            return Mathf.Max(pointSize, 1);
        }

        void FillQuad(ref TMP_MeshInfo meshInfo, int slot, ref UnityEngine.TextCore.Text.NativeTextElementInfo element, Glyph glyph, MeshGroup group, Vector3 textAreaTopLeft, float padding, float xScale, bool convertToLinearSpace, bool isSdfShader)
        {
            int vertexIndex = slot * 4;
            GlyphRect glyphRect = glyph != null ? glyph.glyphRect : default;

            Vector3 bottomLeft = ToLocalPosition(element.bottomLeft.position, textAreaTopLeft);
            Vector3 topLeft = ToLocalPosition(element.topLeft.position, textAreaTopLeft);
            Vector3 topRight = ToLocalPosition(element.topRight.position, textAreaTopLeft);
            Vector3 bottomRight = ToLocalPosition(element.bottomRight.position, textAreaTopLeft);

            // The native bold flag is carried in uv2.y and the glyph scale relative to the text (face scale,
            // sub/superscript, small caps, size tags) in uv2.x; TMP shaders expect the SDF scale in uv0.w,
            // negated for bold.
            bool isSyntheticBold = element.bottomLeft.uv2.y != 0;
            float glyphScale = element.bottomLeft.uv2.x > 0 ? element.bottomLeft.uv2.x : 1;
            float signedScale = (isSyntheticBold ? -xScale : xScale) * glyphScale;

            float stylePadding = 0;
            float glyphPadding = padding;
            if (glyph != null)
            {
                glyphPadding = group.padding;
                stylePadding = isSyntheticBold ? group.boldStylePadding : group.normalStylePadding;
                if (stylePadding + glyphPadding > group.gradientScale)
                    glyphPadding = group.gradientScale - stylePadding;

                ApplyStylePadding(ref bottomLeft, ref topLeft, ref topRight, ref bottomRight, glyph.metrics, padding, glyphPadding, stylePadding);

                if (!isSdfShader)
                    stylePadding = 0;
            }

            meshInfo.vertices[vertexIndex + 0] = bottomLeft;
            meshInfo.vertices[vertexIndex + 1] = topLeft;
            meshInfo.vertices[vertexIndex + 2] = topRight;
            meshInfo.vertices[vertexIndex + 3] = bottomRight;

            Vector2 uvBL, uvTL, uvTR, uvBR;
            ComputeAtlasUVs(ref element, glyphRect, group, glyphPadding + stylePadding, out uvBL, out uvTL, out uvTR, out uvBR);

            meshInfo.uvs0[vertexIndex + 0] = new Vector4(uvBL.x, uvBL.y, 0, signedScale);
            meshInfo.uvs0[vertexIndex + 1] = new Vector4(uvTL.x, uvTL.y, 0, signedScale);
            meshInfo.uvs0[vertexIndex + 2] = new Vector4(uvTR.x, uvTR.y, 0, signedScale);
            meshInfo.uvs0[vertexIndex + 3] = new Vector4(uvBR.x, uvBR.y, 0, signedScale);

            meshInfo.uvs2[vertexIndex + 0] = new Vector2(0, 0);
            meshInfo.uvs2[vertexIndex + 1] = new Vector2(0, 1);
            meshInfo.uvs2[vertexIndex + 2] = new Vector2(1, 1);
            meshInfo.uvs2[vertexIndex + 3] = new Vector2(1, 0);

            if (group.isColorFont)
            {
                meshInfo.colors32[vertexIndex + 0] = new Color32(255, 255, 255, element.bottomLeft.color.a);
                meshInfo.colors32[vertexIndex + 1] = new Color32(255, 255, 255, element.topLeft.color.a);
                meshInfo.colors32[vertexIndex + 2] = new Color32(255, 255, 255, element.topRight.color.a);
                meshInfo.colors32[vertexIndex + 3] = new Color32(255, 255, 255, element.bottomRight.color.a);
            }
            else
            {
                meshInfo.colors32[vertexIndex + 0] = convertToLinearSpace ? element.bottomLeft.color.GammaToLinear() : element.bottomLeft.color;
                meshInfo.colors32[vertexIndex + 1] = convertToLinearSpace ? element.topLeft.color.GammaToLinear() : element.topLeft.color;
                meshInfo.colors32[vertexIndex + 2] = convertToLinearSpace ? element.topRight.color.GammaToLinear() : element.topRight.color;
                meshInfo.colors32[vertexIndex + 3] = convertToLinearSpace ? element.bottomRight.color.GammaToLinear() : element.bottomRight.color;
            }
        }

        Vector3 ToLocalPosition(Vector3 nativePosition, Vector3 textAreaTopLeft)
        {
            return new Vector3(
                textAreaTopLeft.x + nativePosition.x * m_PositionScale,
                textAreaTopLeft.y - nativePosition.y * m_PositionScale,
                0);
        }

        static void ComputeAtlasUVs(ref UnityEngine.TextCore.Text.NativeTextElementInfo element, GlyphRect glyphRect, MeshGroup group, float padding, out Vector2 bl, out Vector2 tl, out Vector2 tr, out Vector2 br)
        {
            float xMin = (glyphRect.x - padding) * group.inverseAtlasWidth;
            float yMin = (glyphRect.y - padding) * group.inverseAtlasHeight;
            float xMax = (glyphRect.x + glyphRect.width + padding) * group.inverseAtlasWidth;
            float yMax = (glyphRect.y + glyphRect.height + padding) * group.inverseAtlasHeight;

            // Native UVs are 0/1 corners for regular glyphs, and fractional for decoration quads
            // (e.g. underline) that only cover part of the glyph; remap those into the glyph rect.
            Vector2 uv0 = element.bottomLeft.uv0;
            Vector2 uv1 = element.topLeft.uv0;
            Vector2 uv2 = element.topRight.uv0;
            Vector2 uv3 = element.bottomRight.uv0;

            bl = new Vector2(Mathf.Lerp(xMin, xMax, uv0.x), Mathf.Lerp(yMin, yMax, uv0.y));
            tl = new Vector2(Mathf.Lerp(xMin, xMax, uv1.x), Mathf.Lerp(yMin, yMax, uv1.y));
            tr = new Vector2(Mathf.Lerp(xMin, xMax, uv2.x), Mathf.Lerp(yMin, yMax, uv2.y));
            br = new Vector2(Mathf.Lerp(xMin, xMax, uv3.x), Mathf.Lerp(yMin, yMax, uv3.y));
        }

        Glyph GetGlyph(MeshGroup group, uint glyphID)
        {
            if (group.tmpSpriteAsset != null)
            {
                int spriteIndex = (int)glyphID - 0xE000;
                var table = group.tmpSpriteAsset.spriteCharacterTable;
                if (spriteIndex >= 0 && spriteIndex < table.Count && table[spriteIndex] != null)
                    return table[spriteIndex].glyph;

                return null;
            }

            if (group.tmpFontAsset != null && group.tmpFontAsset.glyphLookupTable.TryGetValue(glyphID, out var glyph))
                return glyph;

            if (group.coreFontAsset != null)
                return group.coreFontAsset.GetGlyphInCache(glyphID);

            return null;
        }

        void ResolveMissingGlyphs(ref NativeTextInfo nativeTextInfo)
        {
            foreach (var missingGlyphs in s_MissingGlyphsPerFontAsset.Values)
            {
                missingGlyphs.Clear();
            }

            // Remap native OS fallback fonts (created during shaping) to their managed twins.
            OSFontFallbackResolver.Resolve(nativeTextInfo, s_MissingGlyphsPerFontAsset);

            Span<ATGMeshInfo> meshInfos = nativeTextInfo.meshInfos;
            foreach (ref var meshInfo in meshInfos)
            {
                var textAsset = Resources.EntityIdToObject(meshInfo.textAssetId);
                var tmpFontAsset = textAsset as TMP_FontAsset;
                var coreFontAsset = textAsset as TextCoreFontAsset;
                if (tmpFontAsset == null && coreFontAsset == null)
                    continue;

                var elements = meshInfo.textElementInfos;
                for (int i = 0; i < elements.Length; i++)
                {
                    uint glyphID = (uint)elements[i].glyphID;
                    bool isGlyphInAtlas = tmpFontAsset != null
                        ? tmpFontAsset.glyphLookupTable.ContainsKey(glyphID)
                        : coreFontAsset.GetGlyphInCache(glyphID) != null;
                    if (isGlyphInAtlas)
                        continue;

                    if (!s_MissingGlyphsPerFontAsset.TryGetValue(meshInfo.textAssetId, out var missingGlyphs))
                    {
                        missingGlyphs = new HashSet<uint>();
                        s_MissingGlyphsPerFontAsset.Add(meshInfo.textAssetId, missingGlyphs);
                    }
                    missingGlyphs.Add(glyphID);
                }
            }

            foreach (var entry in s_MissingGlyphsPerFontAsset)
            {
                if (entry.Value.Count == 0)
                    continue;

                s_GlyphsToAdd.Clear();
                s_GlyphsToAdd.AddRange(entry.Value);

                var textAsset = Resources.EntityIdToObject(entry.Key);
                if (textAsset is TMP_FontAsset tmpFontAsset)
                {
                    tmpFontAsset.TryAddGlyphs(s_GlyphsToAdd);
                }
                else if (textAsset is TextCoreFontAsset coreFontAsset)
                {
                    coreFontAsset.TryAddGlyphs(s_GlyphsToAdd, populateFontFeatures: false);
                }
            }

            TextCoreFontAsset.UpdateFontAssetsInUpdateQueue();
            TMP_FontAsset.UpdateFontAssetsInUpdateQueue();
        }

        void BuildMeshGroups(ref NativeTextInfo nativeTextInfo)
        {
            m_MeshGroups.Clear();
            m_MaterialToGroupIndex.Clear();

            // Group 0 is always the component's own font so the main mesh keeps rendering with
            // the component's shared material, even when it ends up with no quads.
            var mainFontAsset = m_TextComponent.font;
            var sharedMaterial = m_TextComponent.fontSharedMaterial;
            var mainMaterial = sharedMaterial != null ? sharedMaterial : mainFontAsset.material;
            AddMeshGroup(mainMaterial, mainFontAsset, null, null);

            Span<ATGMeshInfo> meshInfos = nativeTextInfo.meshInfos;
            for (int meshIndex = 0; meshIndex < meshInfos.Length; meshIndex++)
            {
                ref var meshInfo = ref meshInfos[meshIndex];
                var textAsset = Resources.EntityIdToObject(meshInfo.textAssetId);

                var tmpFontAsset = textAsset as TMP_FontAsset;
                var coreFontAsset = textAsset as TextCoreFontAsset;
                var tmpSpriteAsset = textAsset as TMP_SpriteAsset;

                if (tmpFontAsset == null && coreFontAsset == null && tmpSpriteAsset == null)
                    continue;

                var elements = meshInfo.textElementInfos;
                for (int elementIndex = 0; elementIndex < elements.Length; elementIndex++)
                {
                    uint glyphID = (uint)elements[elementIndex].glyphID;

                    int atlasIndex;
                    if (tmpSpriteAsset != null)
                    {
                        // Sprite characters live in the Private Use Area at 0xE000 + sprite index.
                        int spriteIndex = (int)glyphID - 0xE000;
                        if (spriteIndex < 0 || spriteIndex >= tmpSpriteAsset.spriteCharacterTable.Count)
                            continue;
                        atlasIndex = 0;
                    }
                    else if (tmpFontAsset != null)
                    {
                        if (!tmpFontAsset.glyphLookupTable.TryGetValue(glyphID, out var glyph))
                            continue;
                        atlasIndex = glyph.atlasIndex;
                    }
                    else
                    {
                        var glyph = coreFontAsset.GetGlyphInCache(glyphID);
                        if (glyph == null)
                            continue;
                        atlasIndex = glyph.atlasIndex;
                    }

                    Material material = tmpSpriteAsset != null ? tmpSpriteAsset.material : ResolveMaterial(tmpFontAsset, coreFontAsset, atlasIndex);
                    if (material == null)
                        continue;

                    if (!m_MaterialToGroupIndex.TryGetValue(material.GetEntityId(), out int groupIndex))
                        groupIndex = AddMeshGroup(material, tmpFontAsset, coreFontAsset, tmpSpriteAsset);

                    if (m_MeshGroups[groupIndex].elements.Count >= TMP_Math.MAX_QUADS_PER_MESH)
                        groupIndex = AddMeshGroup(material, tmpFontAsset, coreFontAsset, tmpSpriteAsset);

                    m_MeshGroups[groupIndex].elements.Add((meshIndex, elementIndex));
                }
            }
        }

        int AddMeshGroup(Material material, TMP_FontAsset tmpFontAsset, TextCoreFontAsset coreFontAsset, TMP_SpriteAsset tmpSpriteAsset)
        {
            var group = new MeshGroup { material = material, tmpFontAsset = tmpFontAsset, coreFontAsset = coreFontAsset, tmpSpriteAsset = tmpSpriteAsset };
            if (tmpSpriteAsset != null)
            {
                var spriteSheet = tmpSpriteAsset.spriteSheet;
                group.inverseAtlasWidth = spriteSheet != null ? 1.0f / spriteSheet.width : 0;
                group.inverseAtlasHeight = spriteSheet != null ? 1.0f / spriteSheet.height : 0;
            }
            else if (tmpFontAsset != null)
            {
                group.isColorFont = ((GlyphRasterModes)tmpFontAsset.atlasRenderMode & GlyphRasterModes.RASTER_MODE_COLOR) == GlyphRasterModes.RASTER_MODE_COLOR;
                group.inverseAtlasWidth = 1.0f / tmpFontAsset.atlasWidth;
                group.inverseAtlasHeight = 1.0f / tmpFontAsset.atlasHeight;
                group.padding = GetMaterialPadding(material);
                SetStylePadding(group, tmpFontAsset.normalStyle, tmpFontAsset.boldStyle);
            }
            else if (coreFontAsset != null)
            {
                group.isColorFont = coreFontAsset.IsColor();
                group.inverseAtlasWidth = 1.0f / coreFontAsset.atlasWidth;
                group.inverseAtlasHeight = 1.0f / coreFontAsset.atlasHeight;
                group.padding = GetMaterialPadding(material);
                SetStylePadding(group, coreFontAsset.regularStyleWeight, coreFontAsset.boldStyleWeight);
            }

            m_MeshGroups.Add(group);
            m_MaterialToGroupIndex[material.GetEntityId()] = m_MeshGroups.Count - 1;
            return m_MeshGroups.Count - 1;
        }

        float GetMaterialPadding(Material material)
        {
            if (material == m_TextComponent.fontSharedMaterial)
                return m_TextComponent.materialPadding;

            return ShaderUtilities.GetPadding(material, m_TextComponent.extraPadding, m_TextComponent.isUsingBold);
        }

        Material ResolveMaterial(TMP_FontAsset tmpFontAsset, TextCoreFontAsset coreFontAsset, int atlasIndex)
        {
            if (tmpFontAsset != null)
            {
                var sharedMaterial = m_TextComponent.fontSharedMaterial;
                Material baseMaterial;
                if (tmpFontAsset == m_TextComponent.font)
                    baseMaterial = sharedMaterial != null ? sharedMaterial : tmpFontAsset.material;
                else if (TMP_Settings.matchMaterialPreset && sharedMaterial != null)
                    baseMaterial = TMP_MaterialManager.GetFallbackMaterial(sharedMaterial, tmpFontAsset.material);
                else
                    baseMaterial = tmpFontAsset.material;

                return atlasIndex == 0 ? baseMaterial : TMP_MaterialManager.GetFallbackMaterial(tmpFontAsset, baseMaterial, atlasIndex);
            }

            var bridgeMaterial = GetBridgeMaterial(coreFontAsset, atlasIndex);
            var componentMaterial = m_TextComponent.fontSharedMaterial;
            if (TMP_Settings.matchMaterialPreset && componentMaterial != null && bridgeMaterial != null)
                return TMP_MaterialManager.GetFallbackMaterial(componentMaterial, bridgeMaterial);

            return bridgeMaterial;
        }

        // Materials rendering TextCore font assets (OS/emoji fallbacks) with TMP shaders so that
        // uGUI masking and TMP visual conventions keep working.
        static Material GetBridgeMaterial(TextCoreFontAsset fontAsset, int atlasIndex)
        {
            var key = (fontAsset.GetEntityId(), atlasIndex);
            if (s_BridgeMaterials.TryGetValue(key, out var material) && material != null)
                return material;

            if (atlasIndex >= fontAsset.atlasTextures.Length)
                return null;

            var texture = fontAsset.atlasTextures[atlasIndex];

            if (fontAsset.IsColor() || fontAsset.IsBitmap())
            {
                var spriteShader = Shader.Find("TextMeshPro/Sprite");
                if (spriteShader == null)
                    return null;

                material = new Material(spriteShader);
            }
            else
            {
                if (ShaderUtilities.ShaderRef_MobileSDF == null)
                    return null;

                material = new Material(ShaderUtilities.ShaderRef_MobileSDF);
                material.SetFloat(ShaderUtilities.ID_GradientScale, fontAsset.atlasPadding + 1);
                material.SetFloat(ShaderUtilities.ID_WeightNormal, fontAsset.regularStyleWeight);
                material.SetFloat(ShaderUtilities.ID_WeightBold, fontAsset.boldStyleWeight);
            }

            material.SetTexture(ShaderUtilities.ID_MainTex, texture);
            material.SetFloat(ShaderUtilities.ID_TextureWidth, fontAsset.atlasWidth);
            material.SetFloat(ShaderUtilities.ID_TextureHeight, fontAsset.atlasHeight);
            material.hideFlags = HideFlags.HideAndDontSave;

            s_BridgeMaterials[key] = material;
            return material;
        }

        void FillTextInfo(NativeTextInfo nativeTextInfo, IntPtr textGenerationInfo, Vector3 textAreaTopLeft, int[][] elementGroup, int[][] elementSlot)
        {
            var textInfo = m_TextInfo;

            string parsedText = TextGenerationInfo.GetParsedText(textGenerationInfo);
            int characterCount = parsedText.Length;
            int lineCount = Mathf.Max(TextGenerationInfo.GetLineCount(textGenerationInfo), 1);
            int glyphCount = TextGenerationInfo.GetGlyphCount(textGenerationInfo);

            textInfo.characterCount = characterCount;
            textInfo.lineCount = lineCount;
            textInfo.pageCount = 1;
            textInfo.wordCount = 0;
            textInfo.linkCount = 0;
            textInfo.spaceCount = 0;
            textInfo.spriteCount = 0;

            if (textInfo.characterInfo == null || textInfo.characterInfo.Length < characterCount)
                TMP_TextInfo.Resize(ref textInfo.characterInfo, characterCount, true);

            var componentFontAsset = m_TextComponent.font;
            var componentMaterial = m_TextComponent.fontSharedMaterial;
            float componentPointSize = m_TextComponent.fontSize;
            Color32 componentColor = m_TextComponent.color;

            for (int i = 0; i < characterCount; i++)
            {
                ref var characterInfo = ref textInfo.characterInfo[i];
                characterInfo = default;
                characterInfo.character = parsedText[i];
                characterInfo.index = i;
                characterInfo.stringLength = 1;
                characterInfo.elementType = TMP_TextElementType.Character;
                characterInfo.fontAsset = componentFontAsset;
                characterInfo.material = componentMaterial;
                characterInfo.pointSize = componentPointSize;
                characterInfo.color = componentColor;

                if (char.IsWhiteSpace(parsedText[i]))
                    textInfo.spaceCount += 1;
            }

            if (textInfo.lineInfo == null || textInfo.lineInfo.Length < lineCount)
                TMP_TextInfo.Resize(ref textInfo.lineInfo, lineCount, true);

            for (int lineIndex = 0; lineIndex < lineCount; lineIndex++)
            {
                ref var lineInfo = ref textInfo.lineInfo[lineIndex];
                lineInfo = default;

                float lineHeight = TextGenerationInfo.GetLineHeight(textGenerationInfo, lineIndex) * m_PositionScale;
                float ascender = TextGenerationInfo.GetLineAscender(textGenerationInfo, lineIndex) * m_PositionScale;
                float baselineY = TextGenerationInfo.GetLineBaselineY(textGenerationInfo, lineIndex) * m_PositionScale;

                float baseline = textAreaTopLeft.y - baselineY;
                lineInfo.baseline = baseline;
                lineInfo.ascender = baseline + ascender;
                lineInfo.descender = baseline - Mathf.Max(lineHeight - ascender, 0);
                lineInfo.lineHeight = lineHeight;
                lineInfo.firstCharacterIndex = int.MaxValue;
                lineInfo.lastCharacterIndex = 0;
            }

            var meshInfos = nativeTextInfo.meshInfos;

            int wordCount = TextGenerationInfo.GetWords(textGenerationInfo, ref m_Words);

            for (int glyphIndex = 0; glyphIndex < glyphCount; glyphIndex++)
            {
                var renderInfo = TextGenerationInfo.GetGlyphRenderInfo(textGenerationInfo, glyphIndex);

                int meshIndex = renderInfo.meshIndex;
                int elementIndex = renderInfo.textElementInfoIndex;
                if (meshIndex < 0 || meshIndex >= elementGroup.Length || elementIndex < 0 || elementIndex >= elementGroup[meshIndex].Length)
                    continue;

                int groupIndex = elementGroup[meshIndex][elementIndex];
                int characterIndex = renderInfo.textRangeStart;

                if (renderInfo.lineIndex >= 0 && renderInfo.lineIndex < lineCount && characterIndex >= 0)
                {
                    ref var lineInfo = ref textInfo.lineInfo[renderInfo.lineIndex];
                    int lastIndex = Mathf.Min(characterIndex + Mathf.Max(renderInfo.textRangeLength, 1) - 1, characterCount - 1);
                    lineInfo.firstCharacterIndex = Mathf.Min(lineInfo.firstCharacterIndex, characterIndex);
                    lineInfo.lastCharacterIndex = Mathf.Max(lineInfo.lastCharacterIndex, lastIndex);
                }

                if (renderInfo.kind == NativeGlyphKind.Sprite)
                    textInfo.spriteCount += 1;

                if (characterIndex < 0 || characterIndex >= characterCount)
                    continue;

                ref var characterInfo = ref textInfo.characterInfo[characterIndex];
                characterInfo.stringLength = Mathf.Max(renderInfo.textRangeLength, 1);
                characterInfo.lineNumber = Mathf.Clamp(renderInfo.lineIndex, 0, lineCount - 1);
                characterInfo.elementType = renderInfo.kind == NativeGlyphKind.Sprite ? TMP_TextElementType.Sprite : TMP_TextElementType.Character;

                if (groupIndex < 0)
                    continue;

                var group = m_MeshGroups[groupIndex];
                int slot = elementSlot[meshIndex][elementIndex];
                ref var element = ref meshInfos[meshIndex].textElementInfos[elementIndex];

                bool isInvisibleCharacter = char.IsWhiteSpace(characterInfo.character) || characterInfo.character == k_SoftHyphen;
                bool isHiddenByVisibilityLimit = renderInfo.isHiddenByVisibilityLimit;
                characterInfo.isVisible = !isInvisibleCharacter && !isHiddenByVisibilityLimit;
                characterInfo.materialReferenceIndex = groupIndex;
                characterInfo.vertexIndex = slot * 4;

                // Like the legacy generator: keep the quad slot, zero its vertices.
                if (isHiddenByVisibilityLimit)
                    CollapseQuad(ref textInfo.meshInfo[groupIndex], slot);
                characterInfo.fontAsset = group.tmpFontAsset;
                characterInfo.material = group.material;
                characterInfo.color = element.bottomLeft.color;

                characterInfo.bottomLeft = ToLocalPosition(element.bottomLeft.position, textAreaTopLeft);
                characterInfo.topLeft = ToLocalPosition(element.topLeft.position, textAreaTopLeft);
                characterInfo.topRight = ToLocalPosition(element.topRight.position, textAreaTopLeft);
                characterInfo.bottomRight = ToLocalPosition(element.bottomRight.position, textAreaTopLeft);

                characterInfo.origin = characterInfo.bottomLeft.x;
                characterInfo.xAdvance = characterInfo.bottomRight.x;
                characterInfo.scale = componentPointSize / GetSamplingPointSize(group);

                if (characterInfo.lineNumber < lineCount)
                {
                    ref var lineInfo = ref textInfo.lineInfo[characterInfo.lineNumber];
                    characterInfo.baseLine = lineInfo.baseline;
                    characterInfo.ascender = lineInfo.ascender;
                    characterInfo.descender = lineInfo.descender;
                }
            }

            for (int lineIndex = 0; lineIndex < lineCount; lineIndex++)
            {
                ref var lineInfo = ref textInfo.lineInfo[lineIndex];
                if (lineInfo.firstCharacterIndex == int.MaxValue)
                    lineInfo.firstCharacterIndex = 0;

                lineInfo.characterCount = lineInfo.lastCharacterIndex - lineInfo.firstCharacterIndex + 1;
                lineInfo.firstVisibleCharacterIndex = lineInfo.firstCharacterIndex;
                lineInfo.lastVisibleCharacterIndex = lineInfo.lastCharacterIndex;
            }

            if (textInfo.pageInfo == null || textInfo.pageInfo.Length < 1)
                textInfo.pageInfo = new TMP_PageInfo[1];

            textInfo.pageInfo[0].firstCharacterIndex = 0;
            textInfo.pageInfo[0].lastCharacterIndex = Mathf.Max(characterCount - 1, 0);

            FillWordInfo(wordCount, characterCount);
        }

        static void CollapseQuad(ref TMP_MeshInfo meshInfo, int slot)
        {
            int vertexIndex = slot * 4;
            if (meshInfo.vertices == null || vertexIndex + 3 >= meshInfo.vertices.Length)
                return;

            meshInfo.vertices[vertexIndex + 0] = Vector3.zero;
            meshInfo.vertices[vertexIndex + 1] = Vector3.zero;
            meshInfo.vertices[vertexIndex + 2] = Vector3.zero;
            meshInfo.vertices[vertexIndex + 3] = Vector3.zero;
        }

        // Native word segmentation: unlike the legacy generator, punctuation splits words and CJK is segmented.
        void FillWordInfo(int wordCount, int characterCount)
        {
            var textInfo = m_TextInfo;
            var words = m_Words;

            if (textInfo.wordInfo == null || textInfo.wordInfo.Length < Mathf.Max(wordCount, 1))
                TMP_TextInfo.Resize(ref textInfo.wordInfo, Mathf.Max(wordCount, 1));

            int filledWordCount = 0;
            for (int i = 0; i < wordCount; i++)
            {
                int start = words[i].start;
                int length = words[i].length;
                if (length <= 0 || start >= characterCount)
                    continue;

                int last = Mathf.Min(start + length - 1, characterCount - 1);
                ref var wordInfo = ref textInfo.wordInfo[filledWordCount++];
                wordInfo.firstCharacterIndex = start;
                wordInfo.lastCharacterIndex = last;
                wordInfo.characterCount = last - start + 1;
                wordInfo.textComponent = m_TextComponent;

                int lineNumber = textInfo.characterInfo[start].lineNumber;
                if (lineNumber >= 0 && lineNumber < textInfo.lineCount)
                    textInfo.lineInfo[lineNumber].wordCount += 1;
            }

            // Like the legacy generator, no words reports one empty word (GetWord reads its textComponent).
            if (filledWordCount == 0)
            {
                ref var emptyWord = ref textInfo.wordInfo[0];
                emptyWord.firstCharacterIndex = 0;
                emptyWord.lastCharacterIndex = -1;
                emptyWord.characterCount = 0;
                emptyWord.textComponent = m_TextComponent;
            }

            textInfo.wordCount = filledWordCount != 0 && characterCount > 0 ? filledWordCount : 1;
        }
    }
}
