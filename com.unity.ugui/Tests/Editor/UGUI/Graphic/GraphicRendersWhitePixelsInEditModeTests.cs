using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Graphics
{
    // Pixel-level edit-mode end-to-end test. The earlier mesh-state tests cover
    // the data layer (mesh.vertexCount, submesh.vertexCount, bounds). This one
    // catches regressions further downstream by actually rendering a Canvas+Image
    // through a Camera into a RenderTexture and asserting the pixel under the
    // Image is the Image's color.
    //
    // Uses RenderMode.ScreenSpaceCamera: ScreenSpaceOverlay can't be captured
    // in batchmode (no display surface), and WorldSpace requires more transform
    // setup. ScreenSpaceCamera exercises the same Graphic → VertexHelper.FillMesh
    // → CanvasRenderer.SetMesh → Canvas batch → GPU draw pipeline.
    class GraphicRendersWhitePixelsInEditModeTests
    {
        class EditModeGraphic : Graphic { }

        GameObject m_CanvasRoot;
        GameObject m_CameraGO;
        Camera m_Camera;
        RenderTexture m_RT;

        const int kRTSize = 64;

        [SetUp]
        public void SetUp()
        {
            m_RT = new RenderTexture(kRTSize, kRTSize, 0, RenderTextureFormat.ARGB32) { name = "ImageRenderTest" };
            m_RT.Create();

            m_CameraGO = new GameObject("Camera", typeof(Camera));
            m_Camera = m_CameraGO.GetComponent<Camera>();
            m_Camera.clearFlags = CameraClearFlags.SolidColor;
            m_Camera.backgroundColor = Color.black;
            m_Camera.targetTexture = m_RT;
            m_Camera.orthographic = true;
            m_Camera.orthographicSize = kRTSize * 0.5f;
            m_Camera.nearClipPlane = 0.1f;
            m_Camera.farClipPlane = 100f;
            m_CameraGO.transform.position = new Vector3(0, 0, -10);

            m_CanvasRoot = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = m_CanvasRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = m_Camera;
            canvas.planeDistance = 1f;

            var graphicGO = new GameObject("Image", typeof(RectTransform), typeof(CanvasRenderer), typeof(EditModeGraphic));
            graphicGO.transform.SetParent(m_CanvasRoot.transform, worldPositionStays: false);
            var rt = graphicGO.GetComponent<RectTransform>();
            // Anchor stretch to full canvas so the Image covers the entire render texture.
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_CanvasRoot);
            Object.DestroyImmediate(m_CameraGO);
            if (m_RT != null)
            {
                m_RT.Release();
                Object.DestroyImmediate(m_RT);
            }
        }

        [Test]
        public void DefaultGraphic_RendersWhitePixels_InEditMode()
        {
            Canvas.ForceUpdateCanvases();
            m_Camera.Render();

            // Read the center pixel — should be fully covered by the white Image.
            var prev = RenderTexture.active;
            RenderTexture.active = m_RT;
            try
            {
                var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                try
                {
                    tex.ReadPixels(new Rect(kRTSize / 2, kRTSize / 2, 1, 1), 0, 0);
                    tex.Apply();
                    var c = tex.GetPixel(0, 0);
                    Assert.That(c.r, Is.GreaterThan(0.5f),
                        $"Center pixel should be white-ish (Image color), got {c}. If R is ~0 the Image didn't render.");
                    Assert.That(c.g, Is.GreaterThan(0.5f), $"Center pixel green channel was {c.g}.");
                    Assert.That(c.b, Is.GreaterThan(0.5f), $"Center pixel blue channel was {c.b}.");
                }
                finally { Object.DestroyImmediate(tex); }
            }
            finally { RenderTexture.active = prev; }
        }
    }
}
