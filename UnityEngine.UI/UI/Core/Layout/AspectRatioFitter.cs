using System.Collections;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [AddComponentMenu("Layout/Aspect Ratio Fitter", 142)]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class AspectRatioFitter : UIBehaviour, ILayoutSelfController
    {
        public enum AspectMode { None, WidthControlsHeight, HeightControlsWidth, FitInParent, EnvelopeParent }

        [SerializeField] private AspectMode m_AspectMode = AspectMode.None;
        public AspectMode aspectMode { get { return m_AspectMode; } set { if (SetPropertyUtility.SetStruct(ref m_AspectMode, value)) SetDirty(); } }

        [SerializeField] private float m_AspectRatio = 1;
        public float aspectRatio { get { return m_AspectRatio; } set { if (SetPropertyUtility.SetStruct(ref m_AspectRatio, value)) SetDirty(); } }

        [System.NonSerialized]
        private RectTransform m_Rect;

        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        private DrivenRectTransformTracker m_Tracker;

        protected AspectRatioFitter() {}

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty(true);
        }

        protected override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            UpdateRect();
        }

        private void UpdateRect()
        {
            if (!IsActive())
                return;

            m_Tracker.Clear();

            switch (m_AspectMode)
            {
#if UNITY_EDITOR
                case AspectMode.None:
                {
                    if (!Application.isPlaying)
                        m_AspectRatio = Mathf.Clamp(rectTransform.rect.width / rectTransform.rect.height, 0.001f, 1000f);

                    break;
                }
#endif
                case AspectMode.HeightControlsWidth:
                {
                    m_Tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaX);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectTransform.rect.height * m_AspectRatio);
                    break;
                }
                case AspectMode.WidthControlsHeight:
                {
                    m_Tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaY);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rectTransform.rect.width / m_AspectRatio);
                    break;
                }
                case AspectMode.FitInParent:
                case AspectMode.EnvelopeParent:
                {
                    m_Tracker.Add(this, rectTransform,
                        DrivenTransformProperties.Anchors |
                        DrivenTransformProperties.AnchoredPosition |
                        DrivenTransformProperties.SizeDeltaX |
                        DrivenTransformProperties.SizeDeltaY);

                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.anchoredPosition = Vector2.zero;

                    Vector2 sizeDelta = Vector2.zero;
                    Vector2 parentSize = GetParentSize();
                    if ((parentSize.y * aspectRatio < parentSize.x) ^ (m_AspectMode == AspectMode.FitInParent))
                    {
                        sizeDelta.y = GetSizeDeltaToProduceSize(parentSize.x / aspectRatio, 1);
                    }
                    else
                    {
                        sizeDelta.x = GetSizeDeltaToProduceSize(parentSize.y * aspectRatio, 0);
                    }
                    rectTransform.sizeDelta = sizeDelta;

                    break;
                }
            }
        }

        private float GetSizeDeltaToProduceSize(float size, int axis)
        {
            return size - GetParentSize()[axis] * (rectTransform.anchorMax[axis] - rectTransform.anchorMin[axis]);
        }

        private Vector2 GetParentSize()
        {
            RectTransform parent = rectTransform.parent as RectTransform;
            if (!parent)
                return Vector2.zero;
            return parent.rect.size;
        }

        public virtual void SetLayoutHorizontal() {}

        public virtual void SetLayoutVertical() {}

        private IEnumerator DelayUpdate()
        {
            yield return null;
            UpdateRect();
        }

        protected void SetDirty(bool delayUpdate = false)
        {
            if (delayUpdate)
                StartCoroutine(DelayUpdate());
            else
                UpdateRect();
        }

    #if UNITY_EDITOR
        protected override void OnValidate()
        {
            m_AspectRatio = Mathf.Clamp(m_AspectRatio, 0.001f, 1000f);
            SetDirty();
        }

    #endif
    }
}
