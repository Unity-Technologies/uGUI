namespace UnityEngine.UI
{
    /// <summary>
    /// Arranges child elements side by side horizontally.
    /// </summary>
    [AddComponentMenu("Layout/Horizontal Layout Group", 150)]
    [UGUIHelpURL("HorizontalLayoutGroup")]
    public class HorizontalLayoutGroup : HorizontalOrVerticalLayoutGroup
    {
        /// <summary>Protected default constructor. Use <see cref="GameObject.AddComponent{T}"/> to add a HorizontalLayoutGroup to a GameObject.</summary>
        protected HorizontalLayoutGroup()
        {}

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalcAlongAxis(0, false);
        }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void CalculateLayoutInputVertical()
        {
            CalcAlongAxis(1, false);
        }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void SetLayoutHorizontal()
        {
            SetChildrenAlongAxis(0, false);
        }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void SetLayoutVertical()
        {
            SetChildrenAlongAxis(1, false);
        }
    }
}
