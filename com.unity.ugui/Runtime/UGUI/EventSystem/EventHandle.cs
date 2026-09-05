using System;

namespace UnityEngine.EventSystems
{
    /// <summary>
    /// Enum that tracks event State.
    /// </summary>
    [Flags]
    [Obsolete("EventHandle is obsolete and no longer used.")]
    public enum EventHandle
    {
        Unused = 0,
        Used = 1
    }
}
