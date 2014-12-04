using System;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Mask", 13)]
    [ExecuteInEditMode]
    public class Mask : UIBehaviour, IGraphicEnabledDisabled, IMask, ICanvasRaycastFilter, IMaterialModifier
    {
        [SerializeField]
        [FormerlySerializedAs("m_ShowGraphic")]
        private bool m_ShowMaskGraphic = true;

        private Material m_RenderMaterial;

        private Graphic m_Graphic;
        private RectTransform m_RectTransform;

        private Graphic graphic
        {
            get
            {
                if (m_Graphic == null)
                    m_Graphic = GetComponent<Graphic>();

                return m_Graphic;
            }
        }

        public bool showMaskGraphic
        {
            get { return m_ShowMaskGraphic; }
            set
            {
                if (m_ShowMaskGraphic == value)
                    return;

                m_ShowMaskGraphic = value;
                if (graphic != null)
                    graphic.SetMaterialDirty();
            }
        }

        public RectTransform rectTransform
        {
            get { return m_RectTransform ?? (m_RectTransform = GetComponent<RectTransform>()); }
        }

        protected Mask()
        { }

        public virtual bool MaskEnabled()
        {
            return IsActive() && graphic != null;
        }

        public virtual void OnSiblingGraphicEnabledDisabled()
        {
            NotifyMaskStateChanged();
        }

        private void NotifyMaskStateChanged()
        {
            if (graphic != null)
            {
                graphic.canvasRenderer.isMask = IsActive();
                graphic.SetMaterialDirty();
            }

            var components = ComponentListPool.Get();
            GetComponentsInChildren(components);
            for (var i = 0; i < components.Count; i++)
            {
                if (components[i] == null || components[i].gameObject == gameObject)
                    continue;

                var toNotify = components[i] as IMaskable;
                if (toNotify != null)
                    toNotify.ParentMaskStateChanged();
            }
            ComponentListPool.Release(components);
        }

        private void ClearCachedMaterial()
        {
            if (m_RenderMaterial != null)
                Misc.DestroyImmediate(m_RenderMaterial);

            m_RenderMaterial = null;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            NotifyMaskStateChanged();
        }

        protected override void OnDisable()
        {
            // we call base OnDisable first here
            // as we need to have the IsActive return the
            // correct value when we notify the children
            // that the mask state has changed.
            base.OnDisable();
            ClearCachedMaterial();
            NotifyMaskStateChanged();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            NotifyMaskStateChanged();
        }

#endif
        public virtual bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, sp, eventCamera);
        }

        public virtual Material GetModifiedMaterial(Material baseMaterial)
        {
            ClearCachedMaterial();

            if (!IsActive())
                return baseMaterial;

            m_RenderMaterial = new Material(baseMaterial)
            {
                name = "Mask " + " (" + baseMaterial.name + ")",
                hideFlags = HideFlags.HideAndDontSave
            };

            if (m_RenderMaterial.HasProperty("_ColorMask"))
                m_RenderMaterial.SetInt("_ColorMask", m_ShowMaskGraphic ? (int)ColorWriteMask.All : 0);
            else
                Debug.LogWarning("Material " + baseMaterial + " doesn't have color mask", baseMaterial);

            return m_RenderMaterial;
        }
    }
}
