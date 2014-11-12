namespace UnityEngine.EventSystems
{
    /// <summary>
    /// This class is capable of triggering one or more remote functions from a specified event.
    /// Usage: Attach it to an object with a collider, or to a GUI Graphic of your choice.
    /// NOTE: Doing this will make this object intercept ALL events, and no event bubbling will occur from this object!
    /// </summary>

    public enum EventTriggerType
    {
        PointerEnter,
        PointerExit,
        PointerDown,
        PointerUp,
        PointerClick,
        Drag,
        Drop,
        Scroll,
        UpdateSelected,
        Select,
        Deselect,
        Move,
        InitializePotentialDrag,
        BeginDrag,
        EndDrag,
        Sumbit,
        Cancel
    }
}
