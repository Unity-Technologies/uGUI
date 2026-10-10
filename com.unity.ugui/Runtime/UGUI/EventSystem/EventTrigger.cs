using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace UnityEngine.EventSystems
{
    /// <summary>
    /// Receives events from the EventSystem and calls registered functions for each event.
    /// </summary>
    /// <remarks>
    /// The EventTrigger can be used to specify functions you wish to be called for each EventSystem event.
    /// You can assign multiple functions to a single event and whenever the EventTrigger receives that event it will call those functions in the order they were provided.
    ///
    /// NOTE: Attaching this component to a GameObject will make that object intercept ALL events, and no events will propagate to parent objects.
    /// </remarks>
    /// <example>
    /// <para>There are two ways to intercept events: You can extend EventTrigger, and override the functions for the events you want to intercept, as shown in the following example:</para>
    /// <code>
    /// <![CDATA[
    /// using UnityEngine;
    /// using UnityEngine.EventSystems;
    ///
    /// public class EventTriggerExample : EventTrigger
    /// {
    ///     public override void OnBeginDrag(PointerEventData data)
    ///     {
    ///         Debug.Log("OnBeginDrag called.");
    ///     }
    ///
    ///     public override void OnCancel(BaseEventData data)
    ///     {
    ///         Debug.Log("OnCancel called.");
    ///     }
    ///
    ///     public override void OnDeselect(BaseEventData data)
    ///     {
    ///         Debug.Log("OnDeselect called.");
    ///     }
    ///
    ///     public override void OnDrag(PointerEventData data)
    ///     {
    ///         Debug.Log("OnDrag called.");
    ///     }
    ///
    ///     public override void OnDrop(PointerEventData data)
    ///     {
    ///         Debug.Log("OnDrop called.");
    ///     }
    ///
    ///     public override void OnEndDrag(PointerEventData data)
    ///     {
    ///         Debug.Log("OnEndDrag called.");
    ///     }
    ///
    ///     public override void OnInitializePotentialDrag(PointerEventData data)
    ///     {
    ///         Debug.Log("OnInitializePotentialDrag called.");
    ///     }
    ///
    ///     public override void OnMove(AxisEventData data)
    ///     {
    ///         Debug.Log("OnMove called.");
    ///     }
    ///
    ///     public override void OnPointerClick(PointerEventData data)
    ///     {
    ///         Debug.Log("OnPointerClick called.");
    ///     }
    ///
    ///     public override void OnPointerDown(PointerEventData data)
    ///     {
    ///         Debug.Log("OnPointerDown called.");
    ///     }
    ///
    ///     public override void OnPointerEnter(PointerEventData data)
    ///     {
    ///         Debug.Log("OnPointerEnter called.");
    ///     }
    ///
    ///     public override void OnPointerExit(PointerEventData data)
    ///     {
    ///         Debug.Log("OnPointerExit called.");
    ///     }
    ///
    ///     public override void OnPointerUp(PointerEventData data)
    ///     {
    ///         Debug.Log("OnPointerUp called.");
    ///     }
    ///
    ///     public override void OnScroll(PointerEventData data)
    ///     {
    ///         Debug.Log("OnScroll called.");
    ///     }
    ///
    ///     public override void OnSelect(BaseEventData data)
    ///     {
    ///         Debug.Log("OnSelect called.");
    ///     }
    ///
    ///     public override void OnSubmit(BaseEventData data)
    ///     {
    ///         Debug.Log("OnSubmit called.");
    ///     }
    ///
    ///     public override void OnUpdateSelected(BaseEventData data)
    ///     {
    ///         Debug.Log("OnUpdateSelected called.");
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// <para>or you can specify individual delegates:</para>
    /// <code>
    /// <![CDATA[
    /// using UnityEngine;
    /// using UnityEngine.EventSystems;
    ///
    /// public class EventTriggerDelegateExample : MonoBehaviour
    /// {
    ///     void Start()
    ///     {
    ///         EventTrigger trigger = GetComponent<EventTrigger>();
    ///         EventTrigger.Entry entry = new EventTrigger.Entry();
    ///         entry.eventID = EventTriggerType.PointerDown;
    ///         entry.callback.AddListener((data) => { OnPointerDownDelegate((PointerEventData)data); });
    ///         trigger.triggers.Add(entry);
    ///     }
    ///
    ///     public void OnPointerDownDelegate(PointerEventData data)
    ///     {
    ///         Debug.Log("OnPointerDownDelegate called.");
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    [AddComponentMenu("Event/Event Trigger")]
    [UGUIHelpURL("EventTrigger")]
    public class EventTrigger :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler,
        IInitializePotentialDragHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler,
        IScrollHandler,
        IUpdateSelectedHandler,
        ISelectHandler,
        IDeselectHandler,
        IMoveHandler,
        ISubmitHandler,
        ICancelHandler
    {
        /// <summary>
        /// UnityEvent class for Triggers.
        /// </summary>
        [Serializable]
        public class TriggerEvent : UnityEvent<BaseEventData>
        {}

        /// <summary>
        /// An Entry in the EventSystem delegates list.
        /// </summary>
        /// <remarks>
        /// It stores the callback and which event type should this callback be fired.
        /// </remarks>
        [Serializable]
        public class Entry
        {
            /// <summary>
            /// What type of event is the associated callback listening for.
            /// </summary>
            public EventTriggerType eventID = EventTriggerType.PointerClick;

            /// <summary>
            /// The desired TriggerEvent to be Invoked.
            /// </summary>
            public TriggerEvent callback = new TriggerEvent();
        }

        [FormerlySerializedAs("delegates")]
        [SerializeField]
        private List<Entry> m_Delegates;

        /// <summary>Protected default constructor. Use <see cref="GameObject.AddComponent{T}"/> to add an EventTrigger to a GameObject.</summary>
        protected EventTrigger()
        {}

        /// <summary>
        /// All the functions registered in this EventTrigger
        /// </summary>
        public List<Entry> triggers
        {
            get
            {
                if (m_Delegates == null)
                    m_Delegates = new List<Entry>();
                return m_Delegates;
            }
            set { m_Delegates = value; }
        }

        private void Execute(EventTriggerType id, BaseEventData eventData)
        {
            for (int i = 0; i < triggers.Count; ++i)
            {
                var ent = triggers[i];
                if (ent.eventID == id && ent.callback != null)
                    ent.callback.Invoke(eventData);
            }
        }

        /// <inheritdoc/>
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            Execute(EventTriggerType.PointerEnter, eventData);
        }

        /// <inheritdoc/>
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            Execute(EventTriggerType.PointerExit, eventData);
        }

        /// <inheritdoc/>
        public virtual void OnDrag(PointerEventData eventData)
        {
            Execute(EventTriggerType.Drag, eventData);
        }

        /// <inheritdoc/>
        public virtual void OnDrop(PointerEventData eventData)
        {
            Execute(EventTriggerType.Drop, eventData);
        }

        /// <inheritdoc/>
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            Execute(EventTriggerType.PointerDown, eventData);
        }

        /// <inheritdoc/>
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            Execute(EventTriggerType.PointerUp, eventData);
        }

        /// <inheritdoc/>
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            Execute(EventTriggerType.PointerClick, eventData);
        }

        /// <inheritdoc/>
        public virtual void OnSelect(BaseEventData eventData)
        {
            Execute(EventTriggerType.Select, eventData);
        }

        /// <inheritdoc/>
        public virtual void OnDeselect(BaseEventData eventData)
        {
            Execute(EventTriggerType.Deselect, eventData);
        }

        /// <inheritdoc/>
        public virtual void OnScroll(PointerEventData eventData)
        {
            Execute(EventTriggerType.Scroll, eventData);
        }

        /// <inheritdoc/>
        public virtual void OnMove(AxisEventData eventData)
        {
            Execute(EventTriggerType.Move, eventData);
        }

        /// <inheritdoc/>
        public virtual void OnUpdateSelected(BaseEventData eventData)
        {
            Execute(EventTriggerType.UpdateSelected, eventData);
        }

        /// <inheritdoc/>
        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            Execute(EventTriggerType.InitializePotentialDrag, eventData);
        }

        /// <inheritdoc/>
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            Execute(EventTriggerType.BeginDrag, eventData);
        }

        /// <inheritdoc/>
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            Execute(EventTriggerType.EndDrag, eventData);
        }

        /// <inheritdoc/>
        public virtual void OnSubmit(BaseEventData eventData)
        {
            Execute(EventTriggerType.Submit, eventData);
        }

        /// <inheritdoc/>
        public virtual void OnCancel(BaseEventData eventData)
        {
            Execute(EventTriggerType.Cancel, eventData);
        }
    }
}
