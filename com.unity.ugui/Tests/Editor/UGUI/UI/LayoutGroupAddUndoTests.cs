using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;
using UnityEngine.UI;

namespace UnityEditor.UI.Tests
{
    // Covers the reach of the Undo registration in LayoutGroup.Reset: the whole subtree, including the group's own
    // RectTransform. ScrollRectLayoutUndoTests covers the per-layout-group-type cases.
    internal class LayoutGroupAddUndoTests
    {
        GameObject m_Canvas;
        GameObject m_Root;

        static readonly Vector2EqualityComparer k_Vector2Comparer = new Vector2EqualityComparer(0.01f);

        [SetUp]
        public void SetUp()
        {
            m_Canvas = new GameObject("Canvas", typeof(Canvas));
            Undo.ClearAll();
        }

        [TearDown]
        public void TearDown()
        {
            Undo.ClearAll();

            if (m_Root != null)
                Object.DestroyImmediate(m_Root);
            if (m_Canvas != null)
                Object.DestroyImmediate(m_Canvas);
        }

        struct RectState
        {
            public Vector2 anchorMin, anchorMax, anchoredPosition, sizeDelta;
        }

        static Dictionary<RectTransform, RectState> Snapshot(Transform root, bool includeRoot)
        {
            var map = new Dictionary<RectTransform, RectState>();
            foreach (var rect in root.GetComponentsInChildren<RectTransform>(true))
            {
                if (!includeRoot && rect.transform == root)
                    continue;

                map[rect] = new RectState
                {
                    anchorMin = rect.anchorMin,
                    anchorMax = rect.anchorMax,
                    anchoredPosition = rect.anchoredPosition,
                    sizeDelta = rect.sizeDelta
                };
            }
            return map;
        }

        static bool Matches(RectTransform rect, RectState state)
        {
            return k_Vector2Comparer.Equals(rect.anchorMin, state.anchorMin)
                && k_Vector2Comparer.Equals(rect.anchorMax, state.anchorMax)
                && k_Vector2Comparer.Equals(rect.anchoredPosition, state.anchoredPosition)
                && k_Vector2Comparer.Equals(rect.sizeDelta, state.sizeDelta);
        }

        static void AssertRestored(Dictionary<RectTransform, RectState> before, Dictionary<RectTransform, RectState> afterAdd)
        {
            foreach (var kvp in before)
            {
                var rect = kvp.Key;
                Assert.That(rect != null, "A RectTransform was unexpectedly destroyed by the undo.");

                var state = kvp.Value;
                string name = rect.name;

                string context = afterAdd.TryGetValue(rect, out var added)
                    ? $" (post-add was anchoredPosition {added.anchoredPosition}, sizeDelta {added.sizeDelta})"
                    : string.Empty;

                Assert.That(rect.anchorMin, Is.EqualTo(state.anchorMin).Using(k_Vector2Comparer), $"{name}.anchorMin not restored{context}");
                Assert.That(rect.anchorMax, Is.EqualTo(state.anchorMax).Using(k_Vector2Comparer), $"{name}.anchorMax not restored{context}");
                Assert.That(rect.anchoredPosition, Is.EqualTo(state.anchoredPosition).Using(k_Vector2Comparer), $"{name}.anchoredPosition not restored{context}");
                Assert.That(rect.sizeDelta, Is.EqualTo(state.sizeDelta).Using(k_Vector2Comparer), $"{name}.sizeDelta not restored{context}");
            }
        }

        static void AssertAnyChanged(Dictionary<RectTransform, RectState> before, string what)
        {
            foreach (var kvp in before)
            {
                if (!Matches(kvp.Key, kvp.Value))
                    return;
            }

            Assert.Fail($"Adding {what} left every tracked RectTransform unchanged, so this test would pass without exercising the undo registration.");
        }

        static void Rebuild(RectTransform rect)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }

        static RectTransform AddChild(Transform parent, string name, Vector2 anchoredPosition, Vector2 size)
        {
            var child = new GameObject(name, typeof(RectTransform), typeof(LayoutElement));
            child.transform.SetParent(parent, false);

            var rect = (RectTransform)child.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var element = child.GetComponent<LayoutElement>();
            element.preferredWidth = size.x;
            element.preferredHeight = size.y;

            return rect;
        }

        RectTransform CreateRoot(string name, Transform parent)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)gameObject.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(120f, 80f);
            rect.sizeDelta = new Vector2(400f, 300f);

            if (m_Root == null)
                m_Root = gameObject;

            return rect;
        }

        static Dictionary<RectTransform, RectState> AddLayoutGroupThenUndo<T>(GameObject target, RectTransform rebuildRoot,
            Dictionary<RectTransform, RectState> before, Transform snapshotRoot, bool includeRoot) where T : Component
        {
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();

            // This is the path that makes the Editor invoke LayoutGroup.Reset.
            ObjectFactory.AddComponent<T>(target);
            Rebuild(rebuildRoot);

            AssertAnyChanged(before, typeof(T).Name);
            var afterAdd = Snapshot(snapshotRoot, includeRoot);

            Undo.CollapseUndoOperations(undoGroup);
            Undo.PerformUndo();
            Rebuild(rebuildRoot);

            return afterAdd;
        }

        [Test]
        public void UndoAddLayoutGroup_RestoresNestedDescendantRectTransforms()
        {
            var outerRect = CreateRoot("Outer", m_Canvas.transform);

            AddChild(outerRect, "Child0", new Vector2(-60f, 40f), new Vector2(80f, 40f));
            AddChild(outerRect, "Child1", new Vector2(70f, -30f), new Vector2(90f, 50f));

            var nestedRect = CreateRoot("Nested", outerRect);
            nestedRect.anchoredPosition = new Vector2(-20f, -90f);
            nestedRect.sizeDelta = new Vector2(180f, 70f);
            nestedRect.gameObject.AddComponent<LayoutElement>();
            var nestedGroup = nestedRect.gameObject.AddComponent<HorizontalLayoutGroup>();
            nestedGroup.padding = new RectOffset(4, 4, 4, 4);
            AddChild(nestedRect, "GrandChild0", new Vector2(-40f, 0f), new Vector2(50f, 30f));
            AddChild(nestedRect, "GrandChild1", new Vector2(45f, 0f), new Vector2(60f, 30f));

            Rebuild(outerRect);

            var before = Snapshot(outerRect, includeRoot: false);
            Assume.That(before.Count, Is.GreaterThanOrEqualTo(5),
                "Expected the nested hierarchy to contribute grandchildren to the snapshot.");

            var afterAdd = AddLayoutGroupThenUndo<VerticalLayoutGroup>(outerRect.gameObject, outerRect, before, outerRect, includeRoot: false);

            Assert.That(outerRect.GetComponent<VerticalLayoutGroup>(), Is.Null,
                "A single undo should remove the added VerticalLayoutGroup.");
            AssertRestored(before, afterAdd);
        }

        [Test]
        public void UndoAddNestedLayoutGroup_RestoresItsOwnRectTransform()
        {
            var outerRect = CreateRoot("Outer", m_Canvas.transform);
            var outerGroup = outerRect.gameObject.AddComponent<VerticalLayoutGroup>();
            outerGroup.childControlWidth = true;
            outerGroup.childControlHeight = true;
            outerGroup.childForceExpandWidth = false;
            outerGroup.childForceExpandHeight = false;

            var innerRect = CreateRoot("Inner", outerRect);
            innerRect.gameObject.AddComponent<LayoutElement>();
            AddChild(innerRect, "GrandChild0", new Vector2(-30f, 0f), new Vector2(50f, 30f));
            AddChild(innerRect, "GrandChild1", new Vector2(35f, 0f), new Vector2(60f, 30f));

            Rebuild(outerRect);

            var before = Snapshot(innerRect, includeRoot: true);
            Assume.That(before.ContainsKey(innerRect), "The snapshot should include the inner group's own RectTransform.");

            var afterAdd = AddLayoutGroupThenUndo<HorizontalLayoutGroup>(innerRect.gameObject, outerRect, before, innerRect, includeRoot: true);

            Assert.That(innerRect.GetComponent<HorizontalLayoutGroup>(), Is.Null,
                "A single undo should remove the added HorizontalLayoutGroup.");
            AssertRestored(before, afterAdd);
        }
    }
}
