using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace LayoutTests
{
    internal class LayoutElementMinMaxConflictTests
    {
        GameObject m_Canvas;
        GameObject m_GameObject;

        [SetUp]
        public void TestSetup()
        {
            m_Canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            m_GameObject = new GameObject("Element", typeof(RectTransform));
            m_GameObject.transform.SetParent(m_Canvas.transform, false);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_Canvas);
        }

        [Test]
        public void MinAboveMaxOnOneLayoutElementIsReported()
        {
            var element = m_GameObject.AddComponent<LayoutElement>();
            element.minWidth = 90;
            element.maxWidth = 1;

            bool hasConflict = LayoutUtility.TryGetMinMaxConflict(
                RectTransform,
                0,
                out float min,
                out ILayoutElement minSource,
                out float max,
                out ILayoutElement maxSource
            );

            Assert.IsTrue(hasConflict, "A minimum above the maximum was not reported.");
            Assert.AreEqual(90, min);
            Assert.AreEqual(1, max);
            Assert.AreSame(element, minSource, "The minimum was not attributed to the LayoutElement.");
            Assert.AreSame(element, maxSource, "The maximum was not attributed to the LayoutElement.");
        }

        [Test]
        public void MinBelowMaxIsNotReported()
        {
            var element = m_GameObject.AddComponent<LayoutElement>();
            element.minWidth = 10;
            element.maxWidth = 100;

            Assert.IsFalse(HasConflict(0));
        }

        [Test]
        public void MinWithoutMaxIsNotReported()
        {
            var element = m_GameObject.AddComponent<LayoutElement>();
            element.minWidth = 90;

            Assert.IsFalse(HasConflict(0));
        }

        // The minimum and the maximum can come from different components on the same GameObject.
        [Test]
        public void MinAndMaxFromSeparateLayoutElementsAreReported()
        {
            var minimum = m_GameObject.AddComponent<LayoutElement>();
            minimum.minWidth = 90;
            var maximum = m_GameObject.AddComponent<LayoutElement>();
            maximum.maxWidth = 1;

            bool hasConflict = LayoutUtility.TryGetMinMaxConflict(
                RectTransform,
                0,
                out _,
                out ILayoutElement minSource,
                out _,
                out ILayoutElement maxSource
            );

            Assert.IsTrue(hasConflict, "A minimum and maximum from separate components were not checked together.");
            Assert.AreSame(minimum, minSource);
            Assert.AreSame(maximum, maxSource);
        }

        [Test]
        public void DisabledLayoutElementIsNotConsidered()
        {
            m_GameObject.AddComponent<LayoutElement>().minWidth = 90;
            var maximum = m_GameObject.AddComponent<LayoutElement>();
            maximum.maxWidth = 1;
            maximum.enabled = false;

            Assert.IsFalse(HasConflict(0));
        }

        [Test]
        public void EachAxisIsCheckedSeparately()
        {
            var element = m_GameObject.AddComponent<LayoutElement>();
            element.minWidth = 90;
            element.maxWidth = 1;

            Assert.IsTrue(HasConflict(0), "The width conflict was not reported.");
            Assert.IsFalse(HasConflict(1), "A width conflict was reported for the height.");
        }

        bool HasConflict(int axis) => LayoutUtility.TryGetMinMaxConflict(RectTransform, axis, out _, out _, out _, out _);

        RectTransform RectTransform => (RectTransform)m_GameObject.transform;
    }
}
