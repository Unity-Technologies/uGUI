using UnityEngine;
using NUnit.Framework;

public class CanvasRendererTests
{
    private const int Width = 32;
    private const int Height = 32;
    GameObject m_GraphicObj;
    CanvasRenderer m_CanvasRenderer;
    private Texture2D m_DefaultTexture;
    static readonly string k_MaskTexPropName = "_MaskTex";
    static readonly string k_GlowTexPropName = "_GlowTex";
    Texture2D m_MaskTex;
    Texture2D m_GlowTex;

    [SetUp]
    public void SetUp()
    {
        m_GraphicObj = new GameObject("Graphic");
        m_CanvasRenderer = m_GraphicObj.AddComponent<CanvasRenderer>();
        m_MaskTex = CreateTexture(Color.red);
        m_GlowTex = CreateTexture(Color.yellow);
    }

    Texture2D CreateTexture(Color color)
    {
        var tex = new Texture2D(Width, Height);
        Color[] colors = new Color[Width * Height];
        for (int i = 0; i < Width * Height; i++)
            colors[i] = color;
        tex.SetPixels(colors);
        tex.Apply();
        return tex;
    }

    [Test]
    public void InitialData()
    {
        Assert.AreEqual(0, m_CanvasRenderer.GetSecondaryTextureCount());
    }

    [Test]
    public void AddSecondaryTextures()
    {
        m_CanvasRenderer.SetSecondaryTextureCount(2);

        Assert.AreEqual(2, m_CanvasRenderer.GetSecondaryTextureCount());
        Assert.True(string.IsNullOrEmpty(m_CanvasRenderer.GetSecondaryTextureName(0)));
        Assert.Null(m_CanvasRenderer.GetSecondaryTexture(0));
        Assert.True(string.IsNullOrEmpty(m_CanvasRenderer.GetSecondaryTextureName(1)));
        Assert.Null(m_CanvasRenderer.GetSecondaryTexture(1));

        m_CanvasRenderer.SetSecondaryTexture(0, k_MaskTexPropName, m_MaskTex);
        m_CanvasRenderer.SetSecondaryTexture(1, k_GlowTexPropName, m_GlowTex);

        Assert.AreEqual(k_MaskTexPropName, m_CanvasRenderer.GetSecondaryTextureName(0));
        Assert.AreEqual(m_MaskTex, m_CanvasRenderer.GetSecondaryTexture(0));
        Assert.AreEqual(k_GlowTexPropName, m_CanvasRenderer.GetSecondaryTextureName(1));
        Assert.AreEqual(m_GlowTex, m_CanvasRenderer.GetSecondaryTexture(1));
    }

    [Test]
    public void RemoveSecondaryTextures()
    {
        m_CanvasRenderer.SetSecondaryTextureCount(2);
        m_CanvasRenderer.SetSecondaryTexture(0, k_MaskTexPropName, m_MaskTex);
        m_CanvasRenderer.SetSecondaryTexture(1, k_GlowTexPropName, m_GlowTex);

        // The last secondary texture
        m_CanvasRenderer.SetSecondaryTextureCount(1);

        Assert.AreEqual(1, m_CanvasRenderer.GetSecondaryTextureCount());
        Assert.AreEqual(k_MaskTexPropName, m_CanvasRenderer.GetSecondaryTextureName(0));
        Assert.AreEqual(m_MaskTex, m_CanvasRenderer.GetSecondaryTexture(0));
    }

    [Test]
    public void SetSecondaryTextureCount()
    {
        m_CanvasRenderer.SetSecondaryTextureCount(2);
        m_CanvasRenderer.SetSecondaryTexture(0, k_MaskTexPropName, m_MaskTex);
        m_CanvasRenderer.SetSecondaryTexture(1, k_GlowTexPropName, m_GlowTex);

        m_CanvasRenderer.SetSecondaryTextureCount(1);

        Assert.AreEqual(1, m_CanvasRenderer.GetSecondaryTextureCount());
        Assert.AreEqual(k_MaskTexPropName, m_CanvasRenderer.GetSecondaryTextureName(0));
        Assert.AreEqual(m_MaskTex, m_CanvasRenderer.GetSecondaryTexture(0));

        // Increase the number of secondary textures and verify that the new entries are empty
        m_CanvasRenderer.SetSecondaryTextureCount(3);

        Assert.AreEqual(3, m_CanvasRenderer.GetSecondaryTextureCount());
        Assert.AreEqual(k_MaskTexPropName, m_CanvasRenderer.GetSecondaryTextureName(0));
        Assert.AreEqual(m_MaskTex, m_CanvasRenderer.GetSecondaryTexture(0));
        Assert.True(string.IsNullOrEmpty(m_CanvasRenderer.GetSecondaryTextureName(1)));
        Assert.Null(m_CanvasRenderer.GetSecondaryTexture(1));
        Assert.True(string.IsNullOrEmpty(m_CanvasRenderer.GetSecondaryTextureName(2)));
        Assert.Null(m_CanvasRenderer.GetSecondaryTexture(2));

        // Clear all the secondary textures
        m_CanvasRenderer.SetSecondaryTextureCount(0);

        Assert.AreEqual(0, m_CanvasRenderer.GetSecondaryTextureCount());

        // Add an element again and verify that it is empty
        m_CanvasRenderer.SetSecondaryTextureCount(1);

        Assert.True(string.IsNullOrEmpty(m_CanvasRenderer.GetSecondaryTextureName(0)));
        Assert.Null(m_CanvasRenderer.GetSecondaryTexture(0));
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(m_GraphicObj);
    }
}

