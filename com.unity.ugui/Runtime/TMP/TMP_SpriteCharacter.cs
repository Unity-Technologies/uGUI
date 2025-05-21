﻿using System;
using UnityEngine;

namespace TMPro
{
    /// <summary>
    /// A basic element of text representing a pictograph, image, sprite or emoji.
    /// </summary>
    [Serializable]
    public class TMP_SpriteCharacter : TMP_TextElement
    {
        /// <summary>
        /// The name of the sprite element.
        /// </summary>
        public string name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        // =============================================
        // Private backing fields for public properties.
        // =============================================

        [SerializeField]
        private string m_Name;

        // ********************
        // CONSTRUCTORS
        // ********************

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TMP_SpriteCharacter()
        {
            m_ElementType = TextElementType.Sprite;
        }

        /// <summary>
        /// Constructor for new sprite character.
        /// </summary>
        /// <param name="unicode">Unicode value of the sprite character.</param>
        /// <param name="glyph">Glyph used by the sprite character.</param>
        public TMP_SpriteCharacter(uint unicode, TMP_SpriteGlyph glyph)
        {
            m_ElementType = TextElementType.Sprite;

            this.unicode = unicode;
            this.glyphIndex = glyph.index;
            this.glyph = glyph;
            this.scale = 1.0f;
        }

        /// <summary>
        /// Constructor for new sprite character.
        /// </summary>
        /// <param name="unicode">Unicode value of the sprite character.</param>
        /// <param name="spriteAsset">Sprite Asset used by this sprite character.</param>
        /// <param name="glyph">Glyph used by the sprite character.</param>
        public TMP_SpriteCharacter(uint unicode, TMP_SpriteAsset spriteAsset, TMP_SpriteGlyph glyph)
        {
            m_ElementType = TextElementType.Sprite;

            this.unicode = unicode;
            this.textAsset = spriteAsset;
            this.glyph = glyph;
            this.glyphIndex = glyph.index;
            this.scale = 1.0f;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="unicode"></param>
        /// <param name="glyphIndex"></param>
        internal TMP_SpriteCharacter(uint unicode, uint glyphIndex)
        {
            m_ElementType = TextElementType.Sprite;

            this.unicode = unicode;
            this.textAsset = null;
            this.glyph = null;
            this.glyphIndex = glyphIndex;
            this.scale = 1.0f;
        }
    }
}
