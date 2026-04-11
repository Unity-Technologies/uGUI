using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using NUnit.Framework;

namespace RaycastReceiverTest
{
    [TestFixture]
    [Ignore("Ignoring for PRV testing")]
    internal class RaycastReceiverTests
    {
        private GameObject m_PrefabRoot;

        private Camera m_camera;
        private Canvas m_canvas;
        private GraphicRaycaster m_raycaster;
        private EventSystem m_eventSystem;

        private RaycastReceiver m_receiver;

        const string kPrefabPath = "Assets/Resources/RaycastReceiverTestPrefab.prefab";

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
#if UNITY_EDITOR
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            var root = new GameObject("Root");

            UnityEditor.EditorApplication.ExecuteMenuItem("GameObject/Camera");
            UnityEditor.EditorApplication.ExecuteMenuItem("GameObject/UI (Canvas)/Canvas");

            TryGetComponentFromRoots(scene, out m_camera);
            TryGetComponentFromRoots(scene, out m_canvas);
            TryGetComponentFromRoots(scene, out m_eventSystem);

            m_camera.transform.SetParent(root.transform);
            m_canvas.transform.SetParent(root.transform);
            m_eventSystem.transform.SetParent(root.transform);

            //Changed ConcreteGraphic to RaycastReceiver to match the test class intent
            var receiverGO = new GameObject("Receiver", typeof(RectTransform), typeof(RaycastReceiver));
            receiverGO.transform.SetParent(m_canvas.transform);

            //Canvas setup
            m_canvas.worldCamera = m_camera;

            //Ensure it has size so raycasts can hit it
            var rt = receiverGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100, 100);
            rt.anchoredPosition = new Vector2(0, 0);

            if (!System.IO.Directory.Exists("Assets/Resources/"))
                System.IO.Directory.CreateDirectory("Assets/Resources/");

            UnityEditor.PrefabUtility.SaveAsPrefabAsset(root, kPrefabPath);

            GameObject.DestroyImmediate(root);

            m_camera = null;
            m_canvas = null;
            m_eventSystem = null;
#endif
        }

        [SetUp]
        public void TestSetup()
        {
            m_PrefabRoot = Object.Instantiate(Resources.Load("RaycastReceiverTestPrefab")) as GameObject;

            m_camera = m_PrefabRoot.GetComponentInChildren<Camera>();
            m_canvas = m_PrefabRoot.GetComponentInChildren<Canvas>();
            m_raycaster = m_PrefabRoot.GetComponentInChildren<GraphicRaycaster>();
            m_eventSystem = m_PrefabRoot.GetComponentInChildren<EventSystem>();

            m_receiver = m_PrefabRoot.GetComponentInChildren<RaycastReceiver>();
        }

        [TearDown]
        public void TearDown()
        {
            m_camera = null;
            m_canvas = null;
            m_raycaster = null;
            m_eventSystem = null;
            m_receiver = null;

            GameObject.DestroyImmediate(m_PrefabRoot);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.DeleteAsset(kPrefabPath);
#endif
        }

        //Utility function for searching the scene's root objects
        private bool TryGetComponentFromRoots<T>(Scene scene, out T component)
        {
            var roots =  scene.GetRootGameObjects();
            component = default(T);

            foreach (var root in roots)
            {
                if (root.TryGetComponent(out component))
                    return true;
            }

            return false;
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

        //Makes sure that there is go geometry to be drawn by RaycastReceiver
        [Test]
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
}
