using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI.CoroutineTween;

namespace UnityEngine.UI
{
    /// <summary>
    /// Base class for all UI components that should be derived from when creating new Graphic types.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasRenderer))]
    [RequireComponent(typeof(RectTransform))]
    [ExecuteInEditMode]
    public abstract class Graphic
        : UIBehaviour,
          ICanvasElement
    {
        static protected Material s_DefaultUI = null;
        static protected Texture2D s_WhiteTexture = null;

        /// <summary>
        /// Default material used to draw everything if no explicit material was specified.
        /// </summary>

        static public Material defaultGraphicMaterial
        {
            get
            {
                if (s_DefaultUI == null)
                    s_DefaultUI = Canvas.GetDefaultCanvasMaterial();
                return s_DefaultUI;
            }
        }

        // Temporary vertex array pool used to avoid memory allocations as much as possible
        private static readonly ObjectPool<List<UIVertex>> s_VboPool = new ObjectPool<List<UIVertex>>(x => { if (x.Capacity < 300) x.Capacity = 300; }, l => l.Clear());

        // Cached and saved values
        [FormerlySerializedAs("m_Mat")]
        [SerializeField] protected Material m_Material;

        [SerializeField] private Color m_Color = Color.white;
        public Color color { get { return m_Color; } set { if (SetPropertyUtility.SetColor(ref m_Color, value)) SetVerticesDirty(); } }

        [NonSerialized] private RectTransform m_RectTransform;
        [NonSerialized] private CanvasRenderer m_CanvasRender;
        [NonSerialized] private Canvas m_Canvas;

        [NonSerialized] private bool m_VertsDirty;
        [NonSerialized] private bool m_MaterialDirty;


        [NonSerialized] protected UnityAction m_OnDirtyLayoutCallback;
        [NonSerialized] protected UnityAction m_OnDirtyVertsCallback;
        [NonSerialized] protected UnityAction m_OnDirtyMaterialCallback;

        // Tween controls for the Graphic
        [NonSerialized]
        private readonly TweenRunner<ColorTween> m_ColorTweenRunner;

        // Called by Unity prior to deserialization,
        // should not be called by users
        protected Graphic()
        {
            if (m_ColorTweenRunner == null)
                m_ColorTweenRunner = new TweenRunner<ColorTween>();
            m_ColorTweenRunner.Init(this);
        }

        public virtual void SetAllDirty()
        {
            SetLayoutDirty();
            SetVerticesDirty();
            SetMaterialDirty();
        }

        public virtual void SetLayoutDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);

            if (m_OnDirtyLayoutCallback != null)
                m_OnDirtyLayoutCallback();
        }

        public virtual void SetVerticesDirty()
        {
            if (!IsActive())
                return;

            m_VertsDirty = true;
            CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);

            if (m_OnDirtyVertsCallback != null)
                m_OnDirtyVertsCallback();
        }

        public virtual void SetMaterialDirty()
        {
            if (!IsActive())
                return;

            m_MaterialDirty = true;
            CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);

            if (m_OnDirtyMaterialCallback != null)
                m_OnDirtyMaterialCallback();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            if (gameObject.activeInHierarchy)
            {
                // prevent double dirtying...
                if (CanvasUpdateRegistry.IsRebuildingLayout())
                    SetVerticesDirty();
                else
                {
                    SetVerticesDirty();
                    SetLayoutDirty();
                }
            }
        }

        protected override void OnBeforeTransformParentChanged()
        {
            GraphicRegistry.UnregisterGraphicForCanvas(canvas, this);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        protected override void OnTransformParentChanged()
        {
            if (!IsActive())
                return;

            CacheCanvas();
            GraphicRegistry.RegisterGraphicForCanvas(canvas, this);
            SetAllDirty();
        }

        /// <summary>
        /// Absolute depth of the graphic, used by rendering and events -- lowest to highest.
        /// </summary>
        public int depth { get { return canvasRenderer.absoluteDepth; } }

        /// <summary>
        /// Transform gets cached for speed.
        /// </summary>
        public RectTransform rectTransform
        {
            get { return m_RectTransform ?? (m_RectTransform = GetComponent<RectTransform>()); }
        }

        public Canvas canvas
        {
            get
            {
                if (m_Canvas == null)
                    CacheCanvas();
                return m_Canvas;
            }
        }

        private void CacheCanvas()
        {
            var list = CanvasListPool.Get();
            gameObject.GetComponentsInParent(false, list);
            if (list.Count > 0)
                m_Canvas = list[0];
            CanvasListPool.Release(list);
        }

        /// <summary>
        /// UI Renderer component.
        /// </summary>
        public CanvasRenderer canvasRenderer
        {
            get
            {
                if (m_CanvasRender == null)
                    m_CanvasRender = GetComponent<CanvasRenderer>();
                return m_CanvasRender;
            }
        }

        public virtual Material defaultMaterial
        {
            get { return defaultGraphicMaterial; }
        }

        /// <summary>
        /// Returns the material used by this Graphic.
        /// </summary>
        public virtual Material material
        {
            get
            {
                return (m_Material != null) ? m_Material : defaultMaterial;
            }
            set
            {
                if (m_Material == value)
                    return;

                m_Material = value;
                SetMaterialDirty();
            }
        }

        public virtual Material materialForRendering
        {
            get
            {
                var components = ComponentListPool.Get();
                GetComponents(typeof(IMaterialModifier), components);

                var currentMat = material;
                for (var i = 0; i < components.Count; i++)
                    currentMat = (components[i] as IMaterialModifier).GetModifiedMaterial(currentMat);
                ComponentListPool.Release(components);
                return currentMat;
            }
        }

        /// <summary>
        /// Returns the texture used to draw this Graphic.
        /// </summary>
        public virtual Texture mainTexture
        {
            get
            {
                return s_WhiteTexture;
            }
        }

        #region Unity Lifetime calls
        /// <summary>
        /// Mark the Graphic and the canvas as having been changed.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            CacheCanvas();
            GraphicRegistry.RegisterGraphicForCanvas(canvas, this);

#if UNITY_EDITOR
            GraphicRebuildTracker.TrackGraphic(this);
#endif
            if (s_WhiteTexture == null)
                s_WhiteTexture = Texture2D.whiteTexture;

            SetAllDirty();

            SendGraphicEnabledDisabled();
        }

        /// <summary>
        /// Clear references.
        /// </summary>
        protected override void OnDisable()
        {
#if UNITY_EDITOR
            GraphicRebuildTracker.UnTrackGraphic(this);
#endif
            GraphicRegistry.UnregisterGraphicForCanvas(canvas, this);
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            if (canvasRenderer != null)
                canvasRenderer.Clear();

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            SendGraphicEnabledDisabled();
            base.OnDisable();
        }

        private void SendGraphicEnabledDisabled()
        {
            var components = ComponentListPool.Get();
            GetComponents(typeof(IGraphicEnabledDisabled), components);

            for (int i = 0; i < components.Count; i++)
                ((IGraphicEnabledDisabled)components[i]).OnSiblingGraphicEnabledDisabled();

            ComponentListPool.Release(components);
        }

        #endregion

        protected override void OnCanvasHierarchyChanged()
        {
            if (!IsActive())
                return;

            // Use m_Cavas so we dont auto call CacheCanvas
            Canvas currentCanvas = m_Canvas;
            CacheCanvas();

            if (currentCanvas != m_Canvas)
            {
                GraphicRegistry.UnregisterGraphicForCanvas(currentCanvas, this);
                GraphicRegistry.RegisterGraphicForCanvas(canvas, this);
            }
        }

        public virtual void Rebuild(CanvasUpdate update)
        {
            switch (update)
            {
                case CanvasUpdate.PreRender:
                    if (m_VertsDirty)
                    {
                        UpdateGeometry();
                        m_VertsDirty = false;
                    }
                    if (m_MaterialDirty)
                    {
                        UpdateMaterial();
                        m_MaterialDirty = false;
                    }
                    break;
            }
        }

        /// <summary>
        /// Update the renderer's vertices.
        /// </summary>
        protected virtual void UpdateGeometry()
        {
            var vbo = s_VboPool.Get();

            if (rectTransform != null && rectTransform.rect.width >= 0 && rectTransform.rect.height >= 0)
                OnFillVBO(vbo);


            var components = ComponentListPool.Get();
            GetComponents(typeof(IVertexModifier), components);

            for (var i = 0; i < components.Count; i++)
                (components[i] as IVertexModifier).ModifyVertices(vbo);
            ComponentListPool.Release(components);

            canvasRenderer.SetVertices(vbo);
            s_VboPool.Release(vbo);
        }

        /// <summary>
        /// Update the renderer's material.
        /// </summary>
        protected virtual void UpdateMaterial()
        {
            if (IsActive())
                canvasRenderer.SetMaterial(materialForRendering, mainTexture);
        }

        /// <summary>
        /// Fill the vertex buffer data.
        /// </summary>
        protected virtual void OnFillVBO(List<UIVertex> vbo)
        {
            var r = GetPixelAdjustedRect();
            var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);

            var vert = UIVertex.simpleVert;
            vert.color = color;

            vert.position = new Vector3(v.x, v.y);
            vert.uv0 = new Vector2(0f, 0f);
            vbo.Add(vert);

            vert.position = new Vector3(v.x, v.w);
            vert.uv0 = new Vector2(0f, 1f);
            vbo.Add(vert);

            vert.position = new Vector3(v.z, v.w);
            vert.uv0 = new Vector2(1f, 1f);
            vbo.Add(vert);

            vert.position = new Vector3(v.z, v.y);
            vert.uv0 = new Vector2(1f, 0f);
            vbo.Add(vert);
        }

#if UNITY_EDITOR
        public virtual void OnRebuildRequested()
        {
            SetAllDirty();
        }

#endif

        // Call from unity if animation properties have changed
        protected override void OnDidApplyAnimationProperties()
        {
            SetAllDirty();
        }

        /// <summary>
        /// Make the Graphic have the native size of its content.
        /// </summary>
        public virtual void SetNativeSize() {}
        public virtual bool Raycast(Vector2 sp, Camera eventCamera)
        {
            var t = transform;
            var components = ComponentListPool.Get();

            bool ignoreParentGroups = false;

            while (t != null)
            {
                t.GetComponents(components);
                for (var i = 0; i < components.Count; i++)
                {
                    var filter = components[i] as ICanvasRaycastFilter;

                    if (filter == null)
                        continue;

                    var raycastValid = true;

                    var group = components[i] as CanvasGroup;
                    if (group != null)
                    {
                        if (ignoreParentGroups == false && group.ignoreParentGroups)
                        {
                            ignoreParentGroups = true;
                            raycastValid = filter.IsRaycastLocationValid(sp, eventCamera);
                        }
                        else if (!ignoreParentGroups)
                            raycastValid = filter.IsRaycastLocationValid(sp, eventCamera);
                    }
                    else
                    {
                        raycastValid = filter.IsRaycastLocationValid(sp, eventCamera);
                    }

                    if (!raycastValid)
                    {
                        ComponentListPool.Release(components);
                        return false;
                    }
                }
                t = t.parent;
            }
            ComponentListPool.Release(components);
            return true;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetAllDirty();
        }

#endif

        public Vector2 PixelAdjustPoint(Vector2 point)
        {
            if (!canvas || !canvas.pixelPerfect)
                return point;

            return RectTransformUtility.PixelAdjustPoint(point, transform, canvas);
        }

        public Rect GetPixelAdjustedRect()
        {
            if (!canvas || !canvas.pixelPerfect)
                return rectTransform.rect;

            return RectTransformUtility.PixelAdjustRect(rectTransform, canvas);
        }

        public void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha)
        {
            CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha, true);
        }

        private void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha, bool useRGB)
        {
            if (canvasRenderer == null || (!useRGB && !useAlpha))
                return;

            Color currentColor = canvasRenderer.GetColor();
            if (currentColor.Equals(targetColor))
                return;

            ColorTween.ColorTweenMode mode = (useRGB && useAlpha ?
                                              ColorTween.ColorTweenMode.All :
                                              (useRGB ? ColorTween.ColorTweenMode.RGB : ColorTween.ColorTweenMode.Alpha));

            var colorTween = new ColorTween {duration = duration, startColor = canvasRenderer.GetColor(), targetColor = targetColor};
            colorTween.AddOnChangedCallback(canvasRenderer.SetColor);
            colorTween.ignoreTimeScale = ignoreTimeScale;
            colorTween.tweenMode = mode;
            m_ColorTweenRunner.StartTween(colorTween);
        }

        static private Color CreateColorFromAlpha(float alpha)
        {
            var alphaColor = Color.black;
            alphaColor.a = alpha;
            return alphaColor;
        }

        public void CrossFadeAlpha(float alpha, float duration, bool ignoreTimeScale)
        {
            CrossFadeColor(CreateColorFromAlpha(alpha), duration, ignoreTimeScale, true, false);
        }

        public void RegisterDirtyLayoutCallback(UnityAction action)
        {
            m_OnDirtyLayoutCallback += action;
        }

        public void UnregisterDirtyLayoutCallback(UnityAction action)
        {
            m_OnDirtyLayoutCallback -= action;
        }

        public void RegisterDirtyVerticesCallback(UnityAction action)
        {
            m_OnDirtyVertsCallback += action;
        }

        public void UnregisterDirtyVerticesCallback(UnityAction action)
        {
            m_OnDirtyVertsCallback -= action;
        }

        public void RegisterDirtyMaterialCallback(UnityAction action)
        {
            m_OnDirtyMaterialCallback += action;
        }

        public void UnregisterDirtyMaterialCallback(UnityAction action)
        {
            m_OnDirtyMaterialCallback -= action;
        }
    }
}
