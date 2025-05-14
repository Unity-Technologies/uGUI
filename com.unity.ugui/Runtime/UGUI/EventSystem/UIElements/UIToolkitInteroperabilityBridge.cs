using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.UIElements
{
    // This code is disabled unless the com.unity.modules.uielements module is present.
    // The UIElements module is always present in the Editor but it can be stripped from a project build if unused.
#if PACKAGE_UITOOLKIT
    /// <summary>
    /// Enables UI Toolkit interoperability with uGUI events.
    /// </summary>
    internal class UIToolkitInteroperabilityBridge
    {
        [Flags]
        public enum EventHandlerTypes
        {
            ScreenOverlay = 1,
            WorldSpace = 2,
        }

        private EventSystem m_EventSystem;
        internal EventSystem eventSystem
        {
            get => m_EventSystem;
            set
            {
                if (m_EventSystem == value) return;
                Debug.Assert(!m_Started && !m_Enabled, "Expect EventSystem set before OnEnable and Start");
                m_EventSystem = value;
            }
        }

        private bool m_OverrideUIToolkitEvents = true;
        private EventHandlerTypes m_HandlerTypes = EventHandlerTypes.ScreenOverlay | EventHandlerTypes.WorldSpace;
        private LayerMask m_WorldPickingLayers = Physics.DefaultRaycastLayers;
        private float m_WorldPickingMaxDistance = Mathf.Infinity;
        private bool m_CreateDefaultPanelComponents = true;

        private bool m_Started;
        private bool m_Enabled;
        private bool m_IsTrackingPanels;
        private GameObject m_WorldSpaceGo;

        public bool overrideUIToolkitEvents
        {
            get => m_OverrideUIToolkitEvents;
            internal set
            {
                m_OverrideUIToolkitEvents = value;
                ApplyOverrideUIToolkitEvents();
            }
        }

        public EventHandlerTypes handlerTypes
        {
            get => m_HandlerTypes;
            internal set
            {
                m_HandlerTypes = value;
                ApplyOtherProperties();
            }
        }

        public int worldPickingLayers
        {
            get => m_WorldPickingLayers;
            internal set => m_WorldPickingLayers = value;
        }

        public float worldPickingMaxDistance
        {
            get => m_WorldPickingMaxDistance;
            internal set => m_WorldPickingMaxDistance = value;
        }

        public bool createDefaultPanelComponents
        {
            get => m_CreateDefaultPanelComponents;
            internal set
            {
                m_CreateDefaultPanelComponents = value;
                ApplyOtherProperties();
            }
        }

        private bool shouldTrackPanels => overrideUIToolkitEvents && createDefaultPanelComponents &&
                                          m_Started && m_Enabled;

        private void StartTrackingUIToolkitPanels()
        {
            if (m_IsTrackingPanels || !shouldTrackPanels) return;

            foreach (var panel in UIElementsRuntimeUtility.GetSortedPlayerPanels())
            {
                StartTrackingPanel(panel);
            }

            UIElementsRuntimeUtility.onCreatePanel += StartTrackingPanel;
            m_IsTrackingPanels = true;
        }

        private readonly HashSet<BaseRuntimePanel> trackedPanels = new();
        private void StartTrackingPanel(BaseRuntimePanel panel)
        {
            trackedPanels.Add(panel);
        }

        private void StopTrackingUIToolkitPanels()
        {
            if (!m_IsTrackingPanels) return;

            UIElementsRuntimeUtility.onCreatePanel -= StartTrackingPanel;
            m_IsTrackingPanels = false;

            foreach (var panel in trackedPanels)
            {
                DestroyPanelGameObject(panel);
            }
            trackedPanels.Clear();

            DestroyWorldSpacePanelGameObject();
        }

        private void UpdatePanelGameObject(BaseRuntimePanel panel)
        {
            var handlerType = panel.isFlat ? EventHandlerTypes.ScreenOverlay : EventHandlerTypes.WorldSpace;
            if ((m_HandlerTypes & handlerType) != 0)
            {
                CreatePanelGameObject(panel);
            }
            else
            {
                DestroyPanelGameObject(panel);
            }
        }

        private readonly Dictionary<BaseRuntimePanel, Action> destroyedActions = new();
        private void CreatePanelGameObject(BaseRuntimePanel panel)
        {
            if (panel.selectableGameObject == null)
            {
                var go = new GameObject(panel.name, typeof(PanelEventHandler), typeof(PanelRaycaster));
                go.transform.SetParent(m_EventSystem.transform);
                panel.selectableGameObject = go;
                var destroyed = destroyedActions[panel] = () => UIRUtility.Destroy(go);
                panel.destroyed += destroyed;
            }
        }

        private void DestroyPanelGameObject(BaseRuntimePanel panel)
        {
            var go = panel.selectableGameObject;
            if (go != null)
            {
                if (!destroyedActions.Remove(panel, out var destroyed))
                    return; // this object wasn't created by us, so leave it untouched
                panel.destroyed -= destroyed;
                panel.selectableGameObject = null;
                UIRUtility.Destroy(go);
            }
        }

        private void CreateWorldSpacePanelGameObject()
        {
            // This will destroy m_WorldSpaceGo if raycaster components need to change.
            ApplyCameraProperties();

            if (m_WorldSpaceGo == null)
            {
                var go = new GameObject("WorldDocumentRaycaster");
                go.transform.SetParent(m_EventSystem.transform);

                if (m_InputSettings.defaultEventCameraIsMainCamera)
                {
                    go.AddComponent<WorldDocumentRaycaster>();
                }
                else
                {
                    foreach (var cam in m_InputSettings.eventCameras)
                    {
                        go.AddComponent<WorldDocumentRaycaster>().camera = cam;
                    }
                }

                m_WorldSpaceGo = go;
            }
        }

        private void DestroyWorldSpacePanelGameObject()
        {
            var go = m_WorldSpaceGo;
            m_WorldSpaceGo = null;
            UIRUtility.Destroy(go);
        }

        public void Start()
        {
            m_Started = true;
            StartTrackingUIToolkitPanels();
        }

        public void OnEnable()
        {
            if (m_Enabled) return;
            m_Enabled = true;

            if (PanelInputConfiguration.current != null)
                Apply(PanelInputConfiguration.current);
            PanelInputConfiguration.onApply += Apply;

            if (m_Started)
                StartTrackingUIToolkitPanels();

            if (m_OverrideUIToolkitEvents)
                UIElementsRuntimeUtility.RegisterEventSystem(m_EventSystem);
        }

        public void OnDisable()
        {
            if (!m_Enabled) return;
            m_Enabled = false;

            PanelInputConfiguration.onApply -= Apply;

            StopTrackingUIToolkitPanels();
            UIElementsRuntimeUtility.UnregisterEventSystem(m_EventSystem);
        }

        public void Update()
        {
            UpdatePanelGameObjects();
        }

        private PanelInputConfiguration.Settings m_InputSettings = PanelInputConfiguration.Settings.Default;
        void Apply(PanelInputConfiguration input)
        {
            m_InputSettings = input != null ? input.settings : PanelInputConfiguration.Settings.Default;
            m_OverrideUIToolkitEvents =
                m_InputSettings.panelInputRedirection != PanelInputConfiguration.PanelInputRedirection.Never;
            m_HandlerTypes = EventHandlerTypes.ScreenOverlay |
                (m_InputSettings.processWorldSpaceInput ? EventHandlerTypes.WorldSpace : 0);
            m_WorldPickingLayers = m_InputSettings.interactionLayers;
            m_WorldPickingMaxDistance = m_InputSettings.maxInteractionDistance;
            m_CreateDefaultPanelComponents = m_InputSettings.autoCreatePanelComponents;

            ApplyOverrideUIToolkitEvents();
            ApplyCameraProperties();
            ApplyOtherProperties();
        }

        private bool m_OldOverrideUIToolkitEvents = true;
        private EventHandlerTypes m_OldHandlerTypes = EventHandlerTypes.ScreenOverlay | EventHandlerTypes.WorldSpace;
        private bool m_OldCreateDefaultPanelComponents = true;
        private bool m_OldDefaultEventCameraIsMainCamera = true;
        private long m_OldEventCamerasHash = 0;

        private void ApplyOverrideUIToolkitEvents()
        {
            if (m_OldOverrideUIToolkitEvents == m_OverrideUIToolkitEvents) return;
            m_OldOverrideUIToolkitEvents = m_OverrideUIToolkitEvents;

            if (!m_Enabled) return;

            if (m_OverrideUIToolkitEvents)
            {
                UIElementsRuntimeUtility.RegisterEventSystem(m_EventSystem);
            }
            else
            {
                UIElementsRuntimeUtility.UnregisterEventSystem(m_EventSystem);
            }

            UpdatePanelTracking();
        }

        private void ApplyCameraProperties()
        {
            bool dirty = false;

            if (m_OldDefaultEventCameraIsMainCamera != m_InputSettings.defaultEventCameraIsMainCamera)
            {
                m_OldDefaultEventCameraIsMainCamera = m_InputSettings.defaultEventCameraIsMainCamera;
                dirty = true;
            }

            if (!m_InputSettings.defaultEventCameraIsMainCamera)
            {
                var hashCode = 0;
                foreach (var cam in m_InputSettings.eventCameras)
                {
                    hashCode = (hashCode * 397) ^ cam.GetHashCode();
                }

                if (m_OldEventCamerasHash != hashCode)
                {
                    m_OldEventCamerasHash = hashCode;
                    dirty = true;
                }
            }
            else
            {
                m_OldEventCamerasHash = 0;
            }

            if (dirty)
                DestroyWorldSpacePanelGameObject();
        }

        private void ApplyOtherProperties()
        {
            bool dirty = false;

            if (m_OldHandlerTypes != m_HandlerTypes)
            {
                m_OldHandlerTypes = m_HandlerTypes;
                dirty = true;
            }

            if (m_OldCreateDefaultPanelComponents != m_CreateDefaultPanelComponents)
            {
                m_OldCreateDefaultPanelComponents = m_CreateDefaultPanelComponents;
                dirty = true;
            }

            if (dirty)
                UpdatePanelTracking();
        }

        private void UpdatePanelTracking()
        {
            if (shouldTrackPanels)
            {
                StartTrackingUIToolkitPanels();
            }
            else
            {
                StopTrackingUIToolkitPanels();
            }
        }

        private void UpdatePanelGameObjects()
        {
            if (!m_IsTrackingPanels) return;

            bool isWorldSpaceActive = false;

            foreach (var panel in trackedPanels)
            {
                UpdatePanelGameObject(panel);
                isWorldSpaceActive |= !panel.isFlat;
            }

            if (isWorldSpaceActive && (m_HandlerTypes & EventHandlerTypes.WorldSpace) != 0)
            {
                CreateWorldSpacePanelGameObject();
            }
            else
            {
                DestroyWorldSpacePanelGameObject();
            }
        }
    }
#endif
}
