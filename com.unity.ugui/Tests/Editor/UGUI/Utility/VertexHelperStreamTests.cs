using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

[Category("VertexHelper")]
internal class VertexHelperStreamTests
{
    static List<UIVertex> MakeVerts(int count, float baseX)
    {
        var list = new List<UIVertex>(count);
        for (int i = 0; i < count; i++)
        {
            var v = UIVertex.simpleVert;
            v.position = new Vector3(baseX + i, 0f, 0f);
            list.Add(v);
        }
        return list;
    }

    static void AssertVertsMatchBaseX(VertexHelper vh, float baseX)
    {
        var v = new UIVertex();
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref v, i);
            Assert.AreEqual(baseX + i, v.position.x, $"vert {i} position.x mismatch");
        }
    }

    // AddUIVertexTriangleStream historically delegated to CanvasRenderer.SplitUIVertexStreams,
    // which replaces both the vertex and index buffers from index 0 rather than appending.
    [Test]
    public void AddUIVertexTriangleStream_ReplacesVertsAndIndicesFromZero()
    {
        using (var vh = new VertexHelper())
        {
            // Seed with a first stream so the helper is non-empty.
            vh.AddUIVertexTriangleStream(MakeVerts(6, baseX: 100f));
            Assert.AreEqual(6, vh.currentVertCount);
            Assert.AreEqual(6, vh.currentIndexCount);
            AssertVertsMatchBaseX(vh, baseX: 100f);

            // A second, different stream must replace the first, not append to it.
            vh.AddUIVertexTriangleStream(MakeVerts(3, baseX: 0f));

            Assert.AreEqual(3, vh.currentVertCount, "Verts should be replaced from index 0, not appended.");
            Assert.AreEqual(3, vh.currentIndexCount, "Indices should be replaced from index 0, not appended.");

            // Whole buffer must be the second stream (x = 0, 1, 2), not stale tail from the first.
            AssertVertsMatchBaseX(vh, baseX: 0f);
        }
    }

    // Both the vert and index streams replace existing content from index 0; neither appends.
    [Test]
    public void AddUIVertexStream_ReplacesVertsAndIndicesFromZero()
    {
        using (var vh = new VertexHelper())
        {
            // Seed with 4 verts and 6 indices.
            vh.AddUIVertexStream(MakeVerts(4, baseX: 100f), new List<int> { 0, 1, 2, 2, 3, 0 });
            Assert.AreEqual(4, vh.currentVertCount);
            Assert.AreEqual(6, vh.currentIndexCount);
            AssertVertsMatchBaseX(vh, baseX: 100f);

            // A second, different stream must replace both buffers, not append to them.
            vh.AddUIVertexStream(MakeVerts(3, baseX: 0f), new List<int> { 0, 1, 2 });

            Assert.AreEqual(3, vh.currentVertCount, "Verts should be replaced from index 0, not appended.");
            Assert.AreEqual(3, vh.currentIndexCount, "Indices should be replaced from index 0, not appended.");
            AssertVertsMatchBaseX(vh, baseX: 0f);
        }
    }
}
