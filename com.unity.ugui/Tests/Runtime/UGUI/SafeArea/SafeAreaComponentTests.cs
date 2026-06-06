using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace SafeAreaTests
{
    internal class SafeAreaComponentTests
    {
        private GameObject _canvasGo;
        private GameObject _safeAreaGo;
        private Canvas _canvas;
        private RectTransform _rectTransform;
        private SafeArea _safeArea;

        [SetUp]
        public void SetUp()
        {
            _canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            _canvas = _canvasGo.GetComponent<Canvas>();

            _safeAreaGo = new GameObject("SafeArea", typeof(RectTransform), typeof(SafeArea));
            _safeAreaGo.transform.SetParent(_canvasGo.transform, false);

            _rectTransform = _safeAreaGo.GetComponent<RectTransform>();
            _safeArea = _safeAreaGo.GetComponent<SafeArea>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_safeAreaGo != null)
                Object.DestroyImmediate(_safeAreaGo);

            if (_canvasGo != null)
                Object.DestroyImmediate(_canvasGo);
        }

        [Test]
        public void RequireComponent_AddsRectTransform()
        {
            var go = new GameObject("TestObject", typeof(SafeArea));

            try
            {
                var rect = go.GetComponent<RectTransform>();
                var safeArea = go.GetComponent<SafeArea>();

                Assert.NotNull(safeArea);
                Assert.NotNull(rect);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void DisallowMultipleComponent_PreventsAddingSecondInstance()
        {
            var first = _safeAreaGo.GetComponent<SafeArea>();
            Assert.NotNull(first);

            var second = _safeAreaGo.AddComponent<SafeArea>();
            LogAssert.Expect(LogType.Log, $"Can't add 'SafeArea' to {_safeAreaGo.name} because a 'SafeArea' is already added to the game object!");

            Assert.IsNull(second);
            Assert.AreEqual(1, _safeAreaGo.GetComponents<SafeArea>().Length);
        }

        [Test]
        [TestCase(ScreenOrientation.Portrait)]
        [TestCase(ScreenOrientation.PortraitUpsideDown)]
        [TestCase(ScreenOrientation.LandscapeLeft)]
        [TestCase(ScreenOrientation.LandscapeRight)]
        public void ReferenceOrientation_SetterGetter_RoundTrips(ScreenOrientation orientation)
        {
            _safeArea.ReferenceOrientation = orientation;

            Assert.AreEqual(orientation, _safeArea.ReferenceOrientation);
        }

        [Test]
        [TestCase((SafeArea.SafeAreaMode)0)]
        [TestCase(SafeArea.SafeAreaMode.Top)]
        [TestCase(SafeArea.SafeAreaMode.Left)]
        [TestCase(SafeArea.SafeAreaMode.Bottom)]
        [TestCase(SafeArea.SafeAreaMode.Right)]
        [TestCase(SafeArea.SafeAreaMode.Right | SafeArea.SafeAreaMode.Bottom)]
        [TestCase(SafeArea.SafeAreaMode.Right | SafeArea.SafeAreaMode.Bottom | SafeArea.SafeAreaMode.Left |  SafeArea.SafeAreaMode.Top)]
        public void Edges_SetterGetter_RoundTrips(SafeArea.SafeAreaMode mode)
        {
            _safeArea.Edges = mode;

            Assert.AreEqual(mode, _safeArea.Edges);
        }

        [Test]
        [TestCase((SafeArea.AlignmentMode)0)]
        [TestCase(SafeArea.AlignmentMode.CenterHorizontally)]
        [TestCase(SafeArea.AlignmentMode.CenterVertically)]
        [TestCase(SafeArea.AlignmentMode.CenterHorizontally | SafeArea.AlignmentMode.CenterVertically)]
        public void Alignment_SetterGetter_RoundTrips(SafeArea.AlignmentMode mode)
        {
            _safeArea.Alignment = mode;

            Assert.AreEqual(mode, _safeArea.Alignment);
        }

        [Test]
        public void AwakeAndOnEnable_DoNotThrow_WhenCanvasParentExists()
        {
            Assert.DoesNotThrow(() =>
            {
                _safeAreaGo.SetActive(false);
                _safeAreaGo.SetActive(true);
            });
        }

        [Test]
        public void Component_CanExistUnderCanvas()
        {
            Assert.NotNull(_safeArea);
            Assert.NotNull(_rectTransform);
            Assert.NotNull(_canvas);
            Assert.AreEqual(_canvasGo.transform, _safeAreaGo.transform.parent);
        }

        /// <summary>
        /// Implicit Test to cover any unknown errors that throws when the component enables
        /// </summary>
        [Test]
        public void Disable_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                _safeArea.enabled = false;
            });
        }

        /// <summary>
        /// Implicit Test to cover any unknown errors that throws when the component re-enables
        /// </summary>
        [Test]
        public void ReEnable_DoesNotThrow()
        {
            _safeArea.enabled = false;

            Assert.DoesNotThrow(() =>
            {
                _safeArea.enabled = true;
            });
        }

        [Test]
        [TestCase(SafeArea.SafeAreaMode.Top)]
        [TestCase(SafeArea.SafeAreaMode.Left)]
        [TestCase(SafeArea.SafeAreaMode.Bottom)]
        [TestCase(SafeArea.SafeAreaMode.Right)]
        [TestCase(SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Bottom)]
        [TestCase(SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Bottom | SafeArea.SafeAreaMode.Right)]
        public void GetReferenceOrientationMappedDirection_WithCurrentReferenceOrientationEqualToCurrentScreen_ReturnsSameDirection(SafeArea.SafeAreaMode input)
        {
            _safeArea.ReferenceOrientation = Screen.orientation;

            var output = _safeArea.GetReferenceOrientationMappedDirection(input);

            Assert.AreEqual(input, output);
        }

        [Test]
        public void Destroy_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                Object.DestroyImmediate(_safeAreaGo);
                _safeAreaGo = null;
            });
        }
    }
}
