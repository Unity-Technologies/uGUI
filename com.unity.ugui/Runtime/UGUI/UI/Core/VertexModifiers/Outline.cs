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

            int vertCount = vh.currentVertCount;
            int indexCount = vh.currentIndexCount;
            if (vertCount == 0 || indexCount == 0)
            {
                // Without indices nothing draws; clear (matching the old stream path)
                vh.Clear();
                return;
            }

            Color32 color = effectColor;
            float x = effectDistance.x;
            float y = effectDistance.y;

            int copyStart = vh.ReserveVerts(vertCount * 4);
            int frontStart = copyStart + vertCount * 3;

            // One pass: read each original once into vt, emit the four offset copies and
            // keep the original in front. Overwriting slot i last is safe — vt already
            // holds the read, and MakeShadowVert takes vt by value.
            UIVertex vt = default;
            for (int i = 0; i < vertCount; i++)
            {
                vh.PopulateUIVertex(ref vt, i);
                vh.SetUIVertex(vt, frontStart + i);
                vh.SetUIVertex(MakeShadowVert(vt, color, x, -y), copyStart + i);
                vh.SetUIVertex(MakeShadowVert(vt, color, -x, y), copyStart + vertCount + i);
                vh.SetUIVertex(MakeShadowVert(vt, color, -x, -y), copyStart + vertCount * 2 + i);
                vh.SetUIVertex(MakeShadowVert(vt, color, x, y), i);
            }

            vh.AddShiftedIndices(0, indexCount, copyStart);
            vh.AddShiftedIndices(0, indexCount, copyStart + vertCount);
            vh.AddShiftedIndices(0, indexCount, copyStart + vertCount * 2);
            vh.AddShiftedIndices(0, indexCount, frontStart);
        }
    }
}
