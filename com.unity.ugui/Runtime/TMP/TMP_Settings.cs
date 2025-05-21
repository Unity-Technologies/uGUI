﻿using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.TextCore;
using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable 0649 // Disabled warnings related to serialized fields not assigned in this script but used in the editor.

namespace TMPro
{
    /// <summary>
    /// Scaling options for the sprites
    /// </summary>
    //public enum SpriteRelativeScaling
    //{
    //    RelativeToPrimary   = 0x1,
    //    RelativeToCurrent   = 0x2,
    //}

    [System.Serializable][ExcludeFromPresetAttribute]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/TextMeshPro/Settings.html")]
    public class TMP_Settings : ScriptableObject
    {
        private static TMP_Settings s_Instance;

        /// <summary>
        /// Returns the release version of the product.
        /// </summary>
        public static string version
        {
            get { return "1.4.0"; }
        }

        [SerializeField]
        internal string assetVersion;

        internal static string s_CurrentAssetVersion = "2";

        internal void SetAssetVersion()
        {
            assetVersion = s_CurrentAssetVersion;
        }

        /// <summary>
        /// Controls the text wrapping mode of newly created text objects.
        /// </summary>
        public static TextWrappingModes textWrappingMode
        {
            get { return instance.m_TextWrappingMode; }
        }
        [FormerlySerializedAs("m_enableWordWrapping")]
        [SerializeField]
        private TextWrappingModes m_TextWrappingMode;

        /// <summary>
        /// Controls if Kerning is enabled on newly created text objects by default.
        /// </summary>
        [System.Obsolete("The \"enableKerning\" property has been deprecated. Use the \"fontFeatures\" property to control what features are enabled by default on newly created text components.")]
        public static bool enableKerning
        {
            get
            {
                if (instance.m_ActiveFontFeatures != null)
                    return instance.m_ActiveFontFeatures.Contains(OTL_FeatureTag.kern);

                return instance.m_enableKerning;
            }
        }
        [SerializeField]
        private bool m_enableKerning;

        /// <summary>
        /// Controls which font features are enabled by default on newly created text objects.
        /// </summary>
        public static List<OTL_FeatureTag> fontFeatures
        {
            get { return instance.m_ActiveFontFeatures; }
        }
        [SerializeField]
        private List<OTL_FeatureTag> m_ActiveFontFeatures = new List<OTL_FeatureTag> { 0 };

        /// <summary>
        /// Controls if Extra Padding is enabled on newly created text objects by default.
        /// </summary>
        public static bool enableExtraPadding
        {
            get { return instance.m_enableExtraPadding; }
        }
        [SerializeField]
        private bool m_enableExtraPadding;

        /// <summary>
        /// Controls if TintAllSprites is enabled on newly created text objects by default.
        /// </summary>
        public static bool enableTintAllSprites
        {
            get { return instance.m_enableTintAllSprites; }
        }
        [SerializeField]
        private bool m_enableTintAllSprites;

        /// <summary>
        /// Controls if Escape Characters will be parsed in the Text Input Box on newly created text objects.
        /// </summary>
        public static bool enableParseEscapeCharacters
        {
            get { return instance.m_enableParseEscapeCharacters; }
        }
        [SerializeField]
        private bool m_enableParseEscapeCharacters;

        /// <summary>
        /// Controls if Raycast Target is enabled by default on newly created text objects.
        /// </summary>
        public static bool enableRaycastTarget
        {
            get { return instance.m_EnableRaycastTarget; }
        }
        [SerializeField]
        private bool m_EnableRaycastTarget = true;

        /// <summary>
        /// Determines if OpenType Font Features should be retrieved at runtime from the source font file.
        /// </summary>
        public static bool getFontFeaturesAtRuntime
        {
            get { return instance.m_GetFontFeaturesAtRuntime; }
        }
        [SerializeField]
        private bool m_GetFontFeaturesAtRuntime = true;

        /// <summary>
        /// The character that will be used as a replacement for missing glyphs in a font asset.
        /// </summary>
        public static int missingGlyphCharacter
        {
            get { return instance.m_missingGlyphCharacter; }
            set { instance.m_missingGlyphCharacter = value; }
        }
        [SerializeField]
        private int m_missingGlyphCharacter;

        /// <summary>
        /// Determines if the "Clear Dynamic Data on Build" property will be set to true or false on newly created dynamic font assets.
        /// </summary>
        public static bool clearDynamicDataOnBuild
        {
            get { return instance.m_ClearDynamicDataOnBuild; }
        }

        [SerializeField] private bool m_ClearDynamicDataOnBuild = true;

        /// <summary>
        /// Controls the display of warning message in the console.
        /// </summary>
        public static bool warningsDisabled
        {
            get { return instance.m_warningsDisabled; }
        }
        [SerializeField]
        private bool m_warningsDisabled;

        /// <summary>
        /// Returns the Default Font Asset to be used by newly created text objects.
        /// </summary>
        public static TMP_FontAsset defaultFontAsset
        {
            get { return instance.m_defaultFontAsset; }
            set { instance.m_defaultFontAsset = value; }
        }
        [SerializeField]
        private TMP_FontAsset m_defaultFontAsset;

        /// <summary>
        /// The relative path to a Resources folder in the project.
        /// </summary>
        public static string defaultFontAssetPath
        {
            get { return instance.m_defaultFontAssetPath; }
        }
        [SerializeField]
        private string m_defaultFontAssetPath;

        /// <summary>
        /// The Default Point Size of newly created text objects.
        /// </summary>
        public static float defaultFontSize
        {
            get { return instance.m_defaultFontSize; }
        }
        [SerializeField]
        private float m_defaultFontSize;

        /// <summary>
        /// The multiplier used to computer the default Min point size when Text Auto Sizing is used.
        /// </summary>
        public static float defaultTextAutoSizingMinRatio
        {
            get { return instance.m_defaultAutoSizeMinRatio; }
        }
        [SerializeField]
        private float m_defaultAutoSizeMinRatio;

        /// <summary>
        /// The multiplier used to computer the default Max point size when Text Auto Sizing is used.
        /// </summary>
        public static float defaultTextAutoSizingMaxRatio
        {
            get { return instance.m_defaultAutoSizeMaxRatio; }
        }
        [SerializeField]
        private float m_defaultAutoSizeMaxRatio;

        /// <summary>
        /// The Default Size of the Text Container of a TextMeshPro object.
        /// </summary>
        public static Vector2 defaultTextMeshProTextContainerSize
        {
            get { return instance.m_defaultTextMeshProTextContainerSize; }
        }
        [SerializeField]
        private Vector2 m_defaultTextMeshProTextContainerSize;

        /// <summary>
        /// The Default Width of the Text Container of a TextMeshProUI object.
        /// </summary>
        public static Vector2 defaultTextMeshProUITextContainerSize
        {
            get { return instance.m_defaultTextMeshProUITextContainerSize; }
        }
        [SerializeField]
        private Vector2 m_defaultTextMeshProUITextContainerSize;

        /// <summary>
        /// Set the size of the text container of newly created text objects to match the size of the text.
        /// </summary>
        public static bool autoSizeTextContainer
        {
            get { return instance.m_autoSizeTextContainer; }
        }
        [SerializeField]
        private bool m_autoSizeTextContainer;

        /// <summary>
        /// Disables InternalUpdate() calls when true. This can improve performance when the scale of the text object is static.
        /// </summary>
        public static bool isTextObjectScaleStatic
        {
            get { return instance.m_IsTextObjectScaleStatic; }
            set { instance.m_IsTextObjectScaleStatic = value; }
        }
        [SerializeField]
        private bool m_IsTextObjectScaleStatic;


        /// <summary>
        /// Returns the list of Fallback Fonts defined in the TMP Settings file.
        /// </summary>
        public static List<TMP_FontAsset> fallbackFontAssets
        {
            get { return instance.m_fallbackFontAssets; }
            set { instance.m_fallbackFontAssets = value; }
        }
        [SerializeField]
        private List<TMP_FontAsset> m_fallbackFontAssets;

        /// <summary>
        /// Controls whether or not TMP will create a matching material preset or use the default material of the fallback font asset.
        /// </summary>
        public static bool matchMaterialPreset
        {
            get { return instance.m_matchMaterialPreset; }
        }
        [SerializeField]
        private bool m_matchMaterialPreset;

        /// <summary>
        /// Determines if sub text objects will be hidden in the scene hierarchy.
        /// </summary>
        public static bool hideSubTextObjects
        {
            get { return instance.m_HideSubTextObjects; }
        }
        [SerializeField] private bool m_HideSubTextObjects = true;

        /// <summary>
        /// The Default Sprite Asset to be used by default.
        /// </summary>
        public static TMP_SpriteAsset defaultSpriteAsset
        {
            get { return instance.m_defaultSpriteAsset; }
            set { instance.m_defaultSpriteAsset = value; }
        }
        [SerializeField]
        private TMP_SpriteAsset m_defaultSpriteAsset;

        /// <summary>
        /// The relative path to a Resources folder in the project.
        /// </summary>
        public static string defaultSpriteAssetPath
        {
            get { return instance.m_defaultSpriteAssetPath; }
        }
        [SerializeField]
        private string m_defaultSpriteAssetPath;

        /// <summary>
        /// Determines if Emoji support is enabled in the Input Field TouchScreenKeyboard.
        /// </summary>
        public static bool enableEmojiSupport
        {
            get { return instance.m_enableEmojiSupport; }
            set { instance.m_enableEmojiSupport = value; }
        }
        [SerializeField]
        private bool m_enableEmojiSupport;

        /// <summary>
        /// The unicode value of the sprite that will be used when the requested sprite is missing from the sprite asset and potential fallbacks.
        /// </summary>
        public static uint missingCharacterSpriteUnicode
        {
            get { return instance.m_MissingCharacterSpriteUnicode; }
            set { instance.m_MissingCharacterSpriteUnicode = value; }
        }
        [SerializeField]
        private uint m_MissingCharacterSpriteUnicode;

        /// <summary>
        /// list of Fallback Text Assets (Font Assets and Sprite Assets) used to lookup characters defined in the Unicode as Emojis.
        /// </summary>
        public static List<TMP_Asset> emojiFallbackTextAssets
        {
            get => instance.m_EmojiFallbackTextAssets;
            set => instance.m_EmojiFallbackTextAssets = value;
        }
        [SerializeField]
        private List<TMP_Asset> m_EmojiFallbackTextAssets;

        /// <summary>
        /// Determines if sprites will be scaled relative to the primary font asset assigned to the text object or relative to the current font asset.
        /// </summary>
        //public static SpriteRelativeScaling spriteRelativeScaling
        //{
        //    get { return instance.m_SpriteRelativeScaling; }
        //    set { instance.m_SpriteRelativeScaling = value; }
        //}
        //[SerializeField]
        //private SpriteRelativeScaling m_SpriteRelativeScaling = SpriteRelativeScaling.RelativeToCurrent;

        /// <summary>
        /// The relative path to a Resources folder in the project that contains Color Gradient Presets.
        /// </summary>
        public static string defaultColorGradientPresetsPath
        {
            get { return instance.m_defaultColorGradientPresetsPath; }
        }
        [SerializeField]
        private string m_defaultColorGradientPresetsPath;

        /// <summary>
        /// The Default Style Sheet used by the text objects.
        /// </summary>
        public static TMP_StyleSheet defaultStyleSheet
        {
            get { return instance.m_defaultStyleSheet; }
            set { instance.m_defaultStyleSheet = value; }
        }
        [SerializeField]
        private TMP_StyleSheet m_defaultStyleSheet;

        /// <summary>
        /// The relative path to a Resources folder in the project that contains the TMP Style Sheets.
        /// </summary>
        public static string styleSheetsResourcePath
        {
            get { return instance.m_StyleSheetsResourcePath; }
        }
        [SerializeField]
        private string m_StyleSheetsResourcePath;

        /// <summary>
        /// Text file that contains the leading characters used for line breaking for Asian languages.
        /// </summary>
        public static TextAsset leadingCharacters
        {
            get { return instance.m_leadingCharacters; }
        }
        [SerializeField]
        private TextAsset m_leadingCharacters;

        /// <summary>
        /// Text file that contains the following characters used for line breaking for Asian languages.
        /// </summary>
        public static TextAsset followingCharacters
        {
            get { return instance.m_followingCharacters; }
        }
        [SerializeField]
        private TextAsset m_followingCharacters;

        /// <summary>
        ///
        /// </summary>
        public static LineBreakingTable linebreakingRules
        {
            get
            {
                if (instance.m_linebreakingRules == null)
                    LoadLinebreakingRules();

                return instance.m_linebreakingRules;
            }
        }
        [SerializeField]
        private LineBreakingTable m_linebreakingRules;

        // TODO : Potential new feature to explore where multiple font assets share the same atlas texture.
        //internal static TMP_DynamicAtlasTextureGroup managedAtlasTextures
        //{
        //    get
        //    {
        //        if (instance.m_DynamicAtlasTextureGroup == null)
        //        {
        //            instance.m_DynamicAtlasTextureGroup = TMP_DynamicAtlasTextureGroup.CreateDynamicAtlasTextureGroup();
        //        }

        //        return instance.m_DynamicAtlasTextureGroup;
        //    }
        //}
        //[SerializeField]
        //private TMP_DynamicAtlasTextureGroup m_DynamicAtlasTextureGroup;

        /// <summary>
        /// Determines if Modern or Traditional line breaking rules should be used for Korean text.
        /// </summary>
        public static bool useModernHangulLineBreakingRules
        {
            get { return instance.m_UseModernHangulLineBreakingRules; }
            set { instance.m_UseModernHangulLineBreakingRules = value; }
        }
        [SerializeField]
        private bool m_UseModernHangulLineBreakingRules;

        /// <summary>
        /// Get a singleton instance of the settings class.
        /// </summary>
        public static TMP_Settings instance
        {
            get
            {
                if (isTMPSettingsNull)
                {
                    s_Instance = Resources.Load<TMP_Settings>("TMP Settings");

                    #if UNITY_EDITOR
                    // Make sure TextMesh Pro UPM packages resources have been added to the user project
                    if (isTMPSettingsNull && Time.frameCount != 0 || (!isTMPSettingsNull && s_Instance.assetVersion != s_CurrentAssetVersion))
                    {
						// It needs to open after loading the default Editor layout
                        DelayShowPackageImporterWindow();
                    }
                    #endif

                    // Convert use of the "enableKerning" property to the new "fontFeature" property.
                    if (!isTMPSettingsNull && s_Instance.m_ActiveFontFeatures.Count == 1 && s_Instance.m_ActiveFontFeatures[0] == 0)
                    {
                        s_Instance.m_ActiveFontFeatures.Clear();

                        if (s_Instance.m_enableKerning)
                            s_Instance.m_ActiveFontFeatures.Add(OTL_FeatureTag.kern);
                    }
                }

                return s_Instance;
            }
        }

        internal static bool isTMPSettingsNull
        {
            get { return s_Instance == null; }
        }

#if UNITY_EDITOR
        public static async void DelayShowPackageImporterWindow()
        {
            await Task.Delay(TimeSpan.FromSeconds(1f));
            TMP_PackageResourceImporterWindow.ShowPackageImporterWindow();
        }
#endif


        /// <summary>
        /// Static Function to load the TMP Settings file.
        /// </summary>
        /// <returns></returns>
        public static TMP_Settings LoadDefaultSettings()
        {
            if (s_Instance == null)
            {
                // Load settings from TMP_Settings file
                TMP_Settings settings = Resources.Load<TMP_Settings>("TMP Settings");
                if (settings != null)
                    s_Instance = settings;
            }

            return s_Instance;
        }


        /// <summary>
        /// Returns the Sprite Asset defined in the TMP Settings file.
        /// </summary>
        /// <returns></returns>
        public static TMP_Settings GetSettings()
        {
            if (TMP_Settings.instance == null) return null;

            return TMP_Settings.instance;
        }


        /// <summary>
        /// Returns the Font Asset defined in the TMP Settings file.
        /// </summary>
        /// <returns></returns>
        public static TMP_FontAsset GetFontAsset()
        {
            if (TMP_Settings.instance == null) return null;

            return TMP_Settings.instance.m_defaultFontAsset;
        }


        /// <summary>
        /// Returns the Sprite Asset defined in the TMP Settings file.
        /// </summary>
        /// <returns></returns>
        public static TMP_SpriteAsset GetSpriteAsset()
        {
            if (TMP_Settings.instance == null) return null;

            return TMP_Settings.instance.m_defaultSpriteAsset;
        }


        /// <summary>
        /// Returns the Style Sheet defined in the TMP Settings file.
        /// </summary>
        /// <returns></returns>
        public static TMP_StyleSheet GetStyleSheet()
        {
            if (TMP_Settings.instance == null) return null;

            return TMP_Settings.instance.m_defaultStyleSheet;
        }


        public static void LoadLinebreakingRules()
        {
            //Debug.Log("Loading Line Breaking Rules for Asian Languages.");

            if (instance == null) return;

            if (s_Instance.m_linebreakingRules == null)
                s_Instance.m_linebreakingRules = new LineBreakingTable();

            s_Instance.m_linebreakingRules.leadingCharacters = GetCharacters(s_Instance.m_leadingCharacters);
            s_Instance.m_linebreakingRules.followingCharacters = GetCharacters(s_Instance.m_followingCharacters);
        }


        /// <summary>
        ///  Get the characters from the line breaking files
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static HashSet<uint> GetCharacters(TextAsset file)
        {
            HashSet<uint> dict = new HashSet<uint>();
            string text = file.text;

            for (int i = 0; i < text.Length; i++)
            {
                dict.Add(text[i]);
            }

            return dict;
        }


        public class LineBreakingTable
        {
            public HashSet<uint> leadingCharacters;
            public HashSet<uint> followingCharacters;
        }
    }
}
