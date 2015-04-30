using System.Collections.Generic;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Effects/Outline", 15)]
    public class Outline : Shadow
    {
        protected Outline()
        {}

        public override void ModifyVertices(List<UIVertex> verts)
        {
            if (!IsActive())
                return;

            var start = 0;
            var end = verts.Count;
            ApplyShadow(verts, effectColor, start, verts.Count, effectDistance.x, effectDistance.y);

            start = end;
            end = verts.Count;
            ApplyShadow(verts, effectColor, start, verts.Count, effectDistance.x, -effectDistance.y);

            start = end;
            end = verts.Count;
            ApplyShadow(verts, effectColor, start, verts.Count, -effectDistance.x, effectDistance.y);

            start = end;
            end = verts.Count;
            ApplyShadow(verts, effectColor, start, verts.Count, -effectDistance.x, -effectDistance.y);
        }
    }
}
