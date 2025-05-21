using System;
using UnityEngine;

#pragma warning disable CS0660, CS0661

namespace TMPro
{
    /// <summary>
    /// The SingleSubstitutionRecord defines the substitution of a single glyph by another.
    /// </summary>
    [Serializable]
    public struct SingleSubstitutionRecord
    {
        //
    }

    /// <summary>
    /// The MultipleSubstitutionRecord defines the substitution of a single glyph by multiple glyphs.
    /// </summary>
    [Serializable]
    public struct MultipleSubstitutionRecord
    {
        /// <summary>
        /// The index of the target glyph being substituted.
        /// </summary>
        public uint targetGlyphID { get { return m_TargetGlyphID; } set { m_TargetGlyphID = value; } }

        /// <summary>
        /// Array that contains the index of the glyphs replacing the single target glyph.
        /// </summary>
        public uint[] substituteGlyphIDs { get { return m_SubstituteGlyphIDs; } set { m_SubstituteGlyphIDs = value; } }

        // =============================================
        // Private backing fields for public properties.
        // =============================================

        [SerializeField]
        private uint m_TargetGlyphID;

        [SerializeField]
        private uint[] m_SubstituteGlyphIDs;
    }

    /// <summary>
    /// The AlternateSubstitutionRecord defines the substitution of a single glyph by several potential alternative glyphs.
    /// </summary>
    [Serializable]
    public struct AlternateSubstitutionRecord
    {

    }

    /// <summary>
    /// The LigatureSubstitutionRecord defines the substitution of multiple glyphs by a single glyph.
    /// </summary>
    [Serializable]
    public struct LigatureSubstitutionRecord
    {
        /// <summary>
        /// Array that contains the index of the glyphs being substituted.
        /// </summary>
        public uint[] componentGlyphIDs { get { return m_ComponentGlyphIDs; } set { m_ComponentGlyphIDs = value; } }

        /// <summary>
        /// The index of the replacement glyph.
        /// </summary>
        public uint ligatureGlyphID { get { return m_LigatureGlyphID; } set { m_LigatureGlyphID = value; } }

        // =============================================
        // Private backing fields for public properties.
        // =============================================

        [SerializeField]
        private uint[] m_ComponentGlyphIDs;

        [SerializeField]
        private uint m_LigatureGlyphID;
        
        // =============================================
        // Operator overrides
        // =============================================

        public static bool operator==(LigatureSubstitutionRecord lhs, LigatureSubstitutionRecord rhs)
        {
            if (lhs.ligatureGlyphID != rhs.m_LigatureGlyphID)
                return false;
            
            int lhsComponentCount = lhs.m_ComponentGlyphIDs.Length;
            
            if (lhsComponentCount != rhs.m_ComponentGlyphIDs.Length)
                return false;

            for (int i = 0; i < lhsComponentCount; i++)
            {
                if (lhs.m_ComponentGlyphIDs[i] != rhs.m_ComponentGlyphIDs[i])
                    return false;
            }

            return true;
        }

        public static bool operator!=(LigatureSubstitutionRecord lhs, LigatureSubstitutionRecord rhs)
        {
            return !(lhs == rhs);
        }
    }
}
