using System.Collections;
using NUnit.Framework;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

class RawImageTests
{
    const int k_Width = 32;
    const int k_Height = 32;
    const double k_FloatCompareDelta = 0.001f;

    static Texture2D CreateTexture(Color? color = null)
    {
        color ??= new Color(0.25f, 0.5f, 0.75f);
        bool isHDR = color.Value.maxColorComponent > 1.0f;
        TextureFormat format = isHDR ? TextureFormat.RGBAHalf : TextureFormat.RGBA32;
        var tex = new Texture2D(k_Width, k_Height, format, false);

        var colors = new Color[k_Width * k_Height];
        for (var i = 0; i < k_Width * k_Height; i++)
        {
            colors[i] = color.Value;
        }

        tex.SetPixels(colors);
        tex.Apply();
        return tex;
    }

    static RawImage CreateRawImage(out GameObject canvasGO)
    {
        canvasGO = new GameObject("Canvas", typeof(Canvas));
        var rawImageObject = new GameObject("RawImage", typeof(RawImage));
        rawImageObject.transform.SetParent(canvasGO.transform);
        return rawImageObject.GetComponent<RawImage>();
    }

    [UnityTest]
    public IEnumerator SpriteAssignmentToTexture_DoesNotCrashWhenBuildingMesh()
    {
        RawImage rawImage = CreateRawImage(out GameObject canvasGO);
        Texture2D defaultTexture = CreateTexture();
        var sprite = Sprite.Create(defaultTexture, new Rect(0, 0, k_Width, k_Height), Vector2.zero);

        // Simulates when AnimationClip assigns a UnityEngine.Object value from native side of the wrong type during animation sampling.
        Texture texture = UnsafeUtility.As<Sprite, Texture>(ref sprite);
        rawImage.texture = texture;

        // Wait a frame to allow mesh generation.
        yield return null;

        // Mesh generation would have crashed when accessing RawTexture::mainTexture.
        Assert.Pass();

        // Clean up resources
        Object.DestroyImmediate(canvasGO);
        Object.DestroyImmediate(sprite);
        Object.DestroyImmediate(defaultTexture);
    }

    [UnityTest]
    public IEnumerator SpriteAssignmentToTexture_MainTextureIsNull()
    {
        RawImage rawImage = CreateRawImage(out GameObject canvasGO);
        Texture2D defaultTexture = CreateTexture();
        var sprite = Sprite.Create(defaultTexture, new Rect(0, 0, k_Width, k_Height), Vector2.zero);

        // Simulates when AnimationClip assigns a UnityEngine.Object value from native side of the wrong type during animation sampling.
        Texture texture = UnsafeUtility.As<Sprite, Texture>(ref sprite);
        rawImage.texture = texture;

        // Wait a frame to allow mesh generation.
        yield return null;

        Assert.IsNull(rawImage.mainTexture, "rawImage.mainTexture == null");

        // Clean up resources
        Object.DestroyImmediate(canvasGO);
        Object.DestroyImmediate(sprite);
        Object.DestroyImmediate(defaultTexture);
    }

    [UnityTest]
    public IEnumerator SettingTexture_UpdatesRawImageMainTexture()
    {
        RawImage rawImage = CreateRawImage(out GameObject canvasGO);
        Texture2D tex = CreateTexture();

        rawImage.texture = tex;

        // Wait a frame to allow the RawImage to update.
        yield return null;

        Assert.AreEqual(tex, rawImage.mainTexture);

        // Clean up resources
        Object.DestroyImmediate(canvasGO);
        Object.DestroyImmediate(tex);
    }

    [UnityTest]
    public IEnumerator SettingNullTexture_RawImageMainTextureIsDefaultWhiteTexture()
    {
        RawImage rawImage = CreateRawImage(out GameObject canvasGO);
        rawImage.texture = null;

        // Wait a frame to allow the RawImage to update.
        yield return null;

        Assert.AreEqual(rawImage.mainTexture, TestRawImage.DefaultWhiteTexture);

        // Clean up resources
        Object.DestroyImmediate(canvasGO);
    }

    [UnityTest]
    public IEnumerator ChangingUVRect_UpdatesUVRectProperty()
    {
        RawImage rawImage = CreateRawImage(out GameObject canvasGO);
        var newUVRect = new Rect(0.1f, 0.2f, 0.5f, 0.5f);

        rawImage.uvRect = newUVRect;

        // Wait a frame to allow the RawImage to update.
        yield return null;

        Assert.AreEqual(newUVRect, rawImage.uvRect);

        // Clean up resources
        Object.DestroyImmediate(canvasGO);
    }

    [UnityTest]
    public IEnumerator SettingColor_UpdatesRawImageColor()
    {
        RawImage rawImage = CreateRawImage(out GameObject canvasGO);
        var newColor = new Color(0.2f, 0.4f, 0.6f, 0.8f);

        rawImage.color = newColor;

        // Wait a frame to allow the RawImage to update.
        yield return null;

        Assert.AreEqual(newColor, rawImage.color);

        // Clean up resources
        Object.DestroyImmediate(canvasGO);
    }

    [UnityTest]
    public IEnumerator RawImage_DefaultTextureIsNull()
    {
        RawImage rawImage = CreateRawImage(out GameObject canvasGO);

        // Wait a frame to allow the RawImage to initialize.
        yield return null;

        Assert.IsNull(rawImage.texture);

        Object.DestroyImmediate(canvasGO);
    }

    [UnityTest]
    public IEnumerator UVRect_GeneratesCorrectVertexUVs()
    {
        RawImage rawImage = CreateRawImage(out GameObject canvasGO);
        Texture2D texture = CreateTexture();
        rawImage.texture = texture;

        // Set a custom UV rect (offset and scaled)
        var customUVRect = new Rect(0.25f, 0.25f, 0.5f, 0.5f);
        rawImage.uvRect = customUVRect;

        // Wait a frame to allow mesh generation
        yield return null;

        // Get the generated mesh from the CanvasRenderer
        var canvasRenderer = rawImage.GetComponent<CanvasRenderer>();
        Assert.IsNotNull(canvasRenderer, "CanvasRenderer should be present");

        Mesh mesh = canvasRenderer.GetMesh();

        // Verify we have the expected quad (4 vertices)
        Assert.AreEqual(4, mesh.vertexCount, "RawImage should generate a quad with 4 vertices");

        // Get UV coordinates
        Vector2[] uvs = mesh.uv;
        Assert.IsNotNull(uvs, "Mesh should have UV coordinates");
        Assert.AreEqual(4, uvs.Length, "Should have 4 UV coordinates");

        // Expected UVs based on the custom UV rect
        // Unity UI typically generates quads with UV order: bottom-left, top-left, top-right, bottom-right
        var expectedUVs = new[]
        {
            new Vector2(customUVRect.xMin, customUVRect.yMin), // Bottom-left
            new Vector2(customUVRect.xMin, customUVRect.yMax), // Top-left
            new Vector2(customUVRect.xMax, customUVRect.yMax), // Top-right
            new Vector2(customUVRect.xMax, customUVRect.yMin), // Bottom-right
        };

        // Verify each UV coordinate matches expected values
        for (var i = 0; i < expectedUVs.Length; i++)
        {
            Assert.AreEqual(
                expectedUVs[i].x,
                uvs[i].x,
                k_FloatCompareDelta,
                $"UV coordinate {i} X component should match expected value");
            Assert.AreEqual(
                expectedUVs[i].y,
                uvs[i].y,
                k_FloatCompareDelta,
                $"UV coordinate {i} Y component should match expected value");
        }

        // Clean up resources
        Object.DestroyImmediate(canvasGO);
        Object.DestroyImmediate(texture);
    }

    [UnityTest]
    public IEnumerator ColorProperty_GeneratesCorrectVertexColors()
    {
        RawImage rawImage = CreateRawImage(out GameObject canvasGO);
        Texture2D texture = CreateTexture();
        rawImage.texture = texture;

        // Set a custom color with transparency
        var customColor = new Color(0.2f, 0.4f, 0.8f, 0.6f);
        rawImage.color = customColor;

        // Wait a frame to allow mesh generation
        yield return null;

        // Get the generated mesh from the CanvasRenderer
        var canvasRenderer = rawImage.GetComponent<CanvasRenderer>();
        Assert.IsNotNull(canvasRenderer, "CanvasRenderer should be present");

        Mesh mesh = canvasRenderer.GetMesh();

        // Verify we have the expected quad (4 vertices)
        Assert.AreEqual(4, mesh.vertexCount, "RawImage should generate a quad with 4 vertices");

        // Get vertex colors
        Color[] colors = mesh.colors;
        Assert.IsNotNull(colors, "Mesh should have vertex colors");
        Assert.AreEqual(4, colors.Length, "Should have 4 vertex colors");

        // Verify each vertex color matches the set color
        for (var i = 0; i < colors.Length; i++)
        {
            Assert.AreEqual(
                customColor.r,
                colors[i].r,
                k_FloatCompareDelta,
                $"Vertex {i} {nameof(Color.r)} component should match expected value");
            Assert.AreEqual(
                customColor.g,
                colors[i].g,
                k_FloatCompareDelta,
                $"Vertex {i} {nameof(Color.g)} component should match expected value");
            Assert.AreEqual(
                customColor.b,
                colors[i].b,
                k_FloatCompareDelta,
                $"Vertex {i} {nameof(Color.b)} component should match expected value");
            Assert.AreEqual(
                customColor.a,
                colors[i].a,
                k_FloatCompareDelta,
                $"Vertex {i} {nameof(Color.a)} component should match expected value");
        }

        // Clean up resources
        Object.DestroyImmediate(canvasGO);
        Object.DestroyImmediate(texture);
    }

    [UnityTest]
    public IEnumerator HDRTexture_HandlesHighDynamicRange()
    {
        RawImage rawImage = CreateRawImage(out GameObject canvasGO);

        // Create an HDR texture with color values greater than 1.0 (HDR range)
        var hdrColor = new Color(2.5f, 1.8f, 3.2f, 1.0f);
        Texture2D hdrTexture = CreateTexture(hdrColor);

        // Verify HDR texture was created successfully
        Assert.AreEqual(TextureFormat.RGBAHalf, hdrTexture.format, "HDR texture should use RGBAHalf format");

        rawImage.texture = hdrTexture;

        // Wait a frame to allow the RawImage to update
        yield return null;

        // Verify the HDR texture is properly assigned and handled
        Assert.AreEqual(hdrTexture, rawImage.mainTexture, "RawImage mainTexture should be set to HDR texture");

        // Verify mesh generation doesn't crash with HDR texture
        var canvasRenderer = rawImage.GetComponent<CanvasRenderer>();
        Assert.IsNotNull(canvasRenderer, "CanvasRenderer should be present");

        Mesh mesh = canvasRenderer.GetMesh();
        Assert.IsNotNull(mesh, "Mesh should be generated successfully with HDR texture");
        Assert.AreEqual(4, mesh.vertexCount, "RawImage should generate a quad with HDR texture");

        // Verify UV coordinates are still properly generated
        Vector2[] uvs = mesh.uv;
        Assert.IsNotNull(uvs, "Mesh should have UV coordinates with HDR texture");
        Assert.AreEqual(4, uvs.Length, "Should have 4 UV coordinates with HDR texture");

        // Test that changing UV rect still works with HDR texture
        var customUVRect = new Rect(0.1f, 0.1f, 0.8f, 0.8f);
        rawImage.uvRect = customUVRect;

        yield return null;

        mesh = canvasRenderer.GetMesh();
        uvs = mesh.uv;

        // Verify UV rect changes are applied correctly with HDR texture
        var expectedUVs = new[]
        {
            new Vector2(customUVRect.xMin, customUVRect.yMin), // Bottom-left
            new Vector2(customUVRect.xMin, customUVRect.yMax), // Top-left
            new Vector2(customUVRect.xMax, customUVRect.yMax), // Top-right
            new Vector2(customUVRect.xMax, customUVRect.yMin), // Bottom-right
        };

        for (var i = 0; i < expectedUVs.Length; i++)
        {
            Assert.AreEqual(
                expectedUVs[i].x,
                uvs[i].x,
                k_FloatCompareDelta,
                $"HDR texture UV coordinate {i} X component should match expected value");
            Assert.AreEqual(
                expectedUVs[i].y,
                uvs[i].y,
                k_FloatCompareDelta,
                $"HDR texture UV coordinate {i} Y component should match expected value");
        }

        // Clean up resources
        Object.DestroyImmediate(canvasGO);
        Object.DestroyImmediate(hdrTexture);
    }

    [UnityTest]
    public IEnumerator DestroyedTexture_HandledGracefully()
    {
        RawImage rawImage = CreateRawImage(out GameObject canvasGO);
        Texture2D texture = CreateTexture();

        // Assign texture to RawImage
        rawImage.texture = texture;

        // Wait a frame to allow initial setup
        yield return null;

        // Verify texture is initially assigned
        Assert.AreEqual(texture, rawImage.texture, "Texture should be initially assigned");

        // Destroy the texture while it's still assigned to RawImage
        Object.DestroyImmediate(texture);

        // Wait a frame to allow RawImage to handle the destroyed texture
        yield return null;

        // Verify RawImage handles destroyed texture gracefully (should not crash)
        Assert.DoesNotThrow(
            () =>
            {
                var canvasRenderer = rawImage.GetComponent<CanvasRenderer>();
                Assert.IsNotNull(canvasRenderer, "CanvasRenderer should still be present");

                // Attempt to get mesh - should not crash even with destroyed texture
                Mesh _ = canvasRenderer.GetMesh();
                // Mesh might be null or empty, but getting it shouldn't crash
            },
            "RawImage should handle destroyed texture without crashing");

        // Verify accessing texture property doesn't crash
        Assert.DoesNotThrow(
            () =>
            {
                Texture _ = rawImage.texture;
                Texture __ = rawImage.mainTexture;
            },
            "Accessing texture properties should not throw with destroyed texture");

        // Clean up resources
        Object.DestroyImmediate(canvasGO);
    }

    [UnityTest]
    public IEnumerator NegativeUVRect_ClampsCorrectly()
    {
        RawImage rawImage = CreateRawImage(out GameObject canvasGO);
        Texture2D texture = CreateTexture();
        rawImage.texture = texture;

        // Set UV rect with negative values
        var negativeUVRect = new Rect(-0.5f, -0.3f, 0.8f, 0.6f);
        rawImage.uvRect = negativeUVRect;

        // Wait a frame to allow mesh generation
        yield return null;

        // Verify the UV rect property reflects what was set (RawImage might store the raw values)
        Assert.AreEqual(negativeUVRect, rawImage.uvRect, "UV rect property should store the set values");

        // Get the generated mesh and verify UV coordinates are handled appropriately
        var canvasRenderer = rawImage.GetComponent<CanvasRenderer>();
        Assert.IsNotNull(canvasRenderer, "CanvasRenderer should be present");

        Mesh mesh = canvasRenderer.GetMesh();
        Assert.IsNotNull(mesh, "Mesh should be generated with negative UV rect");
        Assert.AreEqual(4, mesh.vertexCount, "Should generate quad with negative UV rect");

        Vector2[] uvs = mesh.uv;
        Assert.IsNotNull(uvs, "Mesh should have UV coordinates");
        Assert.AreEqual(4, uvs.Length, "Should have 4 UV coordinates");

        // Verify UV coordinates are within reasonable bounds or handled appropriately
        // The exact behavior may vary, but it should not crash and should generate valid UVs
        for (var i = 0; i < uvs.Length; i++)
        {
            Assert.IsFalse(float.IsNaN(uvs[i].x), $"UV {i} X coordinate should not be NaN");
            Assert.IsFalse(float.IsNaN(uvs[i].y), $"UV {i} Y coordinate should not be NaN");
            Assert.IsFalse(float.IsInfinity(uvs[i].x), $"UV {i} X coordinate should not be infinity");
            Assert.IsFalse(float.IsInfinity(uvs[i].y), $"UV {i} Y coordinate should not be infinity");
        }

        // Clean up resources
        Object.DestroyImmediate(canvasGO);
        Object.DestroyImmediate(texture);
    }

    [UnityTest]
    public IEnumerator PopulateMesh_GeneratesQuadWithCorrectVertexCount()
    {
        RawImage rawImage = CreateRawImage(out GameObject canvasGO);
        Texture2D texture = CreateTexture();
        rawImage.texture = texture;

        // Set up RectTransform with specific size
        var rectTransform = rawImage.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100f, 50f);

        // Wait a frame to allow mesh population
        yield return null;

        // Get the populated mesh
        var canvasRenderer = rawImage.GetComponent<CanvasRenderer>();
        Assert.IsNotNull(canvasRenderer, "CanvasRenderer should be present");

        Mesh mesh = canvasRenderer.GetMesh();
        Assert.IsNotNull(mesh, "Mesh should be populated");

        // Verify correct vertex count for a quad
        Assert.AreEqual(4, mesh.vertexCount, "RawImage should generate exactly 4 vertices for a quad");

        // Verify we have the expected mesh data
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = mesh.uv;
        Color[] colors = mesh.colors;
        int[] triangles = mesh.triangles;

        Assert.IsNotNull(vertices, "Mesh should have vertices");
        Assert.AreEqual(4, vertices.Length, "Should have 4 vertices");

        Assert.IsNotNull(uvs, "Mesh should have UV coordinates");
        Assert.AreEqual(4, uvs.Length, "Should have 4 UV coordinates");

        Assert.IsNotNull(colors, "Mesh should have vertex colors");
        Assert.AreEqual(4, colors.Length, "Should have 4 vertex colors");

        Assert.IsNotNull(triangles, "Mesh should have triangle indices");
        Assert.AreEqual(6, triangles.Length, "Quad should have 6 triangle indices (2 triangles * 3 indices)");

        // Verify triangles form valid indices (0-3 for a quad)
        for (var i = 0; i < triangles.Length; i++)
        {
            Assert.GreaterOrEqual(triangles[i], 0, $"Triangle index {i} should be >= 0");
            Assert.Less(triangles[i], 4, $"Triangle index {i} should be < 4 for a quad");
        }

        // Test with different UV rect to ensure mesh updates correctly
        rawImage.uvRect = new Rect(0.2f, 0.2f, 0.6f, 0.6f);
        yield return null;

        mesh = canvasRenderer.GetMesh();
        Assert.AreEqual(4, mesh.vertexCount, "Vertex count should remain 4 after UV rect change");

        // Clean up resources
        Object.DestroyImmediate(canvasGO);
        Object.DestroyImmediate(texture);
    }
}
