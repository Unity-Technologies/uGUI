using System;
using System.Text;
using System.Collections.Generic;

namespace UnityEngine.EventSystems
{
    /// <summary>
    /// Each touch event creates one of these containing all the relevant information.
    /// </summary>
    public class PointerEventData : BaseEventData
    {
        public enum InputButton
        {
            Left = 0,
            Right = 1,
            Middle = 2
        }

        public enum FramePressState
        {
            Pressed,
            Released,
            PressedAndReleased,
            NotChanged
        }

        public GameObject pointerEnter { get; set; }

        // The object that received OnPointerDown
        private GameObject m_PointerPress;
        // The object last received OnPointerDown
        public GameObject lastPress { get; private set; }
        // The object that the press happened on even if it can not handle the press event
        public GameObject rawPointerPress { get; set; }
        // The object that received OnDrag
        public GameObject pointerDrag { get; set; }

        public RaycastResult pointerCurrentRaycast { get; set; }
        public RaycastResult pointerPressRaycast { get; set; }

        public List<GameObject> hovered = new List<GameObject>();

        public bool eligibleForClick { get; set; }

        public int pointerId { get; set; }

        // Current position of the mouse or touch event
        public Vector2 position { get; set; }
        // Delta since last update
        public Vector2 delta { get; set; }
        // Position of the press event
        public Vector2 pressPosition { get; set; }
        // World-space position where a ray cast into the screen hits something
        [Obsolete("Use either pointerCurrentRaycast.worldPosition or pointerPressRaycast.worldPosition")]
        public Vector3 worldPosition { get; set; }
        // World-space normal where a ray cast into the screen hits something
        [Obsolete("Use either pointerCurrentRaycast.worldNormal or pointerPressRaycast.worldNormal")]
        public Vector3 worldNormal { get; set; }
        // The last time a click event was sent out (used for double-clicks)
        public float clickTime { get; set; }
        // Number of clicks in a row. 2 for a double-click for example.
        public int clickCount { get; set; }

        public Vector2 scrollDelta { get; set; }
        public bool useDragThreshold { get; set; }
        public bool dragging { get; set; }

        public InputButton button { get; set; }

        public PointerEventData(EventSystem eventSystem) : base(eventSystem)
        {
            eligibleForClick = false;

            pointerId = -1;
            position = Vector2.zero; // Current position of the mouse or touch event
            delta = Vector2.zero; // Delta since last update
            pressPosition = Vector2.zero; // Delta since the event started being tracked
            clickTime = 0.0f; // The last time a click event was sent out (used for double-clicks)
            clickCount = 0; // Number of clicks in a row. 2 for a double-click for example.

            scrollDelta = Vector2.zero;
            useDragThreshold = true;
            dragging = false;
            button = InputButton.Left;
        }

        public bool IsPointerMoving()
        {
            return delta.sqrMagnitude > 0.0f;
        }

        public bool IsScrolling()
        {
            return scrollDelta.sqrMagnitude > 0.0f;
        }

        public Camera enterEventCamera
        {
            get { return pointerCurrentRaycast.module == null ? null : pointerCurrentRaycast.module.eventCamera; }
        }

        public Camera pressEventCamera
        {
            get { return pointerPressRaycast.module == null ? null : pointerPressRaycast.module.eventCamera; }
        }

        public GameObject pointerPress
        {
            get { return m_PointerPress; }
            set
            {
                if (m_PointerPress == value)
                    return;

                lastPress = m_PointerPress;
                m_PointerPress = value;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<b>Position</b>: " + position);
            sb.AppendLine("<b>delta</b>: " + delta);
            sb.AppendLine("<b>eligibleForClick</b>: " + eligibleForClick);
            sb.AppendLine("<b>pointerEnter</b>: " + pointerEnter);
            sb.AppendLine("<b>pointerPress</b>: " + pointerPress);
            sb.AppendLine("<b>lastPointerPress</b>: " + lastPress);
            sb.AppendLine("<b>pointerDrag</b>: " + pointerDrag);
            sb.AppendLine("<b>Use Drag Threshold</b>: " + useDragThreshold);
            sb.AppendLine("<b>Current Rayast:</b>");
            sb.AppendLine(pointerCurrentRaycast.ToString());
            sb.AppendLine("<b>Press Rayast:</b>");
            sb.AppendLine(pointerPressRaycast.ToString());
            return sb.ToString();
        }
    }
}
