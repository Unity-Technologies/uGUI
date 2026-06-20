using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    [AddComponentMenu("Layout/Grid Layout Group", 152)]
    [UGUIHelpURL("GridLayoutGroup")]
    /// <summary>
    /// Layout class to arrange child elements in a grid format.
    /// </summary>
    /// <remarks>
    /// The GridLayoutGroup component is used to layout child layout elements in a uniform grid where all cells have the same size. The size and the spacing between cells is controlled by the GridLayoutGroup itself. The children have no influence on their sizes.
    /// </remarks>
    public class GridLayoutGroup : LayoutGroup
    {
        /// <summary>
        /// Which corner is the starting corner for the grid.
        /// </summary>
        public enum Corner
        {
            /// <summary>
            /// Upper Left corner.
            /// </summary>
            UpperLeft = 0,
            /// <summary>
            /// Upper Right corner.
            /// </summary>
            UpperRight = 1,
            /// <summary>
            /// Lower Left corner.
            /// </summary>
            LowerLeft = 2,
            /// <summary>
            /// Lower Right corner.
            /// </summary>
            LowerRight = 3
        }

        /// <summary>
        /// The grid axis we are looking at.
        /// </summary>
        /// <remarks>
        /// As the storage is a [][] we make access easier by passing a axis.
        /// </remarks>
        public enum Axis
        {
            /// <summary>
            /// Horizontal axis
            /// </summary>
            Horizontal = 0,
            /// <summary>
            /// Vertical axis.
            /// </summary>
            Vertical = 1
        }

        /// <summary>
        /// Constraint type on either the number of columns or rows.
        /// </summary>
        public enum Constraint
        {
            /// <summary>
            /// Don't constrain the number of rows or columns.
            /// </summary>
            Flexible = 0,
            /// <summary>
            /// Constrain the number of columns to a specified number.
            /// </summary>
            FixedColumnCount = 1,
            /// <summary>
            /// Constraint the number of rows to a specified number.
            /// </summary>
            FixedRowCount = 2
        }

        [SerializeField] protected Corner m_StartCorner = Corner.UpperLeft;

        /// <summary>
        /// Which corner should the first cell be placed in?
        /// </summary>
        public Corner startCorner { get { return m_StartCorner; } set { SetProperty(ref m_StartCorner, value); } }

        [SerializeField] protected Axis m_StartAxis = Axis.Horizontal;

        /// <summary>
        /// Which axis should cells be placed along first
        /// </summary>
        /// <remarks>
        /// When startAxis is set to horizontal, an entire row will be filled out before proceeding to the next row. When set to vertical, an entire column will be filled out before proceeding to the next column.
        /// </remarks>
        public Axis startAxis { get { return m_StartAxis; } set { SetProperty(ref m_StartAxis, value); } }

        [SerializeField] protected Vector2 m_CellSize = new Vector2(100, 100);

        /// <summary>
        /// The size to use for each cell in the grid.
        /// </summary>
        public Vector2 cellSize { get { return m_CellSize; } set { SetProperty(ref m_CellSize, value); } }

        [SerializeField] protected Vector2 m_Spacing = Vector2.zero;

        /// <summary>
        /// The spacing to use between layout elements in the grid on both axises.
        /// </summary>
        public Vector2 spacing { get { return m_Spacing; } set { SetProperty(ref m_Spacing, value); } }

        [SerializeField] protected Constraint m_Constraint = Constraint.Flexible;

        /// <summary>
        /// Which constraint to use for the GridLayoutGroup.
        /// </summary>
        /// <remarks>
        /// Specifying a constraint can make the GridLayoutGroup work better in conjunction with a [[ContentSizeFitter]] component. When GridLayoutGroup is used on a RectTransform with a manually specified size, there's no need to specify a constraint.
        /// </remarks>
        public Constraint constraint { get { return m_Constraint; } set { SetProperty(ref m_Constraint, value); } }

        [SerializeField] protected int m_ConstraintCount = 2;

        /// <summary>
        /// How many cells there should be along the constrained axis.
        /// </summary>
        public int constraintCount { get { return m_ConstraintCount; } set { SetProperty(ref m_ConstraintCount, Mathf.Max(1, value)); } }

        /// <summary>
        /// The number of rows that the layout group generates after the layout process is complete.
        /// </summary>
        /// <remarks>
        /// The layout system sets this value to `0` if there are no child GameObjects participating in layout.
        /// When you set <see cref="constraint"/> to <see cref="Constraint.FixedRowCount"/>, this value is equal
        /// to the minimum of <see cref="constraintCount"/> and the number of child GameObjects.
        /// </remarks>
        public int generatedRowCount { get; private set; }

        /// <summary>
        /// The number of columns that the layout group generates after the layout process is complete.
        /// </summary>
        /// <remarks>
        /// The layout system sets this value to `0` if there are no child GameObjects participating in layout.
        /// When you set <see cref="constraint"/> to <see cref="Constraint.FixedColumnCount"/>, this value is equal
        /// to the minimum of <see cref="constraintCount"/> and the number of child GameObjects.
        /// </remarks>
        public int generatedColumnCount { get; private set; }

        protected GridLayoutGroup()
        {}

        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            constraintCount = constraintCount;
        }

        #endif

        /// <summary>
        /// Called by the layout system to calculate the horizontal layout size.
        /// Also see ILayoutElement
        /// </summary>
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            float totalMin, totalMax, totalPreferred = 0;
            float cellWidthWithSpacing = cellSize.x + spacing.x;

            // If constraint type is fixed, we can calculate total min, max, and preferred to be the same desired value.
            if (m_Constraint == Constraint.FixedColumnCount)
            {
                // ConstraintCount is the number of columns, we can use the number of columns to calculate the width
                totalMin = totalMax = totalPreferred = padding.horizontal + cellWidthWithSpacing * m_ConstraintCount - spacing.x;
            }
            else if (m_Constraint == Constraint.FixedRowCount)
            {
                // We can calculate the number of columns based on the constraint count (rows).
                // Then we use the number of columns to calculate the total width of the layout group.
                int columns = Mathf.CeilToInt(rectChildren.Count / (float)m_ConstraintCount - 0.001f);
                totalMin = totalMax = totalPreferred = padding.horizontal + cellWidthWithSpacing * columns - spacing.x;
            }
            else
            {
                // Flexible mode...
                totalMax = LayoutUtility.DefaultMaxSize;
                totalMin = padding.horizontal + cellWidthWithSpacing - spacing.x;

                // Calculate the preferredColumnCount, we want to calculate as close to a square grid as possible.
                // To achieve this, we take the square root of the total child count and round the result.
                float squareRootOfChildren = Mathf.Sqrt(rectChildren.Count);
                int preferredColumnCount = Mathf.CeilToInt(squareRootOfChildren);

                // Calculate the total width of the cells, then subtract spacing.x to remove trailing space.
                totalPreferred = padding.horizontal + cellWidthWithSpacing * preferredColumnCount - spacing.x;
            }

            SetLayoutInputForAxis(totalMin, totalMax, totalPreferred, -1, 0);
        }

        /// <summary>
        /// Called by the layout system to calculate the vertical layout size.
        /// Also see ILayoutElement
        /// </summary>
        public override void CalculateLayoutInputVertical()
        {
            float totalMin, totalMax, totalPreferred = 0;
            float cellHeightWithSpacing = cellSize.y + spacing.y;

            // If constraint type is fixed, we can calculate total min, max, and preferred to be the same desired value.
            if (m_Constraint == Constraint.FixedColumnCount)
            {
                // We can calculate the number of rows based on the constraint count (columns).
                // Then we use the number of rows to calculate the total height of the layout group.
                var rows = Mathf.CeilToInt(rectChildren.Count / (float)m_ConstraintCount - 0.001f);
                totalMin = totalMax = totalPreferred = padding.vertical + cellHeightWithSpacing * rows - spacing.y;
            }
            else if (m_Constraint == Constraint.FixedRowCount)
            {
                // ConstraintCount is the number of rows, we can use the number of rows to calculate the height.
                totalMin = totalMax = totalPreferred = padding.vertical + cellHeightWithSpacing * m_ConstraintCount - spacing.y;
            }
            else
            {
                // Flexible mode...
                totalMax = LayoutUtility.DefaultMaxSize;
                totalMin = padding.vertical + cellHeightWithSpacing - spacing.y;

                // Find the usable width available and the horizontal size of each cell with spacing.
                float usableWidth = rectTransform.rect.width - padding.horizontal + spacing.x + 0.001f;
                float cellWidthWithSpacing = cellSize.x + spacing.x;

                // Calculate how many cells fit into a single row ensuring that least 1 cell per row
                // Then calculate the number of rows the total number of cells will fit in.
                int cellCountX = Mathf.Max(1, Mathf.FloorToInt(usableWidth / cellWidthWithSpacing));
                int rowCount = Mathf.CeilToInt(rectChildren.Count / (float)cellCountX);

                // Calculate the total height of the cells, then subtract spacing.y to remove trailing space.
                totalPreferred =  padding.vertical + cellHeightWithSpacing * rowCount - spacing.y;
            }

            SetLayoutInputForAxis(totalMin, totalMax, totalPreferred, -1, 1);
        }

        /// <summary>
        /// Called by the layout system
        /// Also see ILayoutElement
        /// </summary>
        public override void SetLayoutHorizontal()
        {
            ResetGeneratedCounts();
            SetCellsAlongAxis(0);
        }

        /// <summary>
        /// Called by the layout system
        /// Also see ILayoutElement
        /// </summary>
        public override void SetLayoutVertical()
        {
            SetCellsAlongAxis(1);
        }

        private void SetCellsAlongAxis(int axis)
        {
            // Normally a Layout Controller should only set horizontal values when invoked for the horizontal axis
            // and only vertical values when invoked for the vertical axis.
            // However, in this case we set both the horizontal and vertical position when invoked for the vertical axis.
            // Since we only set the horizontal position and not the size, it shouldn't affect children's layout,
            // and thus shouldn't break the rule that all horizontal layout must be calculated before all vertical layout.
            var rectChildrenCount = rectChildren.Count;
            if (axis == 0)
            {
                // Only set the sizes when invoked for horizontal axis, not the positions.

                for (int i = 0; i < rectChildrenCount; i++)
                {
                    RectTransform rect = rectChildren[i];

                    m_Tracker.Add(this, rect,
                        DrivenTransformProperties.Anchors |
                        DrivenTransformProperties.AnchoredPosition |
                        DrivenTransformProperties.SizeDelta);

                    rect.anchorMin = Vector2.up;
                    rect.anchorMax = Vector2.up;
                    rect.sizeDelta = cellSize;
                }
                return;
            }

            float width = rectTransform.rect.size.x;
            float height = rectTransform.rect.size.y;

            int cellCountX = 1;
            int cellCountY = 1;
            if (m_Constraint == Constraint.FixedColumnCount)
            {
                cellCountX = m_ConstraintCount;

                if (rectChildrenCount > cellCountX)
                    cellCountY = rectChildrenCount / cellCountX + (rectChildrenCount % cellCountX > 0 ? 1 : 0);
            }
            else if (m_Constraint == Constraint.FixedRowCount)
            {
                cellCountY = m_ConstraintCount;

                if (rectChildrenCount > cellCountY)
                    cellCountX = rectChildrenCount / cellCountY + (rectChildrenCount % cellCountY > 0 ? 1 : 0);
            }
            else
            {
                if (cellSize.x + spacing.x <= 0)
                    cellCountX = int.MaxValue;
                else
                    cellCountX = Mathf.Max(1, Mathf.FloorToInt((width - padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));

                if (cellSize.y + spacing.y <= 0)
                    cellCountY = int.MaxValue;
                else
                    cellCountY = Mathf.Max(1, Mathf.FloorToInt((height - padding.vertical + spacing.y + 0.001f) / (cellSize.y + spacing.y)));
            }

            int cornerX = (int)startCorner % 2;
            int cornerY = (int)startCorner / 2;

            int cellsPerMainAxis, actualCellCountX, actualCellCountY;
            if (startAxis == Axis.Horizontal)
            {
                cellsPerMainAxis = cellCountX;
                actualCellCountX = Mathf.Clamp(cellCountX, 1, rectChildrenCount);

                if (m_Constraint == Constraint.FixedRowCount)
                    actualCellCountY = Mathf.Min(cellCountY, rectChildrenCount);
                else
                    actualCellCountY = Mathf.Clamp(cellCountY, 1, Mathf.CeilToInt(rectChildrenCount / (float)cellsPerMainAxis));
            }
            else
            {
                cellsPerMainAxis = cellCountY;
                actualCellCountY = Mathf.Clamp(cellCountY, 1, rectChildrenCount);

                if (m_Constraint == Constraint.FixedColumnCount)
                    actualCellCountX = Mathf.Min(cellCountX, rectChildrenCount);
                else
                    actualCellCountX = Mathf.Clamp(cellCountX, 1, Mathf.CeilToInt(rectChildrenCount / (float)cellsPerMainAxis));
            }

            Vector2 requiredSpace = new Vector2(
                actualCellCountX * cellSize.x + (actualCellCountX - 1) * spacing.x,
                actualCellCountY * cellSize.y + (actualCellCountY - 1) * spacing.y
            );
            Vector2 startOffset = new Vector2(
                GetStartOffset(0, requiredSpace.x),
                GetStartOffset(1, requiredSpace.y)
            );

            // Fixes case 1345471 - Makes sure the constraint column / row amount is always respected
            int childrenToMove = 0;
            if (rectChildrenCount > m_ConstraintCount && Mathf.CeilToInt((float)rectChildrenCount / (float)cellsPerMainAxis) < m_ConstraintCount)
            {
                childrenToMove = m_ConstraintCount - Mathf.CeilToInt((float)rectChildrenCount / (float)cellsPerMainAxis);
                childrenToMove += Mathf.FloorToInt((float)childrenToMove / ((float)cellsPerMainAxis - 1));
                if (rectChildrenCount % cellsPerMainAxis == 1)
                    childrenToMove += 1;
            }

            for (int i = 0; i < rectChildrenCount; i++)
            {
                int positionX;
                int positionY;
                if (startAxis == Axis.Horizontal)
                {
                    if (m_Constraint == Constraint.FixedRowCount && rectChildrenCount - i <= childrenToMove)
                    {
                        positionX = 0;
                        positionY = m_ConstraintCount - (rectChildrenCount - i);
                    }
                    else
                    {
                        positionX = i % cellsPerMainAxis;
                        positionY = i / cellsPerMainAxis;
                    }
                }
                else
                {
                    if (m_Constraint == Constraint.FixedColumnCount && rectChildrenCount - i <= childrenToMove)
                    {
                        positionX = m_ConstraintCount - (rectChildrenCount - i);
                        positionY = 0;
                    }
                    else
                    {
                        positionX = i / cellsPerMainAxis;
                        positionY = i % cellsPerMainAxis;
                    }
                }

                if (cornerX == 1)
                    positionX = actualCellCountX - 1 - positionX;
                if (cornerY == 1)
                    positionY = actualCellCountY - 1 - positionY;

                SetChildAlongAxis(rectChildren[i], 0, startOffset.x + (cellSize[0] + spacing[0]) * positionX, cellSize[0]);
                SetChildAlongAxis(rectChildren[i], 1, startOffset.y + (cellSize[1] + spacing[1]) * positionY, cellSize[1]);
            }

            generatedRowCount = actualCellCountY;
            generatedColumnCount = actualCellCountX;
        }

        /// <summary>
        /// Resets <see cref="generatedRowCount"/> and <see cref="generatedColumnCount"/> to default values.
        /// </summary>
        /// <exclude />
        private void ResetGeneratedCounts()
        {
            generatedRowCount = 0;
            generatedColumnCount = 0;
        }
    }
}
