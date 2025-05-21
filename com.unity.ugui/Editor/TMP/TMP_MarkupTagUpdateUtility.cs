using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


internal class TMP_MarkupTagUpdateUtility
{
    struct MarkupTagDescriptor
    {
        public string name;
        public string tag;
        public string description;

        public MarkupTagDescriptor(string name, string tag, string description)
        {
            this.name = name;
            this.tag = tag;
            this.description = description;
        }

        public MarkupTagDescriptor(string name)
        {
            this.name = name;
            this.tag = null;
            this.description = null;
        }

        public static MarkupTagDescriptor linefeed = new MarkupTagDescriptor("\n");
    }

    private static MarkupTagDescriptor[] m_MarkupTags =
    {
        new MarkupTagDescriptor("BOLD", "b", "// <b>"),
        new MarkupTagDescriptor("SLASH_BOLD", "/b", "// </b>"),
        new MarkupTagDescriptor("ITALIC", "i", "// <i>"),
        new MarkupTagDescriptor("SLASH_ITALIC", "/i", "// </i>"),
        new MarkupTagDescriptor("UNDERLINE", "u", "// <u>"),
        new MarkupTagDescriptor("SLASH_UNDERLINE", "/u", "// </u>"),
        new MarkupTagDescriptor("STRIKETHROUGH", "s", "// <s>"),
        new MarkupTagDescriptor("SLASH_STRIKETHROUGH", "/s", "// </s>"),
        new MarkupTagDescriptor("SUBSCRIPT", "sub", "// <sub>"),
        new MarkupTagDescriptor("SLASH_SUBSCRIPT", "/sub", "// </sub>"),
        new MarkupTagDescriptor("SUPERSCRIPT", "sup", "// <sup>"),
        new MarkupTagDescriptor("SLASH_SUPERSCRIPT", "/sup", "// </sup>"),
        new MarkupTagDescriptor("MARK", "mark", "// <mark>"),
        new MarkupTagDescriptor("SLASH_MARK", "/mark", "// </mark>"),
        MarkupTagDescriptor.linefeed,

        new MarkupTagDescriptor("COLOR", "color", "// <color>"),
        new MarkupTagDescriptor("SLASH_COLOR", "/color", "// </color>"),
        new MarkupTagDescriptor("ALPHA", "alpha", "// <alpha>"),
        new MarkupTagDescriptor("SLASH_ALPHA", "/alpha", "// </alpha>"),
        MarkupTagDescriptor.linefeed,

        new MarkupTagDescriptor("FONT", "font", "// <font=\"Name of Font Asset\"> or <font family=\"Arial\" style=\"Regular\">" ),
        new MarkupTagDescriptor("SLASH_FONT", "/font", "// </font>"),
        new MarkupTagDescriptor("MATERIAL", "material", "// <material=\"Name of Material Preset\"> or as attribute <font=\"Name of font asset\" material=\"Name of material\">"),
        new MarkupTagDescriptor("SLASH_MATERIAL", "/material", "// </material>"),
        new MarkupTagDescriptor("SIZE", "size", "// <size>"),
        new MarkupTagDescriptor("SLASH_SIZE", "/size", "// </size>"),
        new MarkupTagDescriptor("FONT_WEIGHT", "font-weight", "// <font-weight>"),
        new MarkupTagDescriptor("SLASH_FONT_WEIGHT", "/font-weight", "// </font-weight>"),
        new MarkupTagDescriptor("SCALE", "scale", "// <scale>"),
        new MarkupTagDescriptor("SLASH_SCALE", "/scale", "// </scale>"),
        MarkupTagDescriptor.linefeed,

        new MarkupTagDescriptor("SPRITE", "sprite", "// <sprite>"),
        new MarkupTagDescriptor("STYLE", "style", "// <style>"),
        new MarkupTagDescriptor("SLASH_STYLE", "/style", "// </style>"),
        new MarkupTagDescriptor("GRADIENT", "gradient", "// <gradient>"),
        new MarkupTagDescriptor("SLASH_GRADIENT", "/gradient", "// </gradient>"),
        MarkupTagDescriptor.linefeed,

        new MarkupTagDescriptor("A", "a", "// <a>"),
        new MarkupTagDescriptor("SLASH_A", "/a", "// </a>"),
        new MarkupTagDescriptor("LINK", "link", "// <link>"),
        new MarkupTagDescriptor("SLASH_LINK", "/link", "// </link>"),
        MarkupTagDescriptor.linefeed,

        new MarkupTagDescriptor("POSITION", "pos", "// <pos>"),
        new MarkupTagDescriptor("SLASH_POSITION", "/pos", "// </pos>"),
        new MarkupTagDescriptor("VERTICAL_OFFSET", "voffset","// <voffset>"),
        new MarkupTagDescriptor("SLASH_VERTICAL_OFFSET", "/voffset", "// </voffset>"),
        new MarkupTagDescriptor("ROTATE", "rotate", "// <rotate>"),
        new MarkupTagDescriptor("SLASH_ROTATE", "/rotate", "// </rotate>"),
        new MarkupTagDescriptor("TRANSFORM", "transform","// <transform=\"position, rotation, scale\">"),
        new MarkupTagDescriptor("SLASH_TRANSFORM", "/transform", "// </transform>"),
        new MarkupTagDescriptor("SPACE", "space", "// <space>"),
        new MarkupTagDescriptor("SLASH_SPACE", "/space", "// </space>"),
        new MarkupTagDescriptor("CHARACTER_SPACE", "cspace", "// <cspace>"),
        new MarkupTagDescriptor("SLASH_CHARACTER_SPACE", "/cspace", "// </cspace>"),
        new MarkupTagDescriptor("MONOSPACE", "mspace", "// <mspace>"),
        new MarkupTagDescriptor("SLASH_MONOSPACE", "/mspace", "// </mspace>"),
        new MarkupTagDescriptor("CHARACTER_SPACING", "character-spacing", "// <character-spacing>"),
        new MarkupTagDescriptor("SLASH_CHARACTER_SPACING", "/character-spacing", "// </character-spacing>"),
        MarkupTagDescriptor.linefeed,

        new MarkupTagDescriptor("ALIGN", "align", "// <align>"),
        new MarkupTagDescriptor("SLASH_ALIGN", "/align", "// </align>"),
        new MarkupTagDescriptor("WIDTH", "width", "// <width>"),
        new MarkupTagDescriptor("SLASH_WIDTH", "/width", "// </width>"),
        new MarkupTagDescriptor("MARGIN", "margin", "// <margin>"),
        new MarkupTagDescriptor("SLASH_MARGIN", "/margin", "// </margin>"),
        new MarkupTagDescriptor("MARGIN_LEFT", "margin-left", "// <margin-left>"),
        new MarkupTagDescriptor("MARGIN_RIGHT", "margin-right", "// <margin-right>"),
        new MarkupTagDescriptor("INDENT", "indent", "// <indent>"),
        new MarkupTagDescriptor("SLASH_INDENT", "/indent", "// </indent>"),
        new MarkupTagDescriptor("LINE_INDENT", "line-indent", "// <line-indent>"),
        new MarkupTagDescriptor("SLASH_LINE_INDENT", "/line-indent", "// </line-indent>"),
        new MarkupTagDescriptor("LINE_HEIGHT", "line-height", "// <line-height>"),
        new MarkupTagDescriptor("SLASH_LINE_HEIGHT", "/line-height", "// </line-height>"),
        MarkupTagDescriptor.linefeed,

        new MarkupTagDescriptor("NO_BREAK", "nobr", "// <nobr>"),
        new MarkupTagDescriptor("SLASH_NO_BREAK", "/nobr", "// </nobr>"),
        new MarkupTagDescriptor("NO_PARSE", "noparse","// <noparse>"),
        new MarkupTagDescriptor("SLASH_NO_PARSE", "/noparse", "// </noparse>"),
        new MarkupTagDescriptor("PAGE", "page", "// <page>"),
        new MarkupTagDescriptor("SLASH_PAGE", "/page", "// </page>"),
        MarkupTagDescriptor.linefeed,

        new MarkupTagDescriptor("ACTION", "action", "// <action>"),
        new MarkupTagDescriptor("SLASH_ACTION", "/action", "// </action>"),
        MarkupTagDescriptor.linefeed,

        new MarkupTagDescriptor("CLASS", "class", "// <class>"),
        new MarkupTagDescriptor("TABLE", "table", "// <table>"),
        new MarkupTagDescriptor("SLASH_TABLE", "/table", "// </table>"),
        new MarkupTagDescriptor("TH", "th", "// <th>"),
        new MarkupTagDescriptor("SLASH_TH", "/th", "// </th>"),
        new MarkupTagDescriptor("TR", "tr", "// <tr>"),
        new MarkupTagDescriptor("SLASH_TR", "/tr", "// </tr>"),
        new MarkupTagDescriptor("TD", "td", "// <td>"),
        new MarkupTagDescriptor("SLASH_TD", "/td", "// </td>"),
        MarkupTagDescriptor.linefeed,

        new MarkupTagDescriptor("// Text Styles"),
        new MarkupTagDescriptor("LOWERCASE", "lowercase", "// <lowercase>"),
        new MarkupTagDescriptor("SLASH_LOWERCASE", "/lowercase", "// </lowercase>"),
        new MarkupTagDescriptor("ALLCAPS", "allcaps", "// <allcaps>"),
        new MarkupTagDescriptor("SLASH_ALLCAPS", "/allcaps", "// </allcaps>"),
        new MarkupTagDescriptor("UPPERCASE", "uppercase", "// <uppercase>"),
        new MarkupTagDescriptor("SLASH_UPPERCASE", "/uppercase", "// </uppercase>"),
        new MarkupTagDescriptor("SMALLCAPS", "smallcaps", "// <smallcaps>"),
        new MarkupTagDescriptor("SLASH_SMALLCAPS", "/smallcaps", "// </smallcaps>"),
        new MarkupTagDescriptor("CAPITALIZE", "capitalize", "// <capitalize>"),
        new MarkupTagDescriptor("SLASH_CAPITALIZE", "/capitalize", "// </capitalize>"),
        MarkupTagDescriptor.linefeed,

        new MarkupTagDescriptor("// Font Features"),
        new MarkupTagDescriptor("LIGA", "liga", "// <liga>"),
        new MarkupTagDescriptor("SLASH_LIGA", "/liga", "// </liga>"),
        new MarkupTagDescriptor("FRAC", "frac", "// <frac>"),
        new MarkupTagDescriptor("SLASH_FRAC", "/frac", "// </frac>"),
        MarkupTagDescriptor.linefeed,

        new MarkupTagDescriptor("// Attributes"),
        new MarkupTagDescriptor("NAME", "name", "// <sprite name=\"Name of Sprite\">"),
        new MarkupTagDescriptor("INDEX", "index", "// <sprite index=7>"),
        new MarkupTagDescriptor("TINT", "tint", "// <tint=bool>"),
        new MarkupTagDescriptor("ANIM", "anim", "// <anim=\"first frame, last frame, frame rate\">"),
        new MarkupTagDescriptor("HREF", "href", "// <a href=\"url\">text to be displayed.</a>"),
        new MarkupTagDescriptor("ANGLE", "angle", "// <i angle=\"40\">Italic Slant Angle</i>"),
        new MarkupTagDescriptor("FAMILY", "family", "// <font family=\"Arial\">"),
        MarkupTagDescriptor.linefeed,

        new MarkupTagDescriptor("// Named Colors"),
        new MarkupTagDescriptor("RED", "red",""),
        new MarkupTagDescriptor("GREEN", "green", ""),
        new MarkupTagDescriptor("BLUE", "blue", ""),
        new MarkupTagDescriptor("WHITE", "white", ""),
        new MarkupTagDescriptor("BLACK", "black", ""),
        new MarkupTagDescriptor("CYAN", "cyna", ""),
        new MarkupTagDescriptor("MAGENTA", "magenta", ""),
        new MarkupTagDescriptor("YELLOW", "yellow", ""),
        new MarkupTagDescriptor("ORANGE", "orange", ""),
        new MarkupTagDescriptor("PURPLE", "purple", ""),
        MarkupTagDescriptor.linefeed,

        new MarkupTagDescriptor("// Unicode Characters"),
        new MarkupTagDescriptor("BR", "br", "// <br> Line Feed (LF) \\u0A"),
        new MarkupTagDescriptor("ZWSP", "zwsp", "// <zwsp> Zero Width Space \\u200B"),
        new MarkupTagDescriptor("NBSP", "nbsp", "// <nbsp> Non Breaking Space \\u00A0"),
        new MarkupTagDescriptor("SHY", "shy", "// <shy> Soft Hyphen \\u00AD"),
        new MarkupTagDescriptor("ZWJ", "zwj", "// <zwj> Zero Width Joiner \\u200D"),
        new MarkupTagDescriptor("WJ", "wj", "// <wj> Word Joiner \\u2060"),
        MarkupTagDescriptor.linefeed,

        new MarkupTagDescriptor("// Alignment"),
        new MarkupTagDescriptor("LEFT", "left", "// <align=left>"),
        new MarkupTagDescriptor("RIGHT", "right", "// <align=right>"),
        new MarkupTagDescriptor("CENTER", "center", "// <align=center>"),
        new MarkupTagDescriptor("JUSTIFIED", "justified", "// <align=justified>"),
        new MarkupTagDescriptor("FLUSH", "flush", "// <align=flush>"),
        MarkupTagDescriptor.linefeed,

        new MarkupTagDescriptor("// Prefix and Unit suffix"),
        new MarkupTagDescriptor("NONE", "none", ""),
        new MarkupTagDescriptor("PLUS", "+", ""),
        new MarkupTagDescriptor("MINUS", "-", ""),
        new MarkupTagDescriptor("PX", "px", ""),
        new MarkupTagDescriptor("PLUS_PX", "+px", ""),
        new MarkupTagDescriptor("MINUS_PX", "-px", ""),
        new MarkupTagDescriptor("EM", "em", ""),
        new MarkupTagDescriptor("PLUS_EM", "+em", ""),
        new MarkupTagDescriptor("MINUS_EM", "-em", ""),
        new MarkupTagDescriptor("PCT", "pct", ""),
        new MarkupTagDescriptor("PLUS_PCT", "+pct", ""),
        new MarkupTagDescriptor("MINUS_PCT", "-pct", ""),
        new MarkupTagDescriptor("PERCENTAGE", "%", ""),
        new MarkupTagDescriptor("PLUS_PERCENTAGE", "+%", ""),
        new MarkupTagDescriptor("MINUS_PERCENTAGE", "-%", ""),
        new MarkupTagDescriptor("HASH", "#", "// #"),
        MarkupTagDescriptor.linefeed,

        new MarkupTagDescriptor("TRUE", "true", ""),
        new MarkupTagDescriptor("FALSE", "false", ""),
        MarkupTagDescriptor.linefeed,

        new MarkupTagDescriptor("INVALID", "invalid", ""),
        MarkupTagDescriptor.linefeed,

        new MarkupTagDescriptor("NORMAL", "normal", "// <style=\"Normal\">"),
        new MarkupTagDescriptor("DEFAULT", "default", "// <font=\"Default\">"),
    };


    [MenuItem("Window/TextMeshPro/Internal/Update Markup Tag Hash Codes", false, 2200, true)]
    static void UpdateMarkupTagHashCodes()
    {
        Dictionary<int, MarkupTagDescriptor> markupHashCodes = new Dictionary<int, MarkupTagDescriptor>();
        string output = string.Empty;

        for (int i = 0; i < m_MarkupTags.Length; i++)
        {
            MarkupTagDescriptor descriptor = m_MarkupTags[i];
            int hashCode = descriptor.tag == null ? 0 : GetHashCodeCaseInSensitive(descriptor.tag);

            if (descriptor.name == "\n")
                output += "\n";
            else if (hashCode == 0)
                output += descriptor.name + "\n";
            else
            {
                output += descriptor.name + " = " + hashCode + ",\t" + descriptor.description + "\n";

                if (markupHashCodes.ContainsKey(hashCode) == false)
                    markupHashCodes.Add(hashCode, descriptor);
                else
                    Debug.Log("[" + descriptor.name + "] with HashCode [" + hashCode + "] collides with [" + markupHashCodes[hashCode].name + "].");
            }
        }

        Debug.Log(output);
    }

    /// <summary>
    /// Table used to convert character to uppercase.
    /// </summary>
    const string k_lookupStringU = "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[-]^_`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~-";

    /// <summary>
    /// Get uppercase version of this ASCII character.
    /// </summary>
    public static char ToUpperFast(char c)
    {
        if (c > k_lookupStringU.Length - 1)
            return c;

        return k_lookupStringU[c];
    }

    public static int GetHashCodeCaseInSensitive(string s)
    {
        int hashCode = 5381;

        for (int i = 0; i < s.Length; i++)
            hashCode = (hashCode << 5) + hashCode ^ ToUpperFast(s[i]);

        return hashCode;
    }
}
