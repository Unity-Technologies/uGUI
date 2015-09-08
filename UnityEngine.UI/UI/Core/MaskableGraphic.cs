using System;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace UnityEngine.UI
{
    public abstract class MaskableGraphic : Graphic, IClippable, IMaskable, IMaterialModifier
    {
        [NonSerialized]
        protected bool m_ShouldRecalculateStencil = true;

        [NonSerialized]
        protected Material m_MaskMaterial;

        [NonSerialized]
        private RectMask2D m_ParentMask;

        // m_Maskable is whether this graphic is allowed to be masked or not. It has the matching public property maskable.
        // The default for m_Maskable is true, so graphics under a mask are masked out of the box.
        // The maskable property can be turned off from script by the user if masking is not desired.
        // m_IncludeForMasking is whether we actually consider this graphic for masking or not - this is an implementation detail.
        // m_IncludeForMasking should only be true if m_Maskable is true AND a parent of the graphic has an IMask component.
        // Things would still work correctly if m_IncludeForMasking was always true when m_Maskable is, but performance would suffer.
        [NonSerialized]
        private bool m_Maskable = true;

        [NonSerialized]
        [Obsolete("Not used anymore.", true)]
        protected bool m_IncludeForMasking = false;

        [Serializable]
        public class CullStateChangedEvent : UnityEvent<bool> {}

        // Event delegates triggered on click.
        [SerializeField]
        private CullStateChangedEvent m_OnCullStateChanged = new CullStateChangedEvent();

        public CullStateChangedEvent onCullStateChanged
        {
            get { return m_OnCullStateChanged; }
            set { m_OnCullStateChanged = value; }
        }

        public bool maskable
        {
            get { return m_Maskable; }
            set
            {
                if (value == m_Maskable)
                    return;
                m_Maskable = value;
                m_ShouldRecalculateStencil = true;
                SetMaterialDirty();
            }
        }

        [NonSerialized]
        [Obsolete("Not used anymore", true)]
        protected bool m_ShouldRecalculate = true;

        [NonSerialized]
        protected int m_StencilValue;

        public virtual Material GetModifiedMaterial(Material baseMaterial)
        {
            var toUse = baseMaterial;

            if (m_ShouldRecalculateStencil)
            {
                var rootCanvas = MaskUtilities.FindRootSortOverrideCanvas(transform);
                m_StencilValue = maskable ? MaskUtilities.GetStencilDepth(transform, rootCanvas) : 0;
                m_ShouldRecalculateStencil = false;
            }

            // if we have a Mask component then it will
            // generate the mask material. This is an optimisation
            // it adds some coupling between components though :(
            if (m_StencilValue > 0 && GetComponent<Mask>() == null)
            {
                var maskMat = StencilMaterial.Add(toUse, (1 << m_StencilValue) - 1, StencilOp.Keep, CompareFunction.Equal, ColorWriteMask.All, (1 << m_StencilValue) - 1, 0);
                StencilMaterial.Remove(m_MaskMaterial);
                m_MaskMaterial = maskMat;
                toUse = m_MaskMaterial;
            }
            return toUse;
        }

        public virtual void Cull(Rect clipRect, bool validRect)
        {
            if (!canvasRenderer.hasMoved)
                return;

            var cull = !validRect || !clipRect.Overlaps(canvasRect);
            var cullingChanged = canvasRenderer.cull != cull;
            canvasRenderer.cull = cull;

            if (cullingChanged)
            {
                m_OnCullStateChanged.Invoke(cull);
                SetVerticesDirty();
            }
        }

        public virtual void SetClipRect(Rect clipRect, bool validRect)
        {
            if (validRect)
                canvasRenderer.EnableRectClipping(clipRect);
            else
                canvasRenderer.DisableRectClipping();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_ShouldRecalculateStencil = true;
            UpdateClipParent();
            SetMaterialDirty();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            m_ShouldRecalculateStencil = true;
            SetMaterialDirty();
            UpdateClipParent();
            StencilMaterial.Remove(m_MaskMaterial);
            m_MaskMaterial = null;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            m_ShouldRecalculateStencil = true;
            UpdateClipParent();
            SetMaterialDirty();
        }

#endif

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            m_ShouldRecalculateStencil = true;
            UpdateClipParent();
            SetMaterialDirty();
        }

        [Obsolete("Not used anymore.", true)]
        public virtual void ParentMaskStateChanged() {}

        protected override void OnCanvasHierarchyChanged()
        {
            base.OnCanvasHierarchyChanged();
            m_ShouldRecalculateStencil = true;
            UpdateClipParent();
            SetMaterialDirty();
        }

        readonly Vector3[] m_Corners = new Vector3[4];
        private Rect canvasRect
        {
            get
            {
                rectTransform.GetWorldCorners(m_Corners);

                if (canvas)
                {
                    for (int i = 0; i < 4; ++i)
                        m_Corners[i] = canvas.transform.InverseTransformPoint(m_Corners[i]);
                }

                return new Rect(m_Corners[0].x, m_Corners[0].y, m_Corners[2].x - m_Corners[0].x, m_Corners[2].y - m_Corners[0].y);
            }
        }

        private void UpdateClipParent()
        {
            var newParent = (maskable && IsActive()) ? MaskUtilities.GetRectMaskForClippable(this) : null;

            if (newParent != m_ParentMask && m_ParentMask != null)
                m_ParentMask.RemoveClippable(this);

            if (newParent != null)
                newParent.AddClippable(this);

            m_ParentMask = newParent;
        }

        public virtual void RecalculateClipping()
        {
            UpdateClipParent();
        }

        public virtual void RecalculateMasking()
        {
            m_ShouldRecalculateStencil = true;
            SetMaterialDirty();
        }
    }
}
