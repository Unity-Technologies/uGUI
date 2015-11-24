namespace UnityEngine.UI
{
    [AddComponentMenu("Layout/Vertical Layout Group", 151)]
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
