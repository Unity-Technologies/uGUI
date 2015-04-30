using System.Collections.Generic;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Effects/Position As UV1", 16)]
    public class PositionAsUV1 : BaseVertexEffect
    {
        protected PositionAsUV1()
        {}

        public override void ModifyVertices(List<UIVertex> verts)
        {
            if (!IsActive())
                return;

            for (int i = 0; i < verts.Count; i++)
            {
                var vert = verts[i];
                vert.uv1 = new Vector2(verts[i].position.x, verts[i].position.y);
                verts[i] = vert;
            }
        }
    }
}
