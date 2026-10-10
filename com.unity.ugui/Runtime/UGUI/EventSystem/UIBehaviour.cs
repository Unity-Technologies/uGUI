namespace UnityEngine.EventSystems
{
    /// <summary>
    /// Base behaviour that has protected implementations of Unity lifecycle functions.
    /// </summary>
    public abstract class UIBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Called when the behaviour is loaded, before Start and before any OnEnable call.
        /// </summary>
        protected virtual void Awake()
        {}

        /// <summary>
        /// Called when the behaviour becomes enabled.
        /// </summary>
        protected virtual void OnEnable()
        {}

        /// <summary>
        /// Called on the first frame the behaviour is enabled, just before any of the Update methods are called.
        /// </summary>
        protected virtual void Start()
        {}

        /// <summary>
        /// Called when the behaviour becomes disabled, or its parent GameObject is deactivated.
        /// </summary>
        protected virtual void OnDisable()
        {}

        /// <summary>
        /// Called when the behaviour is destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {}

        /// <summary>
        /// Returns true if the GameObject and the Component are active.
        /// </summary>
        /// <returns>True if the GameObject is active in the hierarchy and the component is enabled.</returns>
        public virtual bool IsActive()
        {
            return isActiveAndEnabled;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Called in the editor when the script is loaded or a value changes in the Inspector.
        /// </summary>
        protected virtual void OnValidate()
        {}

        /// <summary>
        /// Called in the editor when you select Reset in the Inspector's context menu, or when you add the component for the first time.
        /// </summary>
        protected virtual void Reset()
        {}
#endif
        /// <summary>
        /// This callback is called when the dimensions of an associated RectTransform change. It is always called before Awake, OnEnable, or Start. The call is also made to all child RectTransforms, regardless of whether their dimensions change (which depends on how they are anchored).
        /// </summary>
        protected virtual void OnRectTransformDimensionsChange()
        {}

        /// <summary>
        /// Called before a direct or indirect parent is reparented to a new Transform.
        /// </summary>
        protected virtual void OnBeforeTransformParentChanged()
        {}

        /// <summary>
        /// Called when a direct or indirect parent has reparented to a new Transform.
        /// See <see cref="MonoBehaviour.OnTransformParentChanged"/> for more information.
        /// </summary>
        protected virtual void OnTransformParentChanged()
        {}

        /// <summary>
        /// Called when an animation clip has applied its values to this behaviour.
        /// </summary>
        protected virtual void OnDidApplyAnimationProperties()
        {}

        /// <summary>
        /// Called when the state of a parent CanvasGroup is changed.
        /// </summary>
        protected virtual void OnCanvasGroupChanged()
        {}

        /// <summary>
        /// Called when the state of the parent Canvas has changed.
        /// </summary>
        protected virtual void OnCanvasHierarchyChanged()
        {}

        /// <summary>
        /// Returns true if the native representation of the behaviour has been destroyed.
        /// </summary>
        /// <remarks>
        /// When a parent canvas is either enabled, disabled or a nested canvas's OverrideSorting is changed this function is called. You can for example use this to modify objects below a canvas that may depend on a parent canvas - for example, if a canvas is disabled you may want to halt some processing of a UI element.
        /// </remarks>
        /// <returns>True if the native representation of the behaviour has been destroyed.</returns>
        public bool IsDestroyed()
        {
            // Workaround for Unity native side of the object
            // having been destroyed but accessing via interface
            // won't call the overloaded ==
            return this == null;
        }
    }
}
