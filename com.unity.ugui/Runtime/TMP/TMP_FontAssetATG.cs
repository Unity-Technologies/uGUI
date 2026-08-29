using System;
using System.Collections.Generic;
using UnityEngine;
using TextCoreFontAsset = UnityEngine.TextCore.Text.FontAsset;

namespace TMPro
{
    // Native font asset bridge used when text generation is served by the Advanced Text Generator (ATG).
    public partial class TMP_FontAsset
    {
        IntPtr m_NativeFontAsset = IntPtr.Zero;

        internal IntPtr nativeFontAsset
        {
            get
            {
                EnsureNativeFontAssetIsCreated();
                return m_NativeFontAsset;
            }
        }

        internal void EnsureNativeFontAssetIsCreated()
        {
            if (m_NativeFontAsset != IntPtr.Zero)
                return;

            if (atlasPopulationMode == AtlasPopulationMode.Static && characterTable.Count > 0)
            {
                Debug.LogWarning($"The Advanced Text Generator cannot use static font asset {name}.", this);
                return;
            }

            if (atlasPopulationMode == AtlasPopulationMode.Dynamic && sourceFontFile == null)
            {
                Debug.LogWarning($"{name} font asset is invalid. Please assign a Source Font File.", this);
                return;
            }

            var fallbacks = GetNativeFallbacks();
            var (regularWeights, italicWeights) = GetNativeWeightFallbacks();

            Font sourceFontEditorRef = null;
#if UNITY_EDITOR
            sourceFontEditorRef = SourceFont_EditorRef;
#endif

            m_NativeFontAsset = TextCoreFontAsset.CreateNativeFontAsset(m_FaceInfo, m_SourceFontFile, sourceFontEditorRef, m_SourceFontFilePath, this.GetEntityId(), fallbacks, regularWeights, italicWeights, m_AtlasRenderMode, italicStyle, boldStyle, (int)(boldSpacing * 64.0f), this);
        }

        void OnDisable()
        {
            DestroyNativeFontAsset();
        }

        internal void DestroyNativeFontAsset()
        {
            if (m_NativeFontAsset == IntPtr.Zero)
                return;

            TextCoreFontAsset.DestroyNativeFontAsset(m_NativeFontAsset, this);
            m_NativeFontAsset = IntPtr.Zero;
        }

        internal void UpdateNativeFallbacks()
        {
            if (m_NativeFontAsset == IntPtr.Zero)
                return;

            TextCoreFontAsset.UpdateNativeFallbacks(m_NativeFontAsset, GetNativeFallbacks());
        }

        // Pushes a swapped source font (e.g. a subset) into the existing native font asset.
        internal void UpdateSourceFontFile()
        {
            if (m_NativeFontAsset == IntPtr.Zero)
                return;

            TextCoreFontAsset.UpdateNativeSourceFontFile(m_NativeFontAsset, m_SourceFontFile);
        }

        internal IntPtr[] GetNativeFallbacks()
        {
            var fallbackList = new List<IntPtr>();
            if (fallbackFontAssetTable == null)
                return fallbackList.ToArray();

            foreach (var fallback in fallbackFontAssetTable)
            {
                if (fallback == null)
                    continue;

                if (fallback.atlasPopulationMode == AtlasPopulationMode.Static && fallback.characterTable.Count > 0)
                {
                    Debug.LogWarning($"The Advanced Text Generator cannot use static font asset {fallback.name} as fallback.", fallback);
                    continue;
                }

                if (HasFallbackRecursion(fallback))
                    continue;

                // When native creation fails (e.g. missing source font), hoist the fallback's own
                // chain so its descendants stay reachable instead of dropping the whole subtree
                var nativeFallback = fallback.nativeFontAsset;
                if (nativeFallback == IntPtr.Zero)
                {
                    fallbackList.AddRange(fallback.GetNativeFallbacks());
                    continue;
                }

                fallbackList.Add(nativeFallback);
            }
            return fallbackList.ToArray();
        }

        (IntPtr[], IntPtr[]) GetNativeWeightFallbacks()
        {
            // Font weight fallback arrays must be exactly size 10 to match the native mapping
            // where each index corresponds to a font weight (index * 100).
            IntPtr[] regularTypefaces = new IntPtr[10];
            IntPtr[] italicTypefaces = new IntPtr[10];

            for (int i = 0; i < fontWeightTable.Length && i < 10; i++)
            {
                var pair = fontWeightTable[i];
                if (pair.regularTypeface != null && !HasFallbackRecursion(pair.regularTypeface))
                    regularTypefaces[i] = pair.regularTypeface.nativeFontAsset;

                if (pair.italicTypeface != null && !HasFallbackRecursion(pair.italicTypeface))
                    italicTypefaces[i] = pair.italicTypeface.nativeFontAsset;
            }

            return (regularTypefaces, italicTypefaces);
        }

        static readonly HashSet<EntityId> s_VisitedFontAssets = new HashSet<EntityId>();

        bool HasFallbackRecursion(TMP_FontAsset fontAsset)
        {
            s_VisitedFontAssets.Clear();
            s_VisitedFontAssets.Add(this.GetEntityId());
            return HasFallbackRecursionInternal(fontAsset);
        }

        static bool HasFallbackRecursionInternal(TMP_FontAsset fontAsset)
        {
            if (!s_VisitedFontAssets.Add(fontAsset.GetEntityId()))
                return true;

            if (fontAsset.fallbackFontAssetTable != null)
            {
                foreach (var child in fontAsset.fallbackFontAssetTable)
                {
                    if (child == null)
                        continue;

                    if (HasFallbackRecursionInternal(child))
                        return true;
                }
            }

            for (int i = 0; i < fontAsset.fontWeightTable.Length; i++)
            {
                var pair = fontAsset.fontWeightTable[i];
                if (pair.regularTypeface != null && HasFallbackRecursionInternal(pair.regularTypeface))
                    return true;

                if (pair.italicTypeface != null && HasFallbackRecursionInternal(pair.italicTypeface))
                    return true;
            }

            s_VisitedFontAssets.Remove(fontAsset.GetEntityId());

            return false;
        }
    }
}
