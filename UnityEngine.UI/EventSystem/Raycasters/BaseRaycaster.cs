using System.Collections.Generic;

namespace UnityEngine.EventSystems
{
    public abstract class BaseRaycaster : UIBehaviour
    {
        public abstract void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList);
        public abstract Camera eventCamera { get; }

        public virtual int priority
        {
            get { return int.MaxValue; }
        }

        #region Unity Lifetime calls

        protected override void OnEnable()
        {
            base.OnEnable();
            RaycasterManager.AddRaycaster(this);
        }

        protected override void OnDisable()
        {
            RaycasterManager.RemoveRaycasters(this);
            base.OnDisable();
        }

        #endregion
    }
}
