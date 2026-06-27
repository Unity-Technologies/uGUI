using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    /// <summary>
    /// Base class for effects that modify the generated Mesh.
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    ///using UnityEngine;
    ///using UnityEngine.UI;
    ///
    ///public class PositionAsUV1 : BaseMeshEffect
    ///{
    ///    public override void ModifyMesh(VertexHelper vh)
    ///    {
    ///        UIVertex vert = new UIVertex();
    ///        for (int i = 0; i < vh.currentVertCount; i++)
    ///        {
    ///            vh.PopulateUIVertex(ref vert, i);
    ///            vert.uv1 =  new Vector2(vert.position.x, vert.position.y);
    ///            vh.SetUIVertex(vert, i);
    ///        }
    ///    }
    ///}
    /// ]]>
    ///</code>
    ///</example>

    [ExecuteAlways]
    public abstract class BaseMeshEffect : UIBehaviour, IMeshModifier
    {
        [NonSerialized]
        private Graphic m_Graphic;

        /// <summary>
        /// The graphic component that the Mesh Effect will aplly to.
        /// </summary>
        protected Graphic graphic
        {
            get
            {
                if (m_Graphic == null)
                    m_Graphic = GetComponent<Graphic>();

                return m_Graphic;
            }
        }

        /// <summary>Called when the component becomes enabled. Marks the associated graphic's vertices as dirty.</summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            if (graphic != null)
                graphic.SetVerticesDirty();
        }

        /// <summary>Called when the component becomes disabled. Marks the associated graphic's vertices as dirty.</summary>
        protected override void OnDisable()
        {
            if (graphic != null)
                graphic.SetVerticesDirty();
            base.OnDisable();
        }

        /// <summary>
        /// Called from the native side any time a animation property is changed.
        /// </summary>
        protected override void OnDidApplyAnimationProperties()
        {
            if (graphic != null)
                graphic.SetVerticesDirty();
            base.OnDidApplyAnimationProperties();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (graphic != null)
                graphic.SetVerticesDirty();
        }

#endif

        /// <summary>
        /// Function that is called when the Graphic is populating the mesh.
        /// </summary>
        /// <param name="mesh">The generated mesh of the Graphic element that needs modification.</param>
		[Obsolete("Use IMeshModifier.ModifyMesh(VertexHelper verts) instead", true)]
        public virtual void ModifyMesh(Mesh mesh)
        {
            using (var vh = new VertexHelper(mesh))
            {
                ModifyMesh(vh);
                vh.FillMesh(mesh);
            }
        }

        /// <summary>
        /// Called when the associated <see cref="Graphic"/> populates its mesh. Override to apply custom vertex modifications.
        /// </summary>
        /// <param name="vh">The <see cref="VertexHelper"/> containing the mesh data to modify.</param>
        public abstract void ModifyMesh(VertexHelper vh);
    }
}
