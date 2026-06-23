using NUnit.Framework;
using UnityEngine.UI;

namespace SafeAreaTests
{
    internal class SafeAreaRotateFlagTests
    {
        [TestCase(0, 0, 0)]
        [TestCase(0, 1, 0)]
        [TestCase(0, -1, 0)]
        [TestCase(0, 4, 0)]
        [TestCase(0, -4, 0)]
        [TestCase(0, 100, 0)]
        [TestCase(0, -100, 0)]
        public void RotateFlag_WhenNoFlagsSet_ReturnsNone(
            SafeArea.SafeAreaMode mode,
            int shift,
            SafeArea.SafeAreaMode expected)
        {
            var actual = SafeArea.RotateFlag(mode, shift);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(SafeArea.SafeAreaMode.Top, 0, SafeArea.SafeAreaMode.Top)]
        [TestCase(SafeArea.SafeAreaMode.Top, 1, SafeArea.SafeAreaMode.Right)]
        [TestCase(SafeArea.SafeAreaMode.Top, 2, SafeArea.SafeAreaMode.Bottom)]
        [TestCase(SafeArea.SafeAreaMode.Top, 3, SafeArea.SafeAreaMode.Left)]
        [TestCase(SafeArea.SafeAreaMode.Top, 4, SafeArea.SafeAreaMode.Top)]
        [TestCase(SafeArea.SafeAreaMode.Top, -1, SafeArea.SafeAreaMode.Left)]
        [TestCase(SafeArea.SafeAreaMode.Top, -2, SafeArea.SafeAreaMode.Bottom)]
        [TestCase(SafeArea.SafeAreaMode.Top, -3, SafeArea.SafeAreaMode.Right)]
        [TestCase(SafeArea.SafeAreaMode.Top, -4, SafeArea.SafeAreaMode.Top)]
        [TestCase(SafeArea.SafeAreaMode.Top, 5, SafeArea.SafeAreaMode.Right)]
        [TestCase(SafeArea.SafeAreaMode.Top, -5, SafeArea.SafeAreaMode.Left)]
        public void RotateFlag_RotatesTopCorrectly(
            SafeArea.SafeAreaMode mode,
            int shift,
            SafeArea.SafeAreaMode expected)
        {
            var actual = SafeArea.RotateFlag(mode, shift);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(SafeArea.SafeAreaMode.Right, 0, SafeArea.SafeAreaMode.Right)]
        [TestCase(SafeArea.SafeAreaMode.Right, 1, SafeArea.SafeAreaMode.Bottom)]
        [TestCase(SafeArea.SafeAreaMode.Right, 2, SafeArea.SafeAreaMode.Left)]
        [TestCase(SafeArea.SafeAreaMode.Right, 3, SafeArea.SafeAreaMode.Top)]
        [TestCase(SafeArea.SafeAreaMode.Right, 4, SafeArea.SafeAreaMode.Right)]
        [TestCase(SafeArea.SafeAreaMode.Right, -1, SafeArea.SafeAreaMode.Top)]
        [TestCase(SafeArea.SafeAreaMode.Right, -2, SafeArea.SafeAreaMode.Left)]
        [TestCase(SafeArea.SafeAreaMode.Right, -3, SafeArea.SafeAreaMode.Bottom)]
        [TestCase(SafeArea.SafeAreaMode.Right, -4, SafeArea.SafeAreaMode.Right)]
        public void RotateFlag_RotatesRightCorrectly(
            SafeArea.SafeAreaMode mode,
            int shift,
            SafeArea.SafeAreaMode expected)
        {
            var actual = SafeArea.RotateFlag(mode, shift);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(SafeArea.SafeAreaMode.Bottom, 0, SafeArea.SafeAreaMode.Bottom)]
        [TestCase(SafeArea.SafeAreaMode.Bottom, 1, SafeArea.SafeAreaMode.Left)]
        [TestCase(SafeArea.SafeAreaMode.Bottom, 2, SafeArea.SafeAreaMode.Top)]
        [TestCase(SafeArea.SafeAreaMode.Bottom, 3, SafeArea.SafeAreaMode.Right)]
        [TestCase(SafeArea.SafeAreaMode.Bottom, 4, SafeArea.SafeAreaMode.Bottom)]
        [TestCase(SafeArea.SafeAreaMode.Bottom, -1, SafeArea.SafeAreaMode.Right)]
        [TestCase(SafeArea.SafeAreaMode.Bottom, -2, SafeArea.SafeAreaMode.Top)]
        [TestCase(SafeArea.SafeAreaMode.Bottom, -3, SafeArea.SafeAreaMode.Left)]
        [TestCase(SafeArea.SafeAreaMode.Bottom, -4, SafeArea.SafeAreaMode.Bottom)]
        public void RotateFlag_RotatesBottomCorrectly(
            SafeArea.SafeAreaMode mode,
            int shift,
            SafeArea.SafeAreaMode expected)
        {
            var actual = SafeArea.RotateFlag(mode, shift);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(SafeArea.SafeAreaMode.Left, 0, SafeArea.SafeAreaMode.Left)]
        [TestCase(SafeArea.SafeAreaMode.Left, 1, SafeArea.SafeAreaMode.Top)]
        [TestCase(SafeArea.SafeAreaMode.Left, 2, SafeArea.SafeAreaMode.Right)]
        [TestCase(SafeArea.SafeAreaMode.Left, 3, SafeArea.SafeAreaMode.Bottom)]
        [TestCase(SafeArea.SafeAreaMode.Left, 4, SafeArea.SafeAreaMode.Left)]
        [TestCase(SafeArea.SafeAreaMode.Left, -1, SafeArea.SafeAreaMode.Bottom)]
        [TestCase(SafeArea.SafeAreaMode.Left, -2, SafeArea.SafeAreaMode.Right)]
        [TestCase(SafeArea.SafeAreaMode.Left, -3, SafeArea.SafeAreaMode.Top)]
        [TestCase(SafeArea.SafeAreaMode.Left, -4, SafeArea.SafeAreaMode.Left)]
        public void RotateFlag_RotatesLeftCorrectly(
            SafeArea.SafeAreaMode mode,
            int shift,
            SafeArea.SafeAreaMode expected)
        {
            var actual = SafeArea.RotateFlag(mode, shift);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(
            (SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Right),
            1,
            (SafeArea.SafeAreaMode.Right | SafeArea.SafeAreaMode.Bottom))]
        [TestCase(
            (SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Right),
            2,
            (SafeArea.SafeAreaMode.Bottom | SafeArea.SafeAreaMode.Left))]
        [TestCase(
            (SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Right),
            3,
            (SafeArea.SafeAreaMode.Left | SafeArea.SafeAreaMode.Top))]
        [TestCase(
            (SafeArea.SafeAreaMode.Left | SafeArea.SafeAreaMode.Bottom),
            1,
            (SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Left))]
        [TestCase(
            (SafeArea.SafeAreaMode.Left | SafeArea.SafeAreaMode.Bottom),
            -1,
            (SafeArea.SafeAreaMode.Bottom | SafeArea.SafeAreaMode.Right))]
        public void RotateFlag_RotatesAdjacentCombinationsCorrectly(
            SafeArea.SafeAreaMode mode,
            int shift,
            SafeArea.SafeAreaMode expected)
        {
            var actual = SafeArea.RotateFlag(mode, shift);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(
            (SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Bottom),
            1,
            (SafeArea.SafeAreaMode.Right | SafeArea.SafeAreaMode.Left))]
        [TestCase(
            (SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Bottom),
            2,
            (SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Bottom))]
        [TestCase(
            (SafeArea.SafeAreaMode.Left | SafeArea.SafeAreaMode.Right),
            1,
            (SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Bottom))]
        [TestCase(
            (SafeArea.SafeAreaMode.Left | SafeArea.SafeAreaMode.Right),
            2,
            (SafeArea.SafeAreaMode.Left | SafeArea.SafeAreaMode.Right))]
        public void RotateFlag_RotatesOppositeCombinationsCorrectly(
            SafeArea.SafeAreaMode mode,
            int shift,
            SafeArea.SafeAreaMode expected)
        {
            var actual = SafeArea.RotateFlag(mode, shift);

            Assert.AreEqual(expected, actual);
        }

        [TestCase((SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Right | SafeArea.SafeAreaMode.Bottom | SafeArea.SafeAreaMode.Left), 0)]
        [TestCase((SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Right | SafeArea.SafeAreaMode.Bottom | SafeArea.SafeAreaMode.Left), 1)]
        [TestCase((SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Right | SafeArea.SafeAreaMode.Bottom | SafeArea.SafeAreaMode.Left), 2)]
        [TestCase((SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Right | SafeArea.SafeAreaMode.Bottom | SafeArea.SafeAreaMode.Left), 3)]
        [TestCase((SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Right | SafeArea.SafeAreaMode.Bottom | SafeArea.SafeAreaMode.Left), 4)]
        [TestCase((SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Right | SafeArea.SafeAreaMode.Bottom | SafeArea.SafeAreaMode.Left), -1)]
        [TestCase((SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Right | SafeArea.SafeAreaMode.Bottom | SafeArea.SafeAreaMode.Left), -4)]
        [TestCase((SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Right | SafeArea.SafeAreaMode.Bottom | SafeArea.SafeAreaMode.Left), 99)]
        public void RotateFlag_WhenAllFlagsSet_ReturnsAllFlagsSet(
            SafeArea.SafeAreaMode mode,
            int shift)
        {
            var actual = SafeArea.RotateFlag(mode, shift);

            Assert.AreEqual(mode, actual);
        }

        [TestCase(SafeArea.SafeAreaMode.Top, 0)]
        [TestCase(SafeArea.SafeAreaMode.Top, 4)]
        [TestCase(SafeArea.SafeAreaMode.Top, 8)]
        [TestCase(SafeArea.SafeAreaMode.Top, -4)]
        [TestCase(SafeArea.SafeAreaMode.Top, -8)]
        [TestCase(SafeArea.SafeAreaMode.Right, 0)]
        [TestCase(SafeArea.SafeAreaMode.Right, 4)]
        [TestCase(SafeArea.SafeAreaMode.Bottom, 8)]
        [TestCase(SafeArea.SafeAreaMode.Left, -8)]
        public void RotateFlag_WhenShiftIsMultipleOfFour_ResultIsSame(
            SafeArea.SafeAreaMode mode,
            int shift)
        {
            var expected = mode;
            var actual = SafeArea.RotateFlag(mode, shift);

            Assert.AreEqual(expected, actual);
        }
    }
}
