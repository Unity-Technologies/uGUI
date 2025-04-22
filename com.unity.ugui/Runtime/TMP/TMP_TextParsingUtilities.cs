using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TMPro
{
    public class TMP_TextParsingUtilities
    {
        private static readonly TMP_TextParsingUtilities s_Instance = new TMP_TextParsingUtilities();

        /// <summary>
        /// Default constructor
        /// </summary>
        static TMP_TextParsingUtilities() { }


        /// <summary>
        /// Get a singleton instance of the TextModuleUtilities.
        /// </summary>
        public static TMP_TextParsingUtilities instance
        {
            get { return s_Instance; }
        }


        /// <summary>
        /// Function returning the hashcode value of a given string.
        /// </summary>
        public static int GetHashCode(string s)
        {
            int hashCode = 0;

            for (int i = 0; i < s.Length; i++)
                hashCode = ((hashCode << 5) + hashCode) ^ ToUpperASCIIFast(s[i]);

            return hashCode;
        }

        public static int GetHashCodeCaseSensitive(string s)
        {
            int hashCode = 0;

            for (int i = 0; i < s.Length; i++)
                hashCode = ((hashCode << 5) + hashCode) ^ s[i];

            return hashCode;
        }


        /// <summary>
        /// Table used to convert character to lowercase.
        /// </summary>
        const string k_LookupStringL = "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@abcdefghijklmnopqrstuvwxyz[-]^_`abcdefghijklmnopqrstuvwxyz{|}~-";

        /// <summary>
        /// Table used to convert character to uppercase.
        /// </summary>
        const string k_LookupStringU = "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[-]^_`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~-";


        /// <summary>
        /// Get lowercase version of this ASCII character.
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ToLowerASCIIFast(char c)
        {
            if (c > k_LookupStringL.Length - 1)
                return c;

            return k_LookupStringL[c];
        }


        /// <summary>
        /// Get uppercase version of this ASCII character.
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ToUpperASCIIFast(char c)
        {
            if (c > k_LookupStringU.Length - 1)
                return c;

            return k_LookupStringU[c];
        }


        /// <summary>
        /// Get uppercase version of this ASCII character.
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ToUpperASCIIFast(uint c)
        {
            if (c > k_LookupStringU.Length - 1)
                return c;

            return k_LookupStringU[(int)c];
        }


        /// <summary>
        /// Get lowercase version of this ASCII character.
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ToLowerASCIIFast(uint c)
        {
            if (c > k_LookupStringL.Length - 1)
                return c;

            return k_LookupStringL[(int)c];
        }


        /// <summary>
        /// Check if Unicode is High Surrogate
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHighSurrogate(uint c)
        {
            return c > 0xD800 && c < 0xDBFF;
        }

        /// <summary>
        /// Check if Unicode is Low Surrogate
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLowSurrogate(uint c)
        {
            return c > 0xDC00 && c < 0xDFFF;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="highSurrogate"></param>
        /// <param name="lowSurrogate"></param>
        /// <returns></returns>
        internal static uint ConvertToUTF32(uint highSurrogate, uint lowSurrogate)
        {
            return ((highSurrogate - CodePoint.HIGH_SURROGATE_START) * 0x400) + ((lowSurrogate - CodePoint.LOW_SURROGATE_START) + CodePoint.UNICODE_PLANE01_START);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        internal static bool IsDiacriticalMark(uint c)
        {
            return c >= 0x300 && c <= 0x36F || c >= 0x1AB0 && c <= 0x1AFF || c >= 0x1DC0 && c <= 0x1DFF || c >= 0x20D0 && c <= 0x20FF || c >= 0xFE20 && c <= 0xFE2F;
        }

        internal static bool IsBaseGlyph(uint c)
        {
            return !(c >= 0x300 && c <= 0x36F || c >= 0x1AB0 && c <= 0x1AFF || c >= 0x1DC0 && c <= 0x1DFF || c >= 0x20D0 && c <= 0x20FF || c >= 0xFE20 && c <= 0xFE2F ||
                // Thai Marks
                c == 0xE31 || c >= 0xE34 && c <= 0xE3A || c >= 0xE47 && c <= 0xE4E ||
                // Hebrew Marks
                c >= 0x591 && c <= 0x5BD || c == 0x5BF || c >= 0x5C1 && c <= 0x5C2 || c >= 0x5C4 && c <= 0x5C5 || c == 0x5C7 ||
                // Arabic Marks
                c >= 0x610 && c <= 0x61A || c >= 0x64B && c <= 0x65F || c == 0x670 || c >= 0x6D6 && c <= 0x6DC || c >= 0x6DF && c <= 0x6E4 || c >= 0x6E7 && c <= 0x6E8 || c >= 0x6EA && c <= 0x6ED ||
                c >= 0x8D3 && c <= 0x8E1 || c >= 0x8E3 && c <= 0x8FF ||
                c >= 0xFBB2 && c <= 0xFBC1
                );
        }

        internal static bool IsEmoji(uint c)
        {
            return /*c == 0x23 || c == 0x2A || c >= 0x30 && c <= 0x39 || c == 0xA9 || c == 0xAE || */
                   c == 0x200D || c == 0x203C || c == 0x2049 || c == 0x20E3 || c == 0x2122 || c == 0x2139 || c >= 0x2194 && c <= 0x2199 || c >= 0x21A9 && c <= 0x21AA || c >= 0x231A && c <= 0x231B || c == 0x2328 || c == 0x2388 || c == 0x23CF || c >= 0x23E9 && c <= 0x23F3 || c >= 0x23F8 && c <= 0x23FA || c == 0x24C2 || c >= 0x25AA && c <= 0x25AB || c == 0x25B6 || c == 0x25C0 || c >= 0x25FB && c <= 0x25FE || c >= 0x2600 && c <= 0x2605 || c >= 0x2607 && c <= 0x2612 || c >= 0x2614 && c <= 0x2685 || c >= 0x2690 && c <= 0x2705 || c >= 0x2708 && c <= 0x2712 || c == 0x2714 || c == 0x2716 || c == 0x271D || c == 0x2721 || c == 0x2728 || c >= 0x2733 && c <= 0x2734 || c == 0x2744 || c == 0x2747 || c == 0x274C || c == 0x274E || c >= 0x2753 && c <= 0x2755 || c == 0x2757 || c >= 0x2763 && c <= 0x2767 || c >= 0x2795 && c <= 0x2797 || c == 0x27A1 || c == 0x27B0 || c == 0x27BF || c >= 0x2934 && c <= 0x2935 || c >= 0x2B05 && c <= 0x2B07 || c >= 0x2B1B && c <= 0x2B1C || c == 0x2B50 || c == 0x2B55 ||
                   c == 0x3030 || c == 0x303D || c == 0x3297 || c == 0x3299 ||
                   c == 0xFE0F ||
                   c >= 0x1F000 && c <= 0x1F0FF || c >= 0x1F10D && c <= 0x1F10F || c == 0x1F12F || c >= 0x1F16C && c <= 0x1F171 || c >= 0x1F17E && c <= 0x1F17F || c == 0x1F18E || c >= 0x1F191 && c <= 0x1F19A || c >= 0x1F1AD && c <= 0x1F1FF || c >= 0x1F201 && c <= 0x1F20F || c == 0x1F21A || c == 0x1F22F || c >= 0x1F232 && c <= 0x1F23A || c >= 0x1F23C && c <= 0x1F23F || c >= 0x1F249 && c <= 0x1F53D || c >= 0x1F546 && c <= 0x1F64F || c >= 0x1F680 && c <= 0x1F6FF || c >= 0x1F774 && c <= 0x1F77F || c >= 0x1F7D5 && c <= 0x1F7FF || c >= 0x1F80C && c <= 0x1F80F || c >= 0x1F848 && c <= 0x1F84F || c >= 0x1F85A && c <= 0x1F85F || c >= 0x1F888 && c <= 0x1F88F || c >= 0x1F8AE && c <= 0x1F8FF || c >= 0x1F90C && c <= 0x1F93A || c >= 0x1F93C && c <= 0x1F945 || c >= 0x1F947 && c <= 0x1FAFF || c >= 0x1FC00 && c <= 0x1FFFD ||
                   c >= 0xE0020 && c <= 0xE007F;
        }

        internal static bool IsHangul(uint c)
        {
            return c >= 0x1100 && c <= 0x11ff || /* Hangul Jamo */
                   c >= 0xA960 && c <= 0xA97F || /* Hangul Jamo Extended-A */
                   c >= 0xD7B0 && c <= 0xD7FF || /* Hangul Jamo Extended-B */
                   c >= 0x3130 && c <= 0x318F || /* Hangul Compatibility Jamo */
                   c >= 0xFFA0 && c <= 0xFFDC || /* Halfwidth Jamo */
                   c >= 0xAC00 && c <= 0xD7AF;   /* Hangul Syllables */
        }

        internal static bool IsCJK(uint c)
        {
            return c >= 0x3100  && c <= 0x312F  || /* Bopomofo */
                   c >= 0x31A0  && c <= 0x31BF  || /* Bopomofo Extended */
                   c >= 0x4E00  && c <= 0x9FFF  || /* CJK Unified Ideographs (Han) */
                   c >= 0x3400  && c <= 0x4DBF  || /* CJK Extension A */
                   c >= 0x20000 && c <= 0x2A6DF || /* CJK Extension B */
                   c >= 0x2A700 && c <= 0x2B73F || /* CJK Extension C */
                   c >= 0x2B740 && c <= 0x2B81F || /* CJK Extension D */
                   c >= 0x2B820 && c <= 0x2CEAF || /* CJK Extension E */
                   c >= 0x2CEB0 && c <= 0x2EBE0 || /* CJK Extension F */
                   c >= 0x30000 && c <= 0x3134A || /* CJK Extension G */
                   c >= 0xF900  && c <= 0xFAFF  || /* CJK Compatibility Ideographs */
                   c >= 0x2F800 && c <= 0x2FA1F || /* CJK Compatibility Ideographs Supplement */
                   c >= 0x2F00  && c <= 0x2FDF  || /* CJK Radicals / Kangxi Radicals */
                   c >= 0x2E80  && c <= 0x2EFF  || /* CJK Radicals Supplement */
                   c >= 0x31C0  && c <= 0x31EF  || /* CJK Strokes */
                   c >= 0x2FF0  && c <= 0x2FFF  || /* Ideographic Description Characters */
                   c >= 0x3040  && c <= 0x309F  || /* Hiragana */
                   c >= 0x1B100 && c <= 0x1B12F || /* Kana Extended-A */
                   c >= 0x1AFF0 && c <= 0x1AFFF || /* Kana Extended-B */
                   c >= 0x1B000 && c <= 0x1B0FF || /* Kana Supplement */
                   c >= 0x1B130 && c <= 0x1B16F || /* Small Kana Extension */
                   c >= 0x3190  && c <= 0x319F  || /* Kanbun */
                   c >= 0x30A0  && c <= 0x30FF  || /* Katakana */
                   c >= 0x31F0  && c <= 0x31FF  || /* Katakana Phonetic Extensions */
                   c >= 0xFF65  && c <= 0xFF9F;    /* Halfwidth Katakana */
        }

    }
}
