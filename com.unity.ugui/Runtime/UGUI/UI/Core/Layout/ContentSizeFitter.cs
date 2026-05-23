using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [AddComponentMenu("Layout/Content Size Fitter", 141)]
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    [UGUIHelpURL("ContentSizeFitter")]
    /// <summary>
    /// Resizes a RectTransform to fit the size of its content.
    /// </summary>
    /// <remarks>
    /// The ContentSizeFitter can be used on GameObjects that have one or more ILayoutElement components, such as Text, Image, HorizontalLayoutGroup, VerticalLayoutGroup, and GridLayoutGroup.
    /// </remarks>
    public class ContentSizeFitter : UIBehaviour, ILayoutSelfController
    {
        /// <summary>
        /// Determines how the size of the layout element will adapt to its size properties.
        /// </summary>
        public enum FitMode
        {
            /// <summary>
            /// Don't perform any resizing.
            /// </summary>
            Unconstrained,
            /// <summary>
            /// Resize to the minimum size of the content.
            /// </summary>
            MinSize,
            /// <summary>
            /// Resize to the preferred size of the content and clamp it between its min and max sizes.
            /// </summary>
            PreferredSize,
            /// <summary>
            /// Clamp size of the content between minimum and maximum sizes.
            /// </summary>
            Clamped,
        }

        // Class-level constants
        private static readonly DrivenTransformProperties[] k_DrivenPropertyByAxis =
        {
            DrivenTransformProperties.SizeDeltaX,
            DrivenTransformProperties.SizeDeltaY
        };

        [Tooltip("Controls how the width of this RectTransform automatically resizes based on its content.")]
        [SerializeField] protected FitMode m_HorizontalFit = FitMode.Unconstrained;

        /// <summary>
        /// The fit mode to use to determine the width.
        /// </summary>
        public FitMode horizontalFit { get { return m_HorizontalFit; } set { if (SetPropertyUtility.SetStruct(ref m_HorizontalFit, value)) SetDirty(); } }

        [Tooltip("Controls how the height of this RectTransform automatically resizes based on its content.")]
        [SerializeField] protected FitMode m_VerticalFit = FitMode.Unconstrained;

        /// <summary>
        /// The fit mode to use to determine the height.
        /// </summary>
        public FitMode verticalFit { get { return m_VerticalFit; } set { if (SetPropertyUtility.SetStruct(ref m_VerticalFit, value)) SetDirty(); } }

        [System.NonSerialized] private RectTransform m_Rect;
        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        // field is never assigned warning
        #pragma warning disable 649
        private DrivenRectTransformTracker m_Tracker;
        #pragma warning restore 649

        protected ContentSizeFitter()
        {}

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        private void HandleSelfFittingAlongAxis(int axis)
        {
            FitMode fitting = (axis == 0 ? horizontalFit : verticalFit);

            switch (fitting)
            {
                case FitMode.Unconstrained:
                    m_Tracker.Add(this, rectTransform, DrivenTransformProperties.None);
                    break;
                case FitMode.MinSize:
                    m_Tracker.Add(this, rectTransform, k_DrivenPropertyByAxis[axis]);
                    rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, LayoutUtility.GetMinSize(m_Rect, axis));
                    break;
                case FitMode.PreferredSize:
                    m_Tracker.Add(this, rectTransform, k_DrivenPropertyByAxis[axis]);
                    rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, LayoutUtility.GetPreferredSize(m_Rect, axis));
                    break;
                case FitMode.Clamped:
                    HandleClampedFittingAlongAxis(axis);
                    break;
            }
        }

        private void HandleClampedFittingAlongAxis(int axis)
        {
            float min = LayoutUtility.GetMinSize(m_Rect, axis);
            float max = LayoutUtility.GetMaxSize(m_Rect, axis);
            var size = Mathf.Clamp(m_Rect.rect.size[axis], min, max);

            rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, size);
        }

        /// <summary>
        /// Calculate and apply the horizontal component of the size to the RectTransform
        /// </summary>
        public virtual void SetLayoutHorizontal()
        {
            m_Tracker.Clear();
            HandleSelfFittingAlongAxis(0);
        }

        /// <summary>
        /// Calculate and apply the vertical component of the size to the RectTransform
        /// </summary>
        public virtual void SetLayoutVertical()
        {
            HandleSelfFittingAlongAxis(1);
        }

        protected void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

    #if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }

    #endif
    }
}
