using UnityEngine.Pool;

namespace UnityEngine.UI
{
    /// <summary>
    /// Adds an outline to a graphic using IVertexModifier.
    /// </summary>
    [AddComponentMenu("UI (Canvas)/Effects/Outline", 81)]
    [UGUIHelpURL("Outline")]
    public class Outline : Shadow
    {
        /// <summary>Protected default constructor. Use <see cref="GameObject.AddComponent{T}"/> to add an Outline to a GameObject.</summary>
        protected Outline()
        {}

        /// <summary>
        /// Applies a four-directional outline effect by appending offset copies of the vertices in each corner direction.
        /// </summary>
        /// <param name="vh">The <see cref="VertexHelper"/> containing the graphic's mesh data to modify.</param>
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
                return;

            var verts = ListPool<UIVertex>.Get();
            vh.GetUIVertexStream(verts);

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

            vh.Clear();
            vh.AddUIVertexTriangleStream(verts);
            ListPool<UIVertex>.Release(verts);
        }
    }
}
