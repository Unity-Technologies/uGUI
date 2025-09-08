using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityEngine.UIElements
{
    // This code is disabled unless the com.unity.modules.uielements module is present.
    // The UIElements module is always present in the Editor but it can be stripped from a project build if unused.
#if PACKAGE_UITOOLKIT
    /// <summary>
    /// A derived BaseRaycaster to raycast against UI Toolkit panel instances at runtime.
    /// </summary>
    [AddComponentMenu("UI Toolkit/Panel Raycaster (UI Toolkit)")]
    public class PanelRaycaster : BaseRaycaster, IRuntimePanelComponent
    {
        private BaseRuntimePanel m_Panel;

        /// <summary>
        /// The panel that this component relates to. If panel is null, this component will have no effect.
        /// Will be set to null automatically if panel is Disposed from an external source.
        /// </summary>
        public IPanel panel
        {
            get => m_Panel;
            set
            {
                var newPanel = (BaseRuntimePanel)value;
                if (m_Panel != newPanel)
                {
                    UnregisterCallbacks();
                    m_Panel = newPanel;
                    RegisterCallbacks();
                }
            }
        }

        void RegisterCallbacks()
        {
            if (m_Panel != null)
            {
                m_Panel.destroyed += OnPanelDestroyed;
            }
        }

        void UnregisterCallbacks()
        {
            if (m_Panel != null)
            {
                m_Panel.destroyed -= OnPanelDestroyed;
            }
        }

        void OnPanelDestroyed()
        {
            panel = null;
        }

        private GameObject selectableGameObject => m_Panel?.selectableGameObject;

        public override int sortOrderPriority => Mathf.FloorToInt(m_Panel?.sortingPriority ?? 0f);
        public override int renderOrderPriority => int.MaxValue - (UIElementsRuntimeUtility.s_ResolvedSortingIndexMax - (m_Panel?.resolvedSortingIndex ?? 0));

        private static ScreenOverlayPanelPicker panelPicker = new ScreenOverlayPanelPicker();

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (m_Panel == null || !m_Panel.isFlat)
                return;

            var displayIndex = m_Panel.targetDisplay;

            Vector3 eventPosition = MultipleDisplayUtilities.GetRelativeMousePositionForRaycast(eventData);
            var position = eventPosition;
            var delta = eventData.delta;

            float h = Screen.height;
            if (displayIndex > 0 && displayIndex < Display.displays.Length)
            {
#if UNITY_ANDROID
                    // Changed for UITK to be coherent for Android which passes display-relative rendering coordinates
                    h = Display.displays[displayIndex].renderingHeight;
#else
                    h = Display.displays[displayIndex].systemHeight;
#endif
            }

            position.y = h - position.y;
            delta.y = -delta.y;

            var currentInputModule = eventData.currentInputModule;
            if (currentInputModule == null)
                return;
            var pointerId = currentInputModule.ConvertUIToolkitPointerId(eventData);

            if (!panelPicker.TryPick((RuntimePanel)m_Panel, pointerId, position, delta, (int)eventPosition.z, out _))
                return;

            resultAppendList.Add(new RaycastResult
            {
                gameObject = selectableGameObject,
                module = this,
                screenPosition = eventPosition,
                displayIndex = m_Panel.targetDisplay,
            });
        }

        public override Camera eventCamera => null;
    }
#endif
}
