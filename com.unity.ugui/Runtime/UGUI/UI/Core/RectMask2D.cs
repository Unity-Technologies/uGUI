using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace UnityEngine.UI
{
    /// <summary>
    /// A 2D rectangular mask that allows for clipping / masking of areas outside the mask.
    /// </summary>
    /// <remarks>
    /// The RectMask2D behaves in a similar way to a standard Mask component. It differs though in some of the restrictions that it has.
    /// A RectMask2D:
    /// *Only works in the 2D plane
    /// *Requires elements on the mask to be coplanar.
    /// *Does not require stencil buffer / extra draw calls
    /// *Requires fewer draw calls
    /// *Culls elements that are outside the mask area.
    /// </remarks>
    [AddComponentMenu("UI (Canvas)/Rect Mask 2D", 14)]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [UGUIHelpURL("RectMask2D")]
    public class RectMask2D : UIBehaviour, IClipper, ICanvasRaycastFilter
    {
        [NonSerialized]
        private readonly RectangularVertexClipper m_VertexClipper = new RectangularVertexClipper();

        [NonSerialized]
        private RectTransform m_RectTransform;

        [NonSerialized]
        private HashSet<MaskableGraphic> m_MaskableTargets = new HashSet<MaskableGraphic>();

        [NonSerialized]
        private HashSet<IClippable> m_ClipTargets = new HashSet<IClippable>();

        [NonSerialized]
        private bool m_ShouldRecalculateClipRects;

        [NonSerialized]
        private List<RectMask2D> m_Clippers = new List<RectMask2D>();

        [NonSerialized]
        private Rect m_LastClipRectCanvasSpace;
        [NonSerialized]
        private bool m_ForceClip;

        [SerializeField]
        private Vector4 m_Padding = new Vector4();

        /// <summary>
        /// Padding to be applied to the masking
        /// X = Left
        /// Y = Bottom
        /// Z = Right
        /// W = Top
        /// </summary>
        public Vector4 padding
        {
            get { return m_Padding; }
            set
            {
                m_Padding = value;
                MaskUtilities.Notify2DMaskStateChanged(this);
            }
        }

        // Version of the serialized data. Until an edit sets it, Right/Top mirror Left/Bottom on read, so data and prefab
        // overrides saved while m_Softness was a Vector2Int reach both edges. Loading never changes it, or overrides would be lost.
        internal const int k_EdgeSoftnessVersion = 1;
        [SerializeField, HideInInspector]
        private int m_Version;

        // Left, Bottom, Right, Top. Data saved while this was a Vector2Int loads into Left and Bottom only.
        [SerializeField]
        private Vector4 m_Softness;

        /// <summary>
        /// The softness to apply to each edge of the mask.
        /// </summary>
        /// <remarks>
        /// X is the left edge, Y the bottom, Z the right and W the top, in the same order as <see cref="padding"/>.
        /// Each edge fades linearly over half that many pixels.
        /// Negative values are clamped to zero.
        /// </remarks>
        public Vector4 edgeSoftness
        {
            get
            {
                if (m_Version < k_EdgeSoftnessVersion)
                    return new Vector4(m_Softness.x, m_Softness.y, m_Softness.x, m_Softness.y);
                return m_Softness;
            }
            set
            {
                m_Softness = Vector4.Max(value, Vector4.zero);
                m_Version = k_EdgeSoftnessVersion;
                MaskUtilities.Notify2DMaskStateChanged(this);
            }
        }

        /// <summary>
        /// The softness to apply to the horizontal and vertical axis.
        /// </summary>
        /// <remarks>
        /// Setting it applies X to the left and right edges and Y to the bottom and top edges.
        /// Getting it returns the larger of the left and right edges, and of the bottom and top edges.
        /// </remarks>
        [Obsolete("softness is deprecated. Use edgeSoftness instead.", false)]
        public Vector2Int softness
        {
            get
            {
                var edges = edgeSoftness;
                return new Vector2Int(Mathf.RoundToInt(Mathf.Max(edges.x, edges.z)), Mathf.RoundToInt(Mathf.Max(edges.y, edges.w)));
            }
            set { edgeSoftness = new Vector4(value.x, value.y, value.x, value.y); }
        }

        /// <remarks>
        /// Returns a non-destroyed instance or a null reference.
        /// </remarks>
        [NonSerialized] private Canvas m_Canvas;
        internal Canvas Canvas
        {
            get
            {
                if (m_Canvas == null)
                {
                    var list = ListPool<Canvas>.Get();
                    gameObject.GetComponentsInParent(false, list);
                    if (list.Count > 0)
                        m_Canvas = list[list.Count - 1];
                    else
                        m_Canvas = null;
                    ListPool<Canvas>.Release(list);
                }

                return m_Canvas;
            }
        }

        /// <summary>
        /// Get the Rect for the mask in canvas space.
        /// </summary>
        public Rect canvasRect
        {
            get
            {
                return m_VertexClipper.GetCanvasRect(rectTransform, Canvas);
            }
        }

        /// <summary>
        /// Get the mask's rect with padding applied, in the canvas's local space.
        /// </summary>
        /// <returns>The padded rect in canvas space, or an empty rect if the padding leaves no area.</returns>
        internal Rect GetPaddedCanvasRect()
        {
            // Padded before projecting, so it stays on its own edge and scales with the mask at any angle.
            Rect rect = rectTransform.rect;
            Rect padded = Rect.MinMaxRect(rect.xMin + m_Padding.x, rect.yMin + m_Padding.y, rect.xMax - m_Padding.z, rect.yMax - m_Padding.w);
            if (padded.width <= 0 || padded.height <= 0)
                return new Rect();

            // Past 90 degrees the mask projects mirrored, giving a negative, invalid rect.
            Rect canvasRect = m_VertexClipper.GetCanvasRect(rectTransform, Canvas, padded);
            return Rect.MinMaxRect(
                Mathf.Min(canvasRect.xMin, canvasRect.xMax),
                Mathf.Min(canvasRect.yMin, canvasRect.yMax),
                Mathf.Max(canvasRect.xMin, canvasRect.xMax),
                Mathf.Max(canvasRect.yMin, canvasRect.yMax));
        }

        /// <summary>
        /// Helper function to get the RectTransform for the mask.
        /// </summary>
        public RectTransform rectTransform
        {
            get { return m_RectTransform ?? (m_RectTransform = GetComponent<RectTransform>()); }
        }

        /// <summary>Protected default constructor. Use <see cref="GameObject.AddComponent{T}"/> to add a RectMask2D to a GameObject.</summary>
        protected RectMask2D()
        {}

        /// <summary>Called when it becomes enabled. Registers with the <see cref="ClipperRegistry"/> and notifies clippable children.</summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            m_ShouldRecalculateClipRects = true;
            ClipperRegistry.Register(this);
            MaskUtilities.Notify2DMaskStateChanged(this);
        }

        /// <summary>Called when it becomes disabled. Unregisters from the <see cref="ClipperRegistry"/> and notifies clippable children.</summary>
        protected override void OnDisable()
        {
            // we call base OnDisable first here
            // as we need to have the IsActive return the
            // correct value when we notify the children
            // that the mask state has changed.
            base.OnDisable();
            m_ClipTargets.Clear();
            m_MaskableTargets.Clear();
            m_Clippers.Clear();
            ClipperRegistry.Disable(this);
            MaskUtilities.Notify2DMaskStateChanged(this);
        }

        /// <summary>Called when destroyed. Unregisters from the <see cref="ClipperRegistry"/>.</summary>
        protected override void OnDestroy()
        {
            ClipperRegistry.Unregister(this);
            base.OnDestroy();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            m_ShouldRecalculateClipRects = true;

            // Dont allow negative softness.
            m_Softness = Vector4.Max(m_Softness, Vector4.zero);

            if (!IsActive())
                return;

            MaskUtilities.Notify2DMaskStateChanged(this);
        }

#endif

        /// <summary>Returns whether the given screen position is within this mask's rect.</summary>
        /// <param name="sp">The screen position to test.</param>
        /// <param name="eventCamera">The camera for the raycast.</param>
        /// <returns>True if the point is inside the masked rectangle, or if the component is disabled.</returns>
        public virtual bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            if (!isActiveAndEnabled)
                return true;

            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, sp, eventCamera, m_Padding);
        }

        private Vector3[] m_Corners = new Vector3[4];

        private Rect rootCanvasRect
        {
            get
            {
                rectTransform.GetWorldCorners(m_Corners);

                if (!ReferenceEquals(Canvas, null))
                {
                    Canvas rootCanvas = Canvas.rootCanvas;
                    for (int i = 0; i < 4; ++i)
                        m_Corners[i] = rootCanvas.transform.InverseTransformPoint(m_Corners[i]);
                }

                return new Rect(m_Corners[0].x, m_Corners[0].y, m_Corners[2].x - m_Corners[0].x, m_Corners[2].y - m_Corners[0].y);
            }
        }

        /// <summary>Applies the mask's clipping rectangle to all registered clippable children.</summary>
        public virtual void PerformClipping()
        {
            if (ReferenceEquals(Canvas, null))
            {
                return;
            }

            //TODO See if an IsActive() test would work well here or whether it might cause unexpected side effects (re case 776771)

            // if the parents are changed
            // or something similar we
            // do a recalculate here
            if (m_ShouldRecalculateClipRects)
            {
                MaskUtilities.GetRectMasksForClip(this, m_Clippers);
                m_ShouldRecalculateClipRects = false;
            }

            // get the compound rects from
            // the clippers that are valid
            bool validRect = true;
            Rect clipRect = Clipping.FindCullAndClipWorldRect(m_Clippers, out validRect);

            // If the mask is in ScreenSpaceOverlay/Camera render mode, its content is only rendered when its rect
            // overlaps that of the root canvas.
            RenderMode renderMode = Canvas.rootCanvas.renderMode;
            bool maskIsCulled =
                (renderMode == RenderMode.ScreenSpaceCamera || renderMode == RenderMode.ScreenSpaceOverlay) &&
                !clipRect.Overlaps(rootCanvasRect, true);

            if (maskIsCulled)
            {
                // Children are only displayed when inside the mask. If the mask is culled, then the children
                // inside the mask are also culled. In that situation, we pass an invalid rect to allow callees
                // to avoid some processing.
                clipRect = Rect.zero;
                validRect = false;
            }

            if (clipRect != m_LastClipRectCanvasSpace)
            {
                foreach (IClippable clipTarget in m_ClipTargets)
                {
                    clipTarget.SetClipRect(clipRect, validRect);
                }

                foreach (MaskableGraphic maskableTarget in m_MaskableTargets)
                {
                    maskableTarget.SetClipRect(clipRect, validRect);
                    maskableTarget.Cull(clipRect, validRect);
                }
            }
            else if (m_ForceClip)
            {
                foreach (IClippable clipTarget in m_ClipTargets)
                {
                    clipTarget.SetClipRect(clipRect, validRect);
                }

                foreach (MaskableGraphic maskableTarget in m_MaskableTargets)
                {
                    maskableTarget.SetClipRect(clipRect, validRect);

                    if (maskableTarget.canvasRenderer.hasMoved)
                        maskableTarget.Cull(clipRect, validRect);
                }
            }
            else
            {
                foreach (MaskableGraphic maskableTarget in m_MaskableTargets)
                {
                    //Case 1170399 - hasMoved is not a valid check when animating on pivot of the object
                    maskableTarget.Cull(clipRect, validRect);
                }
            }

            m_LastClipRectCanvasSpace = clipRect;
            m_ForceClip = false;

            UpdateClipSoftness();
        }

        /// <summary>Propagates the current clip softness to all registered clippable children.</summary>
        public virtual void UpdateClipSoftness()
        {
            if (ReferenceEquals(Canvas, null))
            {
                return;
            }

            Vector2 scale = m_VertexClipper.GetCanvasScale(rectTransform, Canvas);
            Vector4 softness = Vector4.Scale(edgeSoftness, new Vector4(scale.x, scale.y, scale.x, scale.y));

            foreach (IClippable clipTarget in m_ClipTargets)
            {
                clipTarget.SetClipSoftness(softness);
            }

            foreach (MaskableGraphic maskableTarget in m_MaskableTargets)
            {
                maskableTarget.SetClipSoftness(softness);
            }
        }

        /// <summary>
        /// Add a IClippable to be tracked by the mask.
        /// </summary>
        /// <param name="clippable">Add the clippable object for this mask</param>
        public void AddClippable(IClippable clippable)
        {
            if (clippable == null)
                return;
            m_ShouldRecalculateClipRects = true;
            MaskableGraphic maskable = clippable as MaskableGraphic;

            if (maskable == null)
                m_ClipTargets.Add(clippable);
            else
                m_MaskableTargets.Add(maskable);

            m_ForceClip = true;
        }

        /// <summary>
        /// Remove an IClippable from being tracked by the mask.
        /// </summary>
        /// <param name="clippable">Remove the clippable object from this mask</param>
        public void RemoveClippable(IClippable clippable)
        {
            if (clippable == null)
                return;

            m_ShouldRecalculateClipRects = true;
            clippable.SetClipRect(new Rect(), false);

            MaskableGraphic maskable = clippable as MaskableGraphic;

            if (maskable == null)
                m_ClipTargets.Remove(clippable);
            else
                m_MaskableTargets.Remove(maskable);

            m_ForceClip = true;
        }

        /// <inheritdoc/>
        /// <remarks>Sets flag to recalculate clip rects.</remarks>
        protected override void OnTransformParentChanged()
        {
            m_Canvas = null;
            base.OnTransformParentChanged();
            m_ShouldRecalculateClipRects = true;
        }

        /// <inheritdoc/>
        /// <remarks>Sets flag to recalculate clip rects.</remarks>
        protected override void OnCanvasHierarchyChanged()
        {
            m_Canvas = null;
            base.OnCanvasHierarchyChanged();
            m_ShouldRecalculateClipRects = true;
        }
    }
}
