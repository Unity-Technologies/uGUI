using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace UnityEngine.UI
{
    /// <summary>
    /// Abstract base class for layout groups.
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public abstract class LayoutGroup : UIBehaviour, ILayoutElement, ILayoutGroup
    {
        /// <summary>Serialized backing field for <see cref="padding"/>.</summary>
        [SerializeField] protected RectOffset m_Padding = new RectOffset();

        /// <summary>
        /// The padding to add around the child layout elements.
        /// </summary>
        public RectOffset padding { get { return m_Padding; } set { SetProperty(ref m_Padding, value); } }

        /// <summary>Serialized backing field for <see cref="childAlignment"/>.</summary>
        [SerializeField] protected TextAnchor m_ChildAlignment = TextAnchor.UpperLeft;

        /// <summary>
        /// The alignment to use for the child layout elements in the layout group.
        /// </summary>
        /// <remarks>
        /// If a layout element does not specify a flexible width or height, its child elements many not use the available space within the layout group. In this case, use the alignment settings to specify how to align child elements within their layout group.
        /// </remarks>
        public TextAnchor childAlignment { get { return m_ChildAlignment; } set { SetProperty(ref m_ChildAlignment, value); } }

        [System.NonSerialized] private RectTransform m_Rect;
        /// <summary>The RectTransform of this layout group's GameObject.</summary>
        protected RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        /// <summary>Tracks driven RectTransform properties to prevent manual editing during layout.</summary>
        protected DrivenRectTransformTracker m_Tracker;
        private Vector2 m_TotalMinSize = Vector2.zero;
        private Vector2 m_TotalMaxSize = Vector2.positiveInfinity;
        private Vector2 m_TotalPreferredSize = Vector2.zero;
        private Vector2 m_TotalFlexibleSize = Vector2.zero;

        [System.NonSerialized] private List<RectTransform> m_RectChildren = new List<RectTransform>();
        /// <summary>The list of valid child RectTransforms used during layout calculations.</summary>
        protected List<RectTransform> rectChildren { get { return m_RectChildren; } }

        /// <summary>
        /// Called by the layout system. This base implementation collects valid child RectTransforms.
        /// Override in subclasses to calculate the horizontal layout sizes.
        /// </summary>
        public virtual void CalculateLayoutInputHorizontal()
        {
            m_RectChildren.Clear();
            var toIgnoreList = ListPool<Component>.Get();
            for (int i = 0; i < rectTransform.childCount; i++)
            {
                var rect = rectTransform.GetChild(i) as RectTransform;
                if (rect == null || !rect.gameObject.activeInHierarchy)
                    continue;

                rect.GetComponents(typeof(ILayoutIgnorer), toIgnoreList);

                if (toIgnoreList.Count == 0)
                {
                    m_RectChildren.Add(rect);
                    continue;
                }

                for (int j = 0; j < toIgnoreList.Count; j++)
                {
                    var ignorer = (ILayoutIgnorer)toIgnoreList[j];
                    if (!ignorer.ignoreLayout)
                    {
                        m_RectChildren.Add(rect);
                        break;
                    }
                }
            }
            ListPool<Component>.Release(toIgnoreList);
            m_Tracker.Clear();
        }

        /// <summary>Called by the layout system. Override in subclasses to calculate the vertical layout sizes.</summary>
        public abstract void CalculateLayoutInputVertical();

        /// <summary>
        /// See LayoutElement.minWidth
        /// </summary>
        public virtual float minWidth { get { return GetTotalMinSize(0); } }

        /// <inheritdoc/>
        public virtual float maxWidth { get { return GetTotalMaxSize(0); } }

        /// <summary>
        /// See LayoutElement.preferredWidth
        /// </summary>
        public virtual float preferredWidth { get { return GetTotalPreferredSize(0); } }

        /// <summary>
        /// See LayoutElement.flexibleWidth
        /// </summary>
        public virtual float flexibleWidth { get { return GetTotalFlexibleSize(0); } }

        /// <summary>
        /// See LayoutElement.minHeight
        /// </summary>
        public virtual float minHeight { get { return GetTotalMinSize(1); } }

        /// <inheritdoc/>
        public virtual float maxHeight { get { return GetTotalMaxSize(1); } }

        /// <summary>
        /// See LayoutElement.preferredHeight
        /// </summary>
        public virtual float preferredHeight { get { return GetTotalPreferredSize(1); } }

        /// <summary>
        /// See LayoutElement.flexibleHeight
        /// </summary>
        public virtual float flexibleHeight { get { return GetTotalFlexibleSize(1); } }

        /// <summary>
        /// See LayoutElement.layoutPriority
        /// </summary>
        public virtual int layoutPriority { get { return 0; } }

        // ILayoutController Interface

        /// <summary>Called by the layout system. Override in subclasses to perform the horizontal layout.</summary>
        public abstract void SetLayoutHorizontal();
        /// <summary>Called by the layout system. Override in subclasses to perform the vertical layout.</summary>
        public abstract void SetLayoutVertical();

        // Implementation

        /// <summary>Protected default constructor. Use <see cref="GameObject.AddComponent{T}"/> on a concrete subclass.</summary>
        protected LayoutGroup()
        {
            if (m_Padding == null)
                m_Padding = new RectOffset();
        }

        /// <summary>
        /// Called when it becomes enabled. Registers for the OnChildRectTransformDimensionsChange callback and
        /// marks this for a layout rebuild.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            rectTransform.sendChildDimensionsChange = true;
            SetDirty();
        }

        /// <summary>
        /// Called when it becomes disabled. Unregisters from the OnChildRectTransformDimensionsChange callback and
        /// marks this for a layout rebuild.
        /// </summary>
        protected override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            rectTransform.sendChildDimensionsChange = false;
            base.OnDisable();
        }

        /// <summary>
        /// Callback for when properties have been changed by animation.
        /// </summary>
        protected override void OnDidApplyAnimationProperties()
        {
            SetDirty();
        }

        /// <summary>
        /// The min size for the layout group on the given axis.
        /// </summary>
        /// <param name="axis">The axis index. 0 is horizontal and 1 is vertical.</param>
        /// <returns>The min size</returns>
        protected float GetTotalMinSize(int axis)
        {
            return m_TotalMinSize[axis];
        }

        /// <summary>
        /// Gets the maximum size for the layout group on the given axis.
        /// </summary>
        /// <param name="axis"> The axis index. 0 is horizontal and 1 is vertical.</param>
        /// <returns>The maximum size of the layout group along the specified axis.</returns>
        protected float GetTotalMaxSize(int axis)
        {
            return m_TotalMaxSize[axis];
        }

        /// <summary>
        /// The preferred size for the layout group on the given axis.
        /// </summary>
        /// <param name="axis">The axis index. 0 is horizontal and 1 is vertical.</param>
        /// <returns>The preferred size.</returns>
        protected float GetTotalPreferredSize(int axis)
        {
            return m_TotalPreferredSize[axis];
        }

        /// <summary>
        /// The flexible size for the layout group on the given axis.
        /// </summary>
        /// <param name="axis">The axis index. 0 is horizontal and 1 is vertical.</param>
        /// <returns>The flexible size</returns>
        protected float GetTotalFlexibleSize(int axis)
        {
            return m_TotalFlexibleSize[axis];
        }

        /// <summary>
        /// Returns the calculated position of the first child layout element along the given axis.
        /// </summary>
        /// <param name="axis">The axis index. 0 is horizontal and 1 is vertical.</param>
        /// <param name="requiredSpaceWithoutPadding">The total space required on the given axis for all the layout elements including spacing and excluding padding.</param>
        /// <returns>The position of the first child along the given axis.</returns>
        protected float GetStartOffset(int axis, float requiredSpaceWithoutPadding)
        {
            float requiredSpace = requiredSpaceWithoutPadding + (axis == 0 ? padding.horizontal : padding.vertical);
            float availableSpace = rectTransform.rect.size[axis];
            float surplusSpace = availableSpace - requiredSpace;
            float alignmentOnAxis = GetAlignmentOnAxis(axis);
            return (axis == 0 ? padding.left : padding.top) + surplusSpace * alignmentOnAxis;
        }

        /// <summary>
        /// Returns the alignment on the specified axis as a fraction where 0 is left/top, 0.5 is middle, and 1 is right/bottom.
        /// </summary>
        /// <param name="axis">The axis to get alignment along. 0 is horizontal and 1 is vertical.</param>
        /// <returns>The alignment as a fraction where 0 is left/top, 0.5 is middle, and 1 is right/bottom.</returns>
        protected float GetAlignmentOnAxis(int axis)
        {
            if (axis == 0)
                return ((int)childAlignment % 3) * 0.5f;
            else
                return ((int)childAlignment / 3) * 0.5f;
        }

        /// <summary>
        /// Set the calculated layout properties for the given axis.
        /// </summary>
        /// <param name="totalMin">The min size for the layout group.</param>
        /// <param name="totalMax">The maximum size for the layout group.</param>
        /// <param name="totalPreferred">The preferred size for the layout group.</param>
        /// <param name="totalFlexible">The flexible size for the layout group.</param>
        /// <param name="axis">The axis to set sizes for. 0 is horizontal and 1 is vertical.</param>
        protected void SetLayoutInputForAxis(float totalMin, float totalMax, float totalPreferred, float totalFlexible, int axis)
        {
            m_TotalMinSize[axis] = totalMin;
            m_TotalMaxSize[axis] = totalMax;
            m_TotalPreferredSize[axis] = totalPreferred;
            m_TotalFlexibleSize[axis] = totalFlexible;
        }

        /// <summary>
        /// Set the position and size of a child layout element along the given axis.
        /// </summary>
        /// <param name="rect">The RectTransform of the child layout element.</param>
        /// <param name="axis">The axis to set the position and size along. 0 is horizontal and 1 is vertical.</param>
        /// <param name="pos">The position from the left side or top.</param>
        protected void SetChildAlongAxis(RectTransform rect, int axis, float pos)
        {
            if (rect == null)
                return;

            SetChildAlongAxisWithScale(rect, axis, pos, 1.0f);
        }

        /// <summary>
        /// Set the position and size of a child layout element along the given axis.
        /// </summary>
        /// <param name="rect">The RectTransform of the child layout element.</param>
        /// <param name="axis">The axis to set the position and size along. 0 is horizontal and 1 is vertical.</param>
        /// <param name="pos">The position from the left side or top.</param>
        /// <param name="scaleFactor">The scale factor to apply to the child's size.</param>
        protected void SetChildAlongAxisWithScale(RectTransform rect, int axis, float pos, float scaleFactor)
        {
            if (rect == null)
                return;

            m_Tracker.Add(this, rect,
                DrivenTransformProperties.Anchors |
                (axis == 0 ? DrivenTransformProperties.AnchoredPositionX : DrivenTransformProperties.AnchoredPositionY));

            // Inlined rect.SetInsetAndSizeFromParentEdge(...) and refactored code in order to multiply desired size by scaleFactor.
            // sizeDelta must stay the same but the size used in the calculation of the position must be scaled by the scaleFactor.

            rect.anchorMin = Vector2.up;
            rect.anchorMax = Vector2.up;

            Vector2 anchoredPosition = rect.anchoredPosition;
            anchoredPosition[axis] = (axis == 0) ? (pos + rect.sizeDelta[axis] * rect.pivot[axis] * scaleFactor) : (-pos - rect.sizeDelta[axis] * (1f - rect.pivot[axis]) * scaleFactor);
            rect.anchoredPosition = anchoredPosition;
        }

        /// <summary>
        /// Set the position and size of a child layout element along the given axis.
        /// </summary>
        /// <param name="rect">The RectTransform of the child layout element.</param>
        /// <param name="axis">The axis to set the position and size along. 0 is horizontal and 1 is vertical.</param>
        /// <param name="pos">The position from the left side or top.</param>
        /// <param name="size">The size.</param>
        protected void SetChildAlongAxis(RectTransform rect, int axis, float pos, float size)
        {
            if (rect == null)
                return;

            SetChildAlongAxisWithScale(rect, axis, pos, size, 1.0f);
        }

        /// <summary>
        /// Set the position and size of a child layout element along the given axis.
        /// </summary>
        /// <param name="rect">The RectTransform of the child layout element.</param>
        /// <param name="axis">The axis to set the position and size along. 0 is horizontal and 1 is vertical.</param>
        /// <param name="pos">The position from the left side or top.</param>
        /// <param name="size">The size.</param>
        /// <param name="scaleFactor">The scale factor to apply to the child's size.</param>
        protected void SetChildAlongAxisWithScale(RectTransform rect, int axis, float pos, float size, float scaleFactor)
        {
            if (rect == null)
                return;

            m_Tracker.Add(this, rect,
                DrivenTransformProperties.Anchors |
                (axis == 0 ?
                    (DrivenTransformProperties.AnchoredPositionX | DrivenTransformProperties.SizeDeltaX) :
                    (DrivenTransformProperties.AnchoredPositionY | DrivenTransformProperties.SizeDeltaY)
                )
            );

            // Inlined rect.SetInsetAndSizeFromParentEdge(...) and refactored code in order to multiply desired size by scaleFactor.
            // sizeDelta must stay the same but the size used in the calculation of the position must be scaled by the scaleFactor.

            rect.anchorMin = Vector2.up;
            rect.anchorMax = Vector2.up;

            Vector2 sizeDelta = rect.sizeDelta;
            sizeDelta[axis] = size;
            rect.sizeDelta = sizeDelta;

            Vector2 anchoredPosition = rect.anchoredPosition;
            anchoredPosition[axis] = (axis == 0) ? (pos + size * rect.pivot[axis] * scaleFactor) : (-pos - size * (1f - rect.pivot[axis]) * scaleFactor);
            rect.anchoredPosition = anchoredPosition;
        }

        private bool isRootLayoutGroup
        {
            get
            {
                Transform parent = transform.parent;
                if (parent == null)
                    return true;
                return transform.parent.GetComponent(typeof(ILayoutGroup)) == null;
            }
        }

        /// <summary>Called when the RectTransform dimensions change. Registers for a layout rebuild.</summary>
        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            if (isRootLayoutGroup)
                SetDirty();
        }

        /// <summary>Called when child transforms are added or removed. Registers for a layout rebuild.</summary>
        protected virtual void OnTransformChildrenChanged()
        {
            SetDirty();
        }

        /// <summary>
        /// Callback sent from native code whenever the RectTransform dimensions of a direct child are changed.
        /// </summary>
        protected virtual void OnChildRectTransformDimensionsChange()
        {
            if (!CanvasUpdateRegistry.IsRebuildingLayout())
                SetDirty();
        }

        /// <summary>
        /// Helper method used to set a given property if it has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property being set.</typeparam>
        /// <param name="currentValue">A reference to the member value.</param>
        /// <param name="newValue">The new value.</param>
        protected void SetProperty<T>(ref T currentValue, T newValue)
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return;
            currentValue = newValue;
            SetDirty();
        }

        /// <summary>
        /// Mark the LayoutGroup as dirty.
        /// </summary>
        protected void SetDirty()
        {
            if (!IsActive())
                return;

            if (!CanvasUpdateRegistry.IsRebuildingLayout())
                LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            else
                StartCoroutine(DelayedSetDirty(rectTransform));
        }

        IEnumerator DelayedSetDirty(RectTransform rectTransform)
        {
            yield return null;
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
