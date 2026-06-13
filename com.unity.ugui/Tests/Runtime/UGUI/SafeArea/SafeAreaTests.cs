using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace SafeAreaTests
{
    internal class SafeAreaCalculationBlackBoxTests
    {
        private const float Epsilon = 0.0001f;

        private const int ScreenWidth = 1000;
        private const int ScreenHeight = 2000;

        private static void AssertAnchors(
            (Vector2 min, Vector2 max) actual,
            float minX,
            float minY,
            float maxX,
            float maxY)
        {
            Assert.That(actual.min.x, Is.EqualTo(minX).Within(Epsilon));
            Assert.That(actual.min.y, Is.EqualTo(minY).Within(Epsilon));
            Assert.That(actual.max.x, Is.EqualTo(maxX).Within(Epsilon));
            Assert.That(actual.max.y, Is.EqualTo(maxY).Within(Epsilon));
        }

        private static Rect MakeSafeArea(float leftInset, float bottomInset, float rightInset, float topInset)
        {
            return new Rect(
                leftInset,
                bottomInset,
                ScreenWidth - leftInset - rightInset,
                ScreenHeight - bottomInset - topInset);
        }

        [TestCase(0, 0, false)]
        [TestCase(0, SafeArea.AlignmentMode.CenterHorizontally, false)]
        [TestCase(0, SafeArea.AlignmentMode.CenterVertically, false)]
        [TestCase(0, SafeArea.AlignmentMode.CenterHorizontally | SafeArea.AlignmentMode.CenterVertically, false)]
        [TestCase(0, 0, true)]
        [TestCase(0, SafeArea.AlignmentMode.CenterHorizontally, true)]
        [TestCase(0, SafeArea.AlignmentMode.CenterVertically, true)]
        [TestCase(0, SafeArea.AlignmentMode.CenterHorizontally | SafeArea.AlignmentMode.CenterVertically, true)]
        public void CalculateAnchors_WhenNoEdgesRespected_ReturnsFullScreen(
            SafeArea.SafeAreaMode safeAreaFlags,
            SafeArea.AlignmentMode alignmentFlags,
            bool flipped)
        {
            var safeArea = MakeSafeArea(100, 200, 150, 300);

            var actual = SafeArea.CalculateAnchors(
                ScreenWidth,
                ScreenHeight,
                safeArea,
                safeAreaFlags,
                alignmentFlags,
                flipped);

            AssertAnchors(actual, 0f, 0f, 1f, 1f);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CalculateAnchors_WhenSafeAreaIsFullScreen_ReturnsFullScreen(bool flipped)
        {
            var safeArea = new Rect(0, 0, ScreenWidth, ScreenHeight);

            var actual = SafeArea.CalculateAnchors(
                ScreenWidth,
                ScreenHeight,
                safeArea,
                SafeArea.SafeAreaMode.Left | SafeArea.SafeAreaMode.Right | SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Bottom,
                SafeArea.AlignmentMode.CenterHorizontally | SafeArea.AlignmentMode.CenterVertically,
                flipped);

            AssertAnchors(actual, 0f, 0f, 1f, 1f);
        }

        [Test]
        public void CalculateAnchors_WhenAllEdgesRespectedAndNoAlignment_ReturnsNormalizedSafeArea()
        {
            var safeArea = MakeSafeArea(100, 200, 150, 300);

            var actual = SafeArea.CalculateAnchors(
                ScreenWidth,
                ScreenHeight,
                safeArea,
                SafeArea.SafeAreaMode.Left | SafeArea.SafeAreaMode.Right | SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Bottom,
                0,
                false);

            AssertAnchors(actual, 0.1f, 0.1f, 0.85f, 0.85f);
        }

        [Test]
        public void CalculateAnchors_WhenOnlyLeftRespected_OnlyLeftAnchorIsInset()
        {
            var safeArea = MakeSafeArea(100, 200, 150, 300);

            var actual = SafeArea.CalculateAnchors(
                ScreenWidth,
                ScreenHeight,
                safeArea,
                SafeArea.SafeAreaMode.Left,
                0,
                false);

            AssertAnchors(actual, 0.1f, 0f, 1f, 1f);
        }

        [Test]
        public void CalculateAnchors_WhenOnlyRightRespected_OnlyRightAnchorIsInset()
        {
            var safeArea = MakeSafeArea(100, 200, 150, 300);

            var actual = SafeArea.CalculateAnchors(
                ScreenWidth,
                ScreenHeight,
                safeArea,
                SafeArea.SafeAreaMode.Right,
                0,
                false);

            AssertAnchors(actual, 0f, 0f, 0.85f, 1f);
        }

        [Test]
        public void CalculateAnchors_WhenOnlyBottomRespected_OnlyBottomAnchorIsInset()
        {
            var safeArea = MakeSafeArea(100, 200, 150, 300);

            var actual = SafeArea.CalculateAnchors(
                ScreenWidth,
                ScreenHeight,
                safeArea,
                SafeArea.SafeAreaMode.Bottom,
                0,
                false);

            AssertAnchors(actual, 0f, 0.1f, 1f, 1f);
        }

        [Test]
        public void CalculateAnchors_WhenOnlyTopRespected_OnlyTopAnchorIsInset()
        {
            var safeArea = MakeSafeArea(100, 200, 150, 300);

            var actual = SafeArea.CalculateAnchors(
                ScreenWidth,
                ScreenHeight,
                safeArea,
                SafeArea.SafeAreaMode.Top,
                0,
                false);

            AssertAnchors(actual, 0f, 0f, 1f, 0.85f);
        }

        [Test]
        public void CalculateAnchors_WhenLeftAndRightRespected_HorizontalAnchorsAreInset()
        {
            var safeArea = MakeSafeArea(100, 200, 150, 300);

            var actual = SafeArea.CalculateAnchors(
                ScreenWidth,
                ScreenHeight,
                safeArea,
                SafeArea.SafeAreaMode.Left | SafeArea.SafeAreaMode.Right,
                0,
                false);

            AssertAnchors(actual, 0.1f, 0f, 0.85f, 1f);
        }

        [Test]
        public void CalculateAnchors_WhenTopAndBottomRespected_VerticalAnchorsAreInset()
        {
            var safeArea = MakeSafeArea(100, 200, 150, 300);

            var actual = SafeArea.CalculateAnchors(
                ScreenWidth,
                ScreenHeight,
                safeArea,
                SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Bottom,
                0,
                false);

            AssertAnchors(actual, 0f, 0.1f, 1f, 0.85f);
        }

        [Test]
        public void CalculateAnchors_WhenAlignedHorizontally_UsesLargerHorizontalInset()
        {
            var safeArea = MakeSafeArea(100, 200, 150, 300);

            var actual = SafeArea.CalculateAnchors(
                ScreenWidth,
                ScreenHeight,
                safeArea,
                SafeArea.SafeAreaMode.Left | SafeArea.SafeAreaMode.Right,
                SafeArea.AlignmentMode.CenterHorizontally,
                false);

            AssertAnchors(actual, 0.15f, 0f, 0.85f, 1f);
        }

        [Test]
        public void CalculateAnchors_WhenAlignedVertically_UsesLargerVerticalInset()
        {
            var safeArea = MakeSafeArea(100, 200, 150, 300);

            var actual = SafeArea.CalculateAnchors(
                ScreenWidth,
                ScreenHeight,
                safeArea,
                SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Bottom,
                SafeArea.AlignmentMode.CenterVertically,
                false);

            AssertAnchors(actual, 0f, 0.15f, 1f, 0.85f);
        }

        [Test]
        public void CalculateAnchors_WhenAlignedBothAxes_UsesLargestInsetOnEachAxis()
        {
            var safeArea = MakeSafeArea(100, 200, 150, 300);

            var actual = SafeArea.CalculateAnchors(
                ScreenWidth,
                ScreenHeight,
                safeArea,
                SafeArea.SafeAreaMode.Left | SafeArea.SafeAreaMode.Right | SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Bottom,
                SafeArea.AlignmentMode.CenterHorizontally | SafeArea.AlignmentMode.CenterVertically,
                false);

            AssertAnchors(actual, 0.15f, 0.15f, 0.85f, 0.85f);
        }

        [Test]
        public void CalculateAnchors_WhenHorizontallyAlignedAndFlipped_AlignmentAppliesVertically()
        {
            var safeArea = MakeSafeArea(100, 200, 150, 300);

            var actual = SafeArea.CalculateAnchors(
                ScreenWidth,
                ScreenHeight,
                safeArea,
                SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Bottom,
                SafeArea.AlignmentMode.CenterHorizontally,
                true);

            AssertAnchors(actual, 0f, 0.15f, 1f, 0.85f);
        }

        [Test]
        public void CalculateAnchors_WhenVerticallyAlignedAndFlipped_AlignmentAppliesHorizontally()
        {
            var safeArea = MakeSafeArea(100, 200, 150, 300);

            var actual = SafeArea.CalculateAnchors(
                ScreenWidth,
                ScreenHeight,
                safeArea,
                SafeArea.SafeAreaMode.Left | SafeArea.SafeAreaMode.Right,
                SafeArea.AlignmentMode.CenterVertically,
                true);

            AssertAnchors(actual, 0.15f, 0f, 0.85f, 1f);
        }

        [TestCase(0, 0, false)]
        [TestCase(1, 0, false)]
        [TestCase(2, 0, false)]
        [TestCase(4, 0, false)]
        [TestCase(8, 0, false)]
        [TestCase(3, 0, false)]
        [TestCase(12, 0, false)]
        [TestCase(15, 0, false)]
        [TestCase(15, 1, false)]
        [TestCase(15, 2, false)]
        [TestCase(15, 3, false)]
        [TestCase(15, 1, true)]
        [TestCase(15, 2, true)]
        [TestCase(15, 3, true)]
        [TestCase(5, 1, false)]
        [TestCase(10, 2, false)]
        [TestCase(5, 1, true)]
        [TestCase(10, 2, true)]
        public void CalculateAnchors_AlwaysReturnsNormalizedOrderedAnchors(
            int safeAreaFlags,
            int alignmentFlags,
            bool flipped)
        {
            var safeArea = MakeSafeArea(100, 200, 150, 300);

            var actual = SafeArea.CalculateAnchors(
                ScreenWidth,
                ScreenHeight,
                safeArea,
                (SafeArea.SafeAreaMode)safeAreaFlags,
                (SafeArea.AlignmentMode)alignmentFlags,
                flipped);

            Assert.That(actual.min.x, Is.InRange(0f, 1f));
            Assert.That(actual.min.y, Is.InRange(0f, 1f));
            Assert.That(actual.max.x, Is.InRange(0f, 1f));
            Assert.That(actual.max.y, Is.InRange(0f, 1f));

            Assert.That(actual.min.x, Is.LessThanOrEqualTo(actual.max.x));
            Assert.That(actual.min.y, Is.LessThanOrEqualTo(actual.max.y));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(8)]
        [TestCase(3)]
        [TestCase(12)]
        [TestCase(15)]
        public void CalculateAnchors_WithSymmetricInsets_RespectedAxisRemainsSymmetric(int safeAreaFlags)
        {
            var safeArea = MakeSafeArea(100, 200, 100, 200);

            var actual = SafeArea.CalculateAnchors(
                ScreenWidth,
                ScreenHeight,
                safeArea,
                (SafeArea.SafeAreaMode)safeAreaFlags,
                0,
                false);

            if ((((SafeArea.SafeAreaMode)safeAreaFlags) & SafeArea.SafeAreaMode.Left) != 0 &&
                (((SafeArea.SafeAreaMode)safeAreaFlags) & SafeArea.SafeAreaMode.Right) != 0)
            {
                Assert.That(actual.min.x, Is.EqualTo(1f - actual.max.x).Within(Epsilon));
            }

            if ((((SafeArea.SafeAreaMode)safeAreaFlags) & SafeArea.SafeAreaMode.Bottom) != 0 &&
                (((SafeArea.SafeAreaMode)safeAreaFlags) & SafeArea.SafeAreaMode.Top) != 0)
            {
                Assert.That(actual.min.y, Is.EqualTo(1f - actual.max.y).Within(Epsilon));
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CalculateAnchors_WhenInsetsAreZero_ReturnsFullScreen(bool flipped)
        {
            var safeArea = MakeSafeArea(0, 0, 0, 0);

            var actual = SafeArea.CalculateAnchors(
                ScreenWidth,
                ScreenHeight,
                safeArea,
                SafeArea.SafeAreaMode.Left | SafeArea.SafeAreaMode.Right | SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Bottom,
                SafeArea.AlignmentMode.CenterHorizontally | SafeArea.AlignmentMode.CenterVertically,
                flipped);

            AssertAnchors(actual, 0f, 0f, 1f, 1f);
        }
    }
}
