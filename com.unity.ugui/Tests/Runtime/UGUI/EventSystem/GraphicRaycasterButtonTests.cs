using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

internal class GraphicRaycasterButtonTests
{
    Camera m_Camera;
    EventSystem m_EventSystem;
    Canvas m_Canvas;
    Button m_ParentButton;
    Button m_ChildButton;
    Sprite m_Sprite;

    const int TextureSize = 64;
    readonly Texture2D texture = new Texture2D(TextureSize, TextureSize);

    [UnitySetUp]
    public IEnumerator TestSetup()
    {
        m_Camera = new GameObject("Camera").AddComponent<Camera>();
        m_Camera.transform.position = new Vector3(0, 0, -10);

        m_Canvas = new GameObject("Canvas").AddComponent<Canvas>();
        m_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        m_Canvas.gameObject.AddComponent<GraphicRaycaster>();

        m_EventSystem = new GameObject("Event System").AddComponent<EventSystem>();

        Color[] colors = new Color[TextureSize * TextureSize];
        for (int y = 24; y < 40; y++)
            for (int x = 0; x < TextureSize; x++)
                colors[y + TextureSize * x] = colors[x + TextureSize * y] = Color.red;
        texture.SetPixels(colors);
        texture.Apply();

        m_Sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);

        var parentImage = new GameObject("ParentButton", typeof(RectTransform)).AddComponent<Image>();
        parentImage.transform.SetParent(m_Canvas.transform);
        parentImage.rectTransform.anchoredPosition = new Vector2(0, 0);
        parentImage.sprite = m_Sprite;
        parentImage.SetNativeSize();
        m_ParentButton = parentImage.gameObject.AddComponent<Button>();

        // Duplicate Button as a child GO
        m_ChildButton = Object.Instantiate(m_ParentButton, m_ParentButton.transform);
        m_ChildButton.name = "ChildButton";
        var childImage = m_ChildButton.GetComponent<Image>();
        childImage.rectTransform.anchoredPosition = new Vector2(8, 50);

        parentImage.alphaHitTestMinimumThreshold = 0.5f;
        childImage.alphaHitTestMinimumThreshold = 0.5f;

        // Yield one frame so that all tests below work.
        yield return null;
    }

    [Test]
    [TestCase(  0f, -40f, false, false, TestName = "Should not hit either button (outside of both boundaries)")]
    [TestCase(  0f,   0f,  true, false, TestName = "Should hit the parent (opaque pixel), but not the child (outside)")]
    [TestCase(-30f, -30f, false, false, TestName = "Should not hit the parent (transparent pixel), nor the child (outside)")]
    [TestCase( 10f,  30f, false,  true, TestName = "Should not hit the parent (transparent pixel), but hit the child (opaque pixel),")]
    [TestCase(  5f,  30f,  true,  true, TestName = "Should hit the parent (opaque pixel), and the child (opaque pixel)")]
    [TestCase( -5f,  30f,  true, false, TestName = "Should hit the parent (opaque pixel), but not the child (transparent pixel)")]
    [TestCase(-10f,  30f, false, false, TestName = "Should not hit the parent (transparent pixel), nor the child (transparent pixel)")]
    [TestCase(  8f,  50f, false,  true, TestName = "Should not hit the parent (outside), but hit the child (opaque pixel)")]
    [TestCase(-10f,  30f, false, false, TestName = "Should not hit the parent (outside), nor the child (transparent pixel)")]
    public void GraphicRaycaster_ChildButtonOutsideOfParentButton(float x, float y, bool hitParent, bool hitChild)
    {
        var results = new List<RaycastResult>();
        var pointerEvent = new PointerEventData(m_EventSystem)
        {
            position = new Vector2(Screen.width / 2f + x, Screen.height / 2f + y)
        };

        m_EventSystem.RaycastAll(pointerEvent, results);


        if (hitParent == false && hitChild == false)
        {
            Assert.IsEmpty(results, "Expected no results from a raycast.");
        }
        else
        {
            if (hitParent && hitChild)
                Assert.AreEqual(2, results.Count, "Expected 2 results from a raycast.");
            else
                Assert.AreEqual(1, results.Count, "Expected 1 result from a raycast.");

            int hitIndex = 0;
            if (hitChild)
            {
                Assert.AreSame(m_ChildButton.gameObject, results[hitIndex].gameObject);
                hitIndex++;
            }
            if (hitParent)
            {
                Assert.AreSame(m_ParentButton.gameObject, results[hitIndex].gameObject);
            }
        }
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(m_Camera.gameObject);
        Object.DestroyImmediate(m_EventSystem.gameObject);
        Object.DestroyImmediate(m_Canvas.gameObject);
    }
}
