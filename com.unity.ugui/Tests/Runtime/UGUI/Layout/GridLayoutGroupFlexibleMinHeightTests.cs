using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace LayoutTests
{
    // Flexible grids place as many cells per row as the current width fits, so the rows the
    // children need at that width are the grid's minimum height, and an empty grid needs none.
    [UnityPlatform()]
    [Category("RegressionTest")]
    [Description("CoveredBugID = UUM-158391 GridLayoutGroup reports a minimum height of one row when the Flexible constraint is used")]
    internal class GridLayoutGroupFlexibleMinHeightTests
    {
        // 100 x 100 cells in a 250 wide grid fit two per row, so six children need three rows.
        const float k_CellSize = 100;
        const float k_GridWidth = 250;
        const float k_GridHeight = 50;
        const float k_ThreeRowsHeight = 300;
        const float k_Padding = 10;
        const float k_Spacing = 10;

        Canvas m_Canvas;

        [SetUp]
        public void TestSetup()
        {
            m_Canvas = new GameObject("Canvas").AddComponent<Canvas>();
            m_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            m_Canvas.referencePixelsPerUnit = 100;
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(m_Canvas.gameObject);
        }

        // The minimum height covers every row the children need at the current width, not one row.
        [Test]
        public void FlexibleGridWithChildrenReportsAllRowsAsMinimumHeight()
        {
            var grid = CreateFlexibleGrid(6);
            var rect = grid.transform as RectTransform;

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

            Assert.AreEqual(k_ThreeRowsHeight, grid.minHeight, "Expected the grid's min height to cover all three rows");
            Assert.AreEqual(k_ThreeRowsHeight, grid.preferredHeight, "Expected the grid's preferred height to cover all three rows");
            Assert.AreEqual(k_ThreeRowsHeight, LayoutUtility.GetMinHeight(rect), "Expected the queried min height to cover all three rows");
            Assert.AreEqual(k_ThreeRowsHeight, LayoutUtility.GetPreferredHeight(rect), "Expected the queried preferred height to cover all three rows");
        }

        // A ContentSizeFitter in min size mode keeps the grid tall enough for every row.
        [Test]
        public void ContentSizeFitterMinimumSizeKeepsAllRowsOfFlexibleGrid()
        {
            var grid = CreateFlexibleGrid(6);
            var rect = grid.transform as RectTransform;
            rect.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.MinSize;

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

            Assert.AreEqual(k_ThreeRowsHeight, rect.rect.height, "Expected the fitted grid to stay tall enough for all three rows");
        }

        // An empty flexible grid has no rows to reserve on either input.
        [Test]
        public void EmptyFlexibleGridReportsZeroMinAndPreferredHeight()
        {
            var grid = CreateFlexibleGrid(0);
            var rect = grid.transform as RectTransform;

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

            Assert.AreEqual(0, grid.minHeight, "Expected the empty grid to report no minimum height");
            Assert.AreEqual(0, grid.preferredHeight, "Expected the empty grid to report no preferred height");
            Assert.AreEqual(0, LayoutUtility.GetMinHeight(rect), "Expected the queried min height of the empty grid to be zero");
            Assert.AreEqual(0, LayoutUtility.GetPreferredHeight(rect), "Expected the queried preferred height of the empty grid to be zero");
        }

        // With no rows an empty grid still reports its full vertical padding, not padding minus a trailing row spacing.
        [Test]
        public void EmptyFlexibleGridWithPaddingAndSpacingKeepsItsPaddingUnderMinimumSizeFit()
        {
            var grid = CreateFlexibleGrid(0);
            grid.padding = new RectOffset((int)k_Padding, (int)k_Padding, (int)k_Padding, (int)k_Padding);
            grid.spacing = new Vector2(k_Spacing, k_Spacing);
            var rect = grid.transform as RectTransform;
            rect.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.MinSize;

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

            Assert.AreEqual(k_Padding * 2, grid.minHeight, "Expected the empty grid's min height to be its full vertical padding");
            Assert.AreEqual(k_Padding * 2, rect.rect.height, "Expected the fitted empty grid to keep its full vertical padding");
        }

        // A ContentSizeFitter in preferred size mode lets an empty grid collapse to nothing.
        [Test]
        public void ContentSizeFitterPreferredSizeCollapsesEmptyFlexibleGrid()
        {
            var grid = CreateFlexibleGrid(0);
            var rect = grid.transform as RectTransform;
            rect.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

            Assert.AreEqual(0, rect.rect.height, "Expected the empty fitted grid to collapse to zero height");
        }

        GridLayoutGroup CreateFlexibleGrid(int childCount)
        {
            var rectTransform = new GameObject("Grid").AddComponent<RectTransform>();
            rectTransform.SetParent(m_Canvas.transform, false);
            rectTransform.sizeDelta = new Vector2(k_GridWidth, k_GridHeight);

            var grid = rectTransform.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(k_CellSize, k_CellSize);
            grid.spacing = Vector2.zero;
            grid.constraint = GridLayoutGroup.Constraint.Flexible;

            for (int i = 0; i < childCount; i++)
                new GameObject("Cell" + i).AddComponent<RectTransform>().SetParent(rectTransform, false);

            return grid;
        }
    }
}
