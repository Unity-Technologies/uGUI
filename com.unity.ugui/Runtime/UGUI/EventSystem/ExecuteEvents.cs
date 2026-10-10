using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace UnityEngine.EventSystems
{
    /// <summary>
    /// Sends EventSystem events to the handlers implemented by a GameObject and its ancestors.
    /// </summary>
    /// <remarks>
    /// Each event interface in the EventSystems namespace has a matching EventFunction property on this class,
    /// such as `EventSystems.ExecuteEvents.pointerDownHandler` for `EventSystems.IPointerDownHandler`.
    /// Pass one of these properties to Execute or ExecuteHierarchy to invoke that event on a target GameObject.
    /// </remarks>
    public static class ExecuteEvents
    {
        /// <summary>
        /// Invokes a specific EventSystem event on a handler.
        /// </summary>
        /// <typeparam name="T1">The handler interface that the event is sent to.</typeparam>
        /// <param name="handler">The handler to invoke for the event.</param>
        /// <param name="eventData">The event data to pass to the handler.</param>
        public delegate void EventFunction<T1>(T1 handler, BaseEventData eventData);

        /// <summary>
        /// Casts a <see cref="BaseEventData"/> to the type that an event handler expects.
        /// </summary>
        /// <typeparam name="T">The event data type that the handler expects.</typeparam>
        /// <param name="data">The BaseEventData to cast.</param>
        /// <returns>The event data, cast to `T`.</returns>
        public static T ValidateEventData<T>(BaseEventData data) where T : class
        {
            if ((data as T) == null)
                throw new ArgumentException(String.Format("Invalid type: {0} passed to event expecting {1}", data.GetType(), typeof(T)));
            return data as T;
        }

        private static readonly EventFunction<IPointerMoveHandler> s_PointerMoveHandler = Execute;

        private static void Execute(IPointerMoveHandler handler, BaseEventData eventData)
        {
            handler.OnPointerMove(ValidateEventData<PointerEventData>(eventData));
        }

        private static readonly EventFunction<IPointerEnterHandler> s_PointerEnterHandler = Execute;

        private static void Execute(IPointerEnterHandler handler, BaseEventData eventData)
        {
            handler.OnPointerEnter(ValidateEventData<PointerEventData>(eventData));
        }

        private static readonly EventFunction<IPointerExitHandler> s_PointerExitHandler = Execute;

        private static void Execute(IPointerExitHandler handler, BaseEventData eventData)
        {
            handler.OnPointerExit(ValidateEventData<PointerEventData>(eventData));
        }

        private static readonly EventFunction<IPointerDownHandler> s_PointerDownHandler = Execute;

        private static void Execute(IPointerDownHandler handler, BaseEventData eventData)
        {
            handler.OnPointerDown(ValidateEventData<PointerEventData>(eventData));
        }

        private static readonly EventFunction<IPointerUpHandler> s_PointerUpHandler = Execute;

        private static void Execute(IPointerUpHandler handler, BaseEventData eventData)
        {
            handler.OnPointerUp(ValidateEventData<PointerEventData>(eventData));
        }

        private static readonly EventFunction<IPointerClickHandler> s_PointerClickHandler = Execute;

        private static void Execute(IPointerClickHandler handler, BaseEventData eventData)
        {
            handler.OnPointerClick(ValidateEventData<PointerEventData>(eventData));
        }

        private static readonly EventFunction<IInitializePotentialDragHandler> s_InitializePotentialDragHandler = Execute;

        private static void Execute(IInitializePotentialDragHandler handler, BaseEventData eventData)
        {
            handler.OnInitializePotentialDrag(ValidateEventData<PointerEventData>(eventData));
        }

        private static readonly EventFunction<IBeginDragHandler> s_BeginDragHandler = Execute;

        private static void Execute(IBeginDragHandler handler, BaseEventData eventData)
        {
            handler.OnBeginDrag(ValidateEventData<PointerEventData>(eventData));
        }

        private static readonly EventFunction<IDragHandler> s_DragHandler = Execute;

        private static void Execute(IDragHandler handler, BaseEventData eventData)
        {
            handler.OnDrag(ValidateEventData<PointerEventData>(eventData));
        }

        private static readonly EventFunction<IEndDragHandler> s_EndDragHandler = Execute;

        private static void Execute(IEndDragHandler handler, BaseEventData eventData)
        {
            handler.OnEndDrag(ValidateEventData<PointerEventData>(eventData));
        }

        private static readonly EventFunction<IDropHandler> s_DropHandler = Execute;

        private static void Execute(IDropHandler handler, BaseEventData eventData)
        {
            handler.OnDrop(ValidateEventData<PointerEventData>(eventData));
        }

        private static readonly EventFunction<IScrollHandler> s_ScrollHandler = Execute;

        private static void Execute(IScrollHandler handler, BaseEventData eventData)
        {
            handler.OnScroll(ValidateEventData<PointerEventData>(eventData));
        }

        private static readonly EventFunction<IUpdateSelectedHandler> s_UpdateSelectedHandler = Execute;

        private static void Execute(IUpdateSelectedHandler handler, BaseEventData eventData)
        {
            handler.OnUpdateSelected(eventData);
        }

        private static readonly EventFunction<ISelectHandler> s_SelectHandler = Execute;

        private static void Execute(ISelectHandler handler, BaseEventData eventData)
        {
            handler.OnSelect(eventData);
        }

        private static readonly EventFunction<IDeselectHandler> s_DeselectHandler = Execute;

        private static void Execute(IDeselectHandler handler, BaseEventData eventData)
        {
            handler.OnDeselect(eventData);
        }

        private static readonly EventFunction<IMoveHandler> s_MoveHandler = Execute;

        private static void Execute(IMoveHandler handler, BaseEventData eventData)
        {
            handler.OnMove(ValidateEventData<AxisEventData>(eventData));
        }

        private static readonly EventFunction<ISubmitHandler> s_SubmitHandler = Execute;

        private static void Execute(ISubmitHandler handler, BaseEventData eventData)
        {
            handler.OnSubmit(eventData);
        }

        private static readonly EventFunction<ICancelHandler> s_CancelHandler = Execute;

        private static void Execute(ICancelHandler handler, BaseEventData eventData)
        {
            handler.OnCancel(eventData);
        }

        /// <summary>
        /// The event function that sends a pointer move event to an `EventSystems.IPointerMoveHandler`.
        /// </summary>
        public static EventFunction<IPointerMoveHandler> pointerMoveHandler
        {
            get { return s_PointerMoveHandler; }
        }

        /// <summary>
        /// The event function that sends a pointer enter event to an `EventSystems.IPointerEnterHandler`.
        /// </summary>
        public static EventFunction<IPointerEnterHandler> pointerEnterHandler
        {
            get { return s_PointerEnterHandler; }
        }

        /// <summary>
        /// The event function that sends a pointer exit event to an `EventSystems.IPointerExitHandler`.
        /// </summary>
        public static EventFunction<IPointerExitHandler> pointerExitHandler
        {
            get { return s_PointerExitHandler; }
        }

        /// <summary>
        /// The event function that sends a pointer press event to an EventSystems.IPointerDownHandler.
        /// </summary>
        public static EventFunction<IPointerDownHandler> pointerDownHandler
        {
            get { return s_PointerDownHandler; }
        }

        /// <summary>
        /// The event function that sends a pointer release event to an EventSystems.IPointerUpHandler.
        /// </summary>
        public static EventFunction<IPointerUpHandler> pointerUpHandler
        {
            get { return s_PointerUpHandler; }
        }

        /// <summary>
        /// The event function that sends a pointer click event to an EventSystems.IPointerClickHandler.
        /// </summary>
        public static EventFunction<IPointerClickHandler> pointerClickHandler
        {
            get { return s_PointerClickHandler; }
        }

        /// <summary>
        /// The event function that sends an initialize potential drag event to an EventSystems.IInitializePotentialDragHandler.
        /// </summary>
        public static EventFunction<IInitializePotentialDragHandler> initializePotentialDrag
        {
            get { return s_InitializePotentialDragHandler; }
        }

        /// <summary>
        /// The event function that sends a begin drag event to an EventSystems.IBeginDragHandler.
        /// </summary>
        public static EventFunction<IBeginDragHandler> beginDragHandler
        {
            get { return s_BeginDragHandler; }
        }

        /// <summary>
        /// The event function that sends a drag event to an EventSystems.IDragHandler.
        /// </summary>
        public static EventFunction<IDragHandler> dragHandler
        {
            get { return s_DragHandler; }
        }

        /// <summary>
        /// The event function that sends an end drag event to an EventSystems.IEndDragHandler.
        /// </summary>
        public static EventFunction<IEndDragHandler> endDragHandler
        {
            get { return s_EndDragHandler; }
        }

        /// <summary>
        /// The event function that sends a drop event to an EventSystems.IDropHandler.
        /// </summary>
        public static EventFunction<IDropHandler> dropHandler
        {
            get { return s_DropHandler; }
        }

        /// <summary>
        /// The event function that sends a scroll event to an EventSystems.IScrollHandler.
        /// </summary>
        public static EventFunction<IScrollHandler> scrollHandler
        {
            get { return s_ScrollHandler; }
        }

        /// <summary>
        /// The event function that sends an update event to the selected EventSystems.IUpdateSelectedHandler.
        /// </summary>
        public static EventFunction<IUpdateSelectedHandler> updateSelectedHandler
        {
            get { return s_UpdateSelectedHandler; }
        }

        /// <summary>
        /// The event function that sends a selection event to an EventSystems.ISelectHandler.
        /// </summary>
        public static EventFunction<ISelectHandler> selectHandler
        {
            get { return s_SelectHandler; }
        }

        /// <summary>
        /// The event function that sends a deselection event to an EventSystems.IDeselectHandler.
        /// </summary>
        public static EventFunction<IDeselectHandler> deselectHandler
        {
            get { return s_DeselectHandler; }
        }

        /// <summary>
        /// The event function that sends a move event to an EventSystems.IMoveHandler.
        /// </summary>
        public static EventFunction<IMoveHandler> moveHandler
        {
            get { return s_MoveHandler; }
        }

        /// <summary>
        /// The event function that sends a submit event to an EventSystems.ISubmitHandler.
        /// </summary>
        public static EventFunction<ISubmitHandler> submitHandler
        {
            get { return s_SubmitHandler; }
        }

        /// <summary>
        /// The event function that sends a cancel event to an EventSystems.ICancelHandler.
        /// </summary>
        public static EventFunction<ICancelHandler> cancelHandler
        {
            get { return s_CancelHandler; }
        }

        private static void GetEventChain(GameObject root, IList<Transform> eventChain)
        {
            eventChain.Clear();
            if (root == null)
                return;

            var t = root.transform;
            while (t != null)
            {
                eventChain.Add(t);
                t = t.parent;
            }
        }

        /// <summary>
        /// Executes the specified event on every handler of type `T` attached to a GameObject.
        /// </summary>
        /// <remarks>
        /// Handlers on disabled components and inactive GameObjects don't receive the event. If a handler throws an exception, the exception is logged and the remaining handlers still receive the event.
        /// </remarks>
        /// <typeparam name="T">The handler interface that the event is sent to.</typeparam>
        /// <param name="target">The GameObject to send the event to.</param>
        /// <param name="eventData">The event data to pass to each handler.</param>
        /// <param name="functor">The event function that invokes the event on a handler.</param>
        /// <returns>True if the GameObject has at least one handler of type `T`.</returns>
        public static bool Execute<T>(GameObject target, BaseEventData eventData, EventFunction<T> functor) where T : IEventSystemHandler
        {
            var internalHandlers = ListPool<IEventSystemHandler>.Get();
            GetEventList<T>(target, internalHandlers);
            //  if (s_InternalHandlers.Count > 0)
            //      Debug.Log("Executinng " + typeof (T) + " on " + target);

            var internalHandlersCount = internalHandlers.Count;
            for (var i = 0; i < internalHandlersCount; i++)
            {
                T arg;
                try
                {
                    arg = (T)internalHandlers[i];
                }
                catch (Exception e)
                {
                    var temp = internalHandlers[i];
                    Debug.LogException(new Exception(string.Format("Type {0} expected {1} received.", typeof(T).Name, temp.GetType().Name), e));
                    continue;
                }

                try
                {
                    functor(arg, eventData);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            var handlerCount = internalHandlers.Count;
            ListPool<IEventSystemHandler>.Release(internalHandlers);
            return handlerCount > 0;
        }

        private static readonly List<Transform> s_InternalTransformList = new List<Transform>(30);

        /// <summary>
        /// Executes the specified event on the first GameObject in the hierarchy that can handle it.
        /// </summary>
        /// <remarks>
        /// The search starts at `root` and walks up through its parents until it reaches a GameObject with a handler of type `T`. Only that GameObject receives the event.
        /// </remarks>
        /// <typeparam name="T">The handler interface that the event is sent to.</typeparam>
        /// <param name="root">The GameObject to start the search from.</param>
        /// <param name="eventData">The event data to pass to each handler.</param>
        /// <param name="callbackFunction">The event function that invokes the event on a handler.</param>
        /// <returns>The GameObject that received the event, or `null` if no GameObject in the hierarchy can handle it.</returns>
        public static GameObject ExecuteHierarchy<T>(GameObject root, BaseEventData eventData, EventFunction<T> callbackFunction) where T : IEventSystemHandler
        {
            GetEventChain(root, s_InternalTransformList);

            var internalTransformListCount = s_InternalTransformList.Count;
            for (var i = 0; i < internalTransformListCount; i++)
            {
                var transform = s_InternalTransformList[i];
                if (Execute(transform.gameObject, eventData, callbackFunction))
                    return transform.gameObject;
            }
            return null;
        }

        private static bool ShouldSendToComponent<T>(Component component) where T : IEventSystemHandler
        {
            var valid = component is T;
            if (!valid)
                return false;

            var behaviour = component as Behaviour;
            if (behaviour != null)
                return behaviour.isActiveAndEnabled;
            return true;
        }

        /// <summary>
        /// Get the specified object's event event.
        /// </summary>
        private static void GetEventList<T>(GameObject go, IList<IEventSystemHandler> results) where T : IEventSystemHandler
        {
            // Debug.LogWarning("GetEventList<" + typeof(T).Name + ">");
            if (results == null)
                throw new ArgumentException("Results array is null", "results");

            if (go == null || !go.activeInHierarchy)
                return;

            var components = ListPool<Component>.Get();
            go.GetComponents(components);

            var componentsCount = components.Count;
            for (var i = 0; i < componentsCount; i++)
            {
                if (!ShouldSendToComponent<T>(components[i]))
                    continue;

                // Debug.Log(string.Format("{2} found! On {0}.{1}", go, s_GetComponentsScratch[i].GetType(), typeof(T)));
                results.Add(components[i] as IEventSystemHandler);
            }
            ListPool<Component>.Release(components);
            // Debug.LogWarning("end GetEventList<" + typeof(T).Name + ">");
        }

        /// <summary>
        /// Whether the specified game object will be able to handle the specified event.
        /// </summary>
        /// <typeparam name="T">The handler interface to look for.</typeparam>
        /// <param name="go">The GameObject to check.</param>
        /// <returns>True if the GameObject is active, has at least one enabled handler of type `T`.</returns>
        public static bool CanHandleEvent<T>(GameObject go) where T : IEventSystemHandler
        {
            var internalHandlers = ListPool<IEventSystemHandler>.Get();
            GetEventList<T>(go, internalHandlers);
            var handlerCount = internalHandlers.Count;
            ListPool<IEventSystemHandler>.Release(internalHandlers);
            return handlerCount != 0;
        }

        /// <summary>
        /// Bubble the specified event on the game object, figuring out which object will actually receive the event.
        /// </summary>
        /// <typeparam name="T">The handler interface to look for.</typeparam>
        /// <param name="root">The GameObject to start the search from.</param>
        /// <returns>The first active GameObject in the hierarchy that has an enabled handler of type `T`, or `null` if there is none.</returns>
        public static GameObject GetEventHandler<T>(GameObject root) where T : IEventSystemHandler
        {
            if (root == null)
                return null;

            Transform t = root.transform;
            while (t != null)
            {
                if (CanHandleEvent<T>(t.gameObject))
                    return t.gameObject;
                t = t.parent;
            }
            return null;
        }
    }
}
