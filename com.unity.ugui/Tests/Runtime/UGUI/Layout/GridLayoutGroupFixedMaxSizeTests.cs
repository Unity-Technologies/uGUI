using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace LayoutTests
{
    // Test set covering the maximum size of a grid with a fixed column or row count. The grid never
    // stretches its cells, so it should not advertise an upper bound that overrides a preferred size
    // supplied by another layout element on the same GameObject or caps the groups that contain it.
    [UnityPlatform()]
    [Category("RegressionTest")]
    [Description("CoveredBugID = UUM-158101 LayoutUtility.GetPreferredWidth() is capped to the grid's width when GridLayoutGroup uses Fixed Column Count or Fixed Row Count")]
    internal class GridLayoutGroupFixedMaxSizeTests
    {
        // Two 100 x 100 cells laid out in one row, so each fixed grid below measures 200 x 100.
        const float k_GridWidth = 200;
        const float k_GridHeight = 100;
        const float k_OverridingSize = 300;

        Canvas m_Canvas;
        Texture2D m_Texture;
        Sprite m_Sprite;

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
            if (m_Sprite != null)
                UnityEngine.Object.DestroyImmediate(m_Sprite);
            if (m_Texture != null)
                UnityEngine.Object.DestroyImmediate(m_Texture);
        }

        // A fixed grid's size is a preferred size, not a ceiling, on both axes.
        [TestCase(GridLayoutGroup.Constraint.FixedColumnCount)]
        [TestCase(GridLayoutGroup.Constraint.FixedRowCount)]
        public void FixedGridReportsItsSizeAsPreferredButNoMaximum(GridLayoutGroup.Constraint constraint)
        {
            var grid = CreateFixedGrid(m_Canvas.transform, constraint);

            LayoutRebuilder.ForceRebuildLayoutImmediate(grid.transform as RectTransform);

            Assert.AreEqual(k_GridWidth, grid.minWidth, "Expected the grid's min width to stay at its cell width");
            Assert.AreEqual(k_GridWidth, grid.preferredWidth, "Expected the grid's preferred width to stay at its cell width");
            Assert.AreEqual(LayoutUtility.DefaultMaxSize, grid.maxWidth, "Expected the grid's max width to be unbounded");
            Assert.AreEqual(k_GridHeight, grid.minHeight, "Expected the grid's min height to stay at its cell height");
            Assert.AreEqual(k_GridHeight, grid.preferredHeight, "Expected the grid's preferred height to stay at its cell height");
            Assert.AreEqual(LayoutUtility.DefaultMaxSize, grid.maxHeight, "Expected the grid's max height to be unbounded");
        }

        // A higher-priority LayoutElement asking for more than the grid's own size gets it on both axes.
        [TestCase(GridLayoutGroup.Constraint.FixedColumnCount)]
        [TestCase(GridLayoutGroup.Constraint.FixedRowCount)]
        public void FixedGridDoesNotCapLayoutElementPreferredSize(GridLayoutGroup.Constraint constraint)
        {
            var grid = CreateFixedGrid(m_Canvas.transform, constraint);
            var element = grid.gameObject.AddComponent<LayoutElement>();
            element.preferredWidth = k_OverridingSize;
            element.preferredHeight = k_OverridingSize;

            LayoutRebuilder.ForceRebuildLayoutImmediate(grid.transform as RectTransform);

            var rect = grid.transform as RectTransform;
            Assert.AreEqual(k_OverridingSize, LayoutUtility.GetPreferredWidth(rect), "Expected the LayoutElement's preferred width to win");
            Assert.AreEqual(k_OverridingSize, LayoutUtility.GetPreferredHeight(rect), "Expected the LayoutElement's preferred height to win");
        }

        // At equal priority the larger preferred size wins, so an Image bigger than the grid is not cut down.
        [TestCase(GridLayoutGroup.Constraint.FixedColumnCount)]
        [TestCase(GridLayoutGroup.Constraint.FixedRowCount)]
        public void FixedGridDoesNotCapImagePreferredSize(GridLayoutGroup.Constraint constraint)
        {
            var grid = CreateFixedGrid(m_Canvas.transform, constraint);
            var image = grid.gameObject.AddComponent<Image>();
            image.sprite = CreateSprite(512);

            LayoutRebuilder.ForceRebuildLayoutImmediate(grid.transform as RectTransform);

            Assert.AreEqual(512, LayoutUtility.GetPreferredWidth(grid.transform as RectTransform), "Expected the Image's preferred width to win");
        }

        // A ContentSizeFitter sizes the grid to the overriding preferred size, not the grid's own size.
        [TestCase(GridLayoutGroup.Constraint.FixedColumnCount)]
        [TestCase(GridLayoutGroup.Constraint.FixedRowCount)]
        public void ContentSizeFitterSizesFixedGridToLayoutElementPreferredWidth(GridLayoutGroup.Constraint constraint)
        {
            var grid = CreateFixedGrid(m_Canvas.transform, constraint);
            grid.gameObject.AddComponent<LayoutElement>().preferredWidth = k_OverridingSize;
            grid.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            LayoutRebuilder.ForceRebuildLayoutImmediate(grid.transform as RectTransform);

            Assert.AreEqual(k_OverridingSize, (grid.transform as RectTransform).rect.width);
        }

        // A group containing a fixed grid is not capped by the grid's own size along either of its axes.
        [TestCase(GridLayoutGroup.Constraint.FixedColumnCount)]
        [TestCase(GridLayoutGroup.Constraint.FixedRowCount)]
        public void FixedGridDoesNotCapParentGroupPreferredWidth(GridLayoutGroup.Constraint constraint)
        {
            var horizontalParent = CreateGroup<HorizontalLayoutGroup>(m_Canvas.transform);
            var horizontalChildGrid = CreateFixedGrid(horizontalParent.transform, constraint);
            horizontalChildGrid.gameObject.AddComponent<LayoutElement>().preferredWidth = k_OverridingSize;

            var verticalParent = CreateGroup<VerticalLayoutGroup>(m_Canvas.transform);
            var verticalChildGrid = CreateFixedGrid(verticalParent.transform, constraint);
            verticalChildGrid.gameObject.AddComponent<LayoutElement>().preferredWidth = k_OverridingSize;

            LayoutRebuilder.ForceRebuildLayoutImmediate(horizontalParent.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(verticalParent.transform as RectTransform);

            Assert.AreEqual(k_OverridingSize, horizontalParent.preferredWidth, "Expected the grid to not cap a parent along its layout axis");
            Assert.AreEqual(LayoutUtility.DefaultMaxSize, horizontalParent.maxWidth, "Expected the grid to not give a parent a max width along its layout axis");
            Assert.AreEqual(k_OverridingSize, verticalParent.preferredWidth, "Expected the grid to not cap a parent across its layout axis");
            Assert.AreEqual(LayoutUtility.DefaultMaxSize, verticalParent.maxWidth, "Expected the grid to not give a parent a max width across its layout axis");
        }

        // Constraint count 2 fills one row of two cells for FixedColumnCount, and 1 does the same for FixedRowCount.
        static GridLayoutGroup CreateFixedGrid(Transform parent, GridLayoutGroup.Constraint constraint)
        {
            var rectTransform = new GameObject("Grid").AddComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            var grid = rectTransform.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(100, 100);
            grid.spacing = Vector2.zero;
            grid.constraint = constraint;
            grid.constraintCount = constraint == GridLayoutGroup.Constraint.FixedColumnCount ? 2 : 1;

            for (int i = 0; i < 2; i++)
                new GameObject("Cell" + i).AddComponent<RectTransform>().SetParent(rectTransform, false);

            return grid;
        }

        // AddComponent runs LayoutGroup.Reset, which turns childControl* off, so the sizing
        // flags are set explicitly here rather than relying on the serialized defaults.
        static T CreateGroup<T>(Transform parent) where T : HorizontalOrVerticalLayoutGroup
        {
            var rectTransform = new GameObject(typeof(T).Name).AddComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            var group = rectTransform.gameObject.AddComponent<T>();
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = false;
            return group;
        }

        Sprite CreateSprite(int size)
        {
            m_Texture = new Texture2D(size, size);
            m_Sprite = Sprite.Create(m_Texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
            return m_Sprite;
        }
    }
}
