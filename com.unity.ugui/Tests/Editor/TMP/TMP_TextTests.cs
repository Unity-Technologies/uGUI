using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.IO;
using Object = UnityEngine.Object;

namespace TMPro
{
    [Category("Text Parsing & Layout")]
    internal class TMP_TextTests
    {
        private TextMeshPro m_TextComponent;
        GameObject m_canvasObject;
        private TextMeshProUGUI m_TextComponentUGUI;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (Directory.Exists(Path.GetFullPath("Assets/TextMesh Pro")) || Directory.Exists(Path.GetFullPath("Packages/com.unity.textmeshpro.tests/TextMesh Pro")))
            {
                GameObject textObject = new GameObject("Text Object");
                m_TextComponent = textObject.AddComponent<TextMeshPro>();
                m_canvasObject = new GameObject("Canvas Object");
                m_canvasObject.AddComponent<Canvas>();
                GameObject textObjectUGUI = new GameObject("Text Object UGUI");
                textObjectUGUI.transform.SetParent(m_canvasObject.transform);
                m_TextComponentUGUI = textObjectUGUI.AddComponent<TextMeshProUGUI>();
            }
            else
            {
                Debug.Log("Skipping over Editor tests as TMP Essential Resources are missing from the current test project.");
                Assert.Ignore();
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (m_TextComponent != null)
            {
                Object.DestroyImmediate(m_TextComponent.gameObject);
                Object.DestroyImmediate(m_TextComponentUGUI.gameObject);
                Object.DestroyImmediate(m_canvasObject);
                m_TextComponent = null;
                m_TextComponentUGUI = null;
            }
        }

        [Test]
        public void Text3D_EllipsisOverflow_DoesNotThrow_WhenMaterialAtQuadCap()
        {
            // (UUM-134477) Reproduction for the IndexOutOfRangeException thrown when a TMP
            // text with Overflow=Ellipsis is rendered and the ellipsis font
            // asset's material has already filled its 16383-quad submesh.
            m_TextComponent.rectTransform.sizeDelta = new Vector2(10, 15);
            m_TextComponent.text = new string('A', 20000);
            m_TextComponent.fontSize = 1f;
            m_TextComponent.textWrappingMode = TextWrappingModes.Normal;
            m_TextComponent.overflowMode = TextOverflowModes.Ellipsis;
            Assert.DoesNotThrow(() => m_TextComponent.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true));
            LogAssert.NoUnexpectedReceived();

            // Sanity check: the fixture must produce more than 16383 visible
            // characters before truncation — otherwise the test passes for the
            // wrong reason (the cap was never reached and the bug not exercised).
            Assert.IsTrue(m_TextComponent.isTextOverflowing, "Test fixture did not cause text to overflow. Tune rect/font/text to ensure overflow occurs and ellipsis insertion is triggered.");
            Assert.Greater(m_TextComponent.textInfo.characterCount, 16384, "Test fixture did not push enough characters into the mesh to exercise the 16383-quad cap. Tune rect/font/text to fit more characters.");
        }

        [Test]
        public void TextUGUI_EllipsisOverflow_DoesNotThrow_WhenMaterialAtQuadCap()
        {
            // (UUM-134477) Reproduction for the IndexOutOfRangeException thrown when a TMP
            // text with Overflow=Ellipsis is rendered and the ellipsis font
            // asset's material has already filled its 16383-quad submesh.
            m_TextComponentUGUI.rectTransform.sizeDelta = new Vector2(100, 150);
            m_TextComponentUGUI.text = new string('A', 20000);
            m_TextComponentUGUI.fontSize = 1f;
            m_TextComponentUGUI.textWrappingMode = TextWrappingModes.Normal;
            m_TextComponentUGUI.overflowMode = TextOverflowModes.Ellipsis;
            Assert.DoesNotThrow(() => m_TextComponentUGUI.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true));
            LogAssert.NoUnexpectedReceived();

            // Sanity check: the fixture must produce more than 16383 visible
            // characters before truncation — otherwise the test passes for the
            // wrong reason (the cap was never reached and the bug not exercised).
            Assert.IsTrue(m_TextComponentUGUI.isTextOverflowing, "Test fixture did not cause text to overflow. Tune rect/font/text to ensure overflow occurs and ellipsis insertion is triggered.");
            Assert.Greater(m_TextComponentUGUI.textInfo.characterCount, 16384, "Test fixture did not push enough characters into the mesh to exercise the 16383-quad cap. Tune rect/font/text to fit more characters.");
        }
    }
}
