using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TextCore;
using Object = UnityEngine.Object;

namespace TMPro
{
    /// <summary>
    /// Coverage for the source string mapping that SetArraySizes writes into
    /// TMP_CharacterInfo.index and TMP_CharacterInfo.stringLength when several code points are
    /// folded into a single rendered character - a "cluster". Two constructs produce one:
    /// a base character followed by a variation selector, and a ligature.
    ///
    /// The regression these guard against: the VARIATION SELECTOR and LIGATURES blocks advance
    /// the text processing index past the elements they consume, and index / stringLength used
    /// to be read from textProcessingArray[i] *after* that advance. A cluster therefore
    /// reported the span of its LAST consumed element instead of the span of the whole cluster,
    /// so index pointed into the middle of the cluster. TMP_InputField derives every caret
    /// position and every delete range from those two fields, which is why the first Backspace
    /// after typing an Ideographic Variation Sequence appeared to do nothing. The input field
    /// half of this is covered by TMP_InputFieldClusterTests.
    ///
    /// Both renderers carry a duplicated copy of SetArraySizes, so every case here runs against
    /// TextMeshPro and TextMeshProUGUI.
    /// </summary>
    [Category("Text Parsing & Layout")]
    internal class TMP_CharacterClusterTests: TMPTestSuiteWithEssentialResources
    {
        // U+FE0F VARIATION SELECTOR-16. A BMP selector: one UTF-16 code unit.
        const string k_VariationSelector16 = "\uFE0F";

        // U+E0100 VARIATION SELECTOR-17, the first Ideographic Variation Sequence selector.
        // It lives in plane 14, so it is encoded as a surrogate pair: two UTF-16 code units.
        // This is the construct from the original bug report.
        const string k_VariationSelector17 = "\U000E0100";

        // U+1F600 GRINNING FACE. A plain surrogate pair with no variation selector, used as the
        // regression guard for characters the cluster accounting must leave alone.
        const string k_AstralCharacter = "\U0001F600";

        // Copy of the default font asset carrying a synthesized 'f' + 'i' ligature record, so the
        // ligature case does not depend on which font the test project happens to ship.
        TMP_FontAsset m_LigatureFontAsset;

        TextMeshPro m_TextComponent;
        TextMeshProUGUI m_TextComponentUGUI;
        GameObject m_CanvasObject;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            GameObject textObject = new GameObject("Text Object");
            m_TextComponent = textObject.AddComponent<TextMeshPro>();
            m_TextComponent.fontSize = 18;

            m_CanvasObject = new GameObject("Canvas Object");
            m_CanvasObject.AddComponent<Canvas>();

            GameObject textObjectUGUI = new GameObject("Text Object UGUI");
            textObjectUGUI.transform.SetParent(m_CanvasObject.transform);
            m_TextComponentUGUI = textObjectUGUI.AddComponent<TextMeshProUGUI>();
            m_TextComponentUGUI.fontSize = 18;

            // The default font asset is static and ships with no ligature records, so no font in this
            // project can produce an 'fi' substitution. The accounting under test is pure span arithmetic
            // and does not care whether the ligature glyph is the real one, so the fixture synthesizes a
            // record on a private copy rather than depending on a font file that may not be imported.
            m_LigatureFontAsset = Object.Instantiate(TMP_Settings.defaultFontAsset);
            m_LigatureFontAsset.name = "Ligature Test Font Asset";
            m_LigatureFontAsset.ReadFontAssetDefinition();

            Assert.IsTrue(m_LigatureFontAsset.characterLookupTable.ContainsKey('f') &&
                          m_LigatureFontAsset.characterLookupTable.ContainsKey('i') &&
                          m_LigatureFontAsset.characterLookupTable.ContainsKey('x'),
                "The default font asset must contain 'f', 'i' and 'x' for the ligature fixture.");

            uint fGlyphIndex = m_LigatureFontAsset.characterLookupTable['f'].glyphIndex;
            uint iGlyphIndex = m_LigatureFontAsset.characterLookupTable['i'].glyphIndex;

            // Reusing a glyph already in the atlas keeps TryAddGlyphInternal on its early-out path
            // (TMP_FontAsset.cs:2414), which is what lets a static font asset perform the substitution.
            uint ligatureGlyphIndex = m_LigatureFontAsset.characterLookupTable['x'].glyphIndex;

            m_LigatureFontAsset.fontFeatureTable.ligatureRecords.Add(new LigatureSubstitutionRecord
            {
                componentGlyphIDs = new[] { fGlyphIndex, iGlyphIndex },
                ligatureGlyphID = ligatureGlyphIndex
            });

            m_LigatureFontAsset.InitializeLigatureSubstitutionLookupDictionary();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (m_TextComponentUGUI != null)
            {
                Object.DestroyImmediate(m_TextComponentUGUI.gameObject);
                m_TextComponentUGUI = null;
            }

            if (m_CanvasObject != null)
            {
                Object.DestroyImmediate(m_CanvasObject);
                m_CanvasObject = null;
            }

            if (m_TextComponent != null)
            {
                Object.DestroyImmediate(m_TextComponent.gameObject);
                m_TextComponent = null;
            }

            if (m_LigatureFontAsset != null)
            {
                m_LigatureFontAsset.atlasTextures = null;
                m_LigatureFontAsset.m_AtlasTexture = null;
                m_LigatureFontAsset.material = null;

                Object.DestroyImmediate(m_LigatureFontAsset);
                m_LigatureFontAsset = null;
            }
        }

        /// <summary>
        /// The two renderers hold independent copies of SetArraySizes, so each case is asserted
        /// against both.
        /// </summary>
        TMP_Text[] textComponents => new TMP_Text[] { m_TextComponent, m_TextComponentUGUI };

        static void AssertCharacterSpan(TMP_Text textComponent, int characterIndex, int expectedIndex, int expectedStringLength)
        {
            TMP_CharacterInfo characterInfo = textComponent.textInfo.characterInfo[characterIndex];

            Assert.AreEqual(expectedIndex, characterInfo.index,
                $"[{textComponent.GetType().Name}] characterInfo[{characterIndex}].index should be the first code unit of the cluster.");
            Assert.AreEqual(expectedStringLength, characterInfo.stringLength,
                $"[{textComponent.GetType().Name}] characterInfo[{characterIndex}].stringLength should cover every code unit of the cluster.");
        }

        [Test]
        public void VariationSelector16_IsFoldedIntoTheSpanOfItsBaseCharacter()
        {
            // "a" + VS16 renders as one character built from two code units.
            const string sourceText = "a" + k_VariationSelector16;

            foreach (TMP_Text textComponent in textComponents)
            {
                textComponent.text = sourceText;
                textComponent.ForceMeshUpdate();

                Assert.AreEqual(1, textComponent.textInfo.characterCount,
                    $"[{textComponent.GetType().Name}] A variation selector should not be counted as a character of its own.");

                // Before the fix this reported (index 1, stringLength 1) - the selector's own span.
                AssertCharacterSpan(textComponent, 0, 0, 2);
            }
        }

        [Test]
        public void VariationSelector17_IsFoldedIntoTheSpanOfItsBaseCharacter()
        {
            // "a" + VS17 renders as one character built from three code units, because the
            // selector is a surrogate pair. This is the shape from the original bug report.
            const string sourceText = "a" + k_VariationSelector17;

            foreach (TMP_Text textComponent in textComponents)
            {
                textComponent.text = sourceText;
                textComponent.ForceMeshUpdate();

                Assert.AreEqual(1, textComponent.textInfo.characterCount,
                    $"[{textComponent.GetType().Name}] A variation selector should not be counted as a character of its own.");

                // Before the fix this reported (index 1, stringLength 2) - the selector's own
                // span - so TMP_InputField deleted only the surrogate pair and left the base
                // character behind, or indexed a stale characterInfo slot entirely.
                AssertCharacterSpan(textComponent, 0, 0, 3);
            }
        }

        [Test]
        public void VariationSelectorCluster_KeepsTheSpansOfSurroundingCharacters()
        {
            // 'a' | 'b' + VS17 | 'c'  ->  three characters over five code units.
            const string sourceText = "ab" + k_VariationSelector17 + "c";

            foreach (TMP_Text textComponent in textComponents)
            {
                textComponent.text = sourceText;
                textComponent.ForceMeshUpdate();

                Assert.AreEqual(3, textComponent.textInfo.characterCount,
                    $"[{textComponent.GetType().Name}] Unexpected character count.");

                AssertCharacterSpan(textComponent, 0, 0, 1);
                AssertCharacterSpan(textComponent, 1, 1, 3);
                AssertCharacterSpan(textComponent, 2, 4, 1);
            }
        }

        [Test]
        public void CharacterSpans_AreContiguousAndCoverTheWholeSourceString()
        {
            // The invariant TMP_InputField's caret arithmetic depends on: consecutive characters
            // describe adjacent, non-overlapping spans that together cover the source string.
            // A cluster used to leave a hole here, because its index skipped the base character.
            const string sourceText = "ab" + k_VariationSelector17 + "c" + k_VariationSelector16 + "d";

            foreach (TMP_Text textComponent in textComponents)
            {
                textComponent.text = sourceText;
                textComponent.ForceMeshUpdate();

                TMP_TextInfo textInfo = textComponent.textInfo;
                int characterCount = textInfo.characterCount;

                Assert.AreEqual(0, textInfo.characterInfo[0].index,
                    $"[{textComponent.GetType().Name}] The first character should start at the beginning of the source string.");

                for (int i = 0; i < characterCount - 1; i++)
                {
                    Assert.AreEqual(textInfo.characterInfo[i].index + textInfo.characterInfo[i].stringLength,
                        textInfo.characterInfo[i + 1].index,
                        $"[{textComponent.GetType().Name}] characterInfo[{i}] should end exactly where characterInfo[{i + 1}] begins.");
                }

                TMP_CharacterInfo lastCharacter = textInfo.characterInfo[characterCount - 1];

                Assert.AreEqual(sourceText.Length, lastCharacter.index + lastCharacter.stringLength,
                    $"[{textComponent.GetType().Name}] The characters together should cover the whole source string.");
            }
        }

        [Test]
        public void PlainCharacters_SpansAreUnchanged()
        {
            // Regression guard: text with no cluster must map one character to one code unit.
            const string sourceText = "abc";

            foreach (TMP_Text textComponent in textComponents)
            {
                textComponent.text = sourceText;
                textComponent.ForceMeshUpdate();

                Assert.AreEqual(3, textComponent.textInfo.characterCount,
                    $"[{textComponent.GetType().Name}] Unexpected character count.");

                AssertCharacterSpan(textComponent, 0, 0, 1);
                AssertCharacterSpan(textComponent, 1, 1, 1);
                AssertCharacterSpan(textComponent, 2, 2, 1);
            }
        }

        [Test]
        public void SurrogatePairWithoutVariationSelector_SpanIsUnchanged()
        {
            // Regression guard: a surrogate pair is already one character over two code units
            // and no cluster block runs for it, so the accounting must not touch it. The default
            // font asset does not necessarily carry this glyph, but the span is captured before
            // the missing glyph substitution region, so the mapping holds either way.
            const string sourceText = "a" + k_AstralCharacter;

            foreach (TMP_Text textComponent in textComponents)
            {
                textComponent.text = sourceText;
                textComponent.ForceMeshUpdate();

                Assert.AreEqual(2, textComponent.textInfo.characterCount,
                    $"[{textComponent.GetType().Name}] Unexpected character count.");

                AssertCharacterSpan(textComponent, 0, 0, 1);
                AssertCharacterSpan(textComponent, 1, 1, 2);
            }
        }

        [Test]
        public void Ligature_IsFoldedIntoTheSpanOfItsFirstComponent()
        {
            const string sourceText = "fi";

            foreach (TMP_Text textComponent in textComponents)
            {
                TMP_FontAsset originalFont = textComponent.font;
                List<OTL_FeatureTag> originalFontFeatures = new List<OTL_FeatureTag>(textComponent.fontFeatures);

                try
                {
                    textComponent.font = m_LigatureFontAsset;

                    List<OTL_FeatureTag> fontFeatures = new List<OTL_FeatureTag>(originalFontFeatures);

                    if (!fontFeatures.Contains(OTL_FeatureTag.liga))
                    {
                        // Assign through the property rather than mutating the list in place, so
                        // the text object is marked dirty the way it would be from user code.
                        fontFeatures.Add(OTL_FeatureTag.liga);
                        textComponent.fontFeatures = fontFeatures;
                    }

                    textComponent.text = sourceText;
                    textComponent.ForceMeshUpdate();

                    Assert.AreEqual(1, textComponent.textInfo.characterCount,
                        $"[{textComponent.GetType().Name}] 'fi' should be substituted by a single ligature character.");

                    // Before the fix this reported (index 1, stringLength 1) - the span of the
                    // last component consumed - so the start of the cluster was lost.
                    AssertCharacterSpan(textComponent, 0, 0, 2);
                }
                finally
                {
                    // Leave the shared fixture components as the other tests expect them.
                    textComponent.fontFeatures = originalFontFeatures;
                    textComponent.font = originalFont;
                    textComponent.text = string.Empty;
                }
            }
        }
    }
}
