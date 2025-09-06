using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.UI;
using System.Reflection;

internal class ImageTests
{
    private const int Width = 32;
    private const int Height = 32;

    GameObject m_CanvasGO;
    TestableImage m_Image;
    private Texture2D m_DefaultTexture;
    private static SecondarySpriteTexture [] s_EmptySecondaryTexArray = {};

    private bool m_dirtyLayout;
    private bool m_dirtyMaterial;

    [SetUp]
    public void SetUp()
    {
        m_CanvasGO = new GameObject("Canvas", typeof(Canvas));
        GameObject imageObject = new GameObject("Image", typeof(TestableImage));
        imageObject.transform.SetParent(m_CanvasGO.transform);
        m_Image = imageObject.GetComponent<TestableImage>();
        m_Image.RegisterDirtyLayoutCallback(() => m_dirtyLayout = true);
        m_Image.RegisterDirtyMaterialCallback(() => m_dirtyMaterial = true);

        m_DefaultTexture = CreateTexture(Color.magenta);
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
    public void TightMeshSpritePopulatedVertexHelperProperly()
    {
        Texture2D texture = new Texture2D(64, 64);
        m_Image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
        m_Image.type = Image.Type.Simple;
        m_Image.useSpriteMesh = true;

        VertexHelper vh = new VertexHelper();

        m_Image.GenerateImageData(vh);

        Assert.AreEqual(vh.currentVertCount, m_Image.sprite.vertices.Length);
        Assert.AreEqual(vh.currentIndexCount, m_Image.sprite.triangles.Length);
    }

    [UnityTest]
    public IEnumerator CanvasCustomRefPixPerUnitToggleWillUpdateImageMesh()
    {
        var canvas = m_CanvasGO.GetComponent<Canvas>();
        var canvasScaler = m_CanvasGO.AddComponent<CanvasScaler>();

        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        m_Image.transform.SetParent(m_CanvasGO.transform);
        m_Image.type = Image.Type.Sliced;
        var texture = new Texture2D(120, 120);
        m_Image.sprite = Sprite.Create(texture, new Rect(0, 0, 120, 120), new Vector2(0.5f, 0.5f), 100, 1, SpriteMeshType.Tight, new Vector4(30, 30, 30, 30), true);
        m_Image.fillCenter = true;


        canvasScaler.referencePixelsPerUnit = 200;
        yield return null; // skip frame to update canvas properly
        //setup done

        canvas.enabled = false;

        yield return null;

        canvas.enabled = true;
        m_Image.isOnPopulateMeshCalled = false;

        yield return null;

        Assert.IsTrue(m_Image.isOnPopulateMeshCalled);
    }

    [UnityTest]
    public IEnumerator Sprite_Layout()
    {
        m_Image.sprite = Sprite.Create(m_DefaultTexture, new Rect(0, 0, Width, Height), Vector2.zero);
        yield return null;

        m_Image.isGeometryUpdated = false;
        m_dirtyLayout = false;

        var Texture = new Texture2D(Width * 2, Height * 2);
        m_Image.sprite = Sprite.Create(Texture, new Rect(0, 0, Width, Height), Vector2.zero);
        yield return new WaitUntil(() => m_Image.isGeometryUpdated);

        // validate that layout change rebuil is not called
        Assert.IsFalse(m_dirtyLayout);

        m_Image.isGeometryUpdated = false;
        m_dirtyLayout = false;
        m_Image.sprite = Sprite.Create(Texture, new Rect(0, 0, Width / 2, Height / 2), Vector2.zero);
        yield return new WaitUntil(() => m_Image.isGeometryUpdated);

        // validate that layout change rebuil is called
        Assert.IsTrue(m_dirtyLayout);
    }

    [UnityTest]
    public IEnumerator Sprite_Material()
    {
        m_Image.sprite = Sprite.Create(m_DefaultTexture, new Rect(0, 0, Width, Height), Vector2.zero);
        yield return null;

        m_Image.isGeometryUpdated = false;
        m_dirtyMaterial = false;
        m_Image.sprite = Sprite.Create(m_DefaultTexture, new Rect(0, 0, Width / 2, Height / 2), Vector2.zero);
        yield return new WaitUntil(() => m_Image.isGeometryUpdated);

        // validate that material change rebuild is not called
        Assert.IsFalse(m_dirtyMaterial);

        m_Image.isGeometryUpdated = false;
        m_dirtyMaterial = false;
        var Texture = new Texture2D(Width * 2, Height * 2);
        m_Image.sprite = Sprite.Create(Texture, new Rect(0, 0, Width / 2, Height / 2), Vector2.zero);
        yield return new WaitUntil(() => m_Image.isGeometryUpdated);

        // validate that layout change rebuil is called
        Assert.IsTrue(m_dirtyMaterial);
    }

    IEnumerator ValidateSecondaryTextures(SecondarySpriteTexture[] expectedSecondaryTextures)
    {
        yield return new WaitUntil(() => m_Image.isMaterialUpdated);

        if (expectedSecondaryTextures.Length == 0)
            Assert.Null(m_Image.secondaryTextures);
        else
        {
            Assert.NotNull(m_Image.secondaryTextures);
            Assert.AreEqual(expectedSecondaryTextures.Length, m_Image.secondaryTextures.Length);
        }
        Assert.AreEqual(expectedSecondaryTextures.Length, m_Image.canvasRenderer.GetSecondaryTextureCount());

        for (int i = 0; i < expectedSecondaryTextures.Length; ++i)
        {
            Assert.AreEqual(expectedSecondaryTextures[i].name, m_Image.secondaryTextures[i].name);
            Assert.AreEqual(expectedSecondaryTextures[i].texture, m_Image.secondaryTextures[i].texture);

            // Canvas Renderer
            Assert.AreEqual(expectedSecondaryTextures[i].name, m_Image.canvasRenderer.GetSecondaryTextureName(i));
            Assert.AreEqual(expectedSecondaryTextures[i].texture, m_Image.canvasRenderer.GetSecondaryTexture(i));
        }
    }

    [UnityTest]
    public IEnumerator Sprite_NoSecondaryTextures()
    {
        var sprite = Sprite.Create(m_DefaultTexture, new Rect(0, 0, Width, Height), Vector2.zero);

        m_dirtyMaterial = false;
        m_Image.isMaterialUpdated = false;
        m_Image.sprite = sprite;
        Assert.IsTrue(m_dirtyMaterial);

        yield return ValidateSecondaryTextures(s_EmptySecondaryTexArray);
    }

    [UnityTest]
    public IEnumerator Sprite_SecondaryTextures()
    {
        var secondaryTextures = new[]
        {
            new SecondarySpriteTexture()
            {
                name = "_MaskTex",
                texture = CreateTexture(Color.red)
            },
            new SecondarySpriteTexture()
            {
                name = "_GlowTex",
                texture = CreateTexture(Color.yellow)
            }
        };

        var sprite = Sprite.Create(m_DefaultTexture, new Rect(0, 0, Width, Height), Vector2.zero, 100, 0, SpriteMeshType.FullRect, Vector4.zero, false, secondaryTextures);

        m_dirtyMaterial = false;
        m_Image.isMaterialUpdated = false;
        m_Image.sprite = sprite;
        Assert.IsTrue(m_dirtyMaterial);

        yield return ValidateSecondaryTextures(secondaryTextures);
    }

    [UnityTest]
    public IEnumerator Sprite_SecondaryTexturesUpdatedAfterSpriteChanged()
    {
        var spriteWithNoSecTex = Sprite.Create(m_DefaultTexture, new Rect(0, 0, Width, Height), Vector2.zero);

        m_dirtyMaterial = false;
        m_Image.isMaterialUpdated = false;
        m_Image.sprite = spriteWithNoSecTex;
        Assert.IsTrue(m_dirtyMaterial);

        yield return ValidateSecondaryTextures(s_EmptySecondaryTexArray);

        var twoSecondaryTextures = new[]
        {
            new SecondarySpriteTexture()
            {
                name = "_MaskTex",
                texture = CreateTexture(Color.red)
            },
            new SecondarySpriteTexture()
            {
                name = "_GlowTex",
                texture = CreateTexture(Color.yellow)
            }
        };

        var spriteWithTwoSecTexs = Sprite.Create(m_DefaultTexture, new Rect(0, 0, Width, Height), Vector2.zero, 100, 0, SpriteMeshType.FullRect, Vector4.zero, false, twoSecondaryTextures);

        m_dirtyMaterial = false;
        m_Image.isMaterialUpdated = false;
        m_Image.sprite = spriteWithTwoSecTexs;
        Assert.IsTrue(m_dirtyMaterial);

        yield return ValidateSecondaryTextures(twoSecondaryTextures);

        var oneSecondaryTexture = new[]
        {
            new SecondarySpriteTexture()
            {
                name = "_MaskTex",
                texture = CreateTexture(Color.red)
            }
        };
        var spriteWithOneSecTex = Sprite.Create(m_DefaultTexture, new Rect(0, 0, Width, Height), Vector2.zero, 100, 0,
            SpriteMeshType.FullRect, Vector4.zero, false, oneSecondaryTexture);

        m_dirtyMaterial = false;
        m_Image.isMaterialUpdated = false;
        m_Image.sprite = spriteWithOneSecTex;
        Assert.IsTrue(m_dirtyMaterial);

        yield return ValidateSecondaryTextures(oneSecondaryTexture);

        m_Image.isMaterialUpdated = false;
        m_Image.sprite = spriteWithNoSecTex;
        Assert.IsTrue(m_dirtyMaterial);

        yield return ValidateSecondaryTextures(s_EmptySecondaryTexArray);
    }

    //UUM-114080 Prevent a crash caused by calling DestroyImmediate on the default uGUI material
    [Test]
    public void DestroyImmediate_OnDefaultMaterial_LogError()
    {
        var defaultMaterial = m_Image.defaultMaterial;
        var name = defaultMaterial.name;
        Object.DestroyImmediate(defaultMaterial, true);

        LogAssert.Expect(LogType.Error, $"Destroying object \"{name}\" is not allowed at this time.");

        //This is to ensure that the cached default material pointer in native is still valid.
        Assert.That(m_Image.defaultMaterial.name, Is.EqualTo(name));
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(m_CanvasGO);
        GameObject.DestroyImmediate(m_DefaultTexture);
    }
}
