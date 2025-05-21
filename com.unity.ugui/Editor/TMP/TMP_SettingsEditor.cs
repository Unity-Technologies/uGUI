using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

#pragma warning disable 0414 // Disabled a few warnings for not yet implemented features.

namespace TMPro.EditorUtilities
{
    [CustomEditor(typeof(TMP_Settings))]
    public class TMP_SettingsEditor : Editor
    {
        internal class Styles
        {
            public static readonly GUIContent defaultFontAssetLabel = new GUIContent("Default Font Asset", "The Font Asset that will be assigned by default to newly created text objects when no Font Asset is specified.");
            public static readonly GUIContent defaultFontAssetPathLabel = new GUIContent("Path:        Resources/", "The relative path to a Resources folder where the Font Assets and Material Presets are located.\nExample \"Fonts & Materials/\"");

            public static readonly GUIContent fallbackFontAssetsLabel = new GUIContent("Fallback Font Assets", "The Font Assets that will be searched to locate and replace missing characters from a given Font Asset.");
            public static readonly GUIContent fallbackFontAssetsListLabel = new GUIContent("Font Asset List", "The Font Assets that will be searched to locate and replace missing characters from a given Font Asset.");

            public static readonly GUIContent fallbackMaterialSettingsLabel = new GUIContent("Fallback Material Settings");
            public static readonly GUIContent matchMaterialPresetLabel = new GUIContent("Match Material Presets");
            public static readonly GUIContent hideSubTextObjectsPresetLabel = new GUIContent("Hide Sub Text Objects", "Determines if sub text objects will be hidden in the scene hierarchy. Property change will only take effect after entering or existing play mode.");

            public static readonly GUIContent containerDefaultSettingsLabel = new GUIContent("Text Container Default Settings");

            public static readonly GUIContent textMeshProLabel = new GUIContent("TextMeshPro");
            public static readonly GUIContent textMeshProUiLabel = new GUIContent("TextMeshPro UI");
            public static readonly GUIContent enableRaycastTarget = new GUIContent("Enable Raycast Target");
            public static readonly GUIContent autoSizeContainerLabel = new GUIContent("Auto Size Text Container", "Set the size of the text container to match the text.");
            public static readonly GUIContent isTextObjectScaleStaticLabel = new GUIContent("Is Object Scale Static", "Disables calling InternalUpdate() when enabled. This can improve performance when text object scale is static.");

            public static readonly GUIContent textComponentDefaultSettingsLabel = new GUIContent("Text Component Default Settings");
            public static readonly GUIContent defaultFontSize = new GUIContent("Default Font Size");
            public static readonly GUIContent autoSizeRatioLabel = new GUIContent("Text Auto Size Ratios");
            public static readonly GUIContent minLabel = new GUIContent("Min");
            public static readonly GUIContent maxLabel = new GUIContent("Max");

            public static readonly GUIContent textWrappingModeLabel = new GUIContent("Text Wrapping Mode");
            public static readonly GUIContent kerningLabel = new GUIContent("Kerning");
            public static readonly GUIContent fontFeaturesLabel = new GUIContent("Font Features", "Font features that should be set by default on the text component.");
            public static readonly GUIContent extraPaddingLabel = new GUIContent("Extra Padding");
            public static readonly GUIContent tintAllSpritesLabel = new GUIContent("Tint All Sprites");
            public static readonly GUIContent parseEscapeCharactersLabel = new GUIContent("Parse Escape Sequence");

            public static readonly GUIContent dynamicFontSystemSettingsLabel = new GUIContent("Dynamic Font System Settings");
            public static readonly GUIContent getFontFeaturesAtRuntime = new GUIContent("Get Font Features at Runtime", "Determines if OpenType font features should be retrieved from source font files as new characters and glyphs are added to font assets.");
            public static readonly GUIContent dynamicAtlasTextureGroup = new GUIContent("Dynamic Atlas Texture Group");

            public static readonly GUIContent missingGlyphLabel = new GUIContent("Missing Character Unicode", "The character to be displayed when the requested character is not found in any font asset or fallbacks.");
            public static readonly GUIContent clearDynamicDataOnBuildLabel = new GUIContent("Clear Dynamic Data On Build", "Determines if the \"Clear Dynamic Data on Build\" property will be set to true or false on newly created dynamic font assets.");
            public static readonly GUIContent disableWarningsLabel = new GUIContent("Disable warnings", "Disable warning messages in the Console.");

            public static readonly GUIContent defaultSpriteAssetLabel = new GUIContent("Default Sprite Asset", "The Sprite Asset that will be assigned by default when using the <sprite> tag when no Sprite Asset is specified.");
            public static readonly GUIContent missingSpriteCharacterUnicodeLabel = new GUIContent("Missing Sprite Unicode", "The unicode value for the sprite character to be displayed when the requested sprite character is not found in any sprite assets or fallbacks.");
            public static readonly GUIContent enableEmojiSupportLabel = new GUIContent("iOS Emoji Support", "Enables Emoji support for Touch Screen Keyboards on target devices.");
            //public static readonly GUIContent spriteRelativeScale = new GUIContent("Relative Scaling", "Determines if the sprites will be scaled relative to the primary font asset assigned to the text object or relative to the current font asset.");
            public static readonly GUIContent emojifallbackTextAssetsListLabel = new GUIContent("Fallback Emoji Text Assets", "The Text Assets that will be searched to display characters defined as Emojis in the Unicode.");

            public static readonly GUIContent spriteAssetsPathLabel = new GUIContent("Path:        Resources/", "The relative path to a Resources folder where the Sprite Assets are located.\nExample \"Sprite Assets/\"");

            public static readonly GUIContent defaultStyleSheetLabel = new GUIContent("Default Style Sheet", "The Style Sheet that will be used for all text objects in this project.");
            public static readonly GUIContent styleSheetResourcePathLabel = new GUIContent("Path:        Resources/", "The relative path to a Resources folder where the Style Sheets are located.\nExample \"Style Sheets/\"");

            public static readonly GUIContent colorGradientPresetsLabel = new GUIContent("Color Gradient Presets", "The relative path to a Resources folder where the Color Gradient Presets are located.\nExample \"Color Gradient Presets/\"");
            public static readonly GUIContent colorGradientsPathLabel = new GUIContent("Path:        Resources/", "The relative path to a Resources folder where the Color Gradient Presets are located.\nExample \"Color Gradient Presets/\"");

            public static readonly GUIContent lineBreakingLabel = new GUIContent("Line Breaking for Asian languages", "The text assets that contain the Leading and Following characters which define the rules for line breaking with Asian languages.");
            public static readonly GUIContent koreanSpecificRules = new GUIContent("Korean Language Options");
        }

        SerializedProperty m_PropFontAsset;
        SerializedProperty m_PropDefaultFontAssetPath;
        SerializedProperty m_PropDefaultFontSize;
        SerializedProperty m_PropDefaultAutoSizeMinRatio;
        SerializedProperty m_PropDefaultAutoSizeMaxRatio;
        SerializedProperty m_PropDefaultTextMeshProTextContainerSize;
        SerializedProperty m_PropDefaultTextMeshProUITextContainerSize;
        SerializedProperty m_PropAutoSizeTextContainer;
        SerializedProperty m_PropEnableRaycastTarget;
        SerializedProperty m_PropIsTextObjectScaleStatic;

        SerializedProperty m_PropSpriteAsset;
        SerializedProperty m_PropMissingSpriteCharacterUnicode;
        //SerializedProperty m_PropSpriteRelativeScaling;
        SerializedProperty m_PropEnableEmojiSupport;
        SerializedProperty m_PropSpriteAssetPath;

        ReorderableList m_EmojiFallbackTextAssetList;

        SerializedProperty m_PropStyleSheet;
        SerializedProperty m_PropStyleSheetsResourcePath;
        ReorderableList m_GlobalFallbackFontAssetList;

        SerializedProperty m_PropColorGradientPresetsPath;

        SerializedProperty m_PropMatchMaterialPreset;
        SerializedProperty m_PropHideSubTextObjects;
        SerializedProperty m_PropTextWrappingMode;
        SerializedProperty m_PropFontFeatures;
        SerializedProperty m_PropExtraPadding;
        SerializedProperty m_PropTintAllSprites;
        SerializedProperty m_PropParseEscapeCharacters;
        SerializedProperty m_PropMissingGlyphCharacter;
        SerializedProperty m_PropClearDynamicDataOnBuild;

        //SerializedProperty m_DynamicAtlasTextureManager;
        SerializedProperty m_GetFontFeaturesAtRuntime;

        SerializedProperty m_PropWarningsDisabled;

        SerializedProperty m_PropLeadingCharacters;
        SerializedProperty m_PropFollowingCharacters;
        SerializedProperty m_PropUseModernHangulLineBreakingRules;

        private const string k_UndoRedo = "UndoRedoPerformed";
        private bool m_IsFallbackGlyphCacheDirty;

        private static readonly string[] k_FontFeatures = new string[] { "kern", "liga", "mark", "mkmk" };

        public void OnEnable()
        {
            if (target == null)
                return;

            m_PropFontAsset = serializedObject.FindProperty("m_defaultFontAsset");
            m_PropDefaultFontAssetPath = serializedObject.FindProperty("m_defaultFontAssetPath");
            m_PropDefaultFontSize = serializedObject.FindProperty("m_defaultFontSize");
            m_PropDefaultAutoSizeMinRatio = serializedObject.FindProperty("m_defaultAutoSizeMinRatio");
            m_PropDefaultAutoSizeMaxRatio = serializedObject.FindProperty("m_defaultAutoSizeMaxRatio");
            m_PropDefaultTextMeshProTextContainerSize = serializedObject.FindProperty("m_defaultTextMeshProTextContainerSize");
            m_PropDefaultTextMeshProUITextContainerSize = serializedObject.FindProperty("m_defaultTextMeshProUITextContainerSize");
            m_PropAutoSizeTextContainer = serializedObject.FindProperty("m_autoSizeTextContainer");
            m_PropEnableRaycastTarget = serializedObject.FindProperty("m_EnableRaycastTarget");
            m_PropIsTextObjectScaleStatic = serializedObject.FindProperty("m_IsTextObjectScaleStatic");

            m_PropSpriteAsset = serializedObject.FindProperty("m_defaultSpriteAsset");
            m_PropMissingSpriteCharacterUnicode = serializedObject.FindProperty("m_MissingCharacterSpriteUnicode");
            //m_PropSpriteRelativeScaling = serializedObject.FindProperty("m_SpriteRelativeScaling");
            m_PropEnableEmojiSupport = serializedObject.FindProperty("m_enableEmojiSupport");
            m_PropSpriteAssetPath = serializedObject.FindProperty("m_defaultSpriteAssetPath");

            m_PropStyleSheet = serializedObject.FindProperty("m_defaultStyleSheet");
            m_PropStyleSheetsResourcePath = serializedObject.FindProperty("m_StyleSheetsResourcePath");


            m_PropColorGradientPresetsPath = serializedObject.FindProperty("m_defaultColorGradientPresetsPath");

            // Global Fallback ReorderableList
            m_GlobalFallbackFontAssetList = new ReorderableList(serializedObject, serializedObject.FindProperty("m_fallbackFontAssets"), true, true, true, true);

            m_GlobalFallbackFontAssetList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, Styles.fallbackFontAssetsListLabel);
            };

            m_GlobalFallbackFontAssetList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = m_GlobalFallbackFontAssetList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
            };

            m_GlobalFallbackFontAssetList.onChangedCallback = itemList =>
            {
                m_IsFallbackGlyphCacheDirty = true;
            };

            // Emoji Fallback ReorderableList
            m_EmojiFallbackTextAssetList = new ReorderableList(serializedObject, serializedObject.FindProperty("m_EmojiFallbackTextAssets"), true, true, true, true);

            m_EmojiFallbackTextAssetList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Text Asset List");
            };

            m_EmojiFallbackTextAssetList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = m_EmojiFallbackTextAssetList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
            };

            m_EmojiFallbackTextAssetList.onChangedCallback = itemList =>
            {
                m_IsFallbackGlyphCacheDirty = true;
            };

            m_PropMatchMaterialPreset = serializedObject.FindProperty("m_matchMaterialPreset");
            m_PropHideSubTextObjects = serializedObject.FindProperty("m_HideSubTextObjects");

            m_PropTextWrappingMode = serializedObject.FindProperty("m_TextWrappingMode");

            m_PropFontFeatures = serializedObject.FindProperty("m_ActiveFontFeatures");
            m_PropExtraPadding = serializedObject.FindProperty("m_enableExtraPadding");
            m_PropTintAllSprites = serializedObject.FindProperty("m_enableTintAllSprites");
            m_PropParseEscapeCharacters = serializedObject.FindProperty("m_enableParseEscapeCharacters");
            m_PropMissingGlyphCharacter = serializedObject.FindProperty("m_missingGlyphCharacter");
            m_PropClearDynamicDataOnBuild = serializedObject.FindProperty("m_ClearDynamicDataOnBuild");
            m_PropWarningsDisabled = serializedObject.FindProperty("m_warningsDisabled");

            //m_DynamicAtlasTextureManager = serializedObject.FindProperty("m_DynamicAtlasTextureGroup");
            m_GetFontFeaturesAtRuntime = serializedObject.FindProperty("m_GetFontFeaturesAtRuntime");

            m_PropLeadingCharacters = serializedObject.FindProperty("m_leadingCharacters");
            m_PropFollowingCharacters = serializedObject.FindProperty("m_followingCharacters");
            m_PropUseModernHangulLineBreakingRules = serializedObject.FindProperty("m_UseModernHangulLineBreakingRules");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            string evt_cmd = Event.current.commandName;
            m_IsFallbackGlyphCacheDirty = false;

            float labelWidth = EditorGUIUtility.labelWidth;
            float fieldWidth = EditorGUIUtility.fieldWidth;

            // TextMeshPro Font Info Panel
            EditorGUI.indentLevel = 0;

            // FONT ASSET
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(Styles.defaultFontAssetLabel, EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_PropFontAsset, Styles.defaultFontAssetLabel);
            if (EditorGUI.EndChangeCheck())
                m_IsFallbackGlyphCacheDirty = true;

            EditorGUILayout.PropertyField(m_PropDefaultFontAssetPath, Styles.defaultFontAssetPathLabel);
            EditorGUI.indentLevel = 0;

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            // FALLBACK FONT ASSETs
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(Styles.fallbackFontAssetsLabel, EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            m_GlobalFallbackFontAssetList.DoLayoutList();
            if (EditorGUI.EndChangeCheck())
                m_IsFallbackGlyphCacheDirty = true;

            GUILayout.Label(Styles.fallbackMaterialSettingsLabel, EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(m_PropMatchMaterialPreset, Styles.matchMaterialPresetLabel);
            EditorGUILayout.PropertyField(m_PropHideSubTextObjects, Styles.hideSubTextObjectsPresetLabel);
            EditorGUI.indentLevel = 0;

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            // MISSING GLYPHS
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(Styles.dynamicFontSystemSettingsLabel, EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(m_PropMissingGlyphCharacter, Styles.missingGlyphLabel);
            EditorGUILayout.PropertyField(m_PropWarningsDisabled, Styles.disableWarningsLabel);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_GetFontFeaturesAtRuntime, Styles.getFontFeaturesAtRuntime);
            EditorGUILayout.PropertyField(m_PropClearDynamicDataOnBuild, Styles.clearDynamicDataOnBuildLabel);
            //EditorGUILayout.PropertyField(m_DynamicAtlasTextureManager, Styles.dynamicAtlasTextureManager);
            EditorGUI.indentLevel = 0;

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            // TEXT OBJECT DEFAULT PROPERTIES
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(Styles.containerDefaultSettingsLabel, EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;

            EditorGUILayout.PropertyField(m_PropDefaultTextMeshProTextContainerSize, Styles.textMeshProLabel);
            EditorGUILayout.PropertyField(m_PropDefaultTextMeshProUITextContainerSize, Styles.textMeshProUiLabel);
            EditorGUILayout.PropertyField(m_PropEnableRaycastTarget, Styles.enableRaycastTarget);
            EditorGUILayout.PropertyField(m_PropAutoSizeTextContainer, Styles.autoSizeContainerLabel);
            EditorGUILayout.PropertyField(m_PropIsTextObjectScaleStatic, Styles.isTextObjectScaleStaticLabel);
            EditorGUI.indentLevel = 0;

            EditorGUILayout.Space();

            GUILayout.Label(Styles.textComponentDefaultSettingsLabel, EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(m_PropDefaultFontSize, Styles.defaultFontSize);

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel(Styles.autoSizeRatioLabel);
                EditorGUIUtility.labelWidth = 32;
                EditorGUIUtility.fieldWidth = 10;

                EditorGUI.indentLevel = 0;
                EditorGUILayout.PropertyField(m_PropDefaultAutoSizeMinRatio, Styles.minLabel);
                EditorGUILayout.PropertyField(m_PropDefaultAutoSizeMaxRatio, Styles.maxLabel);
                EditorGUI.indentLevel = 1;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUIUtility.fieldWidth = fieldWidth;

            EditorGUILayout.PropertyField(m_PropTextWrappingMode, Styles.textWrappingModeLabel);

            DrawFontFeatures();

            EditorGUILayout.PropertyField(m_PropExtraPadding, Styles.extraPaddingLabel);
            EditorGUILayout.PropertyField(m_PropTintAllSprites, Styles.tintAllSpritesLabel);

            EditorGUILayout.PropertyField(m_PropParseEscapeCharacters, Styles.parseEscapeCharactersLabel);

            EditorGUI.indentLevel = 0;

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            // SPRITE ASSET
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(Styles.defaultSpriteAssetLabel, EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_PropSpriteAsset, Styles.defaultSpriteAssetLabel);
            if (EditorGUI.EndChangeCheck())
                m_IsFallbackGlyphCacheDirty = true;

            EditorGUILayout.PropertyField(m_PropMissingSpriteCharacterUnicode, Styles.missingSpriteCharacterUnicodeLabel);
            EditorGUILayout.PropertyField(m_PropEnableEmojiSupport, Styles.enableEmojiSupportLabel);
            //EditorGUILayout.PropertyField(m_PropSpriteRelativeScaling, Styles.spriteRelativeScale);
            EditorGUILayout.PropertyField(m_PropSpriteAssetPath, Styles.spriteAssetsPathLabel);
            EditorGUI.indentLevel = 0;

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            // EMOJI FALLBACK TEXT ASSETS
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(Styles.emojifallbackTextAssetsListLabel, EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            m_EmojiFallbackTextAssetList.DoLayoutList();
            if (EditorGUI.EndChangeCheck())
                m_IsFallbackGlyphCacheDirty = true;

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            // STYLE SHEET
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(Styles.defaultStyleSheetLabel, EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_PropStyleSheet, Styles.defaultStyleSheetLabel);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                TMP_StyleSheet styleSheet = m_PropStyleSheet.objectReferenceValue as TMP_StyleSheet;
                if (styleSheet != null)
                    styleSheet.RefreshStyles();
            }
            EditorGUILayout.PropertyField(m_PropStyleSheetsResourcePath, Styles.styleSheetResourcePathLabel);
            EditorGUI.indentLevel = 0;

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            // COLOR GRADIENT PRESETS
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(Styles.colorGradientPresetsLabel, EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(m_PropColorGradientPresetsPath, Styles.colorGradientsPathLabel);
            EditorGUI.indentLevel = 0;

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            // LINE BREAKING RULE
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(Styles.lineBreakingLabel, EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(m_PropLeadingCharacters);
            EditorGUILayout.PropertyField(m_PropFollowingCharacters);

            EditorGUILayout.Space();
            GUILayout.Label(Styles.koreanSpecificRules, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_PropUseModernHangulLineBreakingRules, new GUIContent("Use Modern Line Breaking", "Determines if traditional or modern line breaking rules will be used to control line breaking. Traditional line breaking rules use the Leading and Following Character rules whereas Modern uses spaces for line breaking."));

            EditorGUI.indentLevel = 0;

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            if (m_IsFallbackGlyphCacheDirty || evt_cmd == k_UndoRedo)
                TMP_ResourceManager.RebuildFontAssetCache();

            if (serializedObject.ApplyModifiedProperties() || evt_cmd == k_UndoRedo)
            {
                EditorUtility.SetDirty(target);
                TMPro_EventManager.ON_TMP_SETTINGS_CHANGED();
            }
        }

        void DrawFontFeatures()
        {
            int srcMask = 0;

            int featureCount = m_PropFontFeatures.arraySize;
            for (int i = 0; i < featureCount; i++)
            {
                SerializedProperty activeFeatureProperty = m_PropFontFeatures.GetArrayElementAtIndex(i);

                for (int j = 0; j < k_FontFeatures.Length; j++)
                {
                    if (activeFeatureProperty.intValue == k_FontFeatures[j].TagToInt())
                    {
                        srcMask |= 0x1 << j;
                        break;
                    }
                }
            }

            EditorGUI.BeginChangeCheck();

            int mask = EditorGUILayout.MaskField(Styles.fontFeaturesLabel, srcMask, k_FontFeatures);

            if (EditorGUI.EndChangeCheck())
            {
                m_PropFontFeatures.ClearArray();

                int writeIndex = 0;

                for (int i = 0; i < k_FontFeatures.Length; i++)
                {
                    int bit = 0x1 << i;
                    if ((mask & bit) == bit)
                    {
                        m_PropFontFeatures.InsertArrayElementAtIndex(writeIndex);
                        SerializedProperty newFeature = m_PropFontFeatures.GetArrayElementAtIndex(writeIndex);
                        newFeature.intValue = k_FontFeatures[i].TagToInt();

                        writeIndex += 1;
                    }
                }
            }
        }
    }

    class TMP_ResourceImporterProvider : SettingsProvider
    {
        TMP_PackageResourceImporter m_ResourceImporter;

        public TMP_ResourceImporterProvider()
            : base("Project/TextMesh Pro", SettingsScope.Project)
        {
        }

        public override void OnGUI(string searchContext)
        {
            // Lazy creation that supports domain reload
            if (m_ResourceImporter == null)
                m_ResourceImporter = new TMP_PackageResourceImporter(logErrors: false);

            m_ResourceImporter.OnGUI();
        }

        public override void OnDeactivate()
        {
            if (m_ResourceImporter != null)
                m_ResourceImporter.OnDestroy();
        }

        static UnityEngine.Object GetTMPSettings()
        {
            return Resources.Load<TMP_Settings>("TMP Settings");
        }

        [SettingsProviderGroup]
        static SettingsProvider[] CreateTMPSettingsProvider()
        {
            var providers = new List<SettingsProvider> { new TMP_ResourceImporterProvider() };

            if (GetTMPSettings() != null)
            {
                var provider = new AssetSettingsProvider("Project/TextMesh Pro/Settings", GetTMPSettings);
                provider.PopulateSearchKeywordsFromGUIContentProperties<TMP_SettingsEditor.Styles>();
                providers.Add(provider);
            }

            return providers.ToArray();
        }
    }
}
