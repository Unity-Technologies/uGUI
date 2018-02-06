using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Serialization;

namespace UnityEngine.UI
{
    /// <summary>
    /// Image is a textured element in the UI hierarchy.
    /// </summary>

    [AddComponentMenu("UI/Image", 11)]
    public class Image : MaskableGraphic, ISerializationCallbackReceiver, ILayoutElement, ICanvasRaycastFilter
    {
        public enum Type
        {
            Simple,
            Sliced,
            Tiled,
            Filled
        }

        public enum FillMethod
        {
            Horizontal,
            Vertical,
            Radial90,
            Radial180,
            Radial360,
        }

        public enum OriginHorizontal
        {
            Left,
            Right,
        }

        public enum OriginVertical
        {
            Bottom,
            Top,
        }

        public enum Origin90
        {
            BottomLeft,
            TopLeft,
            TopRight,
            BottomRight,
        }

        public enum Origin180
        {
            Bottom,
            Left,
            Top,
            Right,
        }

        public enum Origin360
        {
            Bottom,
            Right,
            Top,
            Left,
        }

        static protected Material s_ETC1DefaultUI = null;

        [FormerlySerializedAs("m_Frame")]
        [SerializeField] private Sprite m_Sprite;
        public Sprite sprite { get { return m_Sprite; } set { if (SetPropertyUtility.SetClass(ref m_Sprite, value)) SetAllDirty(); } }

        [NonSerialized]
        private Sprite m_OverrideSprite;
        public Sprite overrideSprite { get { return activeSprite; } set { if (SetPropertyUtility.SetClass(ref m_OverrideSprite, value)) SetAllDirty(); } }

        private Sprite activeSprite { get { return m_OverrideSprite != null ? m_OverrideSprite : sprite; } }

        /// How the Image is drawn.
        [SerializeField] private Type m_Type = Type.Simple;
        public Type type { get { return m_Type; } set { if (SetPropertyUtility.SetStruct(ref m_Type, value)) SetVerticesDirty(); } }

        [SerializeField] private bool m_PreserveAspect = false;
        public bool preserveAspect { get { return m_PreserveAspect; } set { if (SetPropertyUtility.SetStruct(ref m_PreserveAspect, value)) SetVerticesDirty(); } }

        [SerializeField] private bool m_FillCenter = true;
        public bool fillCenter { get { return m_FillCenter; } set { if (SetPropertyUtility.SetStruct(ref m_FillCenter, value)) SetVerticesDirty(); } }

        /// Filling method for filled sprites.
        [SerializeField] private FillMethod m_FillMethod = FillMethod.Radial360;
        public FillMethod fillMethod { get { return m_FillMethod; } set { if (SetPropertyUtility.SetStruct(ref m_FillMethod, value)) { SetVerticesDirty(); m_FillOrigin = 0; } } }

        /// Amount of the Image shown. 0-1 range with 0 being nothing shown, and 1 being the full Image.
        [Range(0, 1)]
        [SerializeField] private float m_FillAmount = 1.0f;
        public float fillAmount { get { return m_FillAmount; } set { if (SetPropertyUtility.SetStruct(ref m_FillAmount, Mathf.Clamp01(value))) SetVerticesDirty(); } }

        /// Whether the Image should be filled clockwise (true) or counter-clockwise (false).
        [SerializeField] private bool m_FillClockwise = true;
        public bool fillClockwise { get { return m_FillClockwise; } set { if (SetPropertyUtility.SetStruct(ref m_FillClockwise, value)) SetVerticesDirty(); } }

        /// Controls the origin point of the Fill process. Value means different things with each fill method.
        [SerializeField] private int m_FillOrigin;
        public int fillOrigin { get { return m_FillOrigin; } set { if (SetPropertyUtility.SetStruct(ref m_FillOrigin, value)) SetVerticesDirty(); } }

        // Not serialized until we support read-enabled sprites better.
        private float m_AlphaHitTestMinimumThreshold = 0;

        [Obsolete("eventAlphaThreshold has been deprecated. Use eventMinimumAlphaThreshold instead (UnityUpgradable) -> alphaHitTestMinimumThreshold")]
        public float eventAlphaThreshold { get { return 1 - alphaHitTestMinimumThreshold; } set { alphaHitTestMinimumThreshold = 1 - value; } }
        public float alphaHitTestMinimumThreshold { get { return m_AlphaHitTestMinimumThreshold; } set { m_AlphaHitTestMinimumThreshold = value; } }

        protected Image()
        {
            useLegacyMeshGeneration = false;
        }

        /// <summary>
        /// Default material used to draw everything if no explicit material was specified.
        /// </summary>

        static public Material defaultETC1GraphicMaterial
        {
            get
            {
                if (s_ETC1DefaultUI == null)
                    s_ETC1DefaultUI = Canvas.GetETC1SupportedCanvasMaterial();
                return s_ETC1DefaultUI;
            }
        }

        /// <summary>
        /// Image's texture comes from the UnityEngine.Image.
        /// </summary>
        public override Texture mainTexture
        {
            get
            {
                if (activeSprite == null)
                {
                    if (material != null && material.mainTexture != null)
                    {
                        return material.mainTexture;
                    }
                    return s_WhiteTexture;
                }

                return activeSprite.texture;
            }
        }

        /// <summary>
        /// Whether the Image has a border to work with.
        /// </summary>

        public bool hasBorder
        {
            get
            {
                if (activeSprite != null)
                {
                    Vector4 v = activeSprite.border;
                    return v.sqrMagnitude > 0f;
                }
                return false;
            }
        }

        public float pixelsPerUnit
        {
            get
            {
                float spritePixelsPerUnit = 100;
                if (activeSprite)
                    spritePixelsPerUnit = activeSprite.pixelsPerUnit;

                float referencePixelsPerUnit = 100;
                if (canvas)
                    referencePixelsPerUnit = canvas.referencePixelsPerUnit;

                return spritePixelsPerUnit / referencePixelsPerUnit;
            }
        }

        public override Material material
        {
            get
            {
                if (m_Material != null)
                    return m_Material;
#if UNITY_EDITOR
                if (Application.isPlaying && activeSprite && activeSprite.associatedAlphaSplitTexture != null)
                    return defaultETC1GraphicMaterial;
#else

                if (activeSprite && activeSprite.associatedAlphaSplitTexture != null)
                    return defaultETC1GraphicMaterial;
#endif

                return defaultMaterial;
            }

            set
            {
                base.material = value;
            }
        }

        public virtual void OnBeforeSerialize() {}

        public virtual void OnAfterDeserialize()
        {
            if (m_FillOrigin < 0)
                m_FillOrigin = 0;
            else if (m_FillMethod == FillMethod.Horizontal && m_FillOrigin > 1)
                m_FillOrigin = 0;
            else if (m_FillMethod == FillMethod.Vertical && m_FillOrigin > 1)
                m_FillOrigin = 0;
            else if (m_FillOrigin > 3)
                m_FillOrigin = 0;

            m_FillAmount = Mathf.Clamp(m_FillAmount, 0f, 1f);
        }

        /// Image's dimensions used for drawing. X = left, Y = bottom, Z = right, W = top.
        private Vector4 GetDrawingDimensions(bool shouldPreserveAspect)
        {
            var padding = activeSprite == null ? Vector4.zero : Sprites.DataUtility.GetPadding(activeSprite);
            var size = activeSprite == null ? Vector2.zero : new Vector2(activeSprite.rect.width, activeSprite.rect.height);

            Rect r = GetPixelAdjustedRect();
            // Debug.Log(string.Format("r:{2}, size:{0}, padding:{1}", size, padding, r));

            int spriteW = Mathf.RoundToInt(size.x);
            int spriteH = Mathf.RoundToInt(size.y);

            var v = new Vector4(
                    padding.x / spriteW,
                    padding.y / spriteH,
                    (spriteW - padding.z) / spriteW,
                    (spriteH - padding.w) / spriteH);

            if (shouldPreserveAspect && size.sqrMagnitude > 0.0f)
            {
                var spriteRatio = size.x / size.y;
                var rectRatio = r.width / r.height;

                if (spriteRatio > rectRatio)
                {
                    var oldHeight = r.height;
                    r.height = r.width * (1.0f / spriteRatio);
                    r.y += (oldHeight - r.height) * rectTransform.pivot.y;
                }
                else
                {
                    var oldWidth = r.width;
                    r.width = r.height * spriteRatio;
                    r.x += (oldWidth - r.width) * rectTransform.pivot.x;
                }
            }

            v = new Vector4(
                    r.x + r.width * v.x,
                    r.y + r.height * v.y,
                    r.x + r.width * v.z,
                    r.y + r.height * v.w
                    );

            return v;
        }

        public override void SetNativeSize()
        {
            if (activeSprite != null)
            {
                float w = activeSprite.rect.width / pixelsPerUnit;
                float h = activeSprite.rect.height / pixelsPerUnit;
                rectTransform.anchorMax = rectTransform.anchorMin;
                rectTransform.sizeDelta = new Vector2(w, h);
                SetAllDirty();
            }
        }

        /// <summary>
        /// Update the UI renderer mesh.
        /// </summary>
        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (activeSprite == null)
            {
                base.OnPopulateMesh(toFill);
                return;
            }

            switch (type)
            {
                case Type.Simple:
                    GenerateSimpleSprite(toFill, m_PreserveAspect);
                    break;
                case Type.Sliced:
                    GenerateSlicedSprite(toFill);
                    break;
                case Type.Tiled:
                    GenerateTiledSprite(toFill);
                    break;
                case Type.Filled:
                    GenerateFilledSprite(toFill, m_PreserveAspect);
                    break;
            }
        }

        /// <summary>
        /// Update the renderer's material.
        /// </summary>

        protected override void UpdateMaterial()
        {
            base.UpdateMaterial();

            // check if this sprite has an associated alpha texture (generated when splitting RGBA = RGB + A as two textures without alpha)

            if (activeSprite == null)
            {
                canvasRenderer.SetAlphaTexture(null);
                return;
            }

            Texture2D alphaTex = activeSprite.associatedAlphaSplitTexture;

            if (alphaTex != null)
            {
                canvasRenderer.SetAlphaTexture(alphaTex);
            }
        }

        /// <summary>
        /// Generate vertices for a simple Image.
        /// </summary>
        void GenerateSimpleSprite(VertexHelper vh, bool lPreserveAspect)
        {
            Vector4 v = GetDrawingDimensions(lPreserveAspect);
            var uv = (activeSprite != null) ? Sprites.DataUtility.GetOuterUV(activeSprite) : Vector4.zero;

            var color32 = color;
            vh.Clear();
            vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(uv.x, uv.y));
            vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(uv.x, uv.w));
            vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(uv.z, uv.w));
            vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(uv.z, uv.y));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }

        /// <summary>
        /// Generate vertices for a 9-sliced Image.
        /// </summary>

        static readonly Vector2[] s_VertScratch = new Vector2[4];
        static readonly Vector2[] s_UVScratch = new Vector2[4];

        private void GenerateSlicedSprite(VertexHelper toFill)
        {
            if (!hasBorder)
            {
                GenerateSimpleSprite(toFill, false);
                return;
            }

            Vector4 outer, inner, padding, border;

            if (activeSprite != null)
            {
                outer = Sprites.DataUtility.GetOuterUV(activeSprite);
                inner = Sprites.DataUtility.GetInnerUV(activeSprite);
                padding = Sprites.DataUtility.GetPadding(activeSprite);
                border = activeSprite.border;
            }
            else
            {
                outer = Vector4.zero;
                inner = Vector4.zero;
                padding = Vector4.zero;
                border = Vector4.zero;
            }

            Rect rect = GetPixelAdjustedRect();
            Vector4 adjustedBorders = GetAdjustedBorders(border / pixelsPerUnit, rect);
            padding = padding / pixelsPerUnit;

            s_VertScratch[0] = new Vector2(padding.x, padding.y);
            s_VertScratch[3] = new Vector2(rect.width - padding.z, rect.height - padding.w);

            s_VertScratch[1].x = adjustedBorders.x;
            s_VertScratch[1].y = adjustedBorders.y;

            s_VertScratch[2].x = rect.width - adjustedBorders.z;
            s_VertScratch[2].y = rect.height - adjustedBorders.w;

            for (int i = 0; i < 4; ++i)
            {
                s_VertScratch[i].x += rect.x;
                s_VertScratch[i].y += rect.y;
            }

            s_UVScratch[0] = new Vector2(outer.x, outer.y);
            s_UVScratch[1] = new Vector2(inner.x, inner.y);
            s_UVScratch[2] = new Vector2(inner.z, inner.w);
            s_UVScratch[3] = new Vector2(outer.z, outer.w);

            toFill.Clear();

            for (int x = 0; x < 3; ++x)
            {
                int x2 = x + 1;

                for (int y = 0; y < 3; ++y)
                {
                    if (!m_FillCenter && x == 1 && y == 1)
                        continue;

                    int y2 = y + 1;


                    AddQuad(toFill,
                        new Vector2(s_VertScratch[x].x, s_VertScratch[y].y),
                        new Vector2(s_VertScratch[x2].x, s_VertScratch[y2].y),
                        color,
                        new Vector2(s_UVScratch[x].x, s_UVScratch[y].y),
                        new Vector2(s_UVScratch[x2].x, s_UVScratch[y2].y));
                }
            }
        }

        /// <summary>
        /// Generate vertices for a tiled Image.
        /// </summary>

        void GenerateTiledSprite(VertexHelper toFill)
        {
            Vector4 outer, inner, border;
            Vector2 spriteSize;

            if (activeSprite != null)
            {
                outer = Sprites.DataUtility.GetOuterUV(activeSprite);
                inner = Sprites.DataUtility.GetInnerUV(activeSprite);
                border = activeSprite.border;
                spriteSize = activeSprite.rect.size;
            }
            else
            {
                outer = Vector4.zero;
                inner = Vector4.zero;
                border = Vector4.zero;
                spriteSize = Vector2.one * 100;
            }

            Rect rect = GetPixelAdjustedRect();
            float tileWidth = (spriteSize.x - border.x - border.z) / pixelsPerUnit;
            float tileHeight = (spriteSize.y - border.y - border.w) / pixelsPerUnit;
            border = GetAdjustedBorders(border / pixelsPerUnit, rect);

            var uvMin = new Vector2(inner.x, inner.y);
            var uvMax = new Vector2(inner.z, inner.w);

            // Min to max max range for tiled region in coordinates relative to lower left corner.
            float xMin = border.x;
            float xMax = rect.width - border.z;
            float yMin = border.y;
            float yMax = rect.height - border.w;

            toFill.Clear();
            var clipped = uvMax;

            // if either width is zero we cant tile so just assume it was the full width.
            if (tileWidth <= 0)
                tileWidth = xMax - xMin;

            if (tileHeight <= 0)
                tileHeight = yMax - yMin;

            if (activeSprite != null && (hasBorder || activeSprite.packed || activeSprite.texture.wrapMode != TextureWrapMode.Repeat))
            {
                // Sprite has border, or is not in repeat mode, or cannot be repeated because of packing.
                // We cannot use texture tiling so we will generate a mesh of quads to tile the texture.

                // Evaluate how many vertices we will generate. Limit this number to something sane,
                // especially since meshes can not have more than 65000 vertices.

                long nTilesW = 0;
                long nTilesH = 0;
                if (m_FillCenter)
                {
                    nTilesW = (long)Math.Ceiling((xMax - xMin) / tileWidth);
                    nTilesH = (long)Math.Ceiling((yMax - yMin) / tileHeight);

                    double nVertices = 0;
                    if (hasBorder)
                    {
                        nVertices = (nTilesW + 2.0) * (nTilesH + 2.0) * 4.0; // 4 vertices per tile
                    }
                    else
                    {
                        nVertices = nTilesW * nTilesH * 4.0; // 4 vertices per tile
                    }

                    if (nVertices > 65000.0)
                    {
                        Debug.LogError("Too many sprite tiles on Image \"" + name + "\". The tile size will be increased. To remove the limit on the number of tiles, convert the Sprite to an Advanced texture, remove the borders, clear the Packing tag and set the Wrap mode to Repeat.", this);

                        double maxTiles = 65000.0 / 4.0; // Max number of vertices is 65000; 4 vertices per tile.
                        double imageRatio;
                        if (hasBorder)
                        {
                            imageRatio = (nTilesW + 2.0) / (nTilesH + 2.0);
                        }
                        else
                        {
                            imageRatio = (double)nTilesW / nTilesH;
                        }

                        double targetTilesW = Math.Sqrt(maxTiles / imageRatio);
                        double targetTilesH = targetTilesW * imageRatio;
                        if (hasBorder)
                        {
                            targetTilesW -= 2;
                            targetTilesH -= 2;
                        }

                        nTilesW = (long)Math.Floor(targetTilesW);
                        nTilesH = (long)Math.Floor(targetTilesH);
                        tileWidth = (xMax - xMin) / nTilesW;
                        tileHeight = (yMax - yMin) / nTilesH;
                    }
                }
                else
                {
                    if (hasBorder)
                    {
                        // Texture on the border is repeated only in one direction.
                        nTilesW = (long)Math.Ceiling((xMax - xMin) / tileWidth);
                        nTilesH = (long)Math.Ceiling((yMax - yMin) / tileHeight);
                        double nVertices = (nTilesH + nTilesW + 2.0 /*corners*/) * 2.0 /*sides*/ * 4.0 /*vertices per tile*/;
                        if (nVertices > 65000.0)
                        {
                            Debug.LogError("Too many sprite tiles on Image \"" + name + "\". The tile size will be increased. To remove the limit on the number of tiles, convert the Sprite to an Advanced texture, remove the borders, clear the Packing tag and set the Wrap mode to Repeat.", this);

                            double maxTiles = 65000.0 / 4.0; // Max number of vertices is 65000; 4 vertices per tile.
                            double imageRatio = (double)nTilesW / nTilesH;
                            double targetTilesW = (maxTiles - 4 /*corners*/) / (2 * (1.0 + imageRatio));
                            double targetTilesH = targetTilesW * imageRatio;

                            nTilesW = (long)Math.Floor(targetTilesW);
                            nTilesH = (long)Math.Floor(targetTilesH);
                            tileWidth = (xMax - xMin) / nTilesW;
                            tileHeight = (yMax - yMin) / nTilesH;
                        }
                    }
                    else
                    {
                        nTilesH = nTilesW = 0;
                    }
                }

                if (m_FillCenter)
                {
                    // TODO: we could share vertices between quads. If vertex sharing is implemented. update the computation for the number of vertices accordingly.
                    for (long j = 0; j < nTilesH; j++)
                    {
                        float y1 = yMin + j * tileHeight;
                        float y2 = yMin + (j + 1) * tileHeight;
                        if (y2 > yMax)
                        {
                            clipped.y = uvMin.y + (uvMax.y - uvMin.y) * (yMax - y1) / (y2 - y1);
                            y2 = yMax;
                        }
                        clipped.x = uvMax.x;
                        for (long i = 0; i < nTilesW; i++)
                        {
                            float x1 = xMin + i * tileWidth;
                            float x2 = xMin + (i + 1) * tileWidth;
                            if (x2 > xMax)
                            {
                                clipped.x = uvMin.x + (uvMax.x - uvMin.x) * (xMax - x1) / (x2 - x1);
                                x2 = xMax;
                            }
                            AddQuad(toFill, new Vector2(x1, y1) + rect.position, new Vector2(x2, y2) + rect.position, color, uvMin, clipped);
                        }
                    }
                }
                if (hasBorder)
                {
                    clipped = uvMax;
                    for (long j = 0; j < nTilesH; j++)
                    {
                        float y1 = yMin + j * tileHeight;
                        float y2 = yMin + (j + 1) * tileHeight;
                        if (y2 > yMax)
                        {
                            clipped.y = uvMin.y + (uvMax.y - uvMin.y) * (yMax - y1) / (y2 - y1);
                            y2 = yMax;
                        }
                        AddQuad(toFill,
                            new Vector2(0, y1) + rect.position,
                            new Vector2(xMin, y2) + rect.position,
                            color,
                            new Vector2(outer.x, uvMin.y),
                            new Vector2(uvMin.x, clipped.y));
                        AddQuad(toFill,
                            new Vector2(xMax, y1) + rect.position,
                            new Vector2(rect.width, y2) + rect.position,
                            color,
                            new Vector2(uvMax.x, uvMin.y),
                            new Vector2(outer.z, clipped.y));
                    }

                    // Bottom and top tiled border
                    clipped = uvMax;
                    for (long i = 0; i < nTilesW; i++)
                    {
                        float x1 = xMin + i * tileWidth;
                        float x2 = xMin + (i + 1) * tileWidth;
                        if (x2 > xMax)
                        {
                            clipped.x = uvMin.x + (uvMax.x - uvMin.x) * (xMax - x1) / (x2 - x1);
                            x2 = xMax;
                        }
                        AddQuad(toFill,
                            new Vector2(x1, 0) + rect.position,
                            new Vector2(x2, yMin) + rect.position,
                            color,
                            new Vector2(uvMin.x, outer.y),
                            new Vector2(clipped.x, uvMin.y));
                        AddQuad(toFill,
                            new Vector2(x1, yMax) + rect.position,
                            new Vector2(x2, rect.height) + rect.position,
                            color,
                            new Vector2(uvMin.x, uvMax.y),
                            new Vector2(clipped.x, outer.w));
                    }

                    // Corners
                    AddQuad(toFill,
                        new Vector2(0, 0) + rect.position,
                        new Vector2(xMin, yMin) + rect.position,
                        color,
                        new Vector2(outer.x, outer.y),
                        new Vector2(uvMin.x, uvMin.y));
                    AddQuad(toFill,
                        new Vector2(xMax, 0) + rect.position,
                        new Vector2(rect.width, yMin) + rect.position,
                        color,
                        new Vector2(uvMax.x, outer.y),
                        new Vector2(outer.z, uvMin.y));
                    AddQuad(toFill,
                        new Vector2(0, yMax) + rect.position,
                        new Vector2(xMin, rect.height) + rect.position,
                        color,
                        new Vector2(outer.x, uvMax.y),
                        new Vector2(uvMin.x, outer.w));
                    AddQuad(toFill,
                        new Vector2(xMax, yMax) + rect.position,
                        new Vector2(rect.width, rect.height) + rect.position,
                        color,
                        new Vector2(uvMax.x, uvMax.y),
                        new Vector2(outer.z, outer.w));
                }
            }
            else
            {
                // Texture has no border, is in repeat mode and not packed. Use texture tiling.
                Vector2 uvScale = new Vector2((xMax - xMin) / tileWidth, (yMax - yMin) / tileHeight);

                if (m_FillCenter)
                {
                    AddQuad(toFill, new Vector2(xMin, yMin) + rect.position, new Vector2(xMax, yMax) + rect.position, color, Vector2.Scale(uvMin, uvScale), Vector2.Scale(uvMax, uvScale));
                }
            }
        }

        static void AddQuad(VertexHelper vertexHelper, Vector3[] quadPositions, Color32 color, Vector3[] quadUVs)
        {
            int startIndex = vertexHelper.currentVertCount;

            for (int i = 0; i < 4; ++i)
                vertexHelper.AddVert(quadPositions[i], color, quadUVs[i]);

            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

        static void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin, Vector2 uvMax)
        {
            int startIndex = vertexHelper.currentVertCount;

            vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0), color, new Vector2(uvMin.x, uvMin.y));
            vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0), color, new Vector2(uvMin.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0), color, new Vector2(uvMax.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0), color, new Vector2(uvMax.x, uvMin.y));

            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

        private Vector4 GetAdjustedBorders(Vector4 border, Rect adjustedRect)
        {
            Rect originalRect = rectTransform.rect;

            for (int axis = 0; axis <= 1; axis++)
            {
                float borderScaleRatio;

                // The adjusted rect (adjusted for pixel correctness)
                // may be slightly larger than the original rect.
                // Adjust the border to match the adjustedRect to avoid
                // small gaps between borders (case 833201).
                if (originalRect.size[axis] != 0)
                {
                    borderScaleRatio = adjustedRect.size[axis] / originalRect.size[axis];
                    border[axis] *= borderScaleRatio;
                    border[axis + 2] *= borderScaleRatio;
                }

                // If the rect is smaller than the combined borders, then there's not room for the borders at their normal size.
                // In order to avoid artefacts with overlapping borders, we scale the borders down to fit.
                float combinedBorders = border[axis] + border[axis + 2];
                if (adjustedRect.size[axis] < combinedBorders && combinedBorders != 0)
                {
                    borderScaleRatio = adjustedRect.size[axis] / combinedBorders;
                    border[axis] *= borderScaleRatio;
                    border[axis + 2] *= borderScaleRatio;
                }
            }
            return border;
        }

        /// <summary>
        /// Generate vertices for a filled Image.
        /// </summary>

        static readonly Vector3[] s_Xy = new Vector3[4];
        static readonly Vector3[] s_Uv = new Vector3[4];
        void GenerateFilledSprite(VertexHelper toFill, bool preserveAspect)
        {
            toFill.Clear();

            if (m_FillAmount < 0.001f)
                return;

            Vector4 v = GetDrawingDimensions(preserveAspect);
            Vector4 outer = activeSprite != null ? Sprites.DataUtility.GetOuterUV(activeSprite) : Vector4.zero;
            UIVertex uiv = UIVertex.simpleVert;
            uiv.color = color;

            float tx0 = outer.x;
            float ty0 = outer.y;
            float tx1 = outer.z;
            float ty1 = outer.w;

            // Horizontal and vertical filled sprites are simple -- just end the Image prematurely
            if (m_FillMethod == FillMethod.Horizontal || m_FillMethod == FillMethod.Vertical)
            {
                if (fillMethod == FillMethod.Horizontal)
                {
                    float fill = (tx1 - tx0) * m_FillAmount;

                    if (m_FillOrigin == 1)
                    {
                        v.x = v.z - (v.z - v.x) * m_FillAmount;
                        tx0 = tx1 - fill;
                    }
                    else
                    {
                        v.z = v.x + (v.z - v.x) * m_FillAmount;
                        tx1 = tx0 + fill;
                    }
                }
                else if (fillMethod == FillMethod.Vertical)
                {
                    float fill = (ty1 - ty0) * m_FillAmount;

                    if (m_FillOrigin == 1)
                    {
                        v.y = v.w - (v.w - v.y) * m_FillAmount;
                        ty0 = ty1 - fill;
                    }
                    else
                    {
                        v.w = v.y + (v.w - v.y) * m_FillAmount;
                        ty1 = ty0 + fill;
                    }
                }
            }

            s_Xy[0] = new Vector2(v.x, v.y);
            s_Xy[1] = new Vector2(v.x, v.w);
            s_Xy[2] = new Vector2(v.z, v.w);
            s_Xy[3] = new Vector2(v.z, v.y);

            s_Uv[0] = new Vector2(tx0, ty0);
            s_Uv[1] = new Vector2(tx0, ty1);
            s_Uv[2] = new Vector2(tx1, ty1);
            s_Uv[3] = new Vector2(tx1, ty0);

            {
                if (m_FillAmount < 1f && m_FillMethod != FillMethod.Horizontal && m_FillMethod != FillMethod.Vertical)
                {
                    if (fillMethod == FillMethod.Radial90)
                    {
                        if (RadialCut(s_Xy, s_Uv, m_FillAmount, m_FillClockwise, m_FillOrigin))
                            AddQuad(toFill, s_Xy, color, s_Uv);
                    }
                    else if (fillMethod == FillMethod.Radial180)
                    {
                        for (int side = 0; side < 2; ++side)
                        {
                            float fx0, fx1, fy0, fy1;
                            int even = m_FillOrigin > 1 ? 1 : 0;

                            if (m_FillOrigin == 0 || m_FillOrigin == 2)
                            {
                                fy0 = 0f;
                                fy1 = 1f;
                                if (side == even)
                                {
                                    fx0 = 0f;
                                    fx1 = 0.5f;
                                }
                                else
                                {
                                    fx0 = 0.5f;
                                    fx1 = 1f;
                                }
                            }
                            else
                            {
                                fx0 = 0f;
                                fx1 = 1f;
                                if (side == even)
                                {
                                    fy0 = 0.5f;
                                    fy1 = 1f;
                                }
                                else
                                {
                                    fy0 = 0f;
                                    fy1 = 0.5f;
                                }
                            }

                            s_Xy[0].x = Mathf.Lerp(v.x, v.z, fx0);
                            s_Xy[1].x = s_Xy[0].x;
                            s_Xy[2].x = Mathf.Lerp(v.x, v.z, fx1);
                            s_Xy[3].x = s_Xy[2].x;

                            s_Xy[0].y = Mathf.Lerp(v.y, v.w, fy0);
                            s_Xy[1].y = Mathf.Lerp(v.y, v.w, fy1);
                            s_Xy[2].y = s_Xy[1].y;
                            s_Xy[3].y = s_Xy[0].y;

                            s_Uv[0].x = Mathf.Lerp(tx0, tx1, fx0);
                            s_Uv[1].x = s_Uv[0].x;
                            s_Uv[2].x = Mathf.Lerp(tx0, tx1, fx1);
                            s_Uv[3].x = s_Uv[2].x;

                            s_Uv[0].y = Mathf.Lerp(ty0, ty1, fy0);
                            s_Uv[1].y = Mathf.Lerp(ty0, ty1, fy1);
                            s_Uv[2].y = s_Uv[1].y;
                            s_Uv[3].y = s_Uv[0].y;

                            float val = m_FillClockwise ? fillAmount * 2f - side : m_FillAmount * 2f - (1 - side);

                            if (RadialCut(s_Xy, s_Uv, Mathf.Clamp01(val), m_FillClockwise, ((side + m_FillOrigin + 3) % 4)))
                            {
                                AddQuad(toFill, s_Xy, color, s_Uv);
                            }
                        }
                    }
                    else if (fillMethod == FillMethod.Radial360)
                    {
                        for (int corner = 0; corner < 4; ++corner)
                        {
                            float fx0, fx1, fy0, fy1;

                            if (corner < 2)
                            {
                                fx0 = 0f;
                                fx1 = 0.5f;
                            }
                            else
                            {
                                fx0 = 0.5f;
                                fx1 = 1f;
                            }

                            if (corner == 0 || corner == 3)
                            {
                                fy0 = 0f;
                                fy1 = 0.5f;
                            }
                            else
                            {
                                fy0 = 0.5f;
                                fy1 = 1f;
                            }

                            s_Xy[0].x = Mathf.Lerp(v.x, v.z, fx0);
                            s_Xy[1].x = s_Xy[0].x;
                            s_Xy[2].x = Mathf.Lerp(v.x, v.z, fx1);
                            s_Xy[3].x = s_Xy[2].x;

                            s_Xy[0].y = Mathf.Lerp(v.y, v.w, fy0);
                            s_Xy[1].y = Mathf.Lerp(v.y, v.w, fy1);
                            s_Xy[2].y = s_Xy[1].y;
                            s_Xy[3].y = s_Xy[0].y;

                            s_Uv[0].x = Mathf.Lerp(tx0, tx1, fx0);
                            s_Uv[1].x = s_Uv[0].x;
                            s_Uv[2].x = Mathf.Lerp(tx0, tx1, fx1);
                            s_Uv[3].x = s_Uv[2].x;

                            s_Uv[0].y = Mathf.Lerp(ty0, ty1, fy0);
                            s_Uv[1].y = Mathf.Lerp(ty0, ty1, fy1);
                            s_Uv[2].y = s_Uv[1].y;
                            s_Uv[3].y = s_Uv[0].y;

                            float val = m_FillClockwise ?
                                m_FillAmount * 4f - ((corner + m_FillOrigin) % 4) :
                                m_FillAmount * 4f - (3 - ((corner + m_FillOrigin) % 4));

                            if (RadialCut(s_Xy, s_Uv, Mathf.Clamp01(val), m_FillClockwise, ((corner + 2) % 4)))
                                AddQuad(toFill, s_Xy, color, s_Uv);
                        }
                    }
                }
                else
                {
                    AddQuad(toFill, s_Xy, color, s_Uv);
                }
            }
        }

        /// <summary>
        /// Adjust the specified quad, making it be radially filled instead.
        /// </summary>

        static bool RadialCut(Vector3[] xy, Vector3[] uv, float fill, bool invert, int corner)
        {
            // Nothing to fill
            if (fill < 0.001f) return false;

            // Even corners invert the fill direction
            if ((corner & 1) == 1) invert = !invert;

            // Nothing to adjust
            if (!invert && fill > 0.999f) return true;

            // Convert 0-1 value into 0 to 90 degrees angle in radians
            float angle = Mathf.Clamp01(fill);
            if (invert) angle = 1f - angle;
            angle *= 90f * Mathf.Deg2Rad;

            // Calculate the effective X and Y factors
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            RadialCut(xy, cos, sin, invert, corner);
            RadialCut(uv, cos, sin, invert, corner);
            return true;
        }

        /// <summary>
        /// Adjust the specified quad, making it be radially filled instead.
        /// </summary>

        static void RadialCut(Vector3[] xy, float cos, float sin, bool invert, int corner)
        {
            int i0 = corner;
            int i1 = ((corner + 1) % 4);
            int i2 = ((corner + 2) % 4);
            int i3 = ((corner + 3) % 4);

            if ((corner & 1) == 1)
            {
                if (sin > cos)
                {
                    cos /= sin;
                    sin = 1f;

                    if (invert)
                    {
                        xy[i1].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
                        xy[i2].x = xy[i1].x;
                    }
                }
                else if (cos > sin)
                {
                    sin /= cos;
                    cos = 1f;

                    if (!invert)
                    {
                        xy[i2].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
                        xy[i3].y = xy[i2].y;
                    }
                }
                else
                {
                    cos = 1f;
                    sin = 1f;
                }

                if (!invert) xy[i3].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
                else xy[i1].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
            }
            else
            {
                if (cos > sin)
                {
                    sin /= cos;
                    cos = 1f;

                    if (!invert)
                    {
                        xy[i1].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
                        xy[i2].y = xy[i1].y;
                    }
                }
                else if (sin > cos)
                {
                    cos /= sin;
                    sin = 1f;

                    if (invert)
                    {
                        xy[i2].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
                        xy[i3].x = xy[i2].x;
                    }
                }
                else
                {
                    cos = 1f;
                    sin = 1f;
                }

                if (invert) xy[i3].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
                else xy[i1].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
            }
        }

        public virtual void CalculateLayoutInputHorizontal() {}
        public virtual void CalculateLayoutInputVertical() {}

        public virtual float minWidth { get { return 0; } }

        public virtual float preferredWidth
        {
            get
            {
                if (activeSprite == null)
                    return 0;
                if (type == Type.Sliced || type == Type.Tiled)
                    return Sprites.DataUtility.GetMinSize(activeSprite).x / pixelsPerUnit;
                return activeSprite.rect.size.x / pixelsPerUnit;
            }
        }

        public virtual float flexibleWidth { get { return -1; } }

        public virtual float minHeight { get { return 0; } }

        public virtual float preferredHeight
        {
            get
            {
                if (activeSprite == null)
                    return 0;
                if (type == Type.Sliced || type == Type.Tiled)
                    return Sprites.DataUtility.GetMinSize(activeSprite).y / pixelsPerUnit;
                return activeSprite.rect.size.y / pixelsPerUnit;
            }
        }

        public virtual float flexibleHeight { get { return -1; } }

        public virtual int layoutPriority { get { return 0; } }

        public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (alphaHitTestMinimumThreshold <= 0)
                return true;

            if (alphaHitTestMinimumThreshold > 1)
                return false;

            if (activeSprite == null)
                return true;

            Vector2 local;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out local))
                return false;

            Rect rect = GetPixelAdjustedRect();

            // Convert to have lower left corner as reference point.
            local.x += rectTransform.pivot.x * rect.width;
            local.y += rectTransform.pivot.y * rect.height;

            local = MapCoordinate(local, rect);

            // Normalize local coordinates.
            Rect spriteRect = activeSprite.textureRect;
            Vector2 normalized = new Vector2(local.x / spriteRect.width, local.y / spriteRect.height);

            // Convert to texture space.
            float x = Mathf.Lerp(spriteRect.x, spriteRect.xMax, normalized.x) / activeSprite.texture.width;
            float y = Mathf.Lerp(spriteRect.y, spriteRect.yMax, normalized.y) / activeSprite.texture.height;

            try
            {
                return activeSprite.texture.GetPixelBilinear(x, y).a >= alphaHitTestMinimumThreshold;
            }
            catch (UnityException e)
            {
                Debug.LogError("Using alphaHitTestMinimumThreshold greater than 0 on Image whose sprite texture cannot be read. " + e.Message + " Also make sure to disable sprite packing for this sprite.", this);
                return true;
            }
        }

        private Vector2 MapCoordinate(Vector2 local, Rect rect)
        {
            Rect spriteRect = activeSprite.rect;
            if (type == Type.Simple || type == Type.Filled)
                return new Vector2(local.x * spriteRect.width / rect.width, local.y * spriteRect.height / rect.height);

            Vector4 border = activeSprite.border;
            Vector4 adjustedBorder = GetAdjustedBorders(border / pixelsPerUnit, rect);

            for (int i = 0; i < 2; i++)
            {
                if (local[i] <= adjustedBorder[i])
                    continue;

                if (rect.size[i] - local[i] <= adjustedBorder[i + 2])
                {
                    local[i] -= (rect.size[i] - spriteRect.size[i]);
                    continue;
                }

                if (type == Type.Sliced)
                {
                    float lerp = Mathf.InverseLerp(adjustedBorder[i], rect.size[i] - adjustedBorder[i + 2], local[i]);
                    local[i] = Mathf.Lerp(border[i], spriteRect.size[i] - border[i + 2], lerp);
                    continue;
                }
                else
                {
                    local[i] -= adjustedBorder[i];
                    local[i] = Mathf.Repeat(local[i], spriteRect.size[i] - border[i] - border[i + 2]);
                    local[i] += border[i];
                    continue;
                }
            }

            return local;
        }
    }
}
