using System;
using UnityEngine.Serialization;

namespace UnityEngine.UI
{
    [Serializable]
    public struct SpriteState
    {
        [FormerlySerializedAs("highlightedSprite")]
        [FormerlySerializedAs("m_SelectedSprite")]
        [SerializeField]
        private Sprite m_HighlightedSprite;

        [FormerlySerializedAs("pressedSprite")]
        [SerializeField]
        private Sprite m_PressedSprite;

        [FormerlySerializedAs("disabledSprite")]
        [SerializeField]
        private Sprite m_DisabledSprite;

        public Sprite highlightedSprite    { get { return m_HighlightedSprite; } set { m_HighlightedSprite = value; } }
        public Sprite pressedSprite     { get { return m_PressedSprite; } set { m_PressedSprite = value; } }
        public Sprite disabledSprite    { get { return m_DisabledSprite; } set { m_DisabledSprite = value; } }
    }
}
