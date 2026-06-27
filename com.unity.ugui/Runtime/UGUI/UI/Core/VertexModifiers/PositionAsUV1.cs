using System.Linq;

namespace UnityEngine.UI
{
    /// <summary>
    /// An IVertexModifier which sets the raw vertex position into UV1 of the generated verts.
    /// </summary>
    [AddComponentMenu("UI (Canvas)/Effects/Position As UV1", 82)]
    [UGUIHelpURL("PositionAsUV1")]
    public class PositionAsUV1 : BaseMeshEffect
    {
        /// <summary>Protected default constructor. Use <see cref="GameObject.AddComponent{T}"/> to add a PositionAsUV1 effect to a GameObject.</summary>
        protected PositionAsUV1()
        {}

        /// <summary>
        /// Writes each vertex's XY world position into the UV1 channel of the mesh.
        /// </summary>
        /// <param name="vh">The <see cref="VertexHelper"/> containing the mesh data to modify.</param>
        public override void ModifyMesh(VertexHelper vh)
        {
            UIVertex vert = new UIVertex();
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vert, i);
                vert.uv1 =  new Vector2(vert.position.x, vert.position.y);
                vh.SetUIVertex(vert, i);
            }
        }
    }
}
