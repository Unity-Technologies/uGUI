using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [ExecuteInEditMode]
    public abstract class BaseVertexEffect : UIBehaviour, IVertexModifier
    {
        [NonSerialized]
        private Graphic m_Graphic;

        protected Graphic graphic
        {
            get
            {
                if (m_Graphic == null)
                    m_Graphic = GetComponent<Graphic>();

                return m_Graphic;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (graphic != null)
                graphic.SetVerticesDirty();
        }

        protected override void OnDisable()
        {
            if (graphic != null)
                graphic.SetVerticesDirty();
            base.OnDisable();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (graphic != null)
                graphic.SetVerticesDirty();
        }

#endif

        public abstract void ModifyVertices(List<UIVertex> verts);
    }
}
