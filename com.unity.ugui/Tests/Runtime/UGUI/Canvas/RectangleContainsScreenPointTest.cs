using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System.Linq;

internal class RectangleContainsScreenPointTest : MonoBehaviour
{
    RectTransform m_RectTransform;
    Camera m_MainCamera;
    GameObject m_CanvasObject;
    GameObject m_RectObject;

    [SetUp]
    public void Setup()
    {
        m_MainCamera = new GameObject("MainCamera").AddComponent<Camera>();
        m_MainCamera.transform.position = new Vector3(0, 1, -10);
        m_MainCamera.depth = -1;

        m_CanvasObject = new GameObject("Canvas");
        Canvas m_canvas = m_CanvasObject.AddComponent<Canvas>();
        m_canvas.transform.localPosition = new Vector3(0, 1, 90);
        m_canvas.renderMode = RenderMode.ScreenSpaceCamera;
        m_canvas.worldCamera = m_MainCamera;

        m_RectObject = new GameObject("RectTransformObject");
        m_RectTransform = m_RectObject.AddComponent<RectTransform>();
        m_RectTransform.SetParent(m_CanvasObject.transform, false);
    }

    [TearDown]
    public void TearDown()
    {
        Destroy(m_MainCamera.gameObject);
        Destroy(m_CanvasObject);
        Destroy(m_RectObject);
        Destroy(m_RectTransform);
    }

    [Test]
    public void RectangleContainsScreenPoint_ReturnsTrue_ForAllPointsInTheRectangle()
    {
        var fourCourners = new Vector3[4];
        m_RectTransform.GetWorldCorners(fourCourners);

        var worldCorners = fourCourners
            .Select(p => m_MainCamera.WorldToScreenPoint(p))
            .ToArray();

        var minValue = new Vector2(
            x: worldCorners.Min(p => p.x),
            y: worldCorners.Min(p => p.y));

        var maxValue = new Vector2(
            x: worldCorners.Max(p => p.x),
            y: worldCorners.Max(p => p.y));

        var steps = 10000;
        bool ErrorHit = false;

        for (float i = 0; i < steps; i++)
        {
            var point = Vector2.Lerp(minValue, maxValue, i / steps);
            if (!RectTransformUtility.RectangleContainsScreenPoint(m_RectTransform, point, m_MainCamera))
            {
                ErrorHit = true;
                Assert.Fail("Rectangle does not Contains ScreenPoint");
            }
        }
        
        if (!ErrorHit)
        {
            Assert.Pass();
        }
    }
}
