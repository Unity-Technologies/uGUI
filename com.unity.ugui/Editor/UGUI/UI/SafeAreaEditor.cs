using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace UnityEngine.UI
{
    [CustomEditor(typeof(SafeArea))]
    [CanEditMultipleObjects]
    internal class SafeAreaEditor : Editor
    {
        private SerializedProperty m_ReferenceOrientation;
        private SerializedProperty m_Edges;
        private SerializedProperty m_Alignment;

        private HelpBox m_SelfLayoutControllerHelpBox;
        private HelpBox m_ParentLayoutControllerHelpBox;
        private HelpBox m_RootCanvasHelpBox;
        private HelpBox m_MultiSelectWarningHelpBox;
        private StringBuilder m_Builder;

        private const string SelfLayoutWarningText =
            "Safe Area conflicts with Layout Controllers in the same object. " +
            "Remove Layout Controllers or move Safe Area to a different GameObject.";

        private const string ParentLayoutWarningText =
            "Safe Area conflicts with Layout Controllers in the parent object. " +
            "Remove the layout controller in the parent or move Safe Area to a different GameObject.";

        private const string RootCanvasWarningText =
            "Safe Area is attached to a Root Canvas. " +
            "Attach Safe Area to a child object of the Root Canvas instead. " +
            "Refer to the Safe Area documentation for more information.";

        private const string MultiLayoutWarningText = "One or more selections has conflicts with another component. " +
                                              "Select each of them individually for more information.";

        private SafeArea Component => (SafeArea)target;

        private void OnEnable()
        {
            m_ReferenceOrientation = serializedObject.FindProperty("m_ReferenceOrientation");
            m_Edges = serializedObject.FindProperty("m_Edges");
            m_Alignment = serializedObject.FindProperty("m_Alignment");
        }

        /// <inheritdoc/>
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            // Help boxes
            m_SelfLayoutControllerHelpBox = new HelpBox(
                string.Empty,
                HelpBoxMessageType.Warning);
            m_ParentLayoutControllerHelpBox = new HelpBox(
                string.Empty,
                HelpBoxMessageType.Warning);
            m_RootCanvasHelpBox = new HelpBox(
                RootCanvasWarningText,
                HelpBoxMessageType.Warning);
            m_MultiSelectWarningHelpBox = new HelpBox(
                MultiLayoutWarningText,
                HelpBoxMessageType.Warning);
            root.Add(m_ParentLayoutControllerHelpBox);
            root.Add(m_SelfLayoutControllerHelpBox);
            root.Add(m_RootCanvasHelpBox);
            root.Add(m_MultiSelectWarningHelpBox);
            UpdateHelpBoxes();

            root.Add(new PropertyField(m_ReferenceOrientation));
            root.Add(new PropertyField(m_Edges));
            root.Add(new PropertyField(m_Alignment));

            root.schedule.Execute(UpdateHelpBoxes).Every(200);

            return root;
        }

        private void UpdateHelpBoxes()
        {
            var comp = Component;
            if (!comp)
                return;

            bool showSelfWarning = false;
            bool showParentWarning = false;
            bool showRootCanvasWarning = false;
            var multiEdit = serializedObject.isEditingMultipleObjects;

            var hasErrors = false;

            if (multiEdit)
            {
                foreach (var obj in targets)
                {
                    var tgt = (SafeArea)obj;
                    var rectTr = tgt.transform as RectTransform;
                    if (rectTr == null) continue;
                    if (rectTr.drivenByObject == tgt) continue;
                    if (rectTr.drivenByObject != tgt && !tgt.enabled) continue;

                    hasErrors = true;
                    break;
                }
            }
            else if (target is SafeArea safeArea && safeArea.enabled)
            {
                showSelfWarning = ShowSelfWarning(comp);
                showParentWarning = ShowParentWarning(comp);
                showRootCanvasWarning = ShowRootCanvasWarning(comp);
            }

            m_SelfLayoutControllerHelpBox.style.display = showSelfWarning
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            m_ParentLayoutControllerHelpBox.style.display = showParentWarning
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            m_RootCanvasHelpBox.style.display = showRootCanvasWarning
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            m_MultiSelectWarningHelpBox.style.display = hasErrors
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }

        private bool ShowParentWarning(SafeArea comp)
        {
            if (comp.transform.parent == null)
                return false;

            var showWarning = false;
            var layouts = ListPool<ILayoutController>.Get();
            comp.transform.parent.GetComponents(layouts);
            if (layouts.Count > 0)
            {
                m_Builder ??= new StringBuilder();
                m_Builder.Append(ParentLayoutWarningText);
                m_Builder.Append("\n\nConflicting Components:");
                foreach (var layout in layouts)
                {
                    if (layout is not ILayoutSelfController && layout is Behaviour behaviour && behaviour.enabled)
                    {
                        showWarning = true;
                        m_Builder.Append($"\n{layout.GetType().Name}");
                    }
                }

                if (showWarning)
                {
                    m_ParentLayoutControllerHelpBox.text = m_Builder.ToString();
                }
                m_Builder.Clear();
            }
            ListPool<ILayoutController>.Release(layouts);

            return showWarning;
        }

        private static bool ShowRootCanvasWarning(SafeArea comp)
        {
            return comp.TryGetComponent<Canvas>(out var canvas) && canvas.isRootCanvas;
        }

        private bool ShowSelfWarning(SafeArea comp)
        {
            var showWarning = false;
            var layouts = ListPool<ILayoutSelfController>.Get();
            comp.GetComponents(layouts);
            if (layouts.Count > 0)
            {
                m_Builder ??= new StringBuilder();
                m_Builder.Append(SelfLayoutWarningText);
                m_Builder.Append("\n\nConflicting Components:");
                foreach (var layout in layouts)
                {
                    if (layout is Behaviour behaviour && behaviour.enabled)
                    {
                        showWarning = true;
                        m_Builder.Append($"\n{layout.GetType().Name}");
                    }
                }

                if (showWarning)
                {
                    m_SelfLayoutControllerHelpBox.text = m_Builder.ToString();
                }
                m_Builder.Clear();
            }
            ListPool<ILayoutSelfController>.Release(layouts);

            return showWarning;
        }
    }
}
