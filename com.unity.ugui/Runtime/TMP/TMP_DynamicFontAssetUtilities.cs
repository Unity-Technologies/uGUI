using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;


namespace TMPro
{
    internal class TMP_DynamicFontAssetUtilities
    {
        private static TMP_DynamicFontAssetUtilities s_Instance = new TMP_DynamicFontAssetUtilities();

        private Dictionary<ulong, FontReference> s_SystemFontLookup;
        private string[] s_SystemFontPaths;
        private uint s_RegularStyleNameHashCode = 1291372090;

        public struct FontReference
        {
            public string familyName;
            public string styleName;
            public int faceIndex;
            public string filePath;
            public ulong hashCode;

            /// <summary>
            /// Constructor for new FontReference
            /// </summary>
            /// <param name="faceNameAndStyle">String that combines the family name with style name</param>
            /// <param name="index">Index of the font face and style.</param>
            public FontReference(string fontFilePath, string faceNameAndStyle, int index)
            {
                familyName = null;
                styleName = null;
                faceIndex = index;
                uint familyNameHashCode = 0;
                uint styleNameHashCode = 0;
                filePath = fontFilePath;

                int length = faceNameAndStyle.Length;
                char[] conversionArray = new char[length];

                int readingFlag = 0;
                int writingIndex = 0;

                for (int i = 0; i < length; i++)
                {
                    char c = faceNameAndStyle[i];

                    // Read family name
                    if (readingFlag == 0)
                    {
                        bool isSeparator = i + 2 < length && c == ' ' && faceNameAndStyle[i + 1] == '-' && faceNameAndStyle[i + 2] == ' ';

                        if (isSeparator)
                        {
                            readingFlag = 1;
                            this.familyName = new string(conversionArray, 0, writingIndex);
                            i += 2;
                            writingIndex = 0;
                            continue;
                        }

                        familyNameHashCode = (familyNameHashCode << 5) + familyNameHashCode ^ TMP_TextUtilities.ToUpperFast(c);
                        conversionArray[writingIndex++] = c;
                        continue;
                    }

                    // Read style name
                    if (readingFlag == 1)
                    {
                        styleNameHashCode = (styleNameHashCode << 5) + styleNameHashCode ^ TMP_TextUtilities.ToUpperFast(c);
                        conversionArray[writingIndex++] = c;

                        if (i + 1 == length)
                            this.styleName = new string(conversionArray, 0, writingIndex);
                    }
                }

                hashCode = (ulong)styleNameHashCode << 32 | familyNameHashCode;
            }
        }


        void InitializeSystemFontReferenceCache()
        {
            if (s_SystemFontLookup == null)
                s_SystemFontLookup = new Dictionary<ulong, FontReference>();
            else
                s_SystemFontLookup.Clear();

            if (s_SystemFontPaths == null)
                s_SystemFontPaths = Font.GetPathsToOSFonts();

            for (int i = 0; i < s_SystemFontPaths.Length; i++)
            {
                // Load font at the given path
                FontEngineError error = FontEngine.LoadFontFace(s_SystemFontPaths[i]);
                if (error != FontEngineError.Success)
                {
                    Debug.LogWarning("Error [" + error + "] trying to load the font at path [" + s_SystemFontPaths[i] + "].");
                    continue;
                }

                // Get font faces and styles for this font
                string[] fontFaces = FontEngine.GetFontFaces();

                // Iterate over each font face
                for (int j = 0; j < fontFaces.Length; j++)
                {
                    FontReference fontRef = new FontReference(s_SystemFontPaths[i], fontFaces[j], j);

                    if (s_SystemFontLookup.ContainsKey(fontRef.hashCode))
                    {
                        //Debug.Log("<color=#FFFF80>[" + i + "]</color> Family Name <color=#FFFF80>[" + fontRef.familyName + "]</color>   Style Name <color=#FFFF80>[" + fontRef.styleName + "]</color>   Index [" + fontRef.faceIndex + "]   HashCode [" + fontRef.hashCode + "]    Path [" + fontRef.filePath + "].");
                        continue;
                    }

                    // Add font reference to lookup dictionary
                    s_SystemFontLookup.Add(fontRef.hashCode, fontRef);

                    Debug.Log("[" + i + "] Family Name [" + fontRef.familyName + "]   Style Name [" + fontRef.styleName + "]   Index [" + fontRef.faceIndex + "]   HashCode [" + fontRef.hashCode + "]    Path [" + fontRef.filePath + "].");
                }

                // Unload current font face.
                FontEngine.UnloadFontFace();
            }
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="familyName"></param>
        /// <param name="fontRef"></param>
        /// <returns></returns>
        public static bool TryGetSystemFontReference(string familyName, out FontReference fontRef)
        {
            return s_Instance.TryGetSystemFontReferenceInternal(familyName, null, out fontRef);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="familyName"></param>
        /// <param name="styleName"></param>
        /// <param name="fontRef"></param>
        /// <returns></returns>
        public static bool TryGetSystemFontReference(string familyName, string styleName, out FontReference fontRef)
        {
            return s_Instance.TryGetSystemFontReferenceInternal(familyName, styleName, out fontRef);
        }

        bool TryGetSystemFontReferenceInternal(string familyName, string styleName, out FontReference fontRef)
        {
            if (s_SystemFontLookup == null)
                InitializeSystemFontReferenceCache();

            fontRef = new FontReference();

            // Compute family name hash code
            uint familyNameHashCode = TMP_TextUtilities.GetHashCodeCaseInSensitive(familyName);
            uint styleNameHashCode = string.IsNullOrEmpty(styleName) ? s_RegularStyleNameHashCode : TMP_TextUtilities.GetHashCodeCaseInSensitive(styleName);
            ulong key = (ulong)styleNameHashCode << 32 | familyNameHashCode;

            // Lookup font reference
            if (s_SystemFontLookup.ContainsKey(key))
            {
                fontRef = s_SystemFontLookup[key];
                return true;
            }

            // Return if specified family and style name is not found.
            if (styleNameHashCode != s_RegularStyleNameHashCode)
                return false;

            // Return first potential reference for the given family name
            foreach (KeyValuePair<ulong, FontReference> pair in s_SystemFontLookup)
            {
                if (pair.Value.familyName == familyName)
                {
                    fontRef = pair.Value;
                    return true;
                }
            }

            return false;
        }
    }
}
