using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public abstract class MaskableGraphic : Graphic, IMaskable
    {
        // m_Maskable is whether this graphic is allowed to be masked or not. It has the matching public property maskable.
        // The default for m_Maskable is true, so graphics under a mask are masked out of the box.
        // The maskable property can be turned off from script by the user if masking is not desired.
        // m_IncludeForMasking is whether we actually consider this graphic for masking or not - this is an implementation detail.
        // m_IncludeForMasking should only be true if m_Maskable is true AND a parent of the graphic has an IMask component.
        // Things would still work correctly if m_IncludeForMasking was always true when m_Maskable is, but performance would suffer.
        [NonSerialized]
        private bool m_Maskable = true;

        [NonSerialized]
        protected Material m_MaskMaterial;

        [NonSerialized]
        protected bool m_IncludeForMasking = false;

        [NonSerialized]
        protected int m_StencilValue = 0;

        [NonSerialized]
        protected bool m_ShouldRecalculate = true;

        public bool maskable
        {
            get { return m_Maskable; }
            set
            {
                if (value == m_Maskable)
                    return;
                m_Maskable = value;
                SetMaterialDirty();
            }
        }

        /// <summary>
        /// Returns the material used by this Graphic.
        /// </summary>
        public override Material material
        {
            get
            {
                UpdateInternalState();

                if (m_IncludeForMasking)
                {
                    if (m_MaskMaterial == null)
                        m_MaskMaterial = StencilMaterial.Add(base.material, (1 << m_StencilValue) - 1);

                    return m_MaskMaterial ?? base.material;
                }
                return base.material;
            }
            set { base.material = value; }
        }


        private void UpdateInternalState()
        {
            if (!m_ShouldRecalculate)
                return;

            m_StencilValue = GetStencilForGraphic();

            var t = transform.parent;
            m_IncludeForMasking = false;

            var components = ComponentListPool.Get();
            while (m_Maskable && t != null)
            {
                t.GetComponents(typeof(IMask), components);
                if (components.Count > 0)
                {
                    m_IncludeForMasking = true;
                    break;
                }
                t = t.parent;
            }
            m_ShouldRecalculate = false;
            ComponentListPool.Release(components);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_ShouldRecalculate = true;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ClearMaskMaterial();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            ClearMaskMaterial();
        }

#endif

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            m_ShouldRecalculate = true;
        }

        public virtual void ParentMaskStateChanged()
        {
            m_ShouldRecalculate = true;
            SetMaterialDirty();
        }

        private void ClearMaskMaterial()
        {
            StencilMaterial.Remove(m_MaskMaterial);
            m_MaskMaterial = null;
        }

        public override void SetMaterialDirty()
        {
            base.SetMaterialDirty();
            ClearMaskMaterial();
        }

        private int GetStencilForGraphic()
        {
            var maskDepth = 0;
            var t = transform.parent;
            var components = ComponentListPool.Get();
            while (t != null)
            {
                t.GetComponents(typeof(IMask), components);
                for (var i = 0; i < components.Count; i++)
                {
                    var mask = components[i] as IMask;

                    if (mask == null || !mask.MaskEnabled())
                        continue;

                    maskDepth++;
                    maskDepth = Mathf.Clamp(maskDepth, 0, 8);
                    break;
                }
                t = t.parent;
            }
            ComponentListPool.Release(components);
            return maskDepth;
        }
    }
}
