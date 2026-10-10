namespace UnityEngine.EventSystems
{
    /// <summary>
    /// A class that can be used for sending simple events via the event system.
    /// </summary>
    public abstract class AbstractEventData
    {
        /// <summary>
        /// Whether the event has been used.
        /// </summary>
        protected bool m_Used;

        /// <summary>
        /// Reset the event.
        /// </summary>
        public virtual void Reset()
        {
            m_Used = false;
        }

        /// <summary>
        /// Use the event.
        /// </summary>
        /// <remarks>
        /// Internally sets a flag that can be checked via used to see if further processing should happen.
        /// </remarks>
        public virtual void Use()
        {
            m_Used = true;
        }

        /// <summary>
        /// Is the event used?
        /// </summary>
        public virtual bool used
        {
            get { return m_Used; }
        }
    }

    /// <summary>
    /// A class that contains the base event data that is common to all event types in the new EventSystem.
    /// </summary>
    public class BaseEventData : AbstractEventData
    {
        private readonly EventSystem m_EventSystem;

        /// <summary>
        /// Creates a new <see cref="BaseEventData"/> object bound to the given <see cref="EventSystem"/>.
        /// </summary>
        /// <param name="eventSystem">The <see cref="EventSystem"/> that sends this event.</param>
        public BaseEventData(EventSystem eventSystem)
        {
            m_EventSystem = eventSystem;
        }

        /// <summary>
        /// A reference to the <see cref="BaseInputModule"/> that sent this event.
        /// </summary>
        public BaseInputModule currentInputModule
        {
            get { return m_EventSystem.currentInputModule; }
        }

        /// <summary>
        /// The <see cref="GameObject"/> currently considered selected by the <see cref="EventSystem"/>.
        /// </summary>
        public GameObject selectedObject
        {
            get { return m_EventSystem.currentSelectedGameObject; }
            set { m_EventSystem.SetSelectedGameObject(value, this); }
        }
    }
}
