using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    /// <summary>
    /// Labels are graphics that display text.
    /// </summary>

    [AddComponentMenu("UI/Text", 11)]
    public class Text : MaskableGraphic, ILayoutElement
    {
        [SerializeField] private FontData m_FontData = FontData.defaultFontData;

        [TextArea(3, 10)][SerializeField] protected string m_Text = String.Empty;

        private TextGenerator m_TextCache;
        private TextGenerator m_TextCacheForLayout;

        private static float kEpsilon = 0.0001f;
        static protected Material s_DefaultText = null;

        // We use this flag instead of Unregistering/Registering the callback to avoid allocation.
        [NonSerialized] private bool m_DisableFontTextureRebuiltCallback = false;

        protected Text()
        {}

        /// <summary>
        /// Get or set the material used by this Text.
        /// </summary>

        public TextGenerator cachedTextGenerator
        {
            get { return m_TextCache ?? (m_TextCache = m_Text.Length != 0 ? new TextGenerator(m_Text.Length) : new TextGenerator()); }
        }

        public TextGenerator cachedTextGeneratorForLayout
        {
            get { return m_TextCacheForLayout ?? (m_TextCacheForLayout = new TextGenerator()); }
        }

        public override Material defaultMaterial
        {
            get
            {
                if (s_DefaultText == null)
                    s_DefaultText = Canvas.GetDefaultCanvasTextMaterial();
                return s_DefaultText;
            }
        }

        /// <summary>
        /// Text's texture comes from the font.
        /// </summary>
        public override Texture mainTexture
        {
            get
            {
                if (font != null && font.material != null && font.material.mainTexture != null)
                    return font.material.mainTexture;

                if (m_Material != null)
                    return m_Material.mainTexture;

                return base.mainTexture;
            }
        }

        public void FontTextureChanged()
        {
            // Only invoke if we are not destroyed.
            if (!this)
            {
                FontUpdateTracker.UntrackText(this);
                return;
            }

            if (m_DisableFontTextureRebuiltCallback)
                return;

            cachedTextGenerator.Invalidate();

            if (!IsActive())
                return;

            // this is a bit hacky, but it is currently the
            // cleanest solution....
            // if we detect the font texture has changed and are in a rebuild loop
            // we just regenerate the verts for the new UV's
            if (CanvasUpdateRegistry.IsRebuildingGraphics() || CanvasUpdateRegistry.IsRebuildingLayout())
                UpdateGeometry();
            else
                SetAllDirty();
        }

        public Font font
        {
            get
            {
                return m_FontData.font;
            }
            set
            {
                if (m_FontData.font == value)
                    return;

                FontUpdateTracker.UntrackText(this);

                m_FontData.font = value;

                FontUpdateTracker.TrackText(this);

                SetAllDirty();
            }
        }

        /// <summary>
        /// Text that's being displayed by the Text.
        /// </summary>

        public virtual string text
        {
            get
            {
                return m_Text;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    if (String.IsNullOrEmpty(m_Text))
                        return;
                    m_Text = "";
                    SetVerticesDirty();
                }
                else if (m_Text != value)
                {
                    m_Text = value;
                    SetVerticesDirty();
                    SetLayoutDirty();
                }
            }
        }

        /// <summary>
        /// Whether this Text will support rich text.
        /// </summary>

        public bool supportRichText
        {
            get
            {
                return m_FontData.richText;
            }
            set
            {
                if (m_FontData.richText == value)
                    return;
                m_FontData.richText = value;
                SetVerticesDirty();
                SetLayoutDirty();
            }
        }

        /// <summary>
        /// Wrap mode used by the text.
        /// </summary>

        public bool resizeTextForBestFit
        {
            get
            {
                return m_FontData.bestFit;
            }
            set
            {
                if (m_FontData.bestFit == value)
                    return;
                m_FontData.bestFit = value;
                SetVerticesDirty();
                SetLayoutDirty();
            }
        }

        public int resizeTextMinSize
        {
            get
            {
                return m_FontData.minSize;
            }
            set
            {
                if (m_FontData.minSize == value)
                    return;
                m_FontData.minSize = value;

                SetVerticesDirty();
                SetLayoutDirty();
            }
        }

        public int resizeTextMaxSize
        {
            get
            {
                return m_FontData.maxSize;
            }
            set
            {
                if (m_FontData.maxSize == value)
                    return;
                m_FontData.maxSize = value;

                SetVerticesDirty();
                SetLayoutDirty();
            }
        }

        /// <summary>
        /// Alignment anchor used by the text.
        /// </summary>

        public TextAnchor alignment
        {
            get
            {
                return m_FontData.alignment;
            }
            set
            {
                if (m_FontData.alignment == value)
                    return;
                m_FontData.alignment = value;

                SetVerticesDirty();
                SetLayoutDirty();
            }
        }

        public int fontSize
        {
            get
            {
                return m_FontData.fontSize;
            }
            set
            {
                if (m_FontData.fontSize == value)
                    return;
                m_FontData.fontSize = value;

                SetVerticesDirty();
                SetLayoutDirty();
            }
        }

        public HorizontalWrapMode horizontalOverflow
        {
            get
            {
                return m_FontData.horizontalOverflow;
            }
            set
            {
                if (m_FontData.horizontalOverflow == value)
                    return;
                m_FontData.horizontalOverflow = value;

                SetVerticesDirty();
                SetLayoutDirty();
            }
        }

        public VerticalWrapMode verticalOverflow
        {
            get
            {
                return m_FontData.verticalOverflow;
            }
            set
            {
                if (m_FontData.verticalOverflow == value)
                    return;
                m_FontData.verticalOverflow = value;

                SetVerticesDirty();
                SetLayoutDirty();
            }
        }

        public float lineSpacing
        {
            get
            {
                return m_FontData.lineSpacing;
            }
            set
            {
                if (m_FontData.lineSpacing == value)
                    return;
                m_FontData.lineSpacing = value;

                SetVerticesDirty();
                SetLayoutDirty();
            }
        }

        /// <summary>
        /// Font style used by the Text's text.
        /// </summary>

        public FontStyle fontStyle
        {
            get
            {
                return m_FontData.fontStyle;
            }
            set
            {
                if (m_FontData.fontStyle == value)
                    return;
                m_FontData.fontStyle = value;

                SetVerticesDirty();
                SetLayoutDirty();
            }
        }

        public float  pixelsPerUnit
        {
            get
            {
                var localCanvas = canvas;
                if (!localCanvas)
                    return 1;
                // For dynamic fonts, ensure we use one pixel per pixel on the screen.
                if (!font || font.dynamic)
                    return localCanvas.scaleFactor;
                // For non-dynamic fonts, calculate pixels per unit based on specified font size relative to font object's own font size.
                if (m_FontData.fontSize <= 0 || font.fontSize <= 0)
                    return 1;
                return font.fontSize / (float)m_FontData.fontSize;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            cachedTextGenerator.Invalidate();
            FontUpdateTracker.TrackText(this);
        }

        protected override void OnDisable()
        {
            FontUpdateTracker.UntrackText(this);
            base.OnDisable();
        }

        protected override void UpdateGeometry()
        {
            if (font != null)
            {
                base.UpdateGeometry();
            }
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

#endif

        public TextGenerationSettings GetGenerationSettings(Vector2 extents)
        {
            var settings = new TextGenerationSettings();

            // Settings affected by pixels density
            var pixelsPerUnitCached = pixelsPerUnit;
            settings.generationExtents = extents * pixelsPerUnitCached + Vector2.one * kEpsilon;
            if (font != null && font.dynamic)
            {
                settings.fontSize = Mathf.Min(Mathf.FloorToInt(m_FontData.fontSize * pixelsPerUnitCached), 1000);
                settings.resizeTextMinSize = Mathf.Min(Mathf.FloorToInt(m_FontData.minSize * pixelsPerUnitCached), 1000);
                settings.resizeTextMaxSize = Mathf.Min(Mathf.FloorToInt(m_FontData.maxSize * pixelsPerUnitCached), 1000);
            }

            // Other settings
            settings.textAnchor = m_FontData.alignment;
            settings.color = color;
            settings.font = font;
            settings.pivot = rectTransform.pivot;
            settings.richText = m_FontData.richText;
            settings.lineSpacing = m_FontData.lineSpacing;
            settings.fontStyle = m_FontData.fontStyle;
            settings.resizeTextForBestFit = m_FontData.bestFit;
            settings.updateBounds = false;
            settings.horizontalOverflow = m_FontData.horizontalOverflow;
            settings.verticalOverflow = m_FontData.verticalOverflow;

            return settings;
        }

        static public Vector2 GetTextAnchorPivot(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.LowerLeft:    return new Vector2(0, 0);
                case TextAnchor.LowerCenter:  return new Vector2(0.5f, 0);
                case TextAnchor.LowerRight:   return new Vector2(1, 0);
                case TextAnchor.MiddleLeft:   return new Vector2(0, 0.5f);
                case TextAnchor.MiddleCenter: return new Vector2(0.5f, 0.5f);
                case TextAnchor.MiddleRight:  return new Vector2(1, 0.5f);
                case TextAnchor.UpperLeft:    return new Vector2(0, 1);
                case TextAnchor.UpperCenter:  return new Vector2(0.5f, 1);
                case TextAnchor.UpperRight:   return new Vector2(1, 1);
                default: return Vector2.zero;
            }
        }

        /// <summary>
        /// Draw the Text.
        /// </summary>

        protected override void OnFillVBO(List<UIVertex> vbo)
        {
            if (font == null)
                return;

            // We dont care if we the font Texture changes while we are doing our Update.
            // The end result of cachedTextGenerator will be valid for this instance.
            // Otherwise we can get issues like Case 619238.
            m_DisableFontTextureRebuiltCallback = true;

            Vector2 extents = rectTransform.rect.size;

            var settings = GetGenerationSettings(extents);
            cachedTextGenerator.Populate(m_Text, settings);

            Rect inputRect = rectTransform.rect;

            // get the text alignment anchor point for the text in local space
            Vector2 textAnchorPivot = GetTextAnchorPivot(m_FontData.alignment);
            Vector2 refPoint = Vector2.zero;
            refPoint.x = (textAnchorPivot.x == 1 ? inputRect.xMax : inputRect.xMin);
            refPoint.y = (textAnchorPivot.y == 0 ? inputRect.yMin : inputRect.yMax);

            // Determine fraction of pixel to offset text mesh.
            Vector2 roundingOffset = PixelAdjustPoint(refPoint) - refPoint;

            // Apply the offset to the vertices
            IList<UIVertex> verts = cachedTextGenerator.verts;
            float unitsPerPixel = 1 / pixelsPerUnit;
            if (roundingOffset != Vector2.zero)
            {
                for (int i = 0; i < verts.Count; i++)
                {
                    UIVertex uiv = verts[i];
                    uiv.position *= unitsPerPixel;
                    uiv.position.x += roundingOffset.x;
                    uiv.position.y += roundingOffset.y;
                    vbo.Add(uiv);
                }
            }
            else
            {
                for (int i = 0; i < verts.Count; i++)
                {
                    UIVertex uiv = verts[i];
                    uiv.position *= unitsPerPixel;
                    vbo.Add(uiv);
                }
            }
            m_DisableFontTextureRebuiltCallback = false;
        }

        public virtual void CalculateLayoutInputHorizontal() {}
        public virtual void CalculateLayoutInputVertical() {}

        public virtual float minWidth
        {
            get { return 0; }
        }

        public virtual float preferredWidth
        {
            get
            {
                var settings = GetGenerationSettings(Vector2.zero);
                return cachedTextGeneratorForLayout.GetPreferredWidth(m_Text, settings) / pixelsPerUnit;
            }
        }

        public virtual float flexibleWidth { get { return -1; } }

        public virtual float minHeight
        {
            get { return 0; }
        }

        public virtual float preferredHeight
        {
            get
            {
                var settings = GetGenerationSettings(new Vector2(rectTransform.rect.size.x, 0.0f));
                return cachedTextGeneratorForLayout.GetPreferredHeight(m_Text, settings) / pixelsPerUnit;
            }
        }

        public virtual float flexibleHeight { get { return -1; } }

        public virtual int layoutPriority { get { return 0; } }

#if UNITY_EDITOR
        public override void OnRebuildRequested()
        {
            // After a Font asset gets re-imported the managed side gets deleted and recreated,
            // that means the delegates are not persisted.
            // so we need to properly enforce a consistent state here.
            FontUpdateTracker.UntrackText(this);
            FontUpdateTracker.TrackText(this);

            // Also the textgenerator is no longer valid.
            cachedTextGenerator.Invalidate();

            base.OnRebuildRequested();
        }

#endif // if UNITY_EDITOR
    }
}
