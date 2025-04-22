using System.Collections.Generic;
using UnityEngine.TextCore.Text;


namespace TMPro
{
    public class TMP_FontAssetUtilities
    {
        private static readonly TMP_FontAssetUtilities s_Instance = new TMP_FontAssetUtilities();

        /// <summary>
        /// Default constructor
        /// </summary>
        static TMP_FontAssetUtilities() { }


        /// <summary>
        /// Get a singleton instance of the Font Asset Utilities class.
        /// </summary>
        public static TMP_FontAssetUtilities instance
        {
            get { return s_Instance; }
        }


        /// <summary>
        /// HashSet containing instance ID of font assets already searched.
        /// </summary>
        private static HashSet<int> k_SearchedAssets;


        /// <summary>
        /// Returns the text element (character) for the given unicode value taking into consideration the requested font style and weight.
        /// Function searches the source font asset, its list of font assets assigned as alternative typefaces and potentially its fallbacks.
        /// The font asset out parameter contains a reference to the font asset containing the character.
        /// The typeface type indicates whether the returned font asset is the source font asset, an alternative typeface or fallback font asset.
        /// </summary>
        /// <param name="unicode">The unicode value of the requested character</param>
        /// <param name="sourceFontAsset">The font asset to be searched</param>
        /// <param name="includeFallbacks">Include the fallback font assets in the search</param>
        /// <param name="fontStyle">The font style</param>
        /// <param name="fontWeight">The font weight</param>
        /// <param name="isAlternativeTypeface">Indicates if the OUT font asset is an alternative typeface or fallback font asset</param>
        /// <param name="fontAsset">The font asset that contains the requested character</param>
        /// <returns></returns>
        public static Character GetCharacterFromFontAsset(uint unicode, FontAsset sourceFontAsset, bool includeFallbacks, FontStyles fontStyle, FontWeight fontWeight, out bool isAlternativeTypeface)
        {
            if (includeFallbacks)
            {
                if (k_SearchedAssets == null)
                    k_SearchedAssets = new HashSet<int>();
                else
                    k_SearchedAssets.Clear();
            }

            return GetCharacterFromFontAsset_Internal(unicode, sourceFontAsset, includeFallbacks, fontStyle, fontWeight, out isAlternativeTypeface);
        }


        /// <summary>
        /// Internal function returning the text element character for the given unicode value taking into consideration the font style and weight.
        /// Function searches the source font asset, list of font assets assigned as alternative typefaces and list of fallback font assets.
        /// </summary>
        private static Character GetCharacterFromFontAsset_Internal(uint unicode, FontAsset sourceFontAsset, bool includeFallbacks, FontStyles fontStyle, FontWeight fontWeight, out bool isAlternativeTypeface)
        {
            isAlternativeTypeface = false;
            Character character = null;

            #region FONT WEIGHT AND FONT STYLE HANDLING
            // Determine if a font weight or style is used. If so check if an alternative typeface is assigned for the given weight and / or style.
            bool isItalic = (fontStyle & FontStyles.Italic) == FontStyles.Italic;

            if (isItalic || fontWeight != FontWeight.Regular)
            {
                // Get reference to the font weight pairs of the given font asset.
                FontWeightPair[] fontWeights = sourceFontAsset.fontWeightTable;

                int fontWeightIndex = 4;
                switch (fontWeight)
                {
                    case FontWeight.Thin:
                        fontWeightIndex = 1;
                        break;
                    case FontWeight.ExtraLight:
                        fontWeightIndex = 2;
                        break;
                    case FontWeight.Light:
                        fontWeightIndex = 3;
                        break;
                    case FontWeight.Regular:
                        fontWeightIndex = 4;
                        break;
                    case FontWeight.Medium:
                        fontWeightIndex = 5;
                        break;
                    case FontWeight.SemiBold:
                        fontWeightIndex = 6;
                        break;
                    case FontWeight.Bold:
                        fontWeightIndex = 7;
                        break;
                    case FontWeight.Heavy:
                        fontWeightIndex = 8;
                        break;
                    case FontWeight.Black:
                        fontWeightIndex = 9;
                        break;
                }

                FontAsset temp = isItalic ? fontWeights[fontWeightIndex].italicTypeface : fontWeights[fontWeightIndex].regularTypeface;

                if (temp != null)
                {
                    if (temp.characterLookupTable.TryGetValue(unicode, out character))
                    {
                        if (character.textAsset != null)
                        {
                            isAlternativeTypeface = true;
                            return character;
                        }

                        // Remove character from lookup table
                        temp.characterLookupTable.Remove(unicode);
                    }

                    if (temp.atlasPopulationMode == UnityEngine.TextCore.Text.AtlasPopulationMode.Dynamic || temp.atlasPopulationMode == UnityEngine.TextCore.Text.AtlasPopulationMode.DynamicOS)
                    {
                        if (temp.TryAddCharacterInternal(unicode, out character))
                        {
                            isAlternativeTypeface = true;

                            return character;
                        }

                        // Check if the source font file contains the requested character.
                        //if (TryGetCharacterFromFontFile(unicode, fontAsset, out characterData))
                        //{
                        //    isAlternativeTypeface = true;

                        //    return characterData;
                        //}

                        // If we find the requested character, we add it to the font asset character table
                        // and return its character data.
                        // We also add this character to the list of characters we will need to add to the font atlas.
                        // We assume the font atlas has room otherwise this font asset should not be marked as dynamic.
                        // Alternatively, we could also add multiple pages of font atlas textures (feature consideration).
                    }

                    // At this point, we were not able to find the requested character in the alternative typeface
                    // so we check the source font asset and its potential fallbacks.
                }
            }
            #endregion

            // Search the source font asset for the requested character.
            if (sourceFontAsset.characterLookupTable.TryGetValue(unicode, out character))
            {
                if (character.textAsset != null)
                    return character;

                // Remove character from lookup table
                sourceFontAsset.characterLookupTable.Remove(unicode);
            }

            if (sourceFontAsset.atlasPopulationMode == UnityEngine.TextCore.Text.AtlasPopulationMode.Dynamic || sourceFontAsset.atlasPopulationMode == UnityEngine.TextCore.Text.AtlasPopulationMode.DynamicOS)
            {
                if (sourceFontAsset.TryAddCharacterInternal(unicode, out character))
                    return character;
            }

            // Search fallback font assets if we still don't have a valid character and include fallback is set to true.
            if (character == null && includeFallbacks && sourceFontAsset.fallbackFontAssetTable != null)
            {
                // Get reference to the list of fallback font assets.
                List<FontAsset> fallbackFontAssets = sourceFontAsset.fallbackFontAssetTable;
                int fallbackCount = fallbackFontAssets.Count;

                if (fallbackCount == 0)
                    return null;

                for (int i = 0; i < fallbackCount; i++)
                {
                    FontAsset temp = fallbackFontAssets[i];

                    if (temp == null)
                        continue;

                    int id = temp.instanceID;

                    // Try adding font asset to search list. If already present skip to the next one otherwise check if it contains the requested character.
                    if (k_SearchedAssets.Add(id) == false)
                        continue;

                    // Add reference to this search query
                    //sourceFontAsset.FallbackSearchQueryLookup.Add(id);

                    character = GetCharacterFromFontAsset_Internal(unicode, temp, true, fontStyle, fontWeight, out isAlternativeTypeface);

                    if (character != null)
                        return character;
                }
            }

            return null;
        }


        /// <summary>
        /// Returns the text element (character) for the given unicode value taking into consideration the requested font style and weight.
        /// Function searches the provided list of font assets, the list of font assets assigned as alternative typefaces to them as well as their fallbacks.
        /// The font asset out parameter contains a reference to the font asset containing the character.
        /// The typeface type indicates whether the returned font asset is the source font asset, an alternative typeface or fallback font asset.
        /// </summary>
        /// <param name="unicode">The unicode value of the requested character</param>
        /// <param name="sourceFontAsset">The font asset originating the search query</param>
        /// <param name="fontAssets">The list of font assets to search</param>
        /// <param name="includeFallbacks">Determines if the fallback of each font assets on the list will be searched</param>
        /// <param name="fontStyle">The font style</param>
        /// <param name="fontWeight">The font weight</param>
        /// <param name="isAlternativeTypeface">Determines if the OUT font asset is an alternative typeface or fallback font asset</param>
        /// <returns></returns>
        public static Character GetCharacterFromFontAssets(uint unicode, FontAsset sourceFontAsset, List<FontAsset> fontAssets, bool includeFallbacks, FontStyles fontStyle, FontWeight fontWeight, out bool isAlternativeTypeface)
        {
            isAlternativeTypeface = false;

            // Make sure font asset list is valid
            if (fontAssets == null || fontAssets.Count == 0)
                return null;

            if (includeFallbacks)
            {
                if (k_SearchedAssets == null)
                    k_SearchedAssets = new HashSet<int>();
                else
                    k_SearchedAssets.Clear();
            }

            int fontAssetCount = fontAssets.Count;

            for (int i = 0; i < fontAssetCount; i++)
            {
                FontAsset fontAsset = fontAssets[i];

                if (fontAsset == null) continue;

                // Add reference to this search query
                //sourceFontAsset.FallbackSearchQueryLookup.Add(fontAsset.instanceID);

                Character character = GetCharacterFromFontAsset_Internal(unicode, fontAsset, includeFallbacks, fontStyle, fontWeight, out isAlternativeTypeface);

                if (character != null)
                    return character;
            }

            return null;
        }

        internal static TextElement GetTextElementFromTextAssets(uint unicode, FontAsset sourceFontAsset, List<TextAsset> textAssets, bool includeFallbacks, FontStyles fontStyle, FontWeight fontWeight, out bool isAlternativeTypeface)
        {
            isAlternativeTypeface = false;

            // Make sure font asset list is valid
            if (textAssets == null || textAssets.Count == 0)
                return null;

            if (includeFallbacks)
            {
                if (k_SearchedAssets == null)
                    k_SearchedAssets = new HashSet<int>();
                else
                    k_SearchedAssets.Clear();
            }

            int textAssetCount = textAssets.Count;

            for (int i = 0; i < textAssetCount; i++)
            {
                TextAsset textAsset = textAssets[i];

                if (textAsset == null) continue;

                if (textAsset.GetType() == typeof(FontAsset))
                {
                    FontAsset fontAsset = textAsset as FontAsset;
                    var character = GetCharacterFromFontAsset_Internal(unicode, fontAsset, includeFallbacks, fontStyle, fontWeight, out isAlternativeTypeface);

                    if (character != null)
                        return character;
                }
                else
                {
                    SpriteAsset spriteAsset = textAsset as SpriteAsset;
                    SpriteCharacter spriteCharacter = GetSpriteCharacterFromSpriteAsset_Internal(unicode, spriteAsset, true);

                    if (spriteCharacter != null)
                        return spriteCharacter;
                }
            }

            return null;
        }

        // =====================================================================
        // SPRITE ASSET - Functions
        // =====================================================================

        /// <summary>
        ///
        /// </summary>
        /// <param name="unicode"></param>
        /// <param name="spriteAsset"></param>
        /// <param name="includeFallbacks"></param>
        /// <returns></returns>
        public static SpriteCharacter GetSpriteCharacterFromSpriteAsset(uint unicode, SpriteAsset spriteAsset, bool includeFallbacks)
        {
            // Make sure we have a valid sprite asset to search
            if (spriteAsset == null)
                return null;

            SpriteCharacter spriteCharacter;

             // Search sprite asset for potential sprite character for the given unicode value
            if (spriteAsset.spriteCharacterLookupTable.TryGetValue(unicode, out spriteCharacter))
                return spriteCharacter;

            if (includeFallbacks)
            {
                // Clear searched assets
                if (k_SearchedAssets == null)
                    k_SearchedAssets = new HashSet<int>();
                else
                    k_SearchedAssets.Clear();

                // Add current sprite asset to already searched assets.
                k_SearchedAssets.Add(spriteAsset.instanceID);

                List<SpriteAsset> fallbackSpriteAsset = spriteAsset.fallbackSpriteAssets;

                if (fallbackSpriteAsset != null && fallbackSpriteAsset.Count > 0)
                {
                    int fallbackCount = fallbackSpriteAsset.Count;

                    for (int i = 0; i < fallbackCount; i++)
                    {
                        SpriteAsset temp = fallbackSpriteAsset[i];

                        if (temp == null)
                            continue;

                        int id = temp.instanceID;

                        // Try adding asset to search list. If already present skip to the next one otherwise check if it contains the requested character.
                        if (k_SearchedAssets.Add(id) == false)
                            continue;

                        spriteCharacter = GetSpriteCharacterFromSpriteAsset_Internal(unicode, temp, true);

                        if (spriteCharacter != null)
                            return spriteCharacter;
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="unicode"></param>
        /// <param name="spriteAsset"></param>
        /// <param name="includeFallbacks"></param>
        /// <returns></returns>
        static SpriteCharacter GetSpriteCharacterFromSpriteAsset_Internal(uint unicode, SpriteAsset spriteAsset, bool includeFallbacks)
        {
            SpriteCharacter spriteCharacter;

             // Search sprite asset for potential sprite character for the given unicode value
            if (spriteAsset.spriteCharacterLookupTable.TryGetValue(unicode, out spriteCharacter))
                return spriteCharacter;

            if (includeFallbacks)
            {
                List<SpriteAsset> fallbackSpriteAsset = spriteAsset.fallbackSpriteAssets;

                if (fallbackSpriteAsset != null && fallbackSpriteAsset.Count > 0)
                {
                    int fallbackCount = fallbackSpriteAsset.Count;

                    for (int i = 0; i < fallbackCount; i++)
                    {
                        SpriteAsset temp = fallbackSpriteAsset[i];

                        if (temp == null)
                            continue;

                        int id = temp.instanceID;

                        // Try adding asset to search list. If already present skip to the next one otherwise check if it contains the requested character.
                        if (k_SearchedAssets.Add(id) == false)
                            continue;

                        spriteCharacter = GetSpriteCharacterFromSpriteAsset_Internal(unicode, temp, true);

                        if (spriteCharacter != null)
                            return spriteCharacter;
                    }
                }
            }

            return null;
        }


        // =====================================================================
        // FONT ENGINE & FONT FILE MANAGEMENT - Fields, Properties and Functions
        // =====================================================================

        /*
        private static bool k_IsFontEngineInitialized;

        private static bool TryGetCharacterFromFontFile(uint unicode, FontAsset fontAsset, out TMP_Character character)
        {
            character = null;

            // Initialize Font Engine library if not already initialized
            if (k_IsFontEngineInitialized == false)
            {
                FontEngineError error = FontEngine.InitializeFontEngine();

                if (error == 0)
                    k_IsFontEngineInitialized = true;
            }

            // Load the font face for the given font asset.
            // TODO: Add manager to keep track of which font faces are currently loaded.
            FontEngine.LoadFontFace(fontAsset.sourceFontFile, fontAsset.faceInfo.pointSize);

            Glyph glyph = null;
            uint glyphIndex = FontEngine.GetGlyphIndex(unicode);

            // Check if glyph is already contained in the font asset as the same glyph might be referenced by multiple character.
            if (fontAsset.glyphLookupTable.TryGetValue(glyphIndex, out glyph))
            {
                character = fontAsset.AddCharacter_Internal(unicode, glyph);

                return true;
            }

            GlyphLoadFlags glyphLoadFlags = ((GlyphRasterModes)fontAsset.atlasRenderMode & GlyphRasterModes.RASTER_MODE_HINTED) == GlyphRasterModes.RASTER_MODE_HINTED ? GlyphLoadFlags.LOAD_RENDER : GlyphLoadFlags.LOAD_RENDER | GlyphLoadFlags.LOAD_NO_HINTING;

            if (FontEngine.TryGetGlyphWithUnicodeValue(unicode, glyphLoadFlags, out glyph))
            {
                // Add new character to font asset (if needed)
                character = fontAsset.AddCharacter_Internal(unicode, glyph);

                return true;
            }

            return false;
        }


        public static bool TryGetGlyphFromFontFile(uint glyphIndex, FontAsset fontAsset, out Glyph glyph)
        {
            glyph = null;

            // Initialize Font Engine library if not already initialized
            if (k_IsFontEngineInitialized == false)
            {
                FontEngineError error = FontEngine.InitializeFontEngine();

                if (error == 0)
                    k_IsFontEngineInitialized = true;
            }

            // Load the font face for the given font asset.
            // TODO: Add manager to keep track of which font faces are currently loaded.
            FontEngine.LoadFontFace(fontAsset.sourceFontFile, fontAsset.faceInfo.pointSize);

            GlyphLoadFlags glyphLoadFlags = ((GlyphRasterModes)fontAsset.atlasRenderMode & GlyphRasterModes.RASTER_MODE_HINTED) == GlyphRasterModes.RASTER_MODE_HINTED ? GlyphLoadFlags.LOAD_RENDER : GlyphLoadFlags.LOAD_RENDER | GlyphLoadFlags.LOAD_NO_HINTING;

            if (FontEngine.TryGetGlyphWithIndexValue(glyphIndex, glyphLoadFlags, out glyph))
            {
                // Add new glyph to font asset (if needed)
                //fontAsset.AddGlyph_Internal(glyph);

                return true;
            }

            return false;
        }
        */
    }
}
