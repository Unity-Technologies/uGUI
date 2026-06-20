using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UnityEngine.UIElements
{
    // This code is disabled unless the com.unity.modules.uielements module is present.
    // The UIElements module is always present in the Editor but it can be stripped from a project build if unused.
#if PACKAGE_UITOOLKIT
    /// <summary>
    /// Use this class to handle input and send events to UI Toolkit runtime panels.
    /// </summary>
    [AddComponentMenu("UI Toolkit/Panel Event Handler (UI Toolkit)")]
    [UGUIHelpURL("PanelEventHandler")]
    public class PanelEventHandler : UIBehaviour, IPointerMoveHandler, IPointerUpHandler, IPointerDownHandler,
        ISubmitHandler, ICancelHandler, IMoveHandler, IScrollHandler, ISelectHandler, IDeselectHandler,
        IPointerExitHandler, IPointerEnterHandler, IRuntimePanelComponent, IPointerClickHandler
    {
        private IRuntimePanel m_Panel;

        /// <summary>
        /// The panel that this component relates to. If panel is null, this component will have no effect.
        /// Will be set to null automatically if panel is Disposed from an external source.
        /// </summary>
        public IPanel panel
        {
            get => m_Panel;
            set
            {
                var newPanel = (IRuntimePanel)value;
                if (m_Panel != newPanel)
                {
                    UnregisterCallbacks();
                    m_Panel = newPanel;
                    RegisterCallbacks();
                }
            }
        }

        private GameObject selectableGameObject => m_Panel?.selectableGameObject;
        // Can be null if runtime panels have not been created (e.g., if UI Toolkit is stripped or not in use)
        private EventSystem eventSystem => IRuntimePanel.uIElementsRuntimeUtility?.activeEventSystem as EventSystem;

        private bool isCurrentFocusedPanel => m_Panel != null && eventSystem != null &&
                                              eventSystem.currentSelectedGameObject == selectableGameObject;

        private IEventHandler currentFocusedElement => m_Panel?.GetLeafFocusedElement();

        private readonly PointerEvent m_PointerEvent = new PointerEvent();
        private readonly List<PointerEventData> m_ContainedPointers = new();

        private float m_LastClickTime = 0;

        protected override void OnEnable()
        {
            base.OnEnable();
            RegisterCallbacks();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UnregisterCallbacks();
        }

        void RegisterCallbacks()
        {
            if (m_Panel != null)
            {
                m_Panel.destroyed += OnPanelDestroyed;
                m_Panel.RegisterRootFocusCallback(OnElementFocus);
            }
        }

        void UnregisterCallbacks()
        {
            if (m_Panel != null)
            {
                m_Panel.destroyed -= OnPanelDestroyed;
                m_Panel.UnregisterRootFocusCallback(OnElementFocus);
            }
        }

        void OnPanelDestroyed()
        {
            panel = null;
        }

        void OnElementFocus()
        {
            if (!m_Selecting && eventSystem != null)
                eventSystem.SetSelectedGameObject(selectableGameObject);
        }

        private bool m_Selecting;
        public void OnSelect(BaseEventData eventData)
        {
            m_Selecting = true;
            try
            {
                // This shouldn't conflict with EditorWindow calling Panel.Focus (only on Editor-type panels).
                m_Panel?.Focus();
            }
            finally
            {
                m_Selecting = false;
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            m_Panel?.Blur();
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (!ReadPointerData(m_PointerEvent, eventData))
                return;

            var handled = m_Panel.SendPointerMoveEvent(
                m_PointerEvent,
                m_PointerEvent.m_elementTarget,
                m_PointerEvent.m_elementUnderPointer);

            if (handled)
                eventData.Use();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!ReadPointerData(m_PointerEvent, eventData, PointerEventType.Up))
                return;

            var handled = m_Panel.SendPointerUpEvent(
                m_PointerEvent,
                m_PointerEvent.m_elementTarget,
                m_PointerEvent.m_elementUnderPointer);

            if (handled)
                eventData.Use();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!ReadPointerData(m_PointerEvent, eventData, PointerEventType.Down))
                return;

            // Allow KeyDown/KeyUp events to be processed before pointer events.
            var target = currentFocusedElement ?? m_Panel.visualTree_as_IEventHandler;
            ProcessImguiEvents(target);

            if (eventSystem != null)
                eventSystem.SetSelectedGameObject(selectableGameObject);

            var handled = m_Panel.SendPointerDownEvent(
                m_PointerEvent,
                m_PointerEvent.m_elementTarget,
                m_PointerEvent.m_elementUnderPointer,
                eventData.pressEventCamera);

            if (handled)
                eventData.Use();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_ContainedPointers.Remove(eventData);

            if (!ReadPointerData(m_PointerEvent, eventData))
            {
                if (m_Panel != null && !m_Panel.isFlat)
                    m_Panel.PointerLeavesPanel(m_PointerEvent.pointerId);
                return;
            }

            // If a pointer exit is called while the pointer is still on top of this object, it means
            // there's something else removing the pointer, so we might need to send a PointerCancelEvent.
            // This is necessary for touch pointers that are being released, because in UGUI the object
            // that was last hovered will not always be the one receiving the pointer up.
            if (eventData.pointerCurrentRaycast.gameObject == gameObject &&
                eventData.pointerPressRaycast.gameObject != gameObject &&
                m_PointerEvent.pointerId != PointerId.mousePointerId)
            {
                var handled = m_Panel.SendPointerCancelEvent(
                    m_PointerEvent,
                    m_PointerEvent.m_elementTarget,
                    m_PointerEvent.m_elementUnderPointer);

                if (handled)
                    eventData.Use();
            }

            m_Panel.PointerLeavesPanel(m_PointerEvent.pointerId);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!ReadPointerData(m_PointerEvent, eventData))
                return;

            m_ContainedPointers.Add(eventData);
            m_Panel.PointerEntersPanel(m_PointerEvent.pointerId, m_PointerEvent.position);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            m_LastClickTime = Time.unscaledTime;
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (m_Panel == null)
                return;

            // Allow KeyDown/KeyUp events to be processed before navigation events.
            var target = currentFocusedElement ?? m_Panel.visualTree_as_IEventHandler;
            ProcessImguiEvents(target);

            var handled = m_Panel.SendNavigationEvent(
                NavigationEventType.Submit,
                target,
                GetDeviceType(eventData),
                s_Modifiers);

            if (handled)
                eventData.Use();
        }

        public void OnCancel(BaseEventData eventData)
        {
            if (m_Panel == null)
                return;

            // Allow KeyDown/KeyUp events to be processed before navigation events.
            var target = currentFocusedElement ?? m_Panel.visualTree_as_IEventHandler;
            ProcessImguiEvents(target);

            var handled = m_Panel.SendNavigationEvent(
                NavigationEventType.Cancel,
                target,
                GetDeviceType(eventData),
                s_Modifiers);

            if (handled)
                eventData.Use();
        }

        public void OnMove(AxisEventData eventData)
        {
            if (m_Panel == null)
                return;

            // Allow KeyDown/KeyUp events to be processed before navigation events.
            var target = currentFocusedElement ?? m_Panel.visualTree_as_IEventHandler;
            ProcessImguiEvents(target);

            var handled = m_Panel.SendNavigationEvent(
                NavigationEventType.Move,
                target,
                GetDeviceType(eventData),
                s_Modifiers,
                eventData.moveVector);

            if (handled)
                eventData.Use();

            // TODO: if runtime panel has no internal navigation, switch to the next UGUI selectable element.
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (!ReadPointerData(m_PointerEvent, eventData))
                return;

            var uguiScrollDelta = eventData.scrollDelta;
            var scrollTicks = eventSystem.currentInputModule.ConvertPointerEventScrollDeltaToTicks(uguiScrollDelta);

            // ISXB-808: Scale scrollDelta to match the UIToolkit convention.
            var uitkScrollDelta = scrollTicks * WheelEvent.scrollDeltaPerTick;
            uitkScrollDelta.y = -uitkScrollDelta.y;

            var handled = m_Panel.SendWheelEvent(
                uitkScrollDelta,
                m_PointerEvent);

            if (handled)
                eventData.Use();
        }


        /// <summary>
        /// This method is automatically called on every frame.
        /// It can also be called manually to force some queued events to be processed.
        /// </summary>
        public void Update()
        {
            if (isCurrentFocusedPanel)
                ProcessImguiEvents(currentFocusedElement ?? m_Panel.visualTree_as_IEventHandler);

            UpdateWorldSpacePointers();
        }

        void LateUpdate()
        {
            // Empty the Event queue, look for EventModifiers.
            ProcessImguiEvents(null);
        }

        private Event m_Event = new Event();
        private static EventModifiers s_Modifiers = EventModifiers.None;

        // Send IMGUI events to given focus-based target, if any, or simply flush the event queue if not.
        // For uniformity of composite events (keyDown vs navigation), target should remain the same
        // throughout the entire processing cycle.
        void ProcessImguiEvents(IEventHandler target)
        {
            bool first = true;

            while (Event.PopEvent(m_Event))
            {
                if (m_Event.type == EventType.Ignore || m_Event.type == EventType.Repaint ||
                    m_Event.type == EventType.Layout)
                    continue;

                s_Modifiers = first ? m_Event.modifiers : (s_Modifiers | m_Event.modifiers);
                first = false;

                if (target != null)
                {
                    ProcessKeyboardEvent(m_Event, target);
                    if (eventSystem.sendNavigationEvents)
                        ProcessTabEvent(m_Event, target);
                }
            }
        }

        void ProcessKeyboardEvent(Event e, IEventHandler target)
        {
            if (e.type == EventType.KeyUp)
            {
                SendKeyUpEvent(e, target);
            }
            else if (e.type == EventType.KeyDown)
            {
                SendKeyDownEvent(e, target);
            }
        }

        // TODO: add an ITabHandler interface
        void ProcessTabEvent(Event e, IEventHandler target)
        {
            if (e.ShouldSendNavigationMoveEventRuntime())
            {
                SendTabEvent(e, e.shift ? NavigationMoveDirection.Previous : NavigationMoveDirection.Next, target);
            }
        }

        private void SendTabEvent(Event e, NavigationMoveDirection direction, IEventHandler target)
        {
            m_Panel.SendNavigationEvent(
                NavigationEventType.Move,
                target,
                NavigationDeviceType.Keyboard,
                s_Modifiers,
                default,
                direction);

            // Note: we don't call e.Use() here because DefaultEventSystem doesn't either
        }

        private void SendKeyUpEvent(Event e, IEventHandler target)
        {
            m_Panel.SendKeyboardEvent(
                isKeyDown: false,
                e.character,
                e.keyCode,
                s_Modifiers,
                target);

            // Don't call e.Use() because DefaultEventSystem doesn't either
        }

        private void SendKeyDownEvent(Event e, IEventHandler target)
        {
            m_Panel.SendKeyboardEvent(
                isKeyDown: true,
                e.character,
                e.keyCode,
                s_Modifiers,
                target);

            // Don't call e.Use() because DefaultEventSystem doesn't either
        }

        private bool ReadPointerData(PointerEvent pe, PointerEventData eventData, PointerEventType eventType = PointerEventType.Default)
        {
            if (m_Panel == null || eventSystem == null || eventSystem.currentInputModule == null)
                return false;

            pe.Read(this, eventData, eventType);

            if (!pe.ComputeTarget(m_Panel))
                return false;

            return true;
        }


        private UIElements.NavigationDeviceType GetDeviceType(BaseEventData eventData)
        {
            if (eventSystem == null || eventSystem.currentInputModule == null)
                return NavigationDeviceType.Unknown;
            return (UIElements.NavigationDeviceType)eventSystem.currentInputModule.GetNavigationEventDeviceType(
                eventData);
        }

        private void UpdateWorldSpacePointers()
        {
            if (m_Panel == null || m_Panel.isFlat || eventSystem == null || eventSystem.currentInputModule == null)
                return;

            foreach (var eventData in m_ContainedPointers)
            {
                if (!ReadPointerData(m_PointerEvent, eventData))
                    continue;

                //Cannot use m_PointerEvent.elementUnderPointer as it would keep the VisualElement type alive for code stripping.
                m_Panel.SetTopElementUnderPointer(m_PointerEvent.pointerId, m_PointerEvent.m_elementUnderPointer, m_PointerEvent.position);

                m_Panel.CommitElementUnderPointers();
            }
        }

        enum PointerEventType
        {
            Default, Down, Up
        }

        class PointerEvent : IPointerEvent
        {

            public int pointerId { get; private set; }
            public string pointerType { get; private set; }
            public bool isPrimary { get; private set; }
            public int button { get; private set; }
            public int pressedButtons { get; private set; }
            public Vector3 position { get; private set; }
            public Vector3 localPosition { get; private set; }
            public Vector3 deltaPosition { get; private set; }
            public float deltaTime { get; private set; }
            public int clickCount { get; private set; }
            public float pressure { get; private set; }
            public float tangentialPressure { get; private set; }
            public float altitudeAngle { get; private set; }
            public float azimuthAngle { get; private set; }
            public float twist { get; private set; }
            public Vector2 tilt { get; private set; }
            public PenStatus penStatus { get; private set; }
            public Vector2 radius { get; private set; }
            public Vector2 radiusVariance { get; private set; }
            public EventModifiers modifiers { get; private set; }

            public bool shiftKey => (modifiers & EventModifiers.Shift) != 0;
            public bool ctrlKey => (modifiers & EventModifiers.Control) != 0;
            public bool commandKey => (modifiers & EventModifiers.Command) != 0;
            public bool altKey => (modifiers & EventModifiers.Alt) != 0;

            public bool actionKey =>
                Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer
                ? commandKey
                : ctrlKey;

            public Vector3 screenPosition { get; private set; }
            public Vector3 screenDelta { get; private set; }
            public Ray worldRay { get; private set; }
            public IPanelComponent panelComponent { get; private set; }

            //keep reference as object for code stripping the VisualElement type
            internal IEventHandler m_elementTarget;
            public VisualElement elementTarget { get => (VisualElement)m_elementTarget; }

            internal IEventHandler m_elementUnderPointer;
            public VisualElement elementUnderPointer { get => (VisualElement)m_elementUnderPointer; }

            public void Read(PanelEventHandler self, PointerEventData eventData, PointerEventType eventType)
            {
                pointerId = self.eventSystem.currentInputModule.ConvertUIToolkitPointerId(eventData);

                bool InRange(int i, int start, int count) => i >= start && i < start + count;

                pointerType =
                    InRange(pointerId, PointerId.touchPointerIdBase, PointerId.touchPointerCount) ? PointerType.touch :
                    InRange(pointerId, PointerId.penPointerIdBase, PointerId.penPointerCount) ? PointerType.pen :
                    PointerType.mouse;

                isPrimary = pointerId == PointerId.mousePointerId ||
                    pointerId == PointerId.touchPointerIdBase ||
                    pointerId == PointerId.penPointerIdBase;

                // Flip Y axis between input and UITK
                var h = Screen.height;

                Vector3 eventPosition = MultipleDisplayUtilities.GetRelativeMousePositionForRaycast(eventData);
                int eventDisplayIndex = (int)eventPosition.z;

                if (UnityEngineInternal.DisplayInternal.IsASecondaryDisplayIndex(eventDisplayIndex))
                {
#if UNITY_ANDROID
                    // Changed for UITK to be coherent for Android which passes display-relative rendering coordinates
                    h = Display.displays[eventDisplayIndex].renderingHeight;
#else
                    h = Display.displays[eventDisplayIndex].systemHeight;
#endif
                }

                var delta = eventData.delta;
                eventPosition.y = h - eventPosition.y;
                delta.y = -delta.y;

                screenPosition = eventPosition;
                screenDelta = delta;

                deltaTime = 0; //TODO: find out what's expected here. Time since last frame? Since last sent event?
                pressure = eventData.pressure;
                tangentialPressure = eventData.tangentialPressure;
                altitudeAngle = eventData.altitudeAngle;
                azimuthAngle = eventData.azimuthAngle;
                twist = eventData.twist;
                tilt = eventData.tilt;
                penStatus = eventData.penStatus;
                radius = eventData.radius;
                radiusVariance = eventData.radiusVariance;

                modifiers = s_Modifiers;

                if (eventType == PointerEventType.Default)
                {
                    button = -1;
                    clickCount = 0;
                }
                else
                {
                    button = Mathf.Max(0, (int)eventData.button);
                    clickCount = eventData.clickCount;

                    if (eventType == PointerEventType.Down)
                    {
                        // UUM-57082: InputSystem doesn't reset clickCount on delay until after it sends PointerDown
                        // This is not perfect but it's the best we can do with incomplete information.
                        // Can be null if runtime panels have not been created or UI Toolkit is stripped
                        var doubleClickTime = IRuntimePanel.uIElementsRuntimeUtility?.s_DoubleClickTime ?? 300;
                        if (Time.unscaledTime > self.m_LastClickTime + doubleClickTime * 0.001f)
                            clickCount = 0;

                        // Case 1379054: UIToolkit assumes clickCount is increased before PointerDown, but UGUI does it after.
                        clickCount++;
                        // Can be null if runtime panels have not been created or UI Toolkit is stripped
                        IRuntimePanel.pointerDeviceState?.PressButton(pointerId, button);
                    }
                    else if (eventType == PointerEventType.Up)
                    {
                        // Can be null if runtime panels have not been created or UI Toolkit is stripped
                        IRuntimePanel.pointerDeviceState?.ReleaseButton(pointerId, button);
                    }

                    clickCount = Mathf.Max(1, clickCount);
                }

                // Can be null if runtime panels have not been created or UI Toolkit is stripped; default to 0 (no buttons pressed)
                pressedButtons = IRuntimePanel.pointerDeviceState?.GetPressedButtons(pointerId) ?? 0;

                var origin = eventData.pointerCurrentRaycast.origin;
                worldRay = new Ray(origin, eventData.pointerCurrentRaycast.worldPosition - origin);
                panelComponent = eventData.pointerCurrentRaycast.panelComponent;
                m_elementUnderPointer = eventData.pointerCurrentRaycast.m_element;
            }

            public bool ComputeTarget(IRuntimePanel panel)
            {
                Vector3 panelPosition;
                if (panel.isFlat)
                {
                    // PointerEvents making it this far have been validated by PanelRaycaster already
                    panel.ScreenToPanel(screenPosition, screenDelta,
                        out panelPosition, allowOutside:true);
                    m_elementTarget = null;
                }
                else
                {
                    if (panelComponent == null)
                        return false;

                    // Can be null if runtime panels have not been created or UI Toolkit is stripped; default to no capturing element
                    IRuntimePanel capturingPanel = null;
                    IEventHandler capturingElement = null;
                    IRuntimePanel.uIElementsRuntimeUtility?.GetCapturingElement(pointerId, out capturingPanel, out capturingElement);
                    if (capturingElement != null && capturingPanel != panel)
                        return false;

                    m_elementTarget = capturingElement ?? m_elementUnderPointer ?? panelComponent.GetRoot();
                    panelPosition = panelComponent.GetPanelPosition(m_elementTarget , worldRay);
                }

                localPosition = position = panelPosition;
                // Can be null if runtime panels have not been created or UI Toolkit is stripped; default to Vector3.zero
                deltaPosition = IRuntimePanel.pointerDeviceState?.GetPointerDeltaPosition(pointerId, ContextType.Player, position) ?? Vector3.zero;
                return true;
            }

        }
    }
#endif
}
