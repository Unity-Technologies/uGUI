using System;
using UnityEngine.Serialization;

namespace UnityEngine.UI
{
    [Serializable]
    public struct ColorBlock : IEquatable<ColorBlock>
    {
        [FormerlySerializedAs("normalColor")]
        [SerializeField]
        private Color m_NormalColor;

        [FormerlySerializedAs("highlightedColor")]
        [FormerlySerializedAs("m_SelectedColor")]
        [SerializeField]
        private Color m_HighlightedColor;

        [FormerlySerializedAs("pressedColor")]
        [SerializeField]
        private Color m_PressedColor;

        [FormerlySerializedAs("disabledColor")]
        [SerializeField]
        private Color m_DisabledColor;

        [Range(1, 5)]
        [SerializeField]
        private float m_ColorMultiplier;

        [FormerlySerializedAs("fadeDuration")]
        [SerializeField]
        private float m_FadeDuration;

        public Color normalColor       { get { return m_NormalColor; } set { m_NormalColor = value; } }
        public Color highlightedColor  { get { return m_HighlightedColor; } set { m_HighlightedColor = value; } }
        public Color pressedColor      { get { return m_PressedColor; } set { m_PressedColor = value; } }
        public Color disabledColor     { get { return m_DisabledColor; } set { m_DisabledColor = value; } }
        public float colorMultiplier   { get { return m_ColorMultiplier; } set { m_ColorMultiplier = value; } }
        public float fadeDuration      { get { return m_FadeDuration; } set { m_FadeDuration = value; } }

        public static ColorBlock defaultColorBlock
        {
            get
            {
                var c = new ColorBlock
                {
                    m_NormalColor      = new Color32(255, 255, 255, 255),
                    m_HighlightedColor = new Color32(245, 245, 245, 255),
                    m_PressedColor     = new Color32(200, 200, 200, 255),
                    m_DisabledColor    = new Color32(200, 200, 200, 128),
                    colorMultiplier    = 1.0f,
                    fadeDuration       = 0.1f
                };
                return c;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ColorBlock))
                return false;

            return Equals((ColorBlock)obj);
        }

        public bool Equals(ColorBlock other)
        {
            return normalColor == other.normalColor &&
                highlightedColor == other.highlightedColor &&
                pressedColor == other.pressedColor &&
                disabledColor == other.disabledColor &&
                colorMultiplier == other.colorMultiplier &&
                fadeDuration == other.fadeDuration;
        }

        public static bool operator==(ColorBlock point1, ColorBlock point2)
        {
            return point1.Equals(point2);
        }

        public static bool operator!=(ColorBlock point1, ColorBlock point2)
        {
            return !point1.Equals(point2);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
