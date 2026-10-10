namespace UnityEngine.EventSystems
{
    /// <summary>
    /// Event Data associated with Axis Events (Controller / Keyboard).
    /// </summary>
    public class AxisEventData : BaseEventData
    {
        /// <summary>
        /// Raw input vector associated with this event.
        /// </summary>
        public Vector2 moveVector { get; set; }

        /// <summary>
        /// MoveDirection for this event.
        /// </summary>
        public MoveDirection moveDir { get; set; }

        /// <inheritdoc cref="BaseEventData(EventSystem)"/>
        /// <summary>
        /// Creates a new <see cref="AxisEventData"/> object with a zero move vector and no move direction.
        /// </summary>
        public AxisEventData(EventSystem eventSystem)
            : base(eventSystem)
        {
            moveVector = Vector2.zero;
            moveDir = MoveDirection.None;
        }
    }
}
