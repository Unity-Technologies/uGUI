using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Graphics
{
    // Edit-mode version of GraphicRendersBasicQuadTests. Same scene, same assertions,
    // no Play. Catches regressions where the Graphic→VertexHelper→CanvasRenderer
    // path works at runtime but the SceneView preview ends up with an empty submesh.
    class GraphicRendersInEditModeTests
    {
        // The runtime test assembly's EditModeGraphic isn't visible to editor tests;
        // duplicate the trivial declaration here rather than introduce a reference cycle.
        class EditModeGraphic : Graphic { }

        GameObject m_Root;
        Canvas m_Canvas;
        EditModeGraphic m_Graphic;
        CanvasRenderer m_Renderer;

        [SetUp]
        public void SetUp()
        {
            m_Root = new GameObject("Root", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            m_Canvas = m_Root.GetComponent<Canvas>();
            m_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var graphicGO = new GameObject("Graphic", typeof(RectTransform), typeof(CanvasRenderer), typeof(EditModeGraphic));
            graphicGO.transform.SetParent(m_Root.transform, worldPositionStays: false);
            var rt = graphicGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100, 100);
            rt.anchoredPosition = Vector2.zero;

            m_Graphic = graphicGO.GetComponent<EditModeGraphic>();
            m_Renderer = graphicGO.GetComponent<CanvasRenderer>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_Root);
        }

        [Test]
        public void DefaultGraphic_InEditor_ProducesRenderableMeshOnCanvasRenderer()
        {
            Canvas.ForceUpdateCanvases();

            var mesh = m_Renderer.GetMesh();
            Assert.AreEqual(4, mesh.vertexCount, "CanvasRenderer mesh should hold 4 verts.");
            Assert.AreEqual(6, mesh.GetIndices(0).Length, "CanvasRenderer mesh should have 6 indices.");

            var b = mesh.bounds;
            Assert.That(b.size.x, Is.GreaterThan(0f), "mesh bounds.x must be > 0 — culled otherwise");
            Assert.That(b.size.y, Is.GreaterThan(0f), "mesh bounds.y must be > 0 — culled otherwise");

            var sm = mesh.GetSubMesh(0);
            Assert.AreEqual(4, sm.vertexCount,
                "submesh.vertexCount must match mesh.vertexCount — zero here means nothing draws.");
        }
    }
}
