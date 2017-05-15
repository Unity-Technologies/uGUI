using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public abstract class LayoutGroup : UIBehaviour, ILayoutElement, ILayoutGroup
    {
        [SerializeField] protected RectOffset m_Padding = new RectOffset();
        public RectOffset padding { get { return m_Padding; } set { SetProperty(ref m_Padding, value); } }

        [FormerlySerializedAs("m_Alignment")]
        [SerializeField] protected TextAnchor m_ChildAlignment = TextAnchor.UpperLeft;
        public TextAnchor childAlignment { get { return m_ChildAlignment; } set { SetProperty(ref m_ChildAlignment, value); } }

        [System.NonSerialized] private RectTransform m_Rect;
        protected RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        protected DrivenRectTransformTracker m_Tracker;
        private Vector2 m_TotalMinSize = Vector2.zero;
        private Vector2 m_TotalPreferredSize = Vector2.zero;
        private Vector2 m_TotalFlexibleSize = Vector2.zero;

        [System.NonSerialized] private List<RectTransform> m_RectChildren = new List<RectTransform>();
        protected List<RectTransform> rectChildren { get { return m_RectChildren; } }

        // ILayoutElement Interface
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

        public abstract void CalculateLayoutInputVertical();
        public virtual float minWidth { get { return GetTotalMinSize(0); } }
        public virtual float preferredWidth { get { return GetTotalPreferredSize(0); } }
        public virtual float flexibleWidth { get { return GetTotalFlexibleSize(0); } }
        public virtual float minHeight { get { return GetTotalMinSize(1); } }
        public virtual float preferredHeight { get { return GetTotalPreferredSize(1); } }
        public virtual float flexibleHeight { get { return GetTotalFlexibleSize(1); } }
        public virtual int layoutPriority { get { return 0; } }

        // ILayoutController Interface

        public abstract void SetLayoutHorizontal();
        public abstract void SetLayoutVertical();

        // Implementation

        protected LayoutGroup()
        {
            if (m_Padding == null)
                m_Padding = new RectOffset();
        }

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

        protected override void OnDidApplyAnimationProperties()
        {
            SetDirty();
        }

        protected float GetTotalMinSize(int axis)
        {
            return m_TotalMinSize[axis];
        }

        protected float GetTotalPreferredSize(int axis)
        {
            return m_TotalPreferredSize[axis];
        }

        protected float GetTotalFlexibleSize(int axis)
        {
            return m_TotalFlexibleSize[axis];
        }

        protected float GetStartOffset(int axis, float requiredSpaceWithoutPadding)
        {
            float requiredSpace = requiredSpaceWithoutPadding + (axis == 0 ? padding.horizontal : padding.vertical);
            float availableSpace = rectTransform.rect.size[axis];
            float surplusSpace = availableSpace - requiredSpace;
            float alignmentOnAxis = GetAlignmentOnAxis(axis);
            return (axis == 0 ? padding.left : padding.top) + surplusSpace * alignmentOnAxis;
        }

        protected float GetAlignmentOnAxis(int axis)
        {
            if (axis == 0)
                return ((int)childAlignment % 3) * 0.5f;
            else
                return ((int)childAlignment / 3) * 0.5f;
        }

        protected void SetLayoutInputForAxis(float totalMin, float totalPreferred, float totalFlexible, int axis)
        {
            m_TotalMinSize[axis] = totalMin;
            m_TotalPreferredSize[axis] = totalPreferred;
            m_TotalFlexibleSize[axis] = totalFlexible;
        }

        protected void SetChildAlongAxis(RectTransform rect, int axis, float pos)
        {
            if (rect == null)
                return;

            m_Tracker.Add(this, rect,
                DrivenTransformProperties.Anchors |
                (axis == 0 ? DrivenTransformProperties.AnchoredPositionX : DrivenTransformProperties.AnchoredPositionY));

            rect.SetInsetAndSizeFromParentEdge(axis == 0 ? RectTransform.Edge.Left : RectTransform.Edge.Top, pos, rect.sizeDelta[axis]);
        }

        protected void SetChildAlongAxis(RectTransform rect, int axis, float pos, float size)
        {
            if (rect == null)
                return;

            m_Tracker.Add(this, rect,
                DrivenTransformProperties.Anchors |
                (axis == 0 ?
                 (DrivenTransformProperties.AnchoredPositionX | DrivenTransformProperties.SizeDeltaX) :
                 (DrivenTransformProperties.AnchoredPositionY | DrivenTransformProperties.SizeDeltaY)
                ));

            rect.SetInsetAndSizeFromParentEdge(axis == 0 ? RectTransform.Edge.Left : RectTransform.Edge.Top, pos, size);
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

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            if (isRootLayoutGroup)
                SetDirty();
        }

        protected virtual void OnTransformChildrenChanged()
        {
            SetDirty();
        }

        protected void SetProperty<T>(ref T currentValue, T newValue)
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return;
            currentValue = newValue;
            SetDirty();
        }

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
