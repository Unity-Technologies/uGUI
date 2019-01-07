namespace UnityEngine.UI
{
    [AddComponentMenu("Layout/Vertical Layout Group", 151)]
    /// <summary>
    /// Layout child layout elements below each other.
    /// </summary>
    /// <remarks>
    /// The VerticalLayoutGroup component is used to layout child layout elements below each other.
    /// </remarks>
    public class VerticalLayoutGroup : HorizontalOrVerticalLayoutGroup
    {
        protected VerticalLayoutGroup()
        {}

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalcAlongAxis(0, true);
        }

        public override void CalculateLayoutInputVertical()
        {
            CalcAlongAxis(1, true);
        }

        public override void SetLayoutHorizontal()
        {
            SetChildrenAlongAxis(0, true);
        }

        public override void SetLayoutVertical()
        {
            SetChildrenAlongAxis(1, true);
        }
    }
}
