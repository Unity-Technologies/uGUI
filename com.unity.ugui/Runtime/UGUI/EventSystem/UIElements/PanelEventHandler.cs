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
    public class PanelEventHandler : UIBehaviour, IPointerMoveHandler, IPointerUpHandler, IPointerDownHandler,
        ISubmitHandler, ICancelHandler, IMoveHandler, IScrollHandler, ISelectHandler, IDeselectHandler,
        IPointerExitHandler, IPointerEnterHandler, IRuntimePanelComponent, IPointerClickHandler
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

        private GameObject selectableGameObject => m_Panel?.selectableGameObject;
        private EventSystem eventSystem => UIElementsRuntimeUtility.activeEventSystem as EventSystem;

        private bool isCurrentFocusedPanel => m_Panel != null && eventSystem != null &&
                                              eventSystem.currentSelectedGameObject == selectableGameObject;

        private Focusable currentFocusedElement => m_Panel?.focusController.GetLeafFocusedElement();

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
                m_Panel.visualTree.RegisterCallback<FocusEvent>(OnElementFocus, TrickleDown.TrickleDown);
                m_Panel.visualTree.RegisterCallback<BlurEvent>(OnElementBlur, TrickleDown.TrickleDown);
            }
        }

        void UnregisterCallbacks()
        {
            if (m_Panel != null)
            {
                m_Panel.destroyed -= OnPanelDestroyed;
                m_Panel.visualTree.UnregisterCallback<FocusEvent>(OnElementFocus, TrickleDown.TrickleDown);
                m_Panel.visualTree.UnregisterCallback<BlurEvent>(OnElementBlur, TrickleDown.TrickleDown);
            }
        }

        void OnPanelDestroyed()
        {
            panel = null;
        }

        void OnElementFocus(FocusEvent e)
        {
            if (!m_Selecting && eventSystem != null)
                eventSystem.SetSelectedGameObject(selectableGameObject);
        }

        void OnElementBlur(BlurEvent e)
        {
            // Important: if panel discards focus entirely, it doesn't discard EventSystem selection necessarily.
            // Also note that if we arrive here through eventSystem.SetSelectedGameObject -> OnDeselect,
            // eventSystem.currentSelectedGameObject will still have its old value and we can't reaffect it immediately.
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

            using (var e = PointerMoveEvent.GetPooled(m_PointerEvent))
            {
                UpdatePointerEventTarget(e, m_PointerEvent);
                SendEvent(e, eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!ReadPointerData(m_PointerEvent, eventData, PointerEventType.Up))
                return;

            using (var e = PointerUpEvent.GetPooled(m_PointerEvent))
            {
                UpdatePointerEventTarget(e, m_PointerEvent);
                SendEvent(e, eventData);

                if (e.pressedButtons == 0)
                    PointerDeviceState.SetElementWithSoftPointerCapture(e.pointerId, null, null);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!ReadPointerData(m_PointerEvent, eventData, PointerEventType.Down))
                return;

            if (eventSystem != null)
                eventSystem.SetSelectedGameObject(selectableGameObject);

            using (var e = PointerDownEvent.GetPooled(m_PointerEvent))
            {
                UpdatePointerEventTarget(e, m_PointerEvent);
                SendEvent(e, eventData);

                PointerDeviceState.SetElementWithSoftPointerCapture(e.pointerId, e.elementTarget, eventData.pressEventCamera);
            }
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
                using (var e = PointerCancelEvent.GetPooled(m_PointerEvent))
                {
                    UpdatePointerEventTarget(e, m_PointerEvent);
                    SendEvent(e, eventData);

                    if (e.pressedButtons == 0)
                        PointerDeviceState.SetElementWithSoftPointerCapture(e.pointerId, null, null);
                }
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
            var target = currentFocusedElement ?? m_Panel.visualTree;
            ProcessImguiEvents(target);

            using (var e = NavigationSubmitEvent.GetPooled(GetDeviceType(eventData), s_Modifiers))
            {
                e.target = target;
                SendEvent(e, eventData);
            }
        }

        public void OnCancel(BaseEventData eventData)
        {
            if (m_Panel == null)
                return;

            // Allow KeyDown/KeyUp events to be processed before navigation events.
            var target = currentFocusedElement ?? m_Panel.visualTree;
            ProcessImguiEvents(target);

            using (var e = NavigationCancelEvent.GetPooled(GetDeviceType(eventData), s_Modifiers))
            {
                e.target = target;
                SendEvent(e, eventData);
            }
        }

        public void OnMove(AxisEventData eventData)
        {
            if (m_Panel == null)
                return;

            // Allow KeyDown/KeyUp events to be processed before navigation events.
            var target = currentFocusedElement ?? m_Panel.visualTree;
            ProcessImguiEvents(target);

            using (var e = NavigationMoveEvent.GetPooled(eventData.moveVector, GetDeviceType(eventData), s_Modifiers))
            {
                e.target = target;
                SendEvent(e, eventData);
            }

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

            using (var e = WheelEvent.GetPooled(uitkScrollDelta, m_PointerEvent))
            {
                SendEvent(e, eventData);
            }
        }

        private void SendEvent(EventBase e, BaseEventData sourceEventData)
        {
            //e.runtimeEventData = sourceEventData;
            m_Panel.SendEvent(e);
            if (e.isPropagationStopped)
                sourceEventData.Use();
        }

        private void SendEvent(EventBase e, Event sourceEvent)
        {
            m_Panel.SendEvent(e);

            // Don't call sourceEvent.Use() because DefaultEventSystem doesn't call it either
            // and we want to have the same behavior as much as possible.
            // See UGUIEventSystemTests.KeyDownStoppedDoesntPreventNavigationEvents for a test requires this.
        }

        /// <summary>
        /// This method is automatically called on every frame.
        /// It can also be called manually to force some queued events to be processed.
        /// </summary>
        public void Update()
        {
            if (isCurrentFocusedPanel)
                ProcessImguiEvents(currentFocusedElement ?? m_Panel.visualTree);

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
        void ProcessImguiEvents(Focusable target)
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

        void ProcessKeyboardEvent(Event e, Focusable target)
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
        void ProcessTabEvent(Event e, Focusable target)
        {
            if (e.ShouldSendNavigationMoveEventRuntime())
            {
                SendTabEvent(e, e.shift ? NavigationMoveEvent.Direction.Previous : NavigationMoveEvent.Direction.Next, target);
            }
        }

        private void SendTabEvent(Event e, NavigationMoveEvent.Direction direction, Focusable target)
        {
            using (var ev = NavigationMoveEvent.GetPooled(direction, s_Modifiers))
            {
                ev.target = target;
                SendEvent(ev, e);
            }
        }

        private void SendKeyUpEvent(Event e, Focusable target)
        {
            // Use UIElementsRuntimeUtility.CreateEvent because DefaultEventSystem uses it too
            // and we want to have the same behavior as much as possible.
            using (var ev = (KeyUpEvent) UIElementsRuntimeUtility.CreateEvent(e))
            {
                ev.target = target;
                SendEvent(ev, e);
            }
        }

        private void SendKeyDownEvent(Event e, Focusable target)
        {
            // Use UIElementsRuntimeUtility.CreateEvent because DefaultEventSystem uses it too
            // and we want to have the same behavior as much as possible.
            using (var ev = (KeyDownEvent) UIElementsRuntimeUtility.CreateEvent(e))
            {
                ev.target = target;
                SendEvent(ev, e);
            }
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

        private void UpdatePointerEventTarget<TPointerEvent>(TPointerEvent e, PointerEvent eventData)
            where TPointerEvent : PointerEventBase<TPointerEvent>, new()
        {
            e.target = eventData.elementTarget;

            if (!m_Panel.isFlat)
            {
                // World-space panels set their top element manually instead of using RecomputeElementUnderPointer.
                m_Panel.SetTopElementUnderPointer(eventData.pointerId, eventData.elementUnderPointer, e);
            }
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

                m_Panel.SetTopElementUnderPointer(m_PointerEvent.pointerId, m_PointerEvent.elementUnderPointer, m_PointerEvent.position);
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
            public UIDocument document { get; private set; }
            public VisualElement elementTarget { get; private set; }
            public VisualElement elementUnderPointer { get; private set; }

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

                if (eventDisplayIndex > 0 && eventDisplayIndex < Display.displays.Length)
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
                        if (Time.unscaledTime > self.m_LastClickTime + ClickDetector.s_DoubleClickTime * 0.001f)
                            clickCount = 0;

                        // Case 1379054: UIToolkit assumes clickCount is increased before PointerDown, but UGUI does it after.
                        clickCount++;

                        PointerDeviceState.PressButton(pointerId, button);
                    }
                    else if (eventType == PointerEventType.Up)
                    {
                        PointerDeviceState.ReleaseButton(pointerId, button);
                    }

                    clickCount = Mathf.Max(1, clickCount);
                }

                pressedButtons = PointerDeviceState.GetPressedButtons(pointerId);

                var origin = eventData.pointerCurrentRaycast.origin;
                worldRay = new Ray(origin, eventData.pointerCurrentRaycast.worldPosition - origin);
                document = eventData.pointerCurrentRaycast.document;
                elementUnderPointer = eventData.pointerCurrentRaycast.element;
            }

            public bool ComputeTarget(BaseRuntimePanel panel)
            {
                Vector3 panelPosition;
                if (panel.isFlat)
                {
                    // PointerEvents making it this far have been validated by PanelRaycaster already
                    panel.ScreenToPanel(screenPosition, screenDelta,
                        out panelPosition, allowOutside:true);
                    elementTarget = null;
                }
                else
                {
                    if (document == null)
                        return false;

                    var capturingElement = RuntimePanel.s_EventDispatcher.pointerState.GetCapturingElement(pointerId) as VisualElement;
                    if (capturingElement != null && capturingElement.panel != panel)
                        return false;

                    elementTarget = capturingElement ?? elementUnderPointer ?? document.rootVisualElement;
                    panelPosition = GetPanelPosition(elementTarget, document, worldRay);
                }

                localPosition = position = panelPosition;
                deltaPosition = PointerDeviceState.GetPointerDeltaPosition(pointerId, ContextType.Player, position);
                return true;
            }

            Vector3 GetPanelPosition(VisualElement pickedElement, UIDocument document, Ray worldRay)
            {
                var documentRay = document.transform.worldToLocalMatrix.TransformRay(worldRay);
                pickedElement.IntersectWorldRay(documentRay, out var distanceWithinDocument, out _);
                var documentPoint = documentRay.origin + documentRay.direction * distanceWithinDocument;
                return documentPoint;
            }
        }
    }
#endif
}
