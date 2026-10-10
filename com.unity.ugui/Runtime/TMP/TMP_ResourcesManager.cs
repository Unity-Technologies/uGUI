using System.Collections.Generic;
using UnityEngine;


namespace TMPro
{
    /// <summary>
    /// Central registry for TextMesh Pro font assets and access to the loaded <see cref="TMP_Settings"/> asset.
    /// </summary>
    public static class TMP_ResourceManager
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

        static readonly Dictionary<EntityId, FontAssetRef> s_FontAssetReferences = new();
        static readonly Dictionary<int, TMP_FontAsset> s_FontAssetNameReferenceLookup = new();
        static readonly Dictionary<long, TMP_FontAsset> s_FontAssetFamilyNameAndStyleReferenceLookup = new();
        static readonly List<EntityId> s_FontAssetRemovalList = new(16);

        static readonly int k_RegularStyleHashCode = TMP_TextUtilities.GetHashCode("Regular");

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void ResetStaticsOnLoad()
        {
            s_TextSettings = default;
            s_FontAssetReferences.Clear();
            s_FontAssetNameReferenceLookup.Clear();
            s_FontAssetFamilyNameAndStyleReferenceLookup.Clear();
            s_FontAssetRemovalList.Clear();
        }
#endif

        /// <summary>
        /// Add font asset to resource manager.
        /// </summary>
        /// <param name="fontAsset">Font asset to be added to the resource manager.</param>
        public static void AddFontAsset(TMP_FontAsset fontAsset)
        {
            EntityId entityId = fontAsset.entityId;

            if (!s_FontAssetReferences.ContainsKey(entityId))
            {
                RemoveDestroyedFontAssetReferences();

                FontAssetRef fontAssetRef = new FontAssetRef(fontAsset.hashCode, fontAsset.familyNameHashCode, fontAsset.styleNameHashCode, fontAsset);
                s_FontAssetReferences.Add(entityId, fontAssetRef);

                AddToLookup(s_FontAssetNameReferenceLookup, fontAssetRef.nameHashCode, fontAsset);
                AddToLookup(s_FontAssetFamilyNameAndStyleReferenceLookup, fontAssetRef.familyNameAndStyleHashCode, fontAsset);
            }
            else
            {
                FontAssetRef fontAssetRef = s_FontAssetReferences[entityId];

                // Return if font asset name, family and style name have not changed.
                if (fontAssetRef.nameHashCode == fontAsset.hashCode && fontAssetRef.familyNameHashCode == fontAsset.familyNameHashCode && fontAssetRef.styleNameHashCode == fontAsset.styleNameHashCode)
                    return;

                // Check if font asset name has changed
                if (fontAssetRef.nameHashCode != fontAsset.hashCode)
                {
                    RemoveFromLookup(s_FontAssetNameReferenceLookup, fontAssetRef.nameHashCode, fontAsset);

                    fontAssetRef.nameHashCode = fontAsset.hashCode;

                    AddToLookup(s_FontAssetNameReferenceLookup, fontAssetRef.nameHashCode, fontAsset);
                }

                // Check if family or style name has changed
                if (fontAssetRef.familyNameHashCode != fontAsset.familyNameHashCode || fontAssetRef.styleNameHashCode != fontAsset.styleNameHashCode)
                {
                    RemoveFromLookup(s_FontAssetFamilyNameAndStyleReferenceLookup, fontAssetRef.familyNameAndStyleHashCode, fontAsset);

                    fontAssetRef.familyNameHashCode = fontAsset.familyNameHashCode;
                    fontAssetRef.styleNameHashCode = fontAsset.styleNameHashCode;
                    fontAssetRef.familyNameAndStyleHashCode = (long) fontAsset.styleNameHashCode << 32 | (uint) fontAsset.familyNameHashCode;

                    AddToLookup(s_FontAssetFamilyNameAndStyleReferenceLookup, fontAssetRef.familyNameAndStyleHashCode, fontAsset);
                }

                s_FontAssetReferences[entityId] = fontAssetRef;
            }
        }

        /// <summary>
        /// Remove font asset from resource manager.
        /// </summary>
        /// <param name="fontAsset">Font asset to be removed from the resource manager.</param>
        public static void RemoveFontAsset(TMP_FontAsset fontAsset)
        {
            EntityId entityId = fontAsset.entityId;

            if (!s_FontAssetReferences.TryGetValue(entityId, out FontAssetRef reference))
                return;

            s_FontAssetReferences.Remove(entityId);

            bool ownedNameLookup = RemoveFromLookup(s_FontAssetNameReferenceLookup, reference.nameHashCode, fontAsset);
            bool ownedFamilyLookup = RemoveFromLookup(s_FontAssetFamilyNameAndStyleReferenceLookup, reference.familyNameAndStyleHashCode, fontAsset);

            if (ownedNameLookup || ownedFamilyLookup)
                ReassignLookupsToRegisteredFontAssets();
        }

        internal static int registeredFontAssetCount => s_FontAssetReferences.Count;

        static void AddToLookup<TKey>(Dictionary<TKey, TMP_FontAsset> lookup, TKey key, TMP_FontAsset fontAsset)
        {
            if (!lookup.TryGetValue(key, out TMP_FontAsset owner) || owner == null)
                lookup[key] = fontAsset;
        }

        static bool RemoveFromLookup<TKey>(Dictionary<TKey, TMP_FontAsset> lookup, TKey key, TMP_FontAsset fontAsset)
        {
            if (!lookup.TryGetValue(key, out TMP_FontAsset owner) || !ReferenceEquals(owner, fontAsset))
                return false;

            lookup.Remove(key);
            return true;
        }

        static void RemoveDestroyedFontAssetReferences()
        {
            foreach (var pair in s_FontAssetReferences)
            {
                FontAssetRef reference = pair.Value;

                if (reference.fontAsset != null)
                    continue;

                RemoveFromLookup(s_FontAssetNameReferenceLookup, reference.nameHashCode, reference.fontAsset);
                RemoveFromLookup(s_FontAssetFamilyNameAndStyleReferenceLookup, reference.familyNameAndStyleHashCode, reference.fontAsset);
                s_FontAssetRemovalList.Add(pair.Key);
            }

            if (s_FontAssetRemovalList.Count == 0)
                return;

            for (int i = 0; i < s_FontAssetRemovalList.Count; i++)
            {
                s_FontAssetReferences.Remove(s_FontAssetRemovalList[i]);
            }
            s_FontAssetRemovalList.Clear();

            ReassignLookupsToRegisteredFontAssets();
        }

        static void ReassignLookupsToRegisteredFontAssets()
        {
            foreach (var pair in s_FontAssetReferences)
            {
                FontAssetRef reference = pair.Value;

                if (reference.fontAsset == null)
                    continue;

                AddToLookup(s_FontAssetNameReferenceLookup, reference.nameHashCode, reference.fontAsset);
                AddToLookup(s_FontAssetFamilyNameAndStyleReferenceLookup, reference.familyNameAndStyleHashCode, reference.fontAsset);
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
            return TryGetLiveFontAsset(s_FontAssetNameReferenceLookup, nameHashcode, out fontAsset);
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

            return TryGetLiveFontAsset(s_FontAssetFamilyNameAndStyleReferenceLookup, familyAndStyleNameHashCode, out fontAsset);
        }

        static bool TryGetLiveFontAsset<TKey>(Dictionary<TKey, TMP_FontAsset> lookup, TKey key, out TMP_FontAsset fontAsset)
        {
            if (lookup.TryGetValue(key, out fontAsset) && fontAsset == null)
            {
                RemoveDestroyedFontAssetReferences();
                lookup.TryGetValue(key, out fontAsset);
            }

            return fontAsset != null;
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
            RemoveDestroyedFontAssetReferences();

            // Iterate over loaded font assets to update affected font assets
            foreach (var pair in s_FontAssetReferences)
            {
                TMP_FontAsset fontAsset = pair.Value.fontAsset;

                fontAsset.InitializeCharacterLookupDictionary();
                fontAsset.AddSynthesizedCharactersAndFaceMetrics();
            }

            TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, null);
        }

        // internal static void RebuildFontAssetCache(EntityId entityId)
        // {
        //     // Iterate over loaded font assets to update affected font assets
        //     for (int i = 0; i < s_FontAssetReferences.Count; i++)
        //     {
        //         TMP_FontAsset fontAsset = s_FontAssetReferences[i];
        //
        //         if (fontAsset.FallbackSearchQueryLookup.Contains(entityId))
        //             fontAsset.ReadFontAssetDefinition();
        //     }
        // }
    }
}
