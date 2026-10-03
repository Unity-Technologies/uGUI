#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TMPro
{
    // In memory only: without a FilePath it is never written to disk, but it survives domain reloads.
    class TMP_OSFallbackFontAssetStore : ScriptableSingleton<TMP_OSFallbackFontAssetStore>
    {
        [SerializeField]
        List<TMP_FontAsset> m_FontAssets = new List<TMP_FontAsset>();

        internal List<TMP_FontAsset> fontAssets => m_FontAssets;
    }
}
#endif
