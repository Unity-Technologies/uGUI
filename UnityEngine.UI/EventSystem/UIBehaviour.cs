namespace UnityEngine.EventSystems
{
    public abstract class UIBehaviour : MonoBehaviour
    {
        // Whether the OnEnable of this Instace has been called.
        // This is true inside the OnEnable call itself (provided the call to base.OnEnable is at the beginning),
        // and also true during OnDisable (provided the call to base.OnDIsable is at the end).
        // This bool is needed not just in the editor but also at runtime,
        // so components can make assumptions about OnEnable having been called when IsActive() return true.
        [System.NonSerialized]
        private bool m_OnEnableHasBeenCalled = false;

        protected virtual void  Awake()
        { }

        protected virtual void OnEnable()
        {
            m_OnEnableHasBeenCalled = true;
        }

        protected virtual void Start()
        { }

        protected virtual void OnDisable()
        {
            m_OnEnableHasBeenCalled = false;
        }

        protected virtual void OnDestroy()
        { }

        public virtual bool IsActive()
        {
            return enabled && m_OnEnableHasBeenCalled && gameObject.activeInHierarchy;
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        { }

        protected virtual void Reset()
        { }
#endif

        protected virtual void OnRectTransformDimensionsChange()
        { }

        protected virtual void OnBeforeTransformParentChanged()
        { }

        protected virtual void OnTransformParentChanged()
        { }

        protected virtual void OnDidApplyAnimationProperties()
        { }

        protected virtual void OnCanvasGroupChanged()
        { }

        public bool IsDestroyed()
        {
            // Workaround for Unity native side of the object
            // having been destroyed but accessing via interface
            // won't call the overloaded ==
            return this == null;
        }
    }
}
