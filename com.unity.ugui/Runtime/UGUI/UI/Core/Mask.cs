using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace UnityEngine.UI
{
    /// <summary>
    /// A component for masking children elements.
    /// </summary>
    /// <remarks>
    /// By using this element any children elements that have masking enabled will mask where a sibling Graphic would write 0 to the stencil buffer.
    /// </remarks>
    [AddComponentMenu("UI (Canvas)/Mask", 13)]
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    [UGUIHelpURL("Mask")]
    public class Mask : UIBehaviour, ICanvasRaycastFilter, IMaterialModifier
    {
        [NonSerialized]
        private RectTransform m_RectTransform;
		
        /// <summary>The RectTransform of this Mask's GameObject.</summary>
        public RectTransform rectTransform
        {
            get { return m_RectTransform ?? (m_RectTransform = GetComponent<RectTransform>()); }
        }

        [SerializeField]
        private bool m_ShowMaskGraphic = true;

        /// <summary>
        /// Show the graphic that is associated with the Mask render area.
        /// </summary>
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

        [NonSerialized]
        private Graphic m_Graphic;

        /// <summary>
        /// The graphic associated with the Mask.
        /// </summary>
        public Graphic graphic
        {
            get { return m_Graphic ?? (m_Graphic = GetComponent<Graphic>()); }
        }

        [NonSerialized]
        private Material m_MaskMaterial;

        [NonSerialized]
        private Material m_UnmaskMaterial;

        /// <summary>Protected default constructor. Use <see cref="GameObject.AddComponent{T}"/> to add a Mask to a GameObject.</summary>
        protected Mask()
        {}

        /// <summary>
		/// Returns whether this mask is enabled and its graphic is valid.
		/// </summary>
        /// <returns>True if the mask is active and has a valid graphic.</returns>
        public virtual bool MaskEnabled() { return IsActive() && graphic != null; }

        /// <summary>Obsolete. No longer called or used.</summary>
        [Obsolete("Not used anymore.", true)]
        public virtual void OnSiblingGraphicEnabledDisabled() {}

        /// <summary>Called when it becomes enabled. Notifies clippable children and triggers a material update.</summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            if (graphic != null)
            {
                graphic.canvasRenderer.hasPopInstruction = true;
                graphic.SetMaterialDirty();

                // Default the graphic to being the maskable graphic if its found.
                if (graphic is MaskableGraphic)
                    (graphic as MaskableGraphic).isMaskingGraphic = true;
            }

            MaskUtilities.NotifyStencilStateChanged(this);
        }

        /// <summary>Called when it becomes disabled. Notifies clippable children and triggers a material update.</summary>
        protected override void OnDisable()
        {
            // we call base OnDisable first here
            // as we need to have the IsActive return the
            // correct value when we notify the children
            // that the mask state has changed.
            base.OnDisable();
            if (graphic != null)
            {
                graphic.SetMaterialDirty();
                graphic.canvasRenderer.hasPopInstruction = false;
                graphic.canvasRenderer.popMaterialCount = 0;

                if (graphic is MaskableGraphic)
                    (graphic as MaskableGraphic).isMaskingGraphic = false;
            }

            StencilMaterial.Remove(m_MaskMaterial);
            m_MaskMaterial = null;
            StencilMaterial.Remove(m_UnmaskMaterial);
            m_UnmaskMaterial = null;

            MaskUtilities.NotifyStencilStateChanged(this);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (!IsActive())
                return;

            if (graphic != null)
            {
                // Default the graphic to being the maskable graphic if its found.
                if (graphic is MaskableGraphic)
                    (graphic as MaskableGraphic).isMaskingGraphic = true;

                graphic.SetMaterialDirty();
            }

            MaskUtilities.NotifyStencilStateChanged(this);
        }

#endif

        /// <summary>
		/// Returns whether the given screen position hits the visible (unmasked) area of this mask graphic.
		/// </summary>
        /// <param name="sp">The screen position to test.</param>
        /// <param name="eventCamera">The camera used for the raycast.</param>
        /// <returns>True if the position is within the unmasked area.</returns>
        public virtual bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            if (!isActiveAndEnabled)
                return true;

            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, sp, eventCamera);
        }

        /// <summary>
        /// Modifies the material for masking. Used when the mask needs to modify the stencil buffer.
        /// </summary>
        /// <remarks>
        /// The mask creates modified copies of the base <see cref="Material"/> that write
        /// to the stencil buffer so that child graphics can be clipped to the mask shape.
        /// Multiple nested masks use increasing stencil depths up to a maximum of 8.
        /// Override <see cref="GetModifiedMaterial"/> to customize behavior.
        /// </remarks>
        /// <param name="baseMaterial">The base material to apply the masking modification to.</param>
        /// <returns>The modified material for rendering.</returns>
        /// <example>
        /// <para>The graphic system typically calls this method when the mask needs a
        /// modified material. Override to apply custom stencil or shader logic. Call
        /// base first to get the default stencil material, then optionally apply
        /// custom properties (e.g. a different stencil comparison or color write mask).</para>
        /// <code><![CDATA[
        /// public override Material GetModifiedMaterial(Material baseMaterial)
        /// {
        ///     Material modified = base.GetModifiedMaterial(baseMaterial);
        ///     // apply custom stencil or shader logic, e.g. change comparison:
        ///     if (modified != null) modified.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
        ///     return modified;
        /// }
        /// ]]></code>
        /// </example>
        public virtual Material GetModifiedMaterial(Material baseMaterial)
        {
            if (!MaskEnabled())
                return baseMaterial;

            var rootSortCanvas = MaskUtilities.FindRootSortOverrideCanvas(transform);
            var stencilDepth = MaskUtilities.GetStencilDepth(transform, rootSortCanvas);
            if (stencilDepth >= 8)
            {
                Debug.LogWarning("Attempting to use a stencil mask with depth > 8", gameObject);
                return baseMaterial;
            }

            int desiredStencilBit = 1 << stencilDepth;

            // if we are at the first level...
            // we want to destroy what is there
            if (desiredStencilBit == 1)
            {
                var maskMaterial = StencilMaterial.Add(baseMaterial, 1, StencilOp.Replace, CompareFunction.Always, m_ShowMaskGraphic ? ColorWriteMask.All : 0);
                StencilMaterial.Remove(m_MaskMaterial);
                m_MaskMaterial = maskMaterial;

                var unmaskMaterial = StencilMaterial.Add(baseMaterial, 1, StencilOp.Zero, CompareFunction.Always, 0);
                StencilMaterial.Remove(m_UnmaskMaterial);
                m_UnmaskMaterial = unmaskMaterial;
                graphic.canvasRenderer.popMaterialCount = 1;
                graphic.canvasRenderer.SetPopMaterial(m_UnmaskMaterial, 0);

                return m_MaskMaterial;
            }

            //otherwise we need to be a bit smarter and set some read / write masks
            var maskMaterial2 = StencilMaterial.Add(baseMaterial, desiredStencilBit | (desiredStencilBit - 1), StencilOp.Replace, CompareFunction.Equal, m_ShowMaskGraphic ? ColorWriteMask.All : 0, desiredStencilBit - 1, desiredStencilBit | (desiredStencilBit - 1));
            StencilMaterial.Remove(m_MaskMaterial);
            m_MaskMaterial = maskMaterial2;

            graphic.canvasRenderer.hasPopInstruction = true;
            var unmaskMaterial2 = StencilMaterial.Add(baseMaterial, desiredStencilBit - 1, StencilOp.Replace, CompareFunction.Equal, 0, desiredStencilBit - 1, desiredStencilBit | (desiredStencilBit - 1));
            StencilMaterial.Remove(m_UnmaskMaterial);
            m_UnmaskMaterial = unmaskMaterial2;
            graphic.canvasRenderer.popMaterialCount = 1;
            graphic.canvasRenderer.SetPopMaterial(m_UnmaskMaterial, 0);

            return m_MaskMaterial;
        }
    }
}
