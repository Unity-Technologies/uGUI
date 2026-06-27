namespace UnityEngine.UI
{
    /// <summary>
    /// Arranges child layout elements vertically.
    /// </summary>
    [AddComponentMenu("Layout/Vertical Layout Group", 151)]
    [UGUIHelpURL("VerticalLayoutGroup")]
    public class VerticalLayoutGroup : HorizontalOrVerticalLayoutGroup
    {
        /// <summary>Protected default constructor. Use <see cref="GameObject.AddComponent{T}"/> to add a VerticalLayoutGroup to a GameObject.</summary>
        protected VerticalLayoutGroup()
        {}

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalcAlongAxis(0, true);
        }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void CalculateLayoutInputVertical()
        {
            CalcAlongAxis(1, true);
        }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void SetLayoutHorizontal()
        {
            SetChildrenAlongAxis(0, true);
        }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void SetLayoutVertical()
        {
            SetChildrenAlongAxis(1, true);
        }
    }
}
