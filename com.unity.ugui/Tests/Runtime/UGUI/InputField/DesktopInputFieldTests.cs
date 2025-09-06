using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine.UI;
using System.Reflection;

namespace InputfieldTests
{
    internal class DesktopInputFieldTests : BaseInputFieldTests, IPrebuildSetup
    {
        protected const string kPrefabPath = "Assets/Resources/DesktopInputFieldPrefab.prefab";

        public void Setup()
        {
#if UNITY_EDITOR
            CreateInputFieldAsset(kPrefabPath);
#endif
        }

        [SetUp]
        public virtual void TestSetup()
        {
            m_PrefabRoot = UnityEngine.Object.Instantiate(Resources.Load("DesktopInputFieldPrefab")) as GameObject;

            FieldInfo inputModule = typeof(EventSystem).GetField("m_CurrentInputModule", BindingFlags.NonPublic | BindingFlags.Instance);
            inputModule.SetValue(m_PrefabRoot.GetComponentInChildren<EventSystem>(), m_PrefabRoot.GetComponentInChildren<FakeInputModule>());
        }

        [TearDown]
        public virtual void TearDown()
        {
            GUIUtility.systemCopyBuffer = null;
            FontUpdateTracker.UntrackText(m_PrefabRoot.GetComponentInChildren<Text>());
            GameObject.DestroyImmediate(m_PrefabRoot);
        }

        [OneTimeTearDown]
        public void OnetimeTearDown()
        {
#if UNITY_EDITOR
            AssetDatabase.DeleteAsset(kPrefabPath);
#endif
        }

        [UnityTest]
        [UnityPlatform(exclude = new[] { RuntimePlatform.Switch, RuntimePlatform.Switch2 })] // Currently InputField.ActivateInputFieldInternal calls Switch SoftwareKeyboard screen ; without user input or a command to close the SoftwareKeyboard this blocks the tests suite
        public IEnumerator FocusOnPointerClickWithLeftButton()
        {
            InputField inputField = m_PrefabRoot.GetComponentInChildren<InputField>();
            PointerEventData data = new PointerEventData(m_PrefabRoot.GetComponentInChildren<EventSystem>());
            data.button = PointerEventData.InputButton.Left;
            inputField.OnPointerClick(data);

            MethodInfo lateUpdate = typeof(InputField).GetMethod("LateUpdate", BindingFlags.NonPublic | BindingFlags.Instance);
            lateUpdate.Invoke(inputField, null);

#if UNITY_GAMECORE && !UNITY_EDITOR
            if (TouchScreenKeyboard.isSupported)
            {
                // On Xbox, the onScreenKeyboard is going to constrain the application and make it go out of focus.
                // We need to wait for the application to go out of focus before we can close the onScreenKeyboard.
                while (Application.isFocused)
                {
                    yield return null;
                }
            }
#endif

            Assert.IsTrue(inputField.isFocused);

#if UNITY_GAMECORE && !UNITY_EDITOR
            // On Xbox, we then need to close onScreenKeyboard and wait for the application to be focused again.
            // If this is not done, it could have an impact on subsequent tests that require the application to be focused in order to function correctly.
            if (!TouchScreenKeyboard.isSupported || !TouchScreenKeyboard.visible)
            {
                yield break;
            }

            while (!Application.isFocused)
            {
                if (inputField.touchScreenKeyboard != null)
                {
                    inputField.touchScreenKeyboard.active = false;
                }
                yield return null;
            }
#else
            yield break;
#endif
        }

        [UnityTest]
        public IEnumerator DoesNotFocusOnPointerClickWithRightOrMiddleButton()
        {
            InputField inputField = m_PrefabRoot.GetComponentInChildren<InputField>();
            PointerEventData data = new PointerEventData(m_PrefabRoot.GetComponentInChildren<EventSystem>());
            data.button = PointerEventData.InputButton.Middle;
            inputField.OnPointerClick(data);
            yield return null;

            data.button = PointerEventData.InputButton.Right;
            inputField.OnPointerClick(data);
            yield return null;

            Assert.IsFalse(inputField.isFocused);
        }
    }
}
