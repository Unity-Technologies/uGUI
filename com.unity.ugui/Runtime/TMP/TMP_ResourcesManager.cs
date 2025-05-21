﻿using System.Collections.Generic;
using UnityEngine;


namespace TMPro
{
    /// <summary>
    ///
    /// </summary>
    public class TMP_ResourceManager
    {
        // ======================================================
        // TEXT SETTINGS MANAGEMENT
        // ======================================================

        private static TMP_Settings s_TextSettings;

        internal static TMP_Settings GetTextSettings()
        {
            if (s_TextSettings == null)
            {
                // Try loading the TMP Settings from a Resources folder in the user project.
                s_TextSettings = Resources.Load<TMP_Settings>("TextSettings"); // ?? ScriptableObject.CreateInstance<TMP_Settings>();

                #if UNITY_EDITOR
                if (s_TextSettings == null)
                {
                    // Open TMP Resources Importer to enable the user to import the TMP Essential Resources and option TMP Examples & Extras
                    TMP_PackageResourceImporterWindow.ShowPackageImporterWindow();
                }
                #endif
            }

            return s_TextSettings;
        }

        // ======================================================
        // FONT ASSET MANAGEMENT - Fields, Properties and Functions
        // ======================================================

        struct FontAssetRef
        {
            public int nameHashCode;
            public int familyNameHashCode;
            public int styleNameHashCode;
            public long familyNameAndStyleHashCode;
            public readonly TMP_FontAsset fontAsset;

            public FontAssetRef(int nameHashCode, int familyNameHashCode, int styleNameHashCode, TMP_FontAsset fontAsset)
            {
                // Use familyNameHashCode for font assets created at runtime as these asset do not typically have a names.
                this.nameHashCode = nameHashCode != 0 ? nameHashCode : familyNameHashCode;
                this.familyNameHashCode = familyNameHashCode;
                this.styleNameHashCode = styleNameHashCode;
                this.familyNameAndStyleHashCode = (long) styleNameHashCode << 32 | (uint) familyNameHashCode;
                this.fontAsset = fontAsset;
            }
        }

        static readonly Dictionary<int, FontAssetRef> s_FontAssetReferences = new Dictionary<int, FontAssetRef>();
        static readonly Dictionary<int, TMP_FontAsset> s_FontAssetNameReferenceLookup = new Dictionary<int, TMP_FontAsset>();
        static readonly Dictionary<long, TMP_FontAsset> s_FontAssetFamilyNameAndStyleReferenceLookup = new Dictionary<long, TMP_FontAsset>();
        static readonly List<int> s_FontAssetRemovalList = new List<int>(16);

        static readonly int k_RegularStyleHashCode = TMP_TextUtilities.GetHashCode("Regular");

        /// <summary>
        /// Add font asset to resource manager.
        /// </summary>
        /// <param name="fontAsset">Font asset to be added to the resource manager.</param>
        public static void AddFontAsset(TMP_FontAsset fontAsset)
        {
            int instanceID = fontAsset.instanceID;

            if (!s_FontAssetReferences.ContainsKey(instanceID))
            {
                FontAssetRef fontAssetRef = new FontAssetRef(fontAsset.hashCode, fontAsset.familyNameHashCode, fontAsset.styleNameHashCode, fontAsset);
                s_FontAssetReferences.Add(instanceID, fontAssetRef);

                // Add font asset to name reference lookup
                if (!s_FontAssetNameReferenceLookup.ContainsKey(fontAssetRef.nameHashCode))
                    s_FontAssetNameReferenceLookup.Add(fontAssetRef.nameHashCode, fontAsset);

                // Add font asset to family name and style lookup
                if (!s_FontAssetFamilyNameAndStyleReferenceLookup.ContainsKey(fontAssetRef.familyNameAndStyleHashCode))
                    s_FontAssetFamilyNameAndStyleReferenceLookup.Add(fontAssetRef.familyNameAndStyleHashCode, fontAsset);
            }
            else
            {
                FontAssetRef fontAssetRef = s_FontAssetReferences[instanceID];

                // Return if font asset name, family and style name have not changed.
                if (fontAssetRef.nameHashCode == fontAsset.hashCode && fontAssetRef.familyNameHashCode == fontAsset.familyNameHashCode && fontAssetRef.styleNameHashCode == fontAsset.styleNameHashCode)
                    return;

                // Check if font asset name has changed
                if (fontAssetRef.nameHashCode != fontAsset.hashCode)
                {
                    s_FontAssetNameReferenceLookup.Remove(fontAssetRef.nameHashCode);

                    fontAssetRef.nameHashCode = fontAsset.hashCode;

                    if (!s_FontAssetNameReferenceLookup.ContainsKey(fontAssetRef.nameHashCode))
                        s_FontAssetNameReferenceLookup.Add(fontAssetRef.nameHashCode, fontAsset);
                }

                // Check if family or style name has changed
                if (fontAssetRef.familyNameHashCode != fontAsset.familyNameHashCode || fontAssetRef.styleNameHashCode != fontAsset.styleNameHashCode)
                {
                    s_FontAssetFamilyNameAndStyleReferenceLookup.Remove(fontAssetRef.familyNameAndStyleHashCode);

                    fontAssetRef.familyNameHashCode = fontAsset.familyNameHashCode;
                    fontAssetRef.styleNameHashCode = fontAsset.styleNameHashCode;
                    fontAssetRef.familyNameAndStyleHashCode = (long) fontAsset.styleNameHashCode << 32 | (uint) fontAsset.familyNameHashCode;

                    if (!s_FontAssetFamilyNameAndStyleReferenceLookup.ContainsKey(fontAssetRef.familyNameAndStyleHashCode))
                        s_FontAssetFamilyNameAndStyleReferenceLookup.Add(fontAssetRef.familyNameAndStyleHashCode, fontAsset);
                }

                s_FontAssetReferences[instanceID] = fontAssetRef;
            }
        }

        /// <summary>
        /// Remove font asset from resource manager.
        /// </summary>
        /// <param name="fontAsset">Font asset to be removed from the resource manager.</param>
        public static void RemoveFontAsset(TMP_FontAsset fontAsset)
        {
            int instanceID = fontAsset.instanceID;

            if (s_FontAssetReferences.TryGetValue(instanceID, out FontAssetRef reference))
            {
                s_FontAssetNameReferenceLookup.Remove(reference.nameHashCode);
                s_FontAssetFamilyNameAndStyleReferenceLookup.Remove(reference.familyNameAndStyleHashCode);
                s_FontAssetReferences.Remove(instanceID);
            }
        }

        /// <summary>
        /// Try getting a reference to the font asset using the hash code calculated from its file name.
        /// </summary>
        /// <param name="nameHashcode"></param>
        /// <param name="fontAsset"></param>
        /// <returns></returns>
        internal static bool TryGetFontAssetByName(int nameHashcode, out TMP_FontAsset fontAsset)
        {
            fontAsset = null;

            return s_FontAssetNameReferenceLookup.TryGetValue(nameHashcode, out fontAsset);
        }

        /// <summary>
        /// Try getting a reference to the font asset using the hash code calculated from font's family and style name.
        /// </summary>
        /// <param name="familyNameHashCode"></param>
        /// <param name="styleNameHashCode"></param>
        /// <param name="fontAsset"></param>
        /// <returns></returns>
        internal static bool TryGetFontAssetByFamilyName(int familyNameHashCode, int styleNameHashCode, out TMP_FontAsset fontAsset)
        {
            fontAsset = null;

            if (styleNameHashCode == 0)
                styleNameHashCode = k_RegularStyleHashCode;

            long familyAndStyleNameHashCode = (long) styleNameHashCode << 32 | (uint) familyNameHashCode;

            return s_FontAssetFamilyNameAndStyleReferenceLookup.TryGetValue(familyAndStyleNameHashCode, out fontAsset);
        }

        /// <summary>
        /// Clear all font asset glyph lookup cache.
        /// </summary>
        public static void ClearFontAssetGlyphCache()
        {
            RebuildFontAssetCache();
        }

        /// <summary>
        ///
        /// </summary>
        internal static void RebuildFontAssetCache()
        {
            // Iterate over loaded font assets to update affected font assets
            foreach (var pair in s_FontAssetReferences)
            {
                FontAssetRef fontAssetRef = pair.Value;

                TMP_FontAsset fontAsset = fontAssetRef.fontAsset;

                if (fontAsset == null)
                {
                    // Remove font asset from our lookup dictionaries
                    s_FontAssetNameReferenceLookup.Remove(fontAssetRef.nameHashCode);
                    s_FontAssetFamilyNameAndStyleReferenceLookup.Remove(fontAssetRef.familyNameAndStyleHashCode);

                    // Add font asset to our removal list
                    s_FontAssetRemovalList.Add(pair.Key);
                    continue;
                }

                fontAsset.InitializeCharacterLookupDictionary();
                fontAsset.AddSynthesizedCharactersAndFaceMetrics();
            }

            // Remove font assets in our removal list from our font asset references
            for (int i = 0; i < s_FontAssetRemovalList.Count; i++)
            {
                s_FontAssetReferences.Remove(s_FontAssetRemovalList[i]);
            }
            s_FontAssetRemovalList.Clear();

            TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, null);
        }

        // internal static void RebuildFontAssetCache(int instanceID)
        // {
        //     // Iterate over loaded font assets to update affected font assets
        //     for (int i = 0; i < s_FontAssetReferences.Count; i++)
        //     {
        //         TMP_FontAsset fontAsset = s_FontAssetReferences[i];
        //
        //         if (fontAsset.FallbackSearchQueryLookup.Contains(instanceID))
        //             fontAsset.ReadFontAssetDefinition();
        //     }
        // }
    }
}
