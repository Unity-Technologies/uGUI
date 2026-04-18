using System.Diagnostics;
using UnityEngine;
using UnityEngine.TextCore;


namespace TMPro
{
    public struct TMP_Vertex
    {
        public Vector3 position;
        public Vector4 uv;
        public Vector2 uv2;
        //public Vector2 uv4;
        public Color32 color;

        public static TMP_Vertex zero { get { return k_Zero; } }

        //public Vector3 normal;
        //public Vector4 tangent;

        static readonly TMP_Vertex k_Zero = new TMP_Vertex();
    }

    /// <summary>
    /// Represents padding or offset values (left, right, top, bottom) for text layout.
    /// </summary>
    /// <remarks>
    /// Used by TextMesh Pro for margin, padding, and underline/overhang offsets. Set
    /// horizontal or vertical to apply the same value to both sides (e.g. left and right).
    /// </remarks>
    /// <example>
    /// <para>Use for text margins or custom layout. Create with horizontal and vertical
    /// or with separate left, right, top, and bottom values.</para>
    /// <code><![CDATA[
    /// TMP_Offset margin = new TMP_Offset(10f, 5f); // left/right 10, top/bottom 5
    /// TMP_Offset padding = new TMP_Offset(2f, 2f, 2f, 2f);
    /// ]]></code>
    /// </example>
    public struct TMP_Offset
    {
        public float left { get { return m_Left; } set { m_Left = value; } }

        public float right { get { return m_Right; } set { m_Right = value; } }

        public float top { get { return m_Top; } set { m_Top = value; } }

        public float bottom { get { return m_Bottom; } set { m_Bottom = value; } }

        public float horizontal { get { return m_Left; } set { m_Left = value; m_Right = value; } }

        public float vertical { get { return m_Top; } set { m_Top = value; m_Bottom = value; } }

        /// <summary>
        /// A TMP_Offset with all values set to zero.
        /// </summary>
        public static TMP_Offset zero { get { return k_ZeroOffset; } }

        // =============================================
        // Private backing fields for public properties.
        // =============================================

        float m_Left;
        float m_Right;
        float m_Top;
        float m_Bottom;

        static readonly TMP_Offset k_ZeroOffset = new TMP_Offset(0F, 0F, 0F, 0F);

        /// <summary>
        /// Creates a TMP_Offset with the given left, right, top, and bottom values.
        /// </summary>
        /// <param name="left">The left edge offset value for the layout.</param>
        /// <param name="right">The right edge offset value for the layout.</param>
        /// <param name="top">The top edge offset value for the layout.</param>
        /// <param name="bottom">The bottom edge offset value for the layout.</param>
        public TMP_Offset(float left, float right, float top, float bottom)
        {
            m_Left = left;
            m_Right = right;
            m_Top = top;
            m_Bottom = bottom;
        }

        /// <summary>
        /// Creates a TMP_Offset with the same horizontal value for left/right and same vertical value for top/bottom.
        /// </summary>
        /// <param name="horizontal">The horizontal offset applied to both left and right edges.</param>
        /// <param name="vertical">The vertical offset applied to both top and bottom edges.</param>
        public TMP_Offset(float horizontal, float vertical)
        {
            m_Left = horizontal;
            m_Right = horizontal;
            m_Top = vertical;
            m_Bottom = vertical;
        }

        public static bool operator ==(TMP_Offset lhs, TMP_Offset rhs)
        {
            return lhs.m_Left == rhs.m_Left &&
                    lhs.m_Right == rhs.m_Right &&
                    lhs.m_Top == rhs.m_Top &&
                    lhs.m_Bottom == rhs.m_Bottom;
        }

        public static bool operator !=(TMP_Offset lhs, TMP_Offset rhs)
        {
            return !(lhs == rhs);
        }

        public static TMP_Offset operator *(TMP_Offset a, float b)
        {
            return new TMP_Offset(a.m_Left * b, a.m_Right * b, a.m_Top * b, a.m_Bottom * b);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(TMP_Offset other)
        {
            return base.Equals(other);
        }
    }


    /// <summary>
    /// Represents the highlight state (color and padding) for a run of text.
    /// </summary>
    public struct HighlightState
    {
        public Color32 color;
        public TMP_Offset padding;

        public HighlightState(Color32 color, TMP_Offset padding)
        {
            this.color = color;
            this.padding = padding;
        }

        public static bool operator ==(HighlightState lhs, HighlightState rhs)
        {
            return lhs.color.Compare(rhs.color) && lhs.padding == rhs.padding;
        }

        public static bool operator !=(HighlightState lhs, HighlightState rhs)
        {
            return !(lhs == rhs);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(HighlightState other)
        {
            return base.Equals(other);
        }
    }
    /// <summary>
    /// Structure containing information about individual text elements (character or sprites).
    /// </summary>
    [DebuggerDisplay("Unicode '{character}'  ({((uint)character).ToString(\"X\")})")]
    public struct TMP_CharacterInfo
    {
        public TMP_TextElementType elementType;

        public char character; // Should be changed to an uint to handle UTF32

        /// <summary>
        /// Index of the character in the source text.
        /// </summary>
        public int index;
        public int stringLength;

        public TMP_TextElement textElement;
        public Glyph alternativeGlyph;
        public TMP_FontAsset fontAsset;
        public Material material;
        public int materialReferenceIndex;
        public bool isUsingAlternateTypeface;

        public float pointSize;

        //public short wordNumber;
        public int lineNumber;
        //public short charNumber;
        public int pageNumber;


        public int vertexIndex;
        public TMP_Vertex vertex_BL;
        public TMP_Vertex vertex_TL;
        public TMP_Vertex vertex_TR;
        public TMP_Vertex vertex_BR;

        public Vector3 topLeft;
        public Vector3 bottomLeft;
        public Vector3 topRight;
        public Vector3 bottomRight;

        public float origin;
        public float xAdvance;
        public float ascender;
        public float baseLine;
        public float descender;
        internal float adjustedAscender;
        internal float adjustedDescender;
        internal float adjustedHorizontalAdvance;

        public float aspectRatio;
        public float scale;
        public Color32 color;
        public Color32 underlineColor;
        public int underlineVertexIndex;
        public Color32 strikethroughColor;
        public int strikethroughVertexIndex;
        public Color32 highlightColor;
        public HighlightState highlightState;
        public FontStyles style;
        public bool isVisible;
        //public bool isIgnoringAlignment;
    }
}
