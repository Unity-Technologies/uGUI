namespace UnityEngine.EventSystems
{
    public class AxisEventData : BaseEventData
    {
        public Vector2 moveVector { get; set; }
        public MoveDirection moveDir { get; set; }

        public AxisEventData(EventSystem eventSystem)
            : base(eventSystem)
        {
            moveVector = Vector2.zero;
            moveDir = MoveDirection.None;
        }
    }
}
