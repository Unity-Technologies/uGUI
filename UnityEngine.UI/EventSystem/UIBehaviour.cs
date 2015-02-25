namespace UnityEngine.EventSystems
{
    public abstract class UIBehaviour : MonoBehaviour
    {
        protected virtual void Awake()
        { }

        protected virtual void OnEnable()
        { }

        protected virtual void Start()
        { }

        protected virtual void OnDisable()
        { }

        protected virtual void OnDestroy()
        { }

        public virtual bool IsActive()
        {
            return enabled && isActiveAndEnabled && gameObject.activeInHierarchy;
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
