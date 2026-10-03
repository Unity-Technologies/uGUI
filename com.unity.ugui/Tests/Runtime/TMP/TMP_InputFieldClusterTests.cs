using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TMPro
{
    /// <summary>
    /// End to end coverage for the reported bug: "TextMeshPro input field doesn't remove
    /// characters from the first backspace when variant characters are used with the input
    /// field", where a variant character is a base character followed by a variation selector -
    /// typically an Ideographic Variation Sequence, whose selector (U+E0100 and up) is a
    /// surrogate pair.
    ///
    /// In its default editing mode the input field resolves every delete range and caret step
    /// through textInfo.characterInfo[n].index and .stringLength. SetArraySizes used to record
    /// the span of the last code point consumed by the variation selector block instead of the
    /// span of the whole cluster, so the first Backspace either removed the wrong span or
    /// indexed a characterInfo slot past characterCount and removed nothing. The span mapping
    /// itself is covered by TMP_CharacterClusterTests; these tests assert the behaviour a user
    /// sees.
    /// </summary>
    [Category("Text Parsing & Layout")]
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
    internal class TMP_InputFieldClusterTests : TMPTestSuiteWithEssentialResources
    {
        // U+E0100 VARIATION SELECTOR-17, encoded as a surrogate pair: two UTF-16 code units.
        const string k_VariationSelector17 = "\U000E0100";

        // U+1F600 GRINNING FACE. A plain surrogate pair, with no variation selector.
        const string k_AstralCharacter = "\U0001F600";

        GameObject m_CameraObject;
        GameObject m_CanvasObject;
        GameObject m_InputObject;
        GameObject m_EventObject;
        TMP_InputField m_InputField;

        [SetUp]
        public void SetUp()
        {
            m_CameraObject = new GameObject("Camera Object", typeof(Camera));

            m_CanvasObject = new GameObject("Canvas Object", typeof(Canvas), typeof(GraphicRaycaster));
            m_CanvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            m_InputObject = new GameObject("Input Object", typeof(TMP_InputField));
            m_InputObject.transform.SetParent(m_CanvasObject.transform);
            m_InputObject.AddComponent<Image>();

            GameObject textAreaObject = new GameObject("Text Area", typeof(TextMeshProUGUI));
            textAreaObject.transform.SetParent(m_InputObject.transform);

            m_InputField = m_InputObject.GetComponent<TMP_InputField>();
            m_InputField.targetGraphic = m_InputObject.GetComponent<Image>();
            m_InputField.textViewport = m_InputObject.GetComponent<RectTransform>();
            m_InputField.textComponent = textAreaObject.GetComponent<TextMeshProUGUI>();
            m_InputField.textComponent.fontSize = 18;
            m_InputField.lineType = TMP_InputField.LineType.SingleLine;

            // Selecting the whole text on focus would make every delete a selection delete and
            // hide the single character behaviour under test.
            m_InputField.onFocusSelectAll = false;

            m_EventObject = new GameObject("Event Object", typeof(EventSystem), typeof(StandaloneInputModule));

            // Address the EventSystem this fixture created rather than EventSystem.current,
            // which resolves to the first registered one and could belong to another fixture.
            m_EventObject.GetComponent<EventSystem>().SetSelectedGameObject(m_InputObject);
            m_InputField.ActivateInputField();
        }

        [TearDown]
        public void TearDown()
        {
            if (m_InputField != null)
            {
                m_InputField.DeactivateInputField();
                m_InputField = null;
            }

            // SetUp can bail out through Assert.IsTrue before any of these exist.
            if (m_EventObject != null)
            {
                Object.DestroyImmediate(m_EventObject);
                m_EventObject = null;
            }

            if (m_InputObject != null)
            {
                Object.DestroyImmediate(m_InputObject);
                m_InputObject = null;
            }

            if (m_CanvasObject != null)
            {
                Object.DestroyImmediate(m_CanvasObject);
                m_CanvasObject = null;
            }

            if (m_CameraObject != null)
            {
                Object.DestroyImmediate(m_CameraObject);
                m_CameraObject = null;
            }
        }

        /// <summary>
        /// Assigns the text and puts the caret at a known position in the raw string. The
        /// stringPosition setter derives the caret index from characterInfo synchronously, so the
        /// label has to be generated first.
        /// </summary>
        void SetTextAndStringPosition(string value, int stringPosition)
        {
            m_InputField.text = value;
            m_InputField.textComponent.ForceMeshUpdate();
            m_InputField.stringPosition = stringPosition;
        }

        void PressKey(KeyCode keyCode)
        {
            m_InputField.ProcessEvent(new Event { type = EventType.KeyDown, keyCode = keyCode, modifiers = EventModifiers.None });
            m_InputField.ForceLabelUpdate();
            m_InputField.textComponent.ForceMeshUpdate();
        }

        [Test]
        public void Backspace_RemovesTheWholeVariationSelectorCluster_OnTheFirstPress()
        {
            // "a" | "b" + VS17 - four code units, two rendered characters.
            string sourceText = "ab" + k_VariationSelector17;

            SetTextAndStringPosition(sourceText, sourceText.Length);

            PressKey(KeyCode.Backspace);

            // The whole cluster goes in one press. Before the fix this press removed nothing
            // (or only part of the cluster), which is the reported symptom.
            Assert.AreEqual("a", m_InputField.text, "The first Backspace should remove the whole variant character.");

            // And the caret has to land on the cluster boundary that is left behind, otherwise
            // the following press works from a stale position.
            Assert.AreEqual(1, m_InputField.stringPosition, "The caret should be left at the end of the remaining text.");

            PressKey(KeyCode.Backspace);

            Assert.AreEqual(string.Empty, m_InputField.text, "The second Backspace should remove the remaining character.");
        }

        [Test]
        public void Backspace_RemovesTheOnlyVariationSelectorCluster_OnTheFirstPress()
        {
            // The cluster is the entire content, so the field should end up empty.
            string sourceText = "a" + k_VariationSelector17;

            SetTextAndStringPosition(sourceText, sourceText.Length);

            PressKey(KeyCode.Backspace);

            Assert.AreEqual(string.Empty, m_InputField.text, "The first Backspace should empty the field.");
        }

        [Test]
        public void Delete_RemovesTheWholeVariationSelectorCluster_OnTheFirstPress()
        {
            // Caret sits before the cluster, at raw string index 1.
            string sourceText = "ab" + k_VariationSelector17;

            SetTextAndStringPosition(sourceText, 1);

            PressKey(KeyCode.Delete);

            Assert.AreEqual("a", m_InputField.text, "The first Delete should remove the whole variant character.");
        }

        [Test]
        public void Backspace_PlainText_RemovesOneCharacter()
        {
            // Regression guard: nothing about single code unit characters changes.
            SetTextAndStringPosition("abc", 3);

            PressKey(KeyCode.Backspace);

            Assert.AreEqual("ab", m_InputField.text);
        }

        [Test]
        public void Backspace_SurrogatePairWithoutVariationSelector_RemovesBothCodeUnits()
        {
            // Regression guard: a plain surrogate pair was already removed as a unit and must
            // still be, never leaving half a pair behind.
            string sourceText = "a" + k_AstralCharacter;

            SetTextAndStringPosition(sourceText, sourceText.Length);

            PressKey(KeyCode.Backspace);

            Assert.AreEqual("a", m_InputField.text);
        }

        [Test]
        public void Arrows_StepOverTheWholeVariationSelectorCluster()
        {
            // In the default editing mode the arrows move by rendered character, so one press
            // has to cross the whole cluster and land on a cluster boundary - otherwise a
            // following edit would split the base character from its selector.
            string sourceText = "ab" + k_VariationSelector17;

            SetTextAndStringPosition(sourceText, sourceText.Length);

            PressKey(KeyCode.LeftArrow);

            Assert.AreEqual(1, m_InputField.stringPosition, "Arrow-left should land at the start of the cluster.");

            PressKey(KeyCode.RightArrow);

            Assert.AreEqual(sourceText.Length, m_InputField.stringPosition, "Arrow-right should land at the end of the cluster.");
        }

        [Test]
        public void Arrows_InRichTextEditingMode_StepByCodePoint()
        {
            // With rich text editing enabled the arrows deliberately move by code point rather
            // than by rendered character, so crossing a base character plus a surrogate pair
            // selector takes two presses. The raw string positions come straight from
            // char.IsLowSurrogate and are unaffected by the cluster mapping; the caret index is
            // not - it now stays after the glyph until the raw position has fully left the
            // cluster, which is what GetCaretPositionFromStringIndex reports for a position
            // inside a cluster. Pinned here so the two are not allowed to drift apart again.
            string sourceText = "ab" + k_VariationSelector17;

            m_InputField.isRichTextEditingAllowed = true;

            SetTextAndStringPosition(sourceText, sourceText.Length);

            PressKey(KeyCode.LeftArrow);

            Assert.AreEqual(2, m_InputField.stringPosition, "The first arrow-left should skip the surrogate pair selector.");
            Assert.AreEqual(2, m_InputField.caretPosition, "The caret should stay after the cluster while the raw position is still inside it.");

            PressKey(KeyCode.LeftArrow);

            Assert.AreEqual(1, m_InputField.stringPosition, "The second arrow-left should step over the base character.");
            Assert.AreEqual(1, m_InputField.caretPosition, "The caret should move once the raw position reaches the cluster boundary.");
        }

        [Test]
        public void Backspace_InRichTextEditingMode_RemovesTheSelectorOnly()
        {
            // Documented counterpart to the default mode: rich text editing is code point
            // granular by design, so Backspace removes the surrogate pair selector and leaves
            // the base character. This path reads char.IsLowSurrogate rather than characterInfo,
            // so it is unaffected by the cluster mapping fix - pinned so the two modes cannot
            // quietly converge.
            string sourceText = "ab" + k_VariationSelector17;

            m_InputField.isRichTextEditingAllowed = true;

            SetTextAndStringPosition(sourceText, sourceText.Length);

            PressKey(KeyCode.Backspace);

            Assert.AreEqual("ab", m_InputField.text);
        }
    }
}
