using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

[Category("VertexHelper")]
internal class VertexHelperBehaviorTests
{
    // T1: VertexHelper(Mesh) cold-path constructor branches on HasVertexAttribute for
    // every channel. A regression in an "absent channel" path would surface only when
    // the source mesh lacks that channel. Seed from a position-only mesh and assert
    // each absent channel gets the documented default.
    [Test]
    public void Constructor_FromMeshWithOnlyPosition_SeedsDefaultsForAbsentChannels()
    {
        var positions = new[]
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
        };

        var mesh = new Mesh();
        try
        {
            mesh.SetVertices(positions);
            mesh.SetTriangles(new[] { 0, 1, 2 }, 0);

            using (var vh = new VertexHelper(mesh))
            {
                Assert.AreEqual(3, vh.currentVertCount, "vertex count");
                Assert.AreEqual(3, vh.currentIndexCount, "index count");

                var v = new UIVertex();
                for (int i = 0; i < 3; i++)
                {
                    vh.PopulateUIVertex(ref v, i);
                    Assert.AreEqual(positions[i], v.position,                   $"v{i} position");
                    Assert.AreEqual(Vector3.back, v.normal,                     $"v{i} normal default");
                    Assert.AreEqual(new Vector4(1, 0, 0, -1), v.tangent,        $"v{i} tangent default");
                    Assert.AreEqual(new Color32(255, 255, 255, 255), v.color,   $"v{i} color default");
                    Assert.AreEqual(Vector4.zero, v.uv0,                        $"v{i} uv0 default");
                    Assert.AreEqual(Vector4.zero, v.uv1,                        $"v{i} uv1 default");
                    Assert.AreEqual(Vector4.zero, v.uv2,                        $"v{i} uv2 default");
                    Assert.AreEqual(Vector4.zero, v.uv3,                        $"v{i} uv3 default");
                    Assert.AreEqual(Vector4.zero, v.prevPosition,               $"v{i} prevPosition default");
                }
            }
        }
        finally { Object.DestroyImmediate(mesh); }
    }

    // T2: Past the initial capacity (64 verts / 96 indices) VertexHelper's EnsureCapacity
    // grows the backing NativeArray and copies the live range. A bug in the copy length
    // would silently corrupt or drop verts. Encode each vert's index in its position so
    // any off-by-one in the grow copy gets caught on read-back.
    [Test]
    public void GrowPath_VertsPastInitialCapacity_ReadBackIntact()
    {
        using (var vh = new VertexHelper())
        {
            const int count = 65;
            for (int i = 0; i < count; i++)
                vh.AddVert(new Vector3(i, 100 + i, 0), new Color32((byte)(i % 256), 0, 0, 255), new Vector2(i, 0));

            Assert.AreEqual(count, vh.currentVertCount, "vertex count");

            var v = new UIVertex();
            for (int i = 0; i < count; i++)
            {
                vh.PopulateUIVertex(ref v, i);
                Assert.AreEqual(new Vector3(i, 100 + i, 0), v.position,
                    $"vert {i} position — bad grow copy would lose this");
                Assert.AreEqual((byte)(i % 256), v.color.r, $"vert {i} color.r");
                Assert.AreEqual(new Vector4(i, 0, 0, 0), v.uv0, $"vert {i} uv0");
            }
        }
    }

    // T3: A second, smaller FillMesh on the same Mesh must overwrite — no stale tail
    // verts/indices visible. Submesh.vertexCount in particular must reflect the smaller
    // count (this is the field that caused the original "nothing draws" bug).
    [Test]
    public void FillMesh_SmallerSecondFill_DoesNotLeakPriorStream()
    {
        using (var vh = new VertexHelper())
        {
            var mesh = new Mesh();
            try
            {
                // First fill: 100 verts / 300 indices (100 triangles).
                for (int i = 0; i < 100; i++)
                    vh.AddVert(new Vector3(i, 0, 0), Color.white, Vector2.zero);
                for (int i = 0; i < 100; i++)
                    vh.AddTriangle(i, (i + 1) % 100, (i + 2) % 100);
                vh.FillMesh(mesh);

                Assert.AreEqual(100, mesh.vertexCount, "first fill vert count");
                Assert.AreEqual(300, mesh.GetIndices(0).Length, "first fill index count");

                // Smaller second fill.
                vh.Clear();
                vh.AddVert(new Vector3(0, 0, 0), Color.white, Vector2.zero);
                vh.AddVert(new Vector3(0, 1, 0), Color.white, Vector2.zero);
                vh.AddVert(new Vector3(1, 1, 0), Color.white, Vector2.zero);
                vh.AddVert(new Vector3(1, 0, 0), Color.white, Vector2.zero);
                vh.AddTriangle(0, 1, 2);
                vh.AddTriangle(2, 3, 0);
                vh.FillMesh(mesh);

                Assert.AreEqual(4, mesh.vertexCount, "second fill vert count");
                Assert.AreEqual(6, mesh.GetIndices(0).Length, "second fill index count");

                var sm = mesh.GetSubMesh(0);
                Assert.AreEqual(6, sm.indexCount, "submesh.indexCount must shrink");
                Assert.AreEqual(4, sm.vertexCount,
                    "submesh.vertexCount must reflect the smaller second fill — otherwise the GPU draw " +
                    "would submit the stale 100-vert count and either render garbage or crash.");
            }
            finally { Object.DestroyImmediate(mesh); }
        }
    }

    // T4: Each AddVert overload writes a UIVertex with overload-specific defaults for the
    // fields it doesn't take. A typo (e.g. swapping normal and tangent in one overload) would
    // only surface for callers using that specific overload — easy to ship undetected.
    [Test]
    public void AddVert_AllOverloads_WriteExpectedFields()
    {
        using (var vh = new VertexHelper())
        {
            // 1) Full overload (9 args, including prevPosition).
            vh.AddVert(
                position: new Vector3(1, 2, 3),
                color: new Color32(10, 20, 30, 40),
                uv0: new Vector4(0.1f, 0.2f, 0.3f, 0.4f),
                uv1: new Vector4(0.5f, 0.6f, 0.7f, 0.8f),
                uv2: new Vector4(1.1f, 1.2f, 1.3f, 1.4f),
                uv3: new Vector4(1.5f, 1.6f, 1.7f, 1.8f),
                normal: new Vector3(0, 1, 0),
                tangent: new Vector4(1, 0, 0, 1),
                prevPosition: new Vector4(9, 8, 7, 6));

            // 2) 8-arg (no prevPosition → defaults to Vector4.zero).
            vh.AddVert(
                position: new Vector3(11, 12, 13),
                color: new Color32(50, 60, 70, 80),
                uv0: new Vector4(2.1f, 2.2f, 2.3f, 2.4f),
                uv1: new Vector4(2.5f, 2.6f, 2.7f, 2.8f),
                uv2: new Vector4(3.1f, 3.2f, 3.3f, 3.4f),
                uv3: new Vector4(3.5f, 3.6f, 3.7f, 3.8f),
                normal: new Vector3(0, 0, 1),
                tangent: new Vector4(0, 1, 0, 1));

            // 3) 6-arg (only uv0+uv1; uv2/uv3 default zero).
            vh.AddVert(
                position: new Vector3(21, 22, 23),
                color: new Color32(90, 100, 110, 120),
                uv0: new Vector4(4.1f, 4.2f, 4.3f, 4.4f),
                uv1: new Vector4(4.5f, 4.6f, 4.7f, 4.8f),
                normal: new Vector3(1, 0, 0),
                tangent: new Vector4(0, 0, 1, 1));

            // 4) 3-arg (default normal + tangent; uv1-3 default zero).
            vh.AddVert(
                position: new Vector3(31, 32, 33),
                color: new Color32(130, 140, 150, 160),
                uv0: new Vector4(5.1f, 5.2f, 5.3f, 5.4f));

            // 5) UIVertex shorthand (straight assignment).
            var src = new UIVertex
            {
                position = new Vector3(41, 42, 43),
                normal = new Vector3(0.5f, 0.5f, 0.5f),
                tangent = new Vector4(0.1f, 0.2f, 0.3f, 0.4f),
                color = new Color32(170, 180, 190, 200),
                uv0 = new Vector4(6, 7, 8, 9),
                uv1 = new Vector4(10, 11, 12, 13),
                uv2 = new Vector4(14, 15, 16, 17),
                uv3 = new Vector4(18, 19, 20, 21),
                prevPosition = new Vector4(22, 23, 24, 25),
            };
            vh.AddVert(src);

            Assert.AreEqual(5, vh.currentVertCount);

            var v = new UIVertex();

            // Overload 1: full
            vh.PopulateUIVertex(ref v, 0);
            Assert.AreEqual(new Vector3(1, 2, 3), v.position,                   "ov1 position");
            Assert.AreEqual(new Vector3(0, 1, 0), v.normal,                     "ov1 normal");
            Assert.AreEqual(new Vector4(1, 0, 0, 1), v.tangent,                 "ov1 tangent");
            Assert.AreEqual(new Color32(10, 20, 30, 40), v.color,               "ov1 color");
            Assert.AreEqual(new Vector4(0.1f, 0.2f, 0.3f, 0.4f), v.uv0,         "ov1 uv0");
            Assert.AreEqual(new Vector4(0.5f, 0.6f, 0.7f, 0.8f), v.uv1,         "ov1 uv1");
            Assert.AreEqual(new Vector4(1.1f, 1.2f, 1.3f, 1.4f), v.uv2,         "ov1 uv2");
            Assert.AreEqual(new Vector4(1.5f, 1.6f, 1.7f, 1.8f), v.uv3,         "ov1 uv3");
            Assert.AreEqual(new Vector4(9, 8, 7, 6), v.prevPosition,            "ov1 prevPosition");

            // Overload 2: no prevPosition → Vector4.zero
            vh.PopulateUIVertex(ref v, 1);
            Assert.AreEqual(new Vector3(11, 12, 13), v.position,                "ov2 position");
            Assert.AreEqual(new Vector3(0, 0, 1), v.normal,                     "ov2 normal");
            Assert.AreEqual(new Vector4(0, 1, 0, 1), v.tangent,                 "ov2 tangent");
            Assert.AreEqual(new Vector4(2.1f, 2.2f, 2.3f, 2.4f), v.uv0,         "ov2 uv0");
            Assert.AreEqual(new Vector4(3.5f, 3.6f, 3.7f, 3.8f), v.uv3,         "ov2 uv3");
            Assert.AreEqual(Vector4.zero, v.prevPosition,                       "ov2 prevPosition default");

            // Overload 3: only uv0+uv1 → uv2/uv3 default zero
            vh.PopulateUIVertex(ref v, 2);
            Assert.AreEqual(new Vector3(21, 22, 23), v.position,                "ov3 position");
            Assert.AreEqual(new Vector3(1, 0, 0), v.normal,                     "ov3 normal");
            Assert.AreEqual(new Vector4(4.1f, 4.2f, 4.3f, 4.4f), v.uv0,         "ov3 uv0");
            Assert.AreEqual(new Vector4(4.5f, 4.6f, 4.7f, 4.8f), v.uv1,         "ov3 uv1");
            Assert.AreEqual(Vector4.zero, v.uv2,                                "ov3 uv2 default");
            Assert.AreEqual(Vector4.zero, v.uv3,                                "ov3 uv3 default");
            Assert.AreEqual(Vector4.zero, v.prevPosition,                       "ov3 prevPosition default");

            // Overload 4: default normal/tangent + uv1-3 default zero
            vh.PopulateUIVertex(ref v, 3);
            Assert.AreEqual(new Vector3(31, 32, 33), v.position,                "ov4 position");
            Assert.AreEqual(Vector3.back, v.normal,                             "ov4 normal default");
            Assert.AreEqual(new Vector4(1, 0, 0, -1), v.tangent,                "ov4 tangent default");
            Assert.AreEqual(new Vector4(5.1f, 5.2f, 5.3f, 5.4f), v.uv0,         "ov4 uv0");
            Assert.AreEqual(Vector4.zero, v.uv1,                                "ov4 uv1 default");
            Assert.AreEqual(Vector4.zero, v.uv2,                                "ov4 uv2 default");
            Assert.AreEqual(Vector4.zero, v.uv3,                                "ov4 uv3 default");

            // Overload 5: UIVertex shorthand — every field carried through verbatim.
            vh.PopulateUIVertex(ref v, 4);
            Assert.AreEqual(src.position, v.position,         "ov5 position");
            Assert.AreEqual(src.normal, v.normal,             "ov5 normal");
            Assert.AreEqual(src.tangent, v.tangent,           "ov5 tangent");
            Assert.AreEqual(src.color, v.color,               "ov5 color");
            Assert.AreEqual(src.uv0, v.uv0,                   "ov5 uv0");
            Assert.AreEqual(src.uv1, v.uv1,                   "ov5 uv1");
            Assert.AreEqual(src.uv2, v.uv2,                   "ov5 uv2");
            Assert.AreEqual(src.uv3, v.uv3,                   "ov5 uv3");
            Assert.AreEqual(src.prevPosition, v.prevPosition, "ov5 prevPosition");
        }
    }

    // T5: After Dispose, subsequent AddVert/FillMesh must reinitialize the backing
    // NativeArrays (InitializeIfRequired handles this). The Graphic.cs editor cleanup
    // handlers explicitly Dispose s_VertexHelper on play-mode boundaries, so this
    // lifecycle is exercised in real workflows.
    [Test]
    public void DisposeThenReuse_ReallocatesBackingStorageAndProducesValidMesh()
    {
        var vh = new VertexHelper();
        var mesh = new Mesh();
        try
        {
            vh.AddVert(new Vector3(0, 0, 0), Color.white, Vector2.zero);
            vh.AddVert(new Vector3(1, 0, 0), Color.white, Vector2.zero);
            vh.AddVert(new Vector3(0, 1, 0), Color.white, Vector2.zero);
            vh.AddTriangle(0, 1, 2);
            vh.FillMesh(mesh);
            Assert.AreEqual(3, mesh.vertexCount, "first fill vert count");

            vh.Dispose();

            // Second use after Dispose. Must not crash and must produce a valid mesh.
            vh.AddVert(new Vector3(0, 0, 0), Color.white, Vector2.zero);
            vh.AddVert(new Vector3(2, 0, 0), Color.white, Vector2.zero);
            vh.AddVert(new Vector3(0, 2, 0), Color.white, Vector2.zero);
            vh.AddVert(new Vector3(2, 2, 0), Color.white, Vector2.zero);
            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
            vh.FillMesh(mesh);

            Assert.AreEqual(4, mesh.vertexCount, "second fill vert count after Dispose+reuse");
            Assert.AreEqual(6, mesh.GetIndices(0).Length, "second fill index count after Dispose+reuse");

            var v = new UIVertex();
            vh.PopulateUIVertex(ref v, 3);
            Assert.AreEqual(new Vector3(2, 2, 0), v.position, "vert 3 position after reuse");
        }
        finally
        {
            vh.Dispose();
            Object.DestroyImmediate(mesh);
        }
    }
}
