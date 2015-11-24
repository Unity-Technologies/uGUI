using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/2D Rect Mask", 13)]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class RectMask2D : UIBehaviour, IClipper, ICanvasRaycastFilter
    {
        [NonSerialized]
        private readonly RectangularVertexClipper m_VertexClipper = new RectangularVertexClipper();

        [NonSerialized]
        private RectTransform m_RectTransform;

        [NonSerialized]
        private List<IClippable> m_ClipTargets = new List<IClippable>();

        [NonSerialized]
        private bool m_ShouldRecalculateClipRects;

        [NonSerialized]
        private List<RectMask2D> m_Clippers = new List<RectMask2D>();

        [NonSerialized]
        private Rect m_LastClipRectCanvasSpace;
        [NonSerialized]
        private bool m_LastClipRectValid;

        public Rect canvasRect
        {
            get
            {
                Canvas canvas = null;
                var list = ListPool<Canvas>.Get();
                gameObject.GetComponentsInParent(false, list);
                if (list.Count > 0)
                    canvas = list[0];
                ListPool<Canvas>.Release(list);

                return m_VertexClipper.GetCanvasRect(rectTransform, canvas);
            }
        }

        public RectTransform rectTransform
        {
            get { return m_RectTransform ?? (m_RectTransform = GetComponent<RectTransform>()); }
        }

        protected RectMask2D()
        {}

        protected override void OnEnable()
        {
            base.OnEnable();
            m_ShouldRecalculateClipRects = true;
            ClipperRegistry.Register(this);
            MaskUtilities.Notify2DMaskStateChanged(this);
        }

        protected override void OnDisable()
        {
            // we call base OnDisable first here
            // as we need to have the IsActive return the
            // correct value when we notify the children
            // that the mask state has changed.
            base.OnDisable();
            m_ClipTargets.Clear();
            m_Clippers.Clear();
            ClipperRegistry.Unregister(this);
            MaskUtilities.Notify2DMaskStateChanged(this);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            m_ShouldRecalculateClipRects = true;

            if (!IsActive())
                return;

            MaskUtilities.Notify2DMaskStateChanged(this);
        }

#endif

        public virtual bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            if (!isActiveAndEnabled)
                return true;

            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, sp, eventCamera);
        }

        public virtual void PerformClipping()
        {
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
            if (clipRect != m_LastClipRectCanvasSpace)
            {
                for (int i = 0; i < m_ClipTargets.Count; ++i)
                    m_ClipTargets[i].SetClipRect(clipRect, validRect);

                m_LastClipRectCanvasSpace = clipRect;
                m_LastClipRectValid = validRect;
            }

            for (int i = 0; i < m_ClipTargets.Count; ++i)
                m_ClipTargets[i].Cull(m_LastClipRectCanvasSpace, m_LastClipRectValid);
        }

        public void AddClippable(IClippable clippable)
        {
            if (clippable == null)
                return;

            if (!m_ClipTargets.Contains(clippable))
                m_ClipTargets.Add(clippable);

            clippable.SetClipRect(m_LastClipRectCanvasSpace, m_LastClipRectValid);
            clippable.Cull(m_LastClipRectCanvasSpace, m_LastClipRectValid);
        }

        public void RemoveClippable(IClippable clippable)
        {
            if (clippable == null)
                return;

            clippable.SetClipRect(new Rect(), false);
            m_ClipTargets.Remove(clippable);
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            m_ShouldRecalculateClipRects = true;
        }

        protected override void OnCanvasHierarchyChanged()
        {
            base.OnCanvasHierarchyChanged();
            m_ShouldRecalculateClipRects = true;
        }
    }
}
