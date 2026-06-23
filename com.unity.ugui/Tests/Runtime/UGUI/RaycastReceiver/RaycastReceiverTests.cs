using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using NUnit.Framework;

[TestFixture]
internal class RaycastReceiverTests
{
    private Canvas m_canvas;
    private GraphicRaycaster m_raycaster;
    private EventSystem m_eventSystem;
    private RaycastReceiver m_receiver;

    [SetUp]
    public void Setup()
    {
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        m_canvas = canvasGO.GetComponent<Canvas>();
        m_raycaster = canvasGO.GetComponent<GraphicRaycaster>();
        m_canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        m_eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();

        m_receiver = new GameObject("Receiver", typeof(RectTransform)).AddComponent<RaycastReceiver>();
        m_receiver.transform.SetParent(m_canvas.transform);

        // Ensure it has size so raycasts can hit it
        var rt = m_receiver.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 100);
        rt.anchoredPosition = new Vector2(0, 0);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(m_canvas.gameObject);
        Object.DestroyImmediate(m_eventSystem.gameObject);

        m_canvas = null;
        m_raycaster = null;
        m_eventSystem = null;
        m_receiver = null;
    }

    private void IsComponentInResults(Component graphic, List<RaycastResult> results, out bool exists)
    {
        exists = false;
        foreach (var result in results)
        {
            if (result.gameObject == graphic.gameObject)
            {
                exists = true;
                break;
            }
        }
    }

    [Test]
    [Description("Makes sure that there is no geometry to be drawn by RaycastReceiver")]
    public void RaycastReceiver_OnPopulateMesh_ClearsGeometry()
    {
        //Create a VertexHelper and add fake geometry
        var vh = new VertexHelper();
        vh.AddVert(new Vector3(0, 0, 0), Color.white, new Vector2(0, 0));
        vh.AddVert(new Vector3(10, 0, 0), Color.white, new Vector2(1, 0));
        vh.AddVert(new Vector3(10, 10, 0), Color.white, new Vector2(1, 1));
        vh.AddTriangle(0, 1, 2);

        Assert.AreEqual(3, vh.currentVertCount);

        //Invoke OnPopulateMesh via reflection since it is protected
        var methodInfo = typeof(RaycastReceiver).GetMethod(
            "OnPopulateMesh",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            new System.Type[] { typeof(VertexHelper) }, //Needed to avoid ambiguity
            null
        );

        methodInfo.Invoke(m_receiver, new object[] { vh });

        //Assert that the mesh was cleared (0 vertices)
        Assert.AreEqual(0, vh.currentVertCount, "RaycastReceiver should clear the VertexHelper to remain invisible.");
    }

    [UnityTest]
    public IEnumerator RaycastReceiver_InterceptsRaycast()
    {
        //Wait a frame to allow the canvas to process renderers
        yield return null;

        //Create PointerEventData at the center of the screen/canvas
        PointerEventData pointerData = new PointerEventData(m_eventSystem)
        {
            position = new Vector2(Screen.width / 2f, Screen.height / 2f)
        };

        List<RaycastResult> results = new List<RaycastResult>();
        m_raycaster.Raycast(pointerData, results);

        //Verify that the RaycastReceiver intercepted the ray
        IsComponentInResults(m_receiver, results, out bool exists);
        Assert.IsTrue(exists, "RaycastReceiver should intercept raycasts when raycastTarget is true.");
    }

    [UnityTest]
    public IEnumerator RaycastReceiver_IgnoreRaycast_WhenTargetFalse()
    {
        m_receiver.raycastTarget = false;

        //Wait a frame to allow the canvas to process renderers
        yield return null;

        //Convert world position of receiver to screen point for raycast
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, m_receiver.transform.position);
        PointerEventData pointerData = new PointerEventData(m_eventSystem) { position = screenPos };

        List<RaycastResult> results = new List<RaycastResult>();
        m_raycaster.Raycast(pointerData, results);

        IsComponentInResults(m_receiver, results, out bool exists);
        Assert.IsFalse(exists, "RaycastReceiver should NOT intercept raycasts when raycastTarget is false.");
    }

    [UnityTest]
    public IEnumerator RaycastReceiver_RespectsMasking()
    {
        //Setup a game object with a Mask
        var maskObj = new GameObject("Mask", typeof(RectTransform), typeof(Image), typeof(Mask));
        maskObj.transform.SetParent(m_canvas.transform);

        //Set mask size to 50x50
        var maskRect = maskObj.GetComponent<RectTransform>();
        maskRect.sizeDelta = new Vector2(50, 50);
        maskRect.anchoredPosition = Vector2.zero;

        //Move Receiver inside the mask hierarchy
        m_receiver.transform.SetParent(maskObj.transform);
        m_receiver.rectTransform.sizeDelta = new Vector2(20, 20);

        //Position Receiver OUTSIDE the mask bounds (e.g., at x=100)
        m_receiver.rectTransform.anchoredPosition = new Vector2(100, 0);

        yield return null;

        //Convert world position of receiver to screen point for raycast
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, m_receiver.transform.position);
        PointerEventData pointerData = new PointerEventData(m_eventSystem) { position = screenPos };

        List<RaycastResult> results = new List<RaycastResult>();
        m_raycaster.Raycast(pointerData, results);

        //Should NOT hit because it is masked out
        IsComponentInResults(m_receiver, results, out bool hit);
        Assert.IsFalse(hit, "RaycastReceiver should be ignored when outside of a parent Mask.");

        //2nd test...
        //Move Receiver INSIDE the mask bounds
        m_receiver.rectTransform.anchoredPosition = Vector2.zero;

        yield return null;

        //Update screen position
        screenPos = RectTransformUtility.WorldToScreenPoint(null, m_receiver.transform.position);
        pointerData.position = screenPos;
        results.Clear();

        m_raycaster.Raycast(pointerData, results);

        //Should HIT now
        hit = false;
        IsComponentInResults(m_receiver, results, out hit);
        Assert.IsTrue(hit, "RaycastReceiver should be hit when inside a parent Mask.");
    }

    [UnityTest]
    public IEnumerator RaycastReceiver_RespectsRectMask2D()
    {
        //Setup game object with RectMask2D
        var maskObj = new GameObject("RectMask", typeof(RectTransform), typeof(RectMask2D));
        maskObj.transform.SetParent(m_canvas.transform);

        var maskRect = maskObj.GetComponent<RectTransform>();
        maskRect.sizeDelta = new Vector2(50, 50);
        maskRect.anchoredPosition = Vector2.zero;

        //Move Receiver inside the mask hierarchy
        m_receiver.transform.SetParent(maskObj.transform);
        m_receiver.rectTransform.sizeDelta = new Vector2(20, 20);

        //Position Receiver OUTSIDE the mask bounds
        m_receiver.rectTransform.anchoredPosition = new Vector2(100, 0);

        yield return null;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, m_receiver.transform.position);
        PointerEventData pointerData = new PointerEventData(m_eventSystem) { position = screenPos };

        List<RaycastResult> results = new List<RaycastResult>();
        m_raycaster.Raycast(pointerData, results);

        IsComponentInResults(m_receiver, results, out bool hit);
        Assert.IsFalse(hit, "RaycastReceiver should be ignored when outside of a parent RectMask2D.");

        //2nd test...
        //Move Receiver INSIDE the mask bounds
        m_receiver.rectTransform.anchoredPosition = Vector2.zero;

        yield return null;

        //Update screen position
        screenPos = RectTransformUtility.WorldToScreenPoint(null, m_receiver.transform.position);
        pointerData.position = screenPos;
        results.Clear();

        m_raycaster.Raycast(pointerData, results);

        //Should HIT now
        hit = false;
        IsComponentInResults(m_receiver, results, out hit);
        Assert.IsTrue(hit, "RaycastReceiver should be hit when inside a parent RectMask2D.");
    }

    [UnityTest]
    public IEnumerator RaycastReceiver_RespectsCanvasGroup_BlocksRaycasts()
    {
        //Add CanvasGroup to the receiver's parent (or the receiver itself)
        var group = m_receiver.gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = true;

        yield return null;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, m_receiver.transform.position);
        PointerEventData pointerData = new PointerEventData(m_eventSystem) { position = screenPos };

        List<RaycastResult> results = new List<RaycastResult>();
        m_raycaster.Raycast(pointerData, results);

        IsComponentInResults(m_receiver, results, out bool hit);
        Assert.IsTrue(hit, "Should hit when CanvasGroup.blocksRaycasts is true.");

        yield return null;

        //2nd test...
        //Disable blocksRaycasts
        group.blocksRaycasts = false;
        results.Clear();
        m_raycaster.Raycast(pointerData, results);

        hit = false;
        IsComponentInResults(m_receiver, results, out hit);
        Assert.IsFalse(hit, "Should NOT hit when CanvasGroup.blocksRaycasts is false.");
    }
}
