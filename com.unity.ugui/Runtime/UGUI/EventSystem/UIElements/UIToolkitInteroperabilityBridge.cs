using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.UIElements
{
    // This code is disabled unless the com.unity.modules.uielements module is activated in the package manager.
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
        private bool m_PendingRegistration; // True if we need to retry registration when utility becomes available

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

            // Can be null if runtime panels have not been created yet; mark for retry in Update()
            if (IRuntimePanel.uIElementsRuntimeUtility == null)
            {
                m_PendingRegistration = true;
                return;
            }

            foreach (var panel in IRuntimePanel.uIElementsRuntimeUtility.GetSortedPlayerPanelsInternal())
            {
                StartTrackingPanel(panel);
            }

            IRuntimePanel.uIElementsRuntimeUtility.AddOnCreatePanelAction( StartTrackingPanel);
            m_IsTrackingPanels = true;
        }

        // Use object storage to prevent generic type parameter from preserving BaseRuntimePanel during code stripping
        private readonly HashSet<IRuntimePanel> trackedPanels = new();
        private void StartTrackingPanel(IRuntimePanel panel)
        {
            trackedPanels.Add(panel);
        }

        private void StopTrackingUIToolkitPanels()
        {
            if (!m_IsTrackingPanels) return;

            // Can be null if runtime panels have not been created or UI Toolkit is stripped
            IRuntimePanel.uIElementsRuntimeUtility?.RemoveOnCreatePanelAction( StartTrackingPanel);
            m_IsTrackingPanels = false;

            foreach (var panelObj in trackedPanels)
            {
                DestroyPanelGameObject(panelObj);
            }
            trackedPanels.Clear();

            DestroyWorldSpacePanelGameObject();
        }

        private void UpdatePanelGameObject(IRuntimePanel panel)
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

        // Use interface storage to prevent generic type parameter from preserving BaseRuntimePanel during code stripping
        private readonly Dictionary<object, Action> destroyedActions = new();
        private void CreatePanelGameObject(IRuntimePanel panel)
        {
            if (panel.selectableGameObject == null)
            {
                var go = new GameObject(panel.name);

                go.AddComponent<PanelEventHandler>();
                go.AddComponent<PanelRaycaster>();

                go.transform.SetParent(m_EventSystem.transform);
                panel.selectableGameObject = go;
                var destroyed = destroyedActions[panel] = () => DestroyPanelGameObject(panel);
                panel.destroyed += destroyed;
            }
        }

        private void DestroyPanelGameObject(IRuntimePanel panel)
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
                        var raycaster = go.AddComponent<WorldDocumentRaycaster>();
                        if (raycaster != null)
                        {
                            raycaster.camera = cam;
                        }
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

            // Can be null if runtime panels have not been created yet; mark for retry in Update()
            if (m_OverrideUIToolkitEvents)
            {
                if (IRuntimePanel.uIElementsRuntimeUtility != null)
                    IRuntimePanel.uIElementsRuntimeUtility.RegisterEventSystem(m_EventSystem);
                else
                    m_PendingRegistration = true;
            }
        }

        public void OnDisable()
        {
            if (!m_Enabled) return;
            m_Enabled = false;

            PanelInputConfiguration.onApply -= Apply;

            StopTrackingUIToolkitPanels();
            // Can be null if runtime panels have not been created or UI Toolkit is stripped
            IRuntimePanel.uIElementsRuntimeUtility?.UnregisterEventSystem(m_EventSystem);
            m_PendingRegistration = false;
        }

        public void Update()
        {
            // Retry registration if it was pending and utility is now available
            if (m_PendingRegistration && IRuntimePanel.uIElementsRuntimeUtility != null)
            {
                m_PendingRegistration = false;

                // Retry event system registration
                if (m_OverrideUIToolkitEvents && m_Enabled)
                    IRuntimePanel.uIElementsRuntimeUtility.RegisterEventSystem(m_EventSystem);

                // Retry panel tracking
                if (shouldTrackPanels)
                    StartTrackingUIToolkitPanels();
            }

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

            // Can be null if runtime panels have not been created yet; mark for retry in Update()
            if (m_OverrideUIToolkitEvents)
            {
                if (IRuntimePanel.uIElementsRuntimeUtility != null)
                    IRuntimePanel.uIElementsRuntimeUtility.RegisterEventSystem(m_EventSystem);
                else
                    m_PendingRegistration = true;
            }
            else
            {
                IRuntimePanel.uIElementsRuntimeUtility?.UnregisterEventSystem(m_EventSystem);
                m_PendingRegistration = false;
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

        // Use object storage to prevent generic type parameter from preserving BaseRuntimePanel during code stripping
        private List<IRuntimePanel> m_PanelsToRemove = new();
        private void UpdatePanelGameObjects()
        {
            if (!m_IsTrackingPanels) return;

            bool isWorldSpaceActive = false;
            foreach (var panel in trackedPanels)
            {
                if (panel.disposed)
                {
                    m_PanelsToRemove.Add(panel);
                    continue;
                }

                UpdatePanelGameObject(panel);
                isWorldSpaceActive |= !panel.isFlat;
            }

            foreach (var panelObj in m_PanelsToRemove)
            {
                trackedPanels.Remove(panelObj);
            }
            m_PanelsToRemove.Clear();

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
