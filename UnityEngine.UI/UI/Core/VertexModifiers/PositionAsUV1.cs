using System.Linq;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Effects/Position As UV1", 16)]
    public class PositionAsUV1 : BaseMeshEffect
    {
        protected PositionAsUV1()
        {}

        public override void ModifyMesh(Mesh mesh)
        {
            if (!IsActive())
                return;

            var verts = mesh.vertices.ToList();
            var uvs = ListPool<Vector2>.Get();

            for (int i = 0; i < verts.Count; i++)
            {
                var vert = verts[i];
                uvs.Add(new Vector2(verts[i].x, verts[i].y));
                verts[i] = vert;
            }
            mesh.SetUVs(1, uvs);
            ListPool<Vector2>.Release(uvs);
        }
    }
}
