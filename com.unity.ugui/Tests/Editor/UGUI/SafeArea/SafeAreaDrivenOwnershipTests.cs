using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace SafeAreaTests
{
    internal class SafeAreaDrivenOwnershipTests
    {
        // EditMode tests don't tick the player loop, so SafeArea.Update never fires on its own.
        // Invoke it directly to exercise the reclaim-on-poll path.
        private static readonly MethodInfo s_SafeAreaUpdate =
            typeof(SafeArea).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);

        private GameObject _canvasGo;
        private GameObject _parentGo;
        private RectTransform _parentRect;
        private GameObject _targetGo;
        private RectTransform _targetRect;
        private SafeArea _safeArea;

        [SetUp]
        public void SetUp()
        {
            _canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            _parentGo = new GameObject("Parent", typeof(RectTransform));
            _parentGo.transform.SetParent(_canvasGo.transform, false);
            _parentRect = _parentGo.GetComponent<RectTransform>();

            _targetGo = new GameObject("Target", typeof(RectTransform), typeof(SafeArea));
            _targetGo.transform.SetParent(_parentGo.transform, false);

            _targetRect = _targetGo.GetComponent<RectTransform>();
            _safeArea = _targetGo.GetComponent<SafeArea>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_canvasGo != null)
                Object.DestroyImmediate(_canvasGo);
        }

        private void TickSafeAreaUpdate() => s_SafeAreaUpdate.Invoke(_safeArea, null);

        [Test]
        public void ContentSizeFitter_DrivesRect_SafeAreaDoesNotHijack_AndReclaimsWhenFitterDisabled()
        {
            var fitter = _targetGo.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_targetRect);

            Assert.AreEqual(fitter, _targetRect.drivenByObject);

            _safeArea.enabled = false;
            _safeArea.enabled = true;

            Assert.AreEqual(fitter, _targetRect.drivenByObject, "SafeArea should not hijack driven ownership while ContentSizeFitter is driving.");

            fitter.enabled = false;
            TickSafeAreaUpdate();

            Assert.AreEqual(_safeArea, _targetRect.drivenByObject, "SafeArea should reclaim driven ownership after ContentSizeFitter is disabled.");
        }

        [Test]
        public void ParentLayoutGroup_DrivesRect_SafeAreaDoesNotHijack_AndReclaimsWhenLayoutDisabled()
        {
            var layout = _parentGo.AddComponent<HorizontalLayoutGroup>();

            LayoutRebuilder.ForceRebuildLayoutImmediate(_parentRect);

            Assert.AreEqual(layout, _targetRect.drivenByObject);

            _safeArea.enabled = false;
            _safeArea.enabled = true;

            Assert.AreEqual(layout, _targetRect.drivenByObject, "SafeArea should not hijack driven ownership while parent layout group is driving.");

            layout.enabled = false;
            TickSafeAreaUpdate();

            Assert.AreEqual(_safeArea, _targetRect.drivenByObject, "SafeArea should reclaim driven ownership after parent layout group is disabled.");
        }

        [Test]
        public void Scrollbar_DrivesRect_SafeAreaDoesNotHijack_AndReclaimsWhenScrollbarDisabled()
        {
            var scrollbarGo = new GameObject("ScrollbarHost", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            scrollbarGo.transform.SetParent(_canvasGo.transform, false);

            var scrollbar = scrollbarGo.GetComponent<Scrollbar>();
            scrollbar.handleRect = _targetRect;

            Assert.AreEqual(scrollbar, _targetRect.drivenByObject);

            _safeArea.enabled = false;
            _safeArea.enabled = true;

            Assert.AreEqual(scrollbar, _targetRect.drivenByObject, "SafeArea should not hijack driven ownership while Scrollbar is driving.");

            scrollbar.enabled = false;
            TickSafeAreaUpdate();

            Assert.AreEqual(_safeArea, _targetRect.drivenByObject, "SafeArea should reclaim driven ownership after Scrollbar is disabled.");

            Object.DestroyImmediate(scrollbarGo);
        }

        [Test]
        public void ReEnablingSafeArea_DoesNotHijack_WhenAnotherDriverExists_OnSameObject()
        {
            var fitter = _targetGo.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_targetRect);

            Assert.AreEqual(fitter, _targetRect.drivenByObject);

            _safeArea.enabled = false;
            _safeArea.enabled = true;

            Assert.AreEqual(fitter, _targetRect.drivenByObject);
        }

        [Test]
        public void ReEnablingSafeArea_DoesNotHijack_WhenAnotherDriverExists_OnParent()
        {
            var layout = _parentGo.AddComponent<VerticalLayoutGroup>();

            LayoutRebuilder.ForceRebuildLayoutImmediate(_parentRect);

            Assert.AreEqual(layout, _targetRect.drivenByObject);

            _safeArea.enabled = false;
            _safeArea.enabled = true;

            Assert.AreEqual(layout, _targetRect.drivenByObject);
        }

        [Test]
        public void ReEnablingSafeArea_DoesNotHijack_WhenAnotherDriverExists_OnAnotherObject()
        {
            var scrollbarGo = new GameObject("ScrollbarHost", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            scrollbarGo.transform.SetParent(_canvasGo.transform, false);

            var scrollbar = scrollbarGo.GetComponent<Scrollbar>();
            scrollbar.handleRect = _targetRect;

            Assert.AreEqual(scrollbar, _targetRect.drivenByObject);

            _safeArea.enabled = false;
            _safeArea.enabled = true;

            Assert.AreEqual(scrollbar, _targetRect.drivenByObject);

            Object.DestroyImmediate(scrollbarGo);
        }

        [Test]
        public void AddingSecondSafeArea_DoesNotHijackDrivenOwnership()
        {
            Assert.AreEqual(_safeArea, _targetRect.drivenByObject);

            LogAssert.Expect(LogType.Log, $"Can't add 'SafeArea' to {_targetGo.name} because a 'SafeArea' is already added to the game object!");
            var secondSafeArea = _targetGo.AddComponent<SafeArea>();

            Assert.IsNull(secondSafeArea, "DisallowMultipleComponent should prevent a second SafeArea.");
            Assert.AreEqual(_safeArea, _targetRect.drivenByObject, "Original SafeArea should retain ownership.");
        }

        [Test]
        public void SafeArea_ReclaimsOwnership_WhenDriverReleasesWithoutDisable()
        {
            var scrollbarGo = new GameObject("ScrollbarHost", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            scrollbarGo.transform.SetParent(_canvasGo.transform, false);

            var scrollbar = scrollbarGo.GetComponent<Scrollbar>();
            scrollbar.handleRect = _targetRect;

            Assert.AreEqual(scrollbar, _targetRect.drivenByObject);

            scrollbar.handleRect = null;
            TickSafeAreaUpdate();

            Assert.AreEqual(_safeArea, _targetRect.drivenByObject, "SafeArea should reclaim ownership once external driver releases the rect.");

            Object.DestroyImmediate(scrollbarGo);
        }

        [Test]
        public void ReEnable_ClaimsRectTransformDrivenOwnership()
        {
            _safeArea.enabled = true;
            Assert.AreEqual(_safeArea, _targetRect.drivenByObject, "SafeArea should claim RectTransform driven ownership when enabled");

            _safeArea.enabled = false;
            Assert.IsNull(_targetRect.drivenByObject, "SafeArea should not claim RectTransform driven ownership when disabled");

            _safeArea.enabled = true;
            Assert.AreEqual(_safeArea, _targetRect.drivenByObject, "SafeArea should reclaim RectTransform driven ownership when enabled");
        }

        [Test]
        public void Destroy_ClearsRectTransformDrivenOwnership()
        {
            _safeArea.enabled = true;
            Assert.AreEqual(_safeArea, _targetRect.drivenByObject);

            Assert.DoesNotThrow(() =>
            {
                Object.DestroyImmediate(_safeArea);
            });

            Assert.IsNull(_targetRect.drivenByObject);
            _safeArea = null;
        }
    }
}