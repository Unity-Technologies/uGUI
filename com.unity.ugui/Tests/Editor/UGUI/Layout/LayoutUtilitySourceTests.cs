using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI.Tests
{
    internal class LayoutUtilitySourceTests
    {
        GameObject m_GameObject;
        RectTransform m_RectTransform;
        LayoutElement m_FirstElement;
        LayoutElement m_SecondElement;

        [SetUp]
        public void SetUp()
        {
            m_GameObject = new GameObject("LayoutElement", typeof(RectTransform));
            m_RectTransform = (RectTransform)m_GameObject.transform;
            m_FirstElement = m_GameObject.AddComponent<LayoutElement>();
            m_SecondElement = m_GameObject.AddComponent<LayoutElement>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_GameObject);
        }

        // Two same-priority elements with a min each resolve to the larger min, from its element.
        [Test]
        public void MinSize_SamePriorityElements_ReportsLargestMinAndItsElement()
        {
            m_FirstElement.minWidth = 100;
            m_FirstElement.minHeight = 40;
            m_SecondElement.minWidth = 50;
            m_SecondElement.minHeight = 20;

            float minWidth = LayoutUtility.GetMinWidth(m_RectTransform, out var widthSource);
            float minHeight = LayoutUtility.GetMinHeight(m_RectTransform, out var heightSource);

            Assert.AreEqual(LayoutUtility.GetMinWidth(m_RectTransform), minWidth, "Min Width should match the value layout uses.");
            Assert.AreEqual(LayoutUtility.GetMinHeight(m_RectTransform), minHeight, "Min Height should match the value layout uses.");
            Assert.AreEqual(100f, minWidth, "Min Width should be the larger min.");
            Assert.AreEqual(40f, minHeight, "Min Height should be the larger min.");
            Assert.AreSame(m_FirstElement, widthSource, "Min Width source should be the element with the larger min.");
            Assert.AreSame(m_FirstElement, heightSource, "Min Height source should be the element with the larger min.");
        }

        // With no max size enabled, max resolves to Infinity with no source element.
        [Test]
        public void MaxSize_WithoutMaxEnabled_ReportsInfinityAndNoSource()
        {
            m_FirstElement.preferredWidth = 100;

            float maxWidth = LayoutUtility.GetMaxWidth(m_RectTransform, out var widthSource);
            float maxHeight = LayoutUtility.GetMaxHeight(m_RectTransform, out var heightSource);

            Assert.AreEqual(float.PositiveInfinity, maxWidth, "Max Width should default to Infinity when unset.");
            Assert.AreEqual(float.PositiveInfinity, maxHeight, "Max Height should default to Infinity when unset.");
            Assert.IsNull(widthSource, "Max Width should have no source when unset.");
            Assert.IsNull(heightSource, "Max Height should have no source when unset.");
        }

        // Two same-priority elements with a max each resolve to the smaller max, from its element.
        [Test]
        public void MaxSize_SamePriorityElements_ReportsSmallestMaxAndItsElement()
        {
            m_FirstElement.maxWidth = 300;
            m_FirstElement.maxHeight = 80;
            m_SecondElement.maxWidth = 200;
            m_SecondElement.maxHeight = 50;

            float maxWidth = LayoutUtility.GetMaxWidth(m_RectTransform, out var widthSource);
            float maxHeight = LayoutUtility.GetMaxHeight(m_RectTransform, out var heightSource);

            Assert.AreEqual(LayoutUtility.GetMaxWidth(m_RectTransform), maxWidth, "Max Width should match the value layout uses.");
            Assert.AreEqual(LayoutUtility.GetMaxHeight(m_RectTransform), maxHeight, "Max Height should match the value layout uses.");
            Assert.AreEqual(200f, maxWidth, "Max Width should be the smaller max.");
            Assert.AreEqual(50f, maxHeight, "Max Height should be the smaller max.");
            Assert.AreSame(m_SecondElement, widthSource, "Max Width source should be the element with the smaller max.");
            Assert.AreSame(m_SecondElement, heightSource, "Max Height source should be the element with the smaller max.");
        }

        // A preferred size inside the min/max bounds is reported as is, from its own element.
        [Test]
        public void PreferredSize_WithinBounds_ReportsPreferredAndItsElement()
        {
            m_FirstElement.minWidth = 10;
            m_FirstElement.minHeight = 10;
            m_FirstElement.maxWidth = 500;
            m_FirstElement.maxHeight = 500;
            m_SecondElement.preferredWidth = 120;
            m_SecondElement.preferredHeight = 60;

            float preferredWidth = LayoutUtility.GetPreferredWidth(m_RectTransform, out var widthSource);
            float preferredHeight = LayoutUtility.GetPreferredHeight(m_RectTransform, out var heightSource);

            Assert.AreEqual(LayoutUtility.GetPreferredWidth(m_RectTransform), preferredWidth, "Preferred Width should match the value layout uses.");
            Assert.AreEqual(LayoutUtility.GetPreferredHeight(m_RectTransform), preferredHeight, "Preferred Height should match the value layout uses.");
            Assert.AreEqual(120f, preferredWidth, "Preferred Width should be unchanged inside the bounds.");
            Assert.AreEqual(60f, preferredHeight, "Preferred Height should be unchanged inside the bounds.");
            Assert.AreSame(m_SecondElement, widthSource, "Preferred Width source should be the element supplying the preferred size.");
            Assert.AreSame(m_SecondElement, heightSource, "Preferred Height source should be the element supplying the preferred size.");
        }

        // A preferred size below the min resolves to the min, from the min's element.
        [Test]
        public void PreferredSize_BelowMin_ReportsMinAndMinElement()
        {
            m_FirstElement.minWidth = 100;
            m_FirstElement.minHeight = 40;
            m_SecondElement.preferredWidth = 50;
            m_SecondElement.preferredHeight = 20;

            float preferredWidth = LayoutUtility.GetPreferredWidth(m_RectTransform, out var widthSource);
            float preferredHeight = LayoutUtility.GetPreferredHeight(m_RectTransform, out var heightSource);

            Assert.AreEqual(LayoutUtility.GetPreferredWidth(m_RectTransform), preferredWidth, "Preferred Width should match the value layout uses.");
            Assert.AreEqual(LayoutUtility.GetPreferredHeight(m_RectTransform), preferredHeight, "Preferred Height should match the value layout uses.");
            Assert.AreEqual(100f, preferredWidth, "Preferred Width should be raised to the min.");
            Assert.AreEqual(40f, preferredHeight, "Preferred Height should be raised to the min.");
            Assert.AreSame(m_FirstElement, widthSource, "Preferred Width source should be the element supplying the min.");
            Assert.AreSame(m_FirstElement, heightSource, "Preferred Height source should be the element supplying the min.");
        }

        // A preferred size above the max resolves to the max, from the max's element.
        [Test]
        public void PreferredSize_AboveMax_ReportsMaxAndMaxElement()
        {
            m_FirstElement.preferredWidth = 300;
            m_FirstElement.preferredHeight = 90;
            m_SecondElement.maxWidth = 200;
            m_SecondElement.maxHeight = 50;

            float preferredWidth = LayoutUtility.GetPreferredWidth(m_RectTransform, out var widthSource);
            float preferredHeight = LayoutUtility.GetPreferredHeight(m_RectTransform, out var heightSource);

            Assert.AreEqual(LayoutUtility.GetPreferredWidth(m_RectTransform), preferredWidth, "Preferred Width should match the value layout uses.");
            Assert.AreEqual(LayoutUtility.GetPreferredHeight(m_RectTransform), preferredHeight, "Preferred Height should match the value layout uses.");
            Assert.AreEqual(200f, preferredWidth, "Preferred Width should be clamped to the max.");
            Assert.AreEqual(50f, preferredHeight, "Preferred Height should be clamped to the max.");
            Assert.AreSame(m_SecondElement, widthSource, "Preferred Width source should be the element supplying the max.");
            Assert.AreSame(m_SecondElement, heightSource, "Preferred Height source should be the element supplying the max.");
        }

        // Two same-priority elements with a flexible size each resolve to the larger one, from its element.
        [Test]
        public void FlexibleSize_SamePriorityElements_ReportsLargestFlexibleAndItsElement()
        {
            m_FirstElement.flexibleWidth = 1;
            m_FirstElement.flexibleHeight = 1;
            m_SecondElement.flexibleWidth = 3;
            m_SecondElement.flexibleHeight = 2;

            float flexibleWidth = LayoutUtility.GetFlexibleWidth(m_RectTransform, out var widthSource);
            float flexibleHeight = LayoutUtility.GetFlexibleHeight(m_RectTransform, out var heightSource);

            Assert.AreEqual(LayoutUtility.GetFlexibleWidth(m_RectTransform), flexibleWidth, "Flexible Width should match the value layout uses.");
            Assert.AreEqual(LayoutUtility.GetFlexibleHeight(m_RectTransform), flexibleHeight, "Flexible Height should match the value layout uses.");
            Assert.AreEqual(3f, flexibleWidth, "Flexible Width should be the larger flexible size.");
            Assert.AreEqual(2f, flexibleHeight, "Flexible Height should be the larger flexible size.");
            Assert.AreSame(m_SecondElement, widthSource, "Flexible Width source should be the element with the larger flexible size.");
            Assert.AreSame(m_SecondElement, heightSource, "Flexible Height source should be the element with the larger flexible size.");
        }
    }
}
