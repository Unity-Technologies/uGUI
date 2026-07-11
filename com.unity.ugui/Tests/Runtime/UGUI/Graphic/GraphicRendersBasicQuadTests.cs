using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.UI.Tests;

namespace Graphics
{
    // End-to-end smoke test for the Graphic → VertexHelper → CanvasRenderer mesh path.
    // The bug we're guarding against: an Image (or any Graphic) on a Canvas produces no
    // renderable mesh on the CanvasRenderer, even though OnPopulateMesh and FillMesh
    // appear to work in isolation.
    class GraphicRendersBasicQuadTests
    {
        GameObject m_Root;
        Canvas m_Canvas;
        ConcreteGraphic m_Graphic;
        CanvasRenderer m_Renderer;

        [SetUp]
        public void SetUp()
        {
            m_Root = new GameObject("Root", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            m_Canvas = m_Root.GetComponent<Canvas>();
            m_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var graphicGO = new GameObject("Graphic", typeof(RectTransform), typeof(CanvasRenderer), typeof(ConcreteGraphic));
            graphicGO.transform.SetParent(m_Root.transform, worldPositionStays: false);
            var rt = graphicGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100, 100);
            rt.anchoredPosition = Vector2.zero;

            m_Graphic = graphicGO.GetComponent<ConcreteGraphic>();
            m_Renderer = graphicGO.GetComponent<CanvasRenderer>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_Root);
        }

        // Reproduces the "Image in a Canvas doesn't draw" regression: after the
        // canvas update completes, the CanvasRenderer must hold a mesh with the
        // 4 corner verts from the default Graphic.OnPopulateMesh output.
        [UnityTest]
        public IEnumerator DefaultGraphic_OnCanvas_ProducesRenderableMeshOnCanvasRenderer()
        {
            // Let canvas update fire (the registry batches rebuilds per-frame).
            yield return null;
            Canvas.ForceUpdateCanvases();

            // Sanity: managed-side mesh count after rebuild should be 4 verts / 6 indices.
            Assert.AreEqual(4, m_Renderer.GetMesh().vertexCount,
                "CanvasRenderer should hold the 4-vert quad produced by Graphic.OnPopulateMesh; " +
                "if this is 0, FillMesh + SetMesh dropped the mesh on the floor.");
            Assert.AreEqual(6, m_Renderer.GetMesh().GetIndices(0).Length,
                "CanvasRenderer mesh should have 6 indices (two triangles).");

            // Bounds must be non-empty — a zero-bounds mesh gets culled and never draws.
            var bounds = m_Renderer.GetMesh().bounds;
            Assert.That(bounds.size.x, Is.GreaterThan(0f), "mesh bounds.x must be > 0 — culled otherwise");
            Assert.That(bounds.size.y, Is.GreaterThan(0f), "mesh bounds.y must be > 0 — culled otherwise");

            // The submesh's vertexCount drives the GPU draw call. If it's zero, the
            // mesh-level vertexCount above doesn't help — the draw submits zero verts
            // and the Image renders nothing. (This was the actual regression.)
            var sm = m_Renderer.GetMesh().GetSubMesh(0);
            Assert.AreEqual(4, sm.vertexCount,
                "submesh.vertexCount must match mesh.vertexCount — zero here means nothing draws.");
        }
    }
}
