using System.Collections.Generic;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Effects/Outline", 15)]
    public class Outline : Shadow
    {
        protected Outline()
        {}

        public override void ModifyMesh(Mesh mesh)
        {
            if (!IsActive())
                return;

            List<UIVertex> verts = new List<UIVertex>();
            using (var helper = new VertexHelper(mesh))
                helper.GetUIVertexStream(verts);

            var neededCpacity = verts.Count * 5;
            if (verts.Capacity < neededCpacity)
                verts.Capacity = neededCpacity;

            var start = 0;
            var end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, effectDistance.x, effectDistance.y);

            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, effectDistance.x, -effectDistance.y);

            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, -effectDistance.x, effectDistance.y);

            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, -effectDistance.x, -effectDistance.y);

            using (var helper = new VertexHelper())
            {
                helper.AddUIVertexTriangleStream(verts);
                helper.FillMesh(mesh);
            }
        }
    }
}
