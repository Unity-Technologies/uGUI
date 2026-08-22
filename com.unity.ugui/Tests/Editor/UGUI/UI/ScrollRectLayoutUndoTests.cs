using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;
using UnityEngine.UI;

namespace UnityEditor.UI.Tests
{
    // Adding a layout group to a Scroll View repositions its children (and descendants such as the scrollbar
    // handles); undoing the "Add Component" must restore every descendant RectTransform to its pre-add state.
    // (UUM-144838)
    internal class ScrollRectLayoutUndoTests
    {
        GameObject m_Canvas;
        GameObject m_ScrollView;

        static readonly Vector2EqualityComparer k_Vector2Comparer = new Vector2EqualityComparer(0.01f);

        [SetUp]
        public void SetUp()
        {
            m_Canvas = new GameObject("Canvas", typeof(Canvas));

            m_ScrollView = DefaultControls.CreateScrollView(new DefaultControls.Resources());
            var scrollRT = (RectTransform)m_ScrollView.transform;
            scrollRT.SetParent(m_Canvas.transform, false);

            // Off-origin, where any leftover offset is most visible.
            scrollRT.anchorMin = scrollRT.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRT.anchoredPosition = new Vector2(300, 200);

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRT);
        }

        [TearDown]
        public void TearDown()
        {
            if (m_ScrollView != null)
                Object.DestroyImmediate(m_ScrollView);
            if (m_Canvas != null)
                Object.DestroyImmediate(m_Canvas);
        }

        struct RectState
        {
            public Vector2 anchorMin, anchorMax, anchoredPosition, sizeDelta;
        }

        static Dictionary<RectTransform, RectState> SnapshotDescendants(Transform root)
        {
            var map = new Dictionary<RectTransform, RectState>();
            foreach (var rt in root.GetComponentsInChildren<RectTransform>(true))
            {
                if (rt == root) // only descendants are recorded
                    continue;

                map[rt] = new RectState
                {
                    anchorMin = rt.anchorMin,
                    anchorMax = rt.anchorMax,
                    anchoredPosition = rt.anchoredPosition,
                    sizeDelta = rt.sizeDelta
                };
            }
            return map;
        }

        static void AssertRestored(Dictionary<RectTransform, RectState> before)
        {
            foreach (var kvp in before)
            {
                var rt = kvp.Key;
                Assert.That(rt != null, $"'{kvp.Key}' was unexpectedly destroyed by undo.");
                var state = kvp.Value;
                string name = rt.name;
                Assert.That(rt.anchorMin, Is.EqualTo(state.anchorMin).Using(k_Vector2Comparer), $"{name}.anchorMin not restored");
                Assert.That(rt.anchorMax, Is.EqualTo(state.anchorMax).Using(k_Vector2Comparer), $"{name}.anchorMax not restored");
                Assert.That(rt.anchoredPosition, Is.EqualTo(state.anchoredPosition).Using(k_Vector2Comparer), $"{name}.anchoredPosition not restored");
                Assert.That(rt.sizeDelta, Is.EqualTo(state.sizeDelta).Using(k_Vector2Comparer), $"{name}.sizeDelta not restored");
            }
        }

        static void AssertAnyChanged(Dictionary<RectTransform, RectState> before, string layoutGroupName)
        {
            foreach (var kvp in before)
            {
                var rt = kvp.Key;
                var state = kvp.Value;
                if (!k_Vector2Comparer.Equals(rt.anchorMin, state.anchorMin)
                    || !k_Vector2Comparer.Equals(rt.anchorMax, state.anchorMax)
                    || !k_Vector2Comparer.Equals(rt.anchoredPosition, state.anchoredPosition)
                    || !k_Vector2Comparer.Equals(rt.sizeDelta, state.sizeDelta))
                    return;
            }

            Assert.Fail($"Adding {layoutGroupName} left every descendant RectTransform unchanged, so this test would pass without exercising the undo.");
        }

        static void AddLayoutGroupThenUndo<T>(GameObject target, RectTransform scrollRT, Dictionary<RectTransform, RectState> before) where T : Component
        {
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();

            // ObjectFactory.AddComponent matches the Inspector's "Add Component" button.
            ObjectFactory.AddComponent<T>(target);
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRT);

            AssertAnyChanged(before, typeof(T).Name);

            Undo.CollapseUndoOperations(group);
            Undo.PerformUndo();
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRT);
        }

        [Test]
        public void UndoAddVerticalLayoutGroup_RestoresScrollViewDescendants()
        {
            var scrollRT = (RectTransform)m_ScrollView.transform;
            var before = SnapshotDescendants(scrollRT);

            AddLayoutGroupThenUndo<VerticalLayoutGroup>(m_ScrollView, scrollRT, before);

            Assert.That(m_ScrollView.GetComponent<VerticalLayoutGroup>(), Is.Null,
                "A single Undo should remove the added VerticalLayoutGroup.");
            AssertRestored(before);
        }

        [Test]
        public void UndoAddHorizontalLayoutGroup_RestoresScrollViewDescendants()
        {
            var scrollRT = (RectTransform)m_ScrollView.transform;
            var before = SnapshotDescendants(scrollRT);

            AddLayoutGroupThenUndo<HorizontalLayoutGroup>(m_ScrollView, scrollRT, before);

            Assert.That(m_ScrollView.GetComponent<HorizontalLayoutGroup>(), Is.Null);
            AssertRestored(before);
        }

        [Test]
        public void UndoAddGridLayoutGroup_RestoresScrollViewDescendants()
        {
            var scrollRT = (RectTransform)m_ScrollView.transform;
            var before = SnapshotDescendants(scrollRT);

            AddLayoutGroupThenUndo<GridLayoutGroup>(m_ScrollView, scrollRT, before);

            Assert.That(m_ScrollView.GetComponent<GridLayoutGroup>(), Is.Null);
            AssertRestored(before);
        }
    }
}
