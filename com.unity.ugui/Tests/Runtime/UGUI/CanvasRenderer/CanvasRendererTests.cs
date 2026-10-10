using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

internal class CanvasRendererTests
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

    // UUM-154589
    [Test]
    public void SetSecondaryTexture_WithNoSlotsAllocated_IsIgnored()
    {
        LogAssert.Expect(LogType.Error, "Failed setting secondary texture. Index is out of bounds.");
        m_CanvasRenderer.SetSecondaryTexture(0, k_MaskTexPropName, m_MaskTex);

        Assert.AreEqual(0, m_CanvasRenderer.GetSecondaryTextureCount(), "SetSecondaryTexture with no slots allocated should not add a slot.");
    }

    // UUM-154589
    [Test]
    public void SetSecondaryTexture_WithIndexPastAllocatedSlots_IsIgnored()
    {
        m_CanvasRenderer.SetSecondaryTextureCount(2);

        LogAssert.Expect(LogType.Error, "Failed setting secondary texture. Index is out of bounds.");
        m_CanvasRenderer.SetSecondaryTexture(3, k_MaskTexPropName, m_MaskTex);

        Assert.AreEqual(2, m_CanvasRenderer.GetSecondaryTextureCount(), "SetSecondaryTexture past the allocated slots should not change the slot count.");
    }

    [Test]
    public void SetSecondaryTextureCount_WithNegativeCount_IsIgnored()
    {
        m_CanvasRenderer.SetSecondaryTextureCount(2);

        LogAssert.Expect(LogType.Error, "Failed setting secondary texture count. Count cannot be negative.");
        m_CanvasRenderer.SetSecondaryTextureCount(-1);

        Assert.AreEqual(2, m_CanvasRenderer.GetSecondaryTextureCount(), "SetSecondaryTextureCount with a negative count should keep the current slot count.");
    }

    // UUM-154589
    [Test]
    public void MaterialCount_WithNegativeValue_IsIgnored()
    {
        m_CanvasRenderer.materialCount = 2;

        LogAssert.Expect(LogType.Error, "Failed setting material count. Count cannot be negative.");
        m_CanvasRenderer.materialCount = -1;

        Assert.AreEqual(2, m_CanvasRenderer.materialCount, "A negative materialCount should keep the current count.");
    }

    // UUM-154589
    [Test]
    public void PopMaterialCount_WithNegativeValue_IsIgnored()
    {
        m_CanvasRenderer.popMaterialCount = 2;

        LogAssert.Expect(LogType.Error, "Failed setting pop material count. Count cannot be negative.");
        m_CanvasRenderer.popMaterialCount = -1;

        Assert.AreEqual(2, m_CanvasRenderer.popMaterialCount, "A negative popMaterialCount should keep the current count.");
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(m_GraphicObj);
    }
}

