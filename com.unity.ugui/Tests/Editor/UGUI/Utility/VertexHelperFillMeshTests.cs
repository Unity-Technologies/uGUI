using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[Category("VertexHelper")]
internal class VertexHelperFillMeshTests
{
    // Mimics what Graphic.OnPopulateMesh does for a default 100x100 Image at the origin:
    // four corner verts + two triangles, then FillMesh into a Mesh. If the rebuilt Mesh
    // isn't a renderable quad after this, no UI element on the branch can draw.
    [Test]
    public void FillMesh_BasicQuad_ProducesRenderableMesh()
    {
        using (var vh = new VertexHelper())
        {
            var mesh = new Mesh();
            try
            {
                Color32 color = Color.white;
                vh.AddVert(new Vector3(  0,   0, 0), color, new Vector2(0, 0));
                vh.AddVert(new Vector3(  0, 100, 0), color, new Vector2(0, 1));
                vh.AddVert(new Vector3(100, 100, 0), color, new Vector2(1, 1));
                vh.AddVert(new Vector3(100,   0, 0), color, new Vector2(1, 0));
                vh.AddTriangle(0, 1, 2);
                vh.AddTriangle(2, 3, 0);

                vh.FillMesh(mesh);

                Assert.AreEqual(4, mesh.vertexCount, "vertex count");
                Assert.AreEqual(1, mesh.subMeshCount, "submesh count");

                var indices = mesh.GetIndices(0);
                Assert.AreEqual(6, indices.Length, "index count");
                Assert.AreEqual(new[] { 0, 1, 2, 2, 3, 0 }, indices, "index data");

                var verts = mesh.vertices;
                Assert.AreEqual(new Vector3(  0,   0, 0), verts[0], "vert 0");
                Assert.AreEqual(new Vector3(  0, 100, 0), verts[1], "vert 1");
                Assert.AreEqual(new Vector3(100, 100, 0), verts[2], "vert 2");
                Assert.AreEqual(new Vector3(100,   0, 0), verts[3], "vert 3");

                var b = mesh.bounds;
                Assert.AreEqual(new Vector3(50, 50, 0), b.center, "bounds center");
                Assert.AreEqual(new Vector3(100, 100, 0), b.size, "bounds size");

                // The submesh's own vertexCount drives the GPU draw call. If FillMesh
                // leaves it at zero (the SubMeshDescriptor default), nothing draws even
                // though mesh.vertexCount is 4.
                var sm = mesh.GetSubMesh(0);
                Assert.AreEqual(6, sm.indexCount, "submesh indexCount");
                Assert.AreEqual(0, sm.indexStart, "submesh indexStart");
                Assert.AreEqual(MeshTopology.Triangles, sm.topology, "submesh topology");
                Assert.AreEqual(4, sm.vertexCount, "submesh vertexCount — 0 means the draw call submits zero verts");

                // The shader binds to attributes by semantic; if our declared attribute
                // set differs from the UI shader's expectations, the GPU reads garbage.
                Assert.IsTrue(mesh.HasVertexAttribute(VertexAttribute.Position), "Position attribute");
                Assert.IsTrue(mesh.HasVertexAttribute(VertexAttribute.Color),    "Color attribute");
                Assert.IsTrue(mesh.HasVertexAttribute(VertexAttribute.TexCoord0), "TexCoord0 attribute");
            }
            finally
            {
                Object.DestroyImmediate(mesh);
            }
        }
    }

    // Reusing the same VertexHelper for a second, smaller stream must produce a Mesh
    // shaped by the second stream alone — no stale tail verts or indices from the first.
    [Test]
    public void FillMesh_ReusedVertexHelper_DoesNotLeakPriorStream()
    {
        using (var vh = new VertexHelper())
        {
            var mesh = new Mesh();
            try
            {
                Color32 color = Color.white;

                // First stream: 4 verts, 6 indices at (0..100).
                vh.AddVert(new Vector3(  0,   0, 0), color, Vector2.zero);
                vh.AddVert(new Vector3(  0, 100, 0), color, Vector2.zero);
                vh.AddVert(new Vector3(100, 100, 0), color, Vector2.zero);
                vh.AddVert(new Vector3(100,   0, 0), color, Vector2.zero);
                vh.AddTriangle(0, 1, 2);
                vh.AddTriangle(2, 3, 0);
                vh.FillMesh(mesh);

                // Second stream: smaller triangle at (0..10).
                vh.Clear();
                vh.AddVert(new Vector3( 0,  0, 0), color, Vector2.zero);
                vh.AddVert(new Vector3( 0, 10, 0), color, Vector2.zero);
                vh.AddVert(new Vector3(10,  0, 0), color, Vector2.zero);
                vh.AddTriangle(0, 1, 2);
                vh.FillMesh(mesh);

                Assert.AreEqual(3, mesh.vertexCount, "vertex count after reuse");
                Assert.AreEqual(3, mesh.GetIndices(0).Length, "index count after reuse");

                var b = mesh.bounds;
                Assert.AreEqual(new Vector3(5, 5, 0), b.center, "bounds center should reflect 10x10 triangle only");
                Assert.AreEqual(new Vector3(10, 10, 0), b.size, "bounds size should reflect 10x10 triangle only");
            }
            finally
            {
                Object.DestroyImmediate(mesh);
            }
        }
    }
}
