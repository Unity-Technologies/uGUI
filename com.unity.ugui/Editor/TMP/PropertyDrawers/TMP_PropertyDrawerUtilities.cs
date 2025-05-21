using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;


namespace TMPro.EditorUtilities
{
    internal struct GlyphProxy
    {
        public uint index;
        public GlyphRect glyphRect;
        public GlyphMetrics metrics;
        public int atlasIndex;
    }

    internal static class TMP_PropertyDrawerUtilities
    {
        internal static bool s_RefreshGlyphProxyLookup;
        private static Dictionary<SerializedObject, Dictionary<uint, GlyphProxy>> s_GlyphProxyLookups = new Dictionary<SerializedObject, Dictionary<uint, GlyphProxy>>();

        internal static void ClearGlyphProxyLookups()
        {
            s_GlyphProxyLookups.Clear();
        }

        internal static void RefreshGlyphProxyLookup(SerializedObject so)
        {
            if (!s_GlyphProxyLookups.ContainsKey(so))
                return;

            Dictionary<uint, GlyphProxy> glyphProxyLookup = s_GlyphProxyLookups[so];

            glyphProxyLookup.Clear();
            PopulateGlyphProxyLookupDictionary(so, glyphProxyLookup);

            s_RefreshGlyphProxyLookup = false;
        }

        internal static Dictionary<uint, GlyphProxy> GetGlyphProxyLookupDictionary(SerializedObject so)
        {
            if (s_GlyphProxyLookups.ContainsKey(so))
                return s_GlyphProxyLookups[so];

            Dictionary<uint, GlyphProxy> glyphProxyLookup = new Dictionary<uint, GlyphProxy>();
            PopulateGlyphProxyLookupDictionary(so, glyphProxyLookup);
            s_GlyphProxyLookups.Add(so, glyphProxyLookup);

            return glyphProxyLookup;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="so"></param>
        /// <param name="lookupDictionary"></param>
        static void PopulateGlyphProxyLookupDictionary(SerializedObject so, Dictionary<uint, GlyphProxy> lookupDictionary)
        {
            if (lookupDictionary == null)
                return;

            // Get reference to serialized property for the glyph table
            SerializedProperty glyphTable = so.FindProperty("m_GlyphTable");

            for (int i = 0; i < glyphTable.arraySize; i++)
            {
                SerializedProperty glyphProperty = glyphTable.GetArrayElementAtIndex(i);
                GlyphProxy proxy = GetGlyphProxyFromSerializedProperty(glyphProperty);

                lookupDictionary.Add(proxy.index, proxy);
            }
        }

        internal static GlyphRect GetGlyphRectFromGlyphSerializedProperty(SerializedProperty property)
        {
            SerializedProperty glyphRectProp = property.FindPropertyRelative("m_GlyphRect");

            GlyphRect glyphRect = new GlyphRect();
            glyphRect.x = glyphRectProp.FindPropertyRelative("m_X").intValue;
            glyphRect.y = glyphRectProp.FindPropertyRelative("m_Y").intValue;
            glyphRect.width = glyphRectProp.FindPropertyRelative("m_Width").intValue;
            glyphRect.height = glyphRectProp.FindPropertyRelative("m_Height").intValue;

            return glyphRect;
        }

        internal static GlyphMetrics GetGlyphMetricsFromGlyphSerializedProperty(SerializedProperty property)
        {
            SerializedProperty glyphMetricsProperty = property.FindPropertyRelative("m_Metrics");

            GlyphMetrics glyphMetrics = new GlyphMetrics();
            glyphMetrics.horizontalBearingX = glyphMetricsProperty.FindPropertyRelative("m_HorizontalBearingX").floatValue;
            glyphMetrics.horizontalBearingY = glyphMetricsProperty.FindPropertyRelative("m_HorizontalBearingY").floatValue;
            glyphMetrics.horizontalAdvance = glyphMetricsProperty.FindPropertyRelative("m_HorizontalAdvance").floatValue;
            glyphMetrics.width = glyphMetricsProperty.FindPropertyRelative("m_Width").floatValue;
            glyphMetrics.height = glyphMetricsProperty.FindPropertyRelative("m_Height").floatValue;

            return glyphMetrics;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        internal static GlyphProxy GetGlyphProxyFromSerializedProperty(SerializedProperty property)
        {
            GlyphProxy proxy = new GlyphProxy();
            proxy.index = (uint)property.FindPropertyRelative("m_Index").intValue;
            proxy.glyphRect = GetGlyphRectFromGlyphSerializedProperty(property);
            proxy.metrics = GetGlyphMetricsFromGlyphSerializedProperty(property);
            proxy.atlasIndex = property.FindPropertyRelative("m_AtlasIndex").intValue;

            return proxy;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="serializedObject"></param>
        /// <param name="glyphIndex"></param>
        /// <param name="texture"></param>
        /// <returns></returns>
        internal static bool TryGetAtlasTextureFromSerializedObject(SerializedObject serializedObject, int glyphIndex, out Texture2D texture)
        {
            SerializedProperty atlasTextureProperty = serializedObject.FindProperty("m_AtlasTextures");

            texture = atlasTextureProperty.GetArrayElementAtIndex(glyphIndex).objectReferenceValue as Texture2D;

            if (texture == null)
                return false;

            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="serializedObject"></param>
        /// <param name="texture"></param>
        /// <param name="mat"></param>
        /// <returns></returns>
        internal static bool TryGetMaterial(SerializedObject serializedObject, Texture2D texture, out Material mat)
        {
            GlyphRenderMode atlasRenderMode = (GlyphRenderMode)serializedObject.FindProperty("m_AtlasRenderMode").intValue;

            if (((GlyphRasterModes)atlasRenderMode & GlyphRasterModes.RASTER_MODE_BITMAP) == GlyphRasterModes.RASTER_MODE_BITMAP)
            {
                #if TEXTCORE_FONT_ENGINE_1_5_OR_NEWER
                if (atlasRenderMode == GlyphRenderMode.COLOR || atlasRenderMode == GlyphRenderMode.COLOR_HINTED)
                    mat = TMP_FontAssetEditor.internalRGBABitmapMaterial;
                else
                    mat = TMP_FontAssetEditor.internalBitmapMaterial;
                #else
                mat = TMP_FontAssetEditor.internalBitmapMaterial;
                #endif

                if (mat == null)
                    return false;

                mat.mainTexture = texture;
                mat.color = Color.white;
            }
            else
            {
                mat = TMP_FontAssetEditor.internalSDFMaterial;

                if (mat == null)
                    return false;

                int padding = serializedObject.FindProperty("m_AtlasPadding").intValue;
                mat.mainTexture = texture;
                mat.SetFloat(ShaderUtilities.ID_GradientScale, padding + 1);
            }

            return true;
        }
    }
}
