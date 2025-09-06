using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.TestTools.Utils;

[TestFixture]
internal class CheckMeshColorsAndColors32Match
{
    GameObject m_CanvasGO;
    GameObject m_ColorMeshGO;
    GameObject m_Color32MeshGO;
    GameObject m_CameraGO;
    Texture2D m_ScreenTexture;
    RenderTexture m_ScreenRenderTexture;
    Camera m_ScreenCamera;

    [SetUp]
    public void TestSetup()
    {
        // Create Camera
        m_ScreenRenderTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
        m_ScreenRenderTexture.Create();
        m_CameraGO = new GameObject("Camera");
        m_ScreenCamera = m_CameraGO.AddComponent<Camera>();
        m_ScreenCamera.orthographic = true;
        m_ScreenCamera.orthographicSize = 1;
        m_ScreenCamera.targetTexture = m_ScreenRenderTexture;

        // Create Canvas
        m_CanvasGO = new GameObject("Canvas");
        Canvas canvas = m_CanvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = m_ScreenCamera;

        // Create Color UI GameObject
        m_ColorMeshGO = new GameObject("ColorMesh");
        CanvasRenderer colorMeshCanvasRenderer = m_ColorMeshGO.AddComponent<CanvasRenderer>();
        RectTransform colorMeshRectTransform = m_ColorMeshGO.AddComponent<RectTransform>();
        colorMeshRectTransform.pivot = colorMeshRectTransform.anchorMin = colorMeshRectTransform.anchorMax = Vector2.zero;
        m_ColorMeshGO.transform.SetParent(m_CanvasGO.transform);
        m_ColorMeshGO.transform.localPosition = Vector3.zero;

        // Create Color32 UI GameObject
        m_Color32MeshGO = new GameObject("Color32Mesh");
        CanvasRenderer color32MeshCanvasRenderer = m_Color32MeshGO.AddComponent<CanvasRenderer>();
        RectTransform color32MeshRectTransform = m_Color32MeshGO.AddComponent<RectTransform>();
        color32MeshRectTransform.pivot = color32MeshRectTransform.anchorMin = color32MeshRectTransform.anchorMax = Vector2.zero;
        m_Color32MeshGO.transform.SetParent(m_CanvasGO.transform);
        m_Color32MeshGO.transform.localPosition = Vector3.zero;

        Material material = new Material(Shader.Find("UI/Default"));

        // Setup Color mesh and add it to Color CanvasRenderer
        Mesh meshColor = new Mesh();
        meshColor.vertices = new Vector3[3] { new Vector3(0, 0, 0), new Vector3(0, -3, 0), new Vector3(-3, 0, 0) };
        meshColor.triangles = new int[3] { 0, 1, 2 };
        meshColor.normals = new Vector3[3] { Vector3.zero, Vector3.zero, Vector3.zero };
        meshColor.colors = new Color[3] { Color.white, Color.white, Color.white };
        meshColor.uv = new Vector2[3] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0) };

        colorMeshCanvasRenderer.SetMesh(meshColor);
        colorMeshCanvasRenderer.SetMaterial(material, null);

        // Setup Color32 mesh and add it to Color32 CanvasRenderer
        Mesh meshColor32 = new Mesh();
        meshColor32.vertices = new Vector3[3] { new Vector3(0, 0, 0), new Vector3(0, 3, 0), new Vector3(3, 0, 0) };
        meshColor32.triangles = new int[3] { 0, 1, 2 };
        meshColor32.normals = new Vector3[3] { Vector3.zero, Vector3.zero, Vector3.zero };
        meshColor32.colors32 = new Color32[3] { Color.white, Color.white, Color.white };
        meshColor32.uv = new Vector2[3] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0) };

        color32MeshCanvasRenderer.SetMesh(meshColor32);
        color32MeshCanvasRenderer.SetMaterial(material, null);
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(m_CanvasGO);
        m_ScreenCamera.targetTexture = null;
        GameObject.DestroyImmediate(m_ScreenRenderTexture);
        GameObject.DestroyImmediate(m_CameraGO);
    }

    [UnityTest]
    public IEnumerator CheckMeshColorsAndColors32Matches()
    {
        Assert.That(m_ScreenRenderTexture.IsCreated(), "RenderTexture is not created");

        Texture2D screenTexture = new Texture2D(m_ScreenRenderTexture.width, m_ScreenRenderTexture.height);
        RenderTexture.active = m_ScreenRenderTexture;
        screenTexture.ReadPixels(new Rect(0, 0, m_ScreenRenderTexture.width, m_ScreenRenderTexture.height), 0, 0);
        screenTexture.Apply();

        yield return null;

        Color screenPixelColorForMeshColor = screenTexture.GetPixel(Screen.width / 2 + 100, Screen.height / 2 + 100);
        Color screenPixelColorForMesh32Color = screenTexture.GetPixel(100, 100);
        Color screenPixelOutsideMesh = screenTexture.GetPixel(Screen.width / 2 + 100, 100);

        Assert.That(screenPixelColorForMesh32Color, Is.EqualTo(screenPixelColorForMeshColor).Using(new ColorEqualityComparer(0.0f)), "UI Mesh with Colors does not match UI Mesh with Colors32");
        Assert.That(screenPixelOutsideMesh, Is.Not.EqualTo(screenPixelColorForMeshColor).Using(new ColorEqualityComparer(0.0f)), "Empty space should not be matching the color of the UI Mesh");
    }
}
