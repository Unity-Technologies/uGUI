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
        /// <summary>
        /// The event hasn't been used.
        /// </summary>
        Unused = 0,

        /// <summary>
        /// The event has been used.
        /// </summary>
        Used = 1
    }
}
