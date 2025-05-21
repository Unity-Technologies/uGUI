using UnityEngine;
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace TMPro
{
    public class TMP_CanvasTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            if (!Directory.Exists(Path.GetFullPath("Assets/TextMesh Pro")))
            {
                Debug.Log("Skipping over Editor tests as TMP Essential Resources are missing from the current test project.");
                Assert.Ignore();
                return;
            }
        }

        [Test]
        public void EnablingAndDisablingCanvasDoesNotRegenerateText() // (UUM-45320)
        {
            var go = new GameObject();
            var canvas = go.AddComponent<Canvas>();

            var goChild = new GameObject();
            var text = goChild.AddComponent<TextMeshProUGUI>();

            goChild.transform.SetParent(go.transform);

            // Force text to be generated
            text.text = "Hello World";
            text.ForceMeshUpdate();

            Assert.IsFalse(text.havePropertiesChanged, "Text should not have changed yet");

            canvas.enabled = false;
            canvas.enabled = true;

            Assert.IsFalse(text.havePropertiesChanged, "Text should not have changed after enabling / disabling canvas");
        }

    }
}
