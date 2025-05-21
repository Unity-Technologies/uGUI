using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.IO;


namespace TMPro
{
    [Category("Text Parsing & Layout")]
    class TMP_EditorTests
    {
        private TextMeshPro m_TextComponent;

        // Characters: 22  Spaces: 4  Words: 5  Lines:
        private const string m_TextBlock_00 = "A simple line of text.";

        // Characters: 104  Spaces: 14  Words: 15  Lines:
        private const string m_TextBlock_01 = "Unity 2017 introduces new features that help teams of artists and developers build experiences together.";

        // Characters: 1500  Spaces: 228  Words: 241
        private const string m_TextBlock_02 = "The European languages are members of the same family. Their separate existence is a myth. For science, music, sport, etc, Europe uses the same vocabulary. The languages only differ in their grammar, their pronunciation and their most common words." +
            "Everyone realizes why a new common language would be desirable: one could refuse to pay expensive translators.To achieve this, it would be necessary to have uniform grammar, pronunciation and more common words.If several languages coalesce, the grammar of the resulting language is more simple and regular than that of the individual languages." +
            "The new common language will be more simple and regular than the existing European languages.It will be as simple as Occidental; in fact, it will be Occidental.To an English person, it will seem like simplified English, as a skeptical Cambridge friend of mine told me what Occidental is. The European languages are members of the same family." +
            "Their separate existence is a myth. For science, music, sport, etc, Europe uses the same vocabulary.The languages only differ in their grammar, their pronunciation and their most common words.Everyone realizes why a new common language would be desirable: one could refuse to pay expensive translators.To achieve this, it would be necessary to" +
            "have uniform grammar, pronunciation and more common words.If several languages coalesce, the grammar of the resulting language is more simple and regular than that of the individual languages.The new common language will be";

        // Characters: 2500  Spaces: 343  Words: 370
        private const string m_TextBlock_03 = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a, venenatis vitae, justo. " +
            "Nullam dictum felis eu pede mollis pretium.Integer tincidunt.Cras dapibus.Vivamus elementum semper nisi. Aenean vulputate eleifend tellus. Aenean leo ligula, porttitor eu, consequat vitae, eleifend ac, enim.Aliquam lorem ante, dapibus in, viverra quis, feugiat a, tellus.Phasellus viverra nulla ut metus varius laoreet.Quisque rutrum. Aenean imperdiet. Etiam ultricies nisi vel augue.Curabitur ullamcorper ultricies nisi. " +
            "Nam eget dui.Etiam rhoncus. Maecenas tempus, tellus eget condimentum rhoncus, sem quam semper libero, sit amet adipiscing sem neque sed ipsum.Nam quam nunc, blandit vel, luctus pulvinar, hendrerit id, lorem.Maecenas nec odio et ante tincidunt tempus.Donec vitae sapien ut libero venenatis faucibus.Nullam quis ante.Etiam sit amet orci eget eros faucibus tincidunt. Duis leo. Sed fringilla mauris sit amet nibh. " +
            "Donec sodales sagittis magna. Sed consequat, leo eget bibendum sodales, augue velit cursus nunc, quis gravida magna mi a libero. Fusce vulputate eleifend sapien. Vestibulum purus quam, scelerisque ut, mollis sed, nonummy id, metus.Nullam accumsan lorem in dui.Cras ultricies mi eu turpis hendrerit fringilla.Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; In ac dui quis mi consectetuer lacinia. Nam pretium turpis et arcu. " +
            "Duis arcu tortor, suscipit eget, imperdiet nec, imperdiet iaculis, ipsum. Sed aliquam ultrices mauris.Integer ante arcu, accumsan a, consectetuer eget, posuere ut, mauris.Praesent adipiscing. Phasellus ullamcorper ipsum rutrum nunc.Nunc nonummy metus.Vestibulum volutpat pretium libero. Cras id dui.Aenean ut eros et nisl sagittis vestibulum.Nullam nulla eros, ultricies sit amet, nonummy id, imperdiet feugiat, pede.Sed lectus. Donec mollis hendrerit risus. Phasellus nec sem in justo pellentesque facilisis. " +
            "Etiam imperdiet imperdiet orci. Nunc nec neque.Phasellus leo dolor, tempus non, auctor et, hendrerit quis, nisi.Curabitur ligula sapien, tincidunt non, euismod vitae, posuere imperdiet, leo.Maecenas malesuada. Praesent nan. The end of this of this long block of text.";

        // Characters: 3423  Spaces: 453  Words: 500
        private const string m_TextBlock_04 = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit.Aenean commodo ligula eget dolor.Aenean massa.Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus.Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem.Nulla consequat massa quis enim.Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu.In enim justo, rhoncus ut, imperdiet a, venenatis vitae, justo.Nullam dictum felis eu pede mollis pretium.Integer tincidunt. Cras dapibus. Vivamus elementum semper nisi. Aenean vulputate eleifend tellus." +
            "Aenean leo ligula, porttitor eu, consequat vitae, eleifend ac, enim.Aliquam lorem ante, dapibus in, viverra quis, feugiat a, tellus.Phasellus viverra nulla ut metus varius laoreet.Quisque rutrum. Aenean imperdiet. Etiam ultricies nisi vel augue.Curabitur ullamcorper ultricies nisi. Nam eget dui.Etiam rhoncus. Maecenas tempus, tellus eget condimentum rhoncus, sem quam semper libero, sit amet adipiscing sem neque sed ipsum.Nam quam nunc, blandit vel, luctus pulvinar, hendrerit id, lorem.Maecenas nec odio et ante tincidunt tempus.Donec vitae sapien ut libero venenatis faucibus.Nullam quis ante." +
            "Etiam sit amet orci eget eros faucibus tincidunt. Duis leo. Sed fringilla mauris sit amet nibh. Donec sodales sagittis magna. Sed consequat, leo eget bibendum sodales, augue velit cursus nunc, quis gravida magna mi a libero. Fusce vulputate eleifend sapien. Vestibulum purus quam, scelerisque ut, mollis sed, nonummy id, metus.Nullam accumsan lorem in dui.Cras ultricies mi eu turpis hendrerit fringilla.Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; In ac dui quis mi consectetuer lacinia.Nam pretium turpis et arcu." +
            "Duis arcu tortor, suscipit eget, imperdiet nec, imperdiet iaculis, ipsum. Sed aliquam ultrices mauris.Integer ante arcu, accumsan a, consectetuer eget, posuere ut, mauris.Praesent adipiscing. Phasellus ullamcorper ipsum rutrum nunc.Nunc nonummy metus.Vestibulum volutpat pretium libero. Cras id dui.Aenean ut eros et nisl sagittis vestibulum.Nullam nulla eros, ultricies sit amet, nonummy id, imperdiet feugiat, pede.Sed lectus. Donec mollis hendrerit risus. Phasellus nec sem in justo pellentesque facilisis.Etiam imperdiet imperdiet orci. Nunc nec neque." +
            "Phasellus leo dolor, tempus non, auctor et, hendrerit quis, nisi.Curabitur ligula sapien, tincidunt non, euismod vitae, posuere imperdiet, leo.Maecenas malesuada. Praesent congue erat at massa.Sed cursus turpis vitae tortor.Donec posuere vulputate arcu. Phasellus accumsan cursus velit. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Sed aliquam, nisi quis porttitor congue, elit erat euismod orci, ac placerat dolor lectus quis orci.Phasellus consectetuer vestibulum elit.Aenean tellus metus, bibendum sed, posuere ac, mattis non, nunc.Vestibulum fringilla pede sit amet augue." +
            "In turpis. Pellentesque posuere. Praesent turpis. Aenean posuere, tortor sed cursus feugiat, nunc augue blandit nunc, eu sollicitudin urna dolor sagittis lacus. Donec elit libero, sodales nec, volutpat a, suscipit non, turpis.Nullam sagittis. Suspendisse pulvinar, augue ac venenatis condimentum, sem libero volutpat nibh, nec pellentesque velit pede quis nunc. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Fusce id purus.Ut varius tincidunt libero.Phasellus dolor.Maecenas vestibulum mollis";

        //
        private const string m_TextBlock_05 = "This block of text contains <b>bold</b> and <i>italicized</i> characters.";

        private const string m_TextBlock_06 = "<align=center><style=H1><#ffffff><u>Multiple<#80f0ff> Alignment</color> per text object</u></color></style></align><line-height=2em>\n" +
            "</line-height> The <<#ffffa0>align</color>> tag in TextMesh<#40a0ff>Pro</color> provides the ability to control the alignment of lines and paragraphs which is essential when working with text.\n" +
            "<align=left> You may want some block of text to be<#80f0ff>left aligned</color> <<#ffffa0>align=<#80f0ff>left</color></color>> which is sort of the standard.</align>\n" +
            "<style=Quote><#ffffa0>\"Using <#80f0ff>Center Alignment</color> <<#ffffa0>align=<#80f0ff>center</color></color>> for a title or displaying a quote is another good example of text alignment.\"</color></style>\n" +
            "<align=right><#80f0ff>Right Alignment</color> <<#ffffa0>align=<#80f0ff>right</color></color>> can be useful to create contrast between lines and paragraphs of text.\n" +
            "<align=justified><#80f0ff>Justified Alignment</color> <<#ffffa0>align=<#80f0ff>justified</color></color>> results in text that is flush on both the left and right margins. Used well, justified type can look clean and classy.\n" +
            "<style=Quote><align=left><#ffffa0>\"Text formatting and alignment has a huge impact on how people will read and perceive your text.\"</color>\n" +
            "<size=65%><align=right> -Stephan Bouchard</style>";

        private readonly string[] testStrings = new string[] { m_TextBlock_00, m_TextBlock_01, m_TextBlock_02, m_TextBlock_03, m_TextBlock_04, m_TextBlock_05, m_TextBlock_06 };


        [OneTimeSetUp]
        public void Setup()
        {
            if (Directory.Exists(Path.GetFullPath("Assets/TextMesh Pro")) || Directory.Exists(Path.GetFullPath("Packages/com.unity.textmeshpro.tests/TextMesh Pro")))
            {
                GameObject textObject = new GameObject("Text Object");
                m_TextComponent = textObject.AddComponent<TextMeshPro>();

                m_TextComponent.fontSize = 18;
            }
            else
            {
                Debug.Log("Skipping over Editor tests as TMP Essential Resources are missing from the current test project.");
                Assert.Ignore();
            }
        }


        [Test]
        [TestCase("/Package Resources/TMP Essential Resources.unitypackage", "ce4ff17ca867d2b48b5c8a4181611901")]
        [TestCase("/Package Resources/TMP Examples & Extras.unitypackage", "bc00e25696e4132499f56528d3fed2e3")]
        [TestCase("/PackageConversionData.json", "05f5bfd584002f948982a1498890f9a9")]
        public void InternalResourceCheck(string filePath, string guid)
        {
            string packageRelativePath = EditorUtilities.TMP_EditorUtility.packageRelativePath;
            string packageFullPath = EditorUtilities.TMP_EditorUtility.packageFullPath;

            Assert.AreEqual(AssetDatabase.AssetPathToGUID(packageRelativePath + filePath), guid);
            Assert.IsTrue(File.Exists(packageFullPath + filePath));
        }

        // =============================================
        // Font Asset Creation Tests
        // =============================================


        // =============================================
        // Text Parsing and Layout Tests
        // =============================================

        [Test]
        [TestCase(4, 3423, 453, 500, 1)]
        [TestCase(3, 2500, 343, 370, 1)]
        [TestCase(2, 1500, 228, 241, 1)]
        [TestCase(1, 104, 14, 15, 1)]
        [TestCase(0, 22, 4, 5, 1)]
        public void TextParsing_TextInfoTest_WordWrappingDisabled(int sourceTextIndex, int characterCount, int spaceCount, int wordCount, int lineCount)
        {
            m_TextComponent.text = testStrings[sourceTextIndex];
            m_TextComponent.textWrappingMode = TextWrappingModes.NoWrap;
            m_TextComponent.alignment = TextAlignmentOptions.TopLeft;

            // Size the RectTransform
            m_TextComponent.rectTransform.sizeDelta = new Vector2(50, 5);

            // Force text generation to populate the TextInfo data structure.
            m_TextComponent.ForceMeshUpdate();

            Assert.AreEqual(m_TextComponent.textInfo.characterCount, characterCount);
            Assert.AreEqual(m_TextComponent.textInfo.spaceCount, spaceCount);
            Assert.AreEqual(m_TextComponent.textInfo.wordCount, wordCount);
            Assert.AreEqual(m_TextComponent.textInfo.lineCount, lineCount);
        }


        [Test]
        [TestCase(4, 3423, 453, 500, 29)]
        [TestCase(3, 2500, 343, 370, 21)]
        [TestCase(2, 1500, 228, 241, 13)]
        [TestCase(1, 104, 14, 15, 1)]
        [TestCase(0, 22, 4, 5, 1)]
        public void TextParsing_TextInfoTest_WordWrappingEnabled(int sourceTextIndex, int characterCount, int spaceCount, int wordCount, int lineCount)
        {
            m_TextComponent.text = testStrings[sourceTextIndex];
            m_TextComponent.textWrappingMode = TextWrappingModes.Normal;
            m_TextComponent.alignment = TextAlignmentOptions.TopLeft;

            // Size the RectTransform
            m_TextComponent.rectTransform.sizeDelta = new Vector2(100, 50);

            // Force text generation to populate the TextInfo data structure.
            m_TextComponent.ForceMeshUpdate();

            Assert.AreEqual(m_TextComponent.textInfo.characterCount, characterCount);
            Assert.AreEqual(m_TextComponent.textInfo.spaceCount, spaceCount);
            Assert.AreEqual(m_TextComponent.textInfo.wordCount, wordCount);
            Assert.AreEqual(m_TextComponent.textInfo.lineCount, lineCount);
        }


        [Test]
        [TestCase(4, 3423, 453, 500, 27)]
        [TestCase(3, 2500, 343, 370, 20)]
        [TestCase(2, 1500, 228, 241, 13)]
        public void TextParsing_TextInfoTest_TopJustifiedAlignment(int sourceTextIndex, int characterCount, int spaceCount, int wordCount, int lineCount)
        {
            m_TextComponent.text = testStrings[sourceTextIndex];
            m_TextComponent.textWrappingMode = TextWrappingModes.Normal;
            m_TextComponent.alignment = TextAlignmentOptions.TopJustified;

            // Size the RectTransform
            m_TextComponent.rectTransform.sizeDelta = new Vector2(100, 50);

            // Force text generation to populate the TextInfo data structure.
            m_TextComponent.ForceMeshUpdate();

            Assert.AreEqual(m_TextComponent.textInfo.characterCount, characterCount);
            Assert.AreEqual(m_TextComponent.textInfo.spaceCount, spaceCount);
            Assert.AreEqual(m_TextComponent.textInfo.wordCount, wordCount);
            Assert.AreEqual(m_TextComponent.textInfo.lineCount, lineCount);
        }


        [Test]
        [TestCase(6, 768, 124, 126, 14)]
        [TestCase(5, 59, 8, 9, 1)]
        public void TextParsing_TextInfoTest_RichText(int sourceTextIndex, int characterCount, int spaceCount, int wordCount, int lineCount)
        {
            m_TextComponent.text = testStrings[sourceTextIndex];
            m_TextComponent.textWrappingMode = TextWrappingModes.Normal;
            m_TextComponent.alignment = TextAlignmentOptions.TopLeft;

            // Size the RectTransform
            m_TextComponent.rectTransform.sizeDelta = new Vector2(70, 35);

            // Force text generation to populate the TextInfo data structure.
            m_TextComponent.ForceMeshUpdate();

            Assert.AreEqual(m_TextComponent.textInfo.characterCount, characterCount);
            Assert.AreEqual(m_TextComponent.textInfo.spaceCount, spaceCount);
            Assert.AreEqual(m_TextComponent.textInfo.wordCount, wordCount);
            Assert.AreEqual(m_TextComponent.textInfo.lineCount, lineCount);
        }

        // =============================================
        // Markup tag specific tests
        // =============================================

        [Test]
        [TestCase("<scale=1.0>ABC</scale>", -35.0f, -33.8069763f, -33.8069763f, -32.6139526f, -32.6139526f, -31.3162785f)]
        [TestCase("<scale=0.8>ABC</scale>", -35.0f, -34.0455818f, -34.0455818f, -33.0911636f, -33.0911636f, -32.0530243f)]
        [TestCase("<scale=1.2>ABC</scale>", -35.0f, -33.5683708f, -33.5683708f, -32.1367455f, -32.1367455f, -30.5795345f)]
        public void MarkupTag_Scale(string sourceText, float origin1, float advance1, float origin2, float advance2, float origin3, float advance3)
        {
            m_TextComponent.text = sourceText;
            m_TextComponent.textWrappingMode = TextWrappingModes.Normal;
            m_TextComponent.alignment = TextAlignmentOptions.TopLeft;

            // Size the RectTransform
            m_TextComponent.rectTransform.sizeDelta = new Vector2(70, 35);

            // Force text generation to populate the TextInfo data structure.
            m_TextComponent.ForceMeshUpdate();

            Assert.AreEqual(origin1, m_TextComponent.textInfo.characterInfo[0].origin);
            Assert.AreEqual(advance1, m_TextComponent.textInfo.characterInfo[0].xAdvance);
            Assert.AreEqual(origin2, m_TextComponent.textInfo.characterInfo[1].origin);
            Assert.AreEqual(advance2, m_TextComponent.textInfo.characterInfo[1].xAdvance);
            Assert.AreEqual(origin3, m_TextComponent.textInfo.characterInfo[2].origin);
            Assert.AreEqual(advance3, m_TextComponent.textInfo.characterInfo[2].xAdvance);
        }

        [Test]
        [TestCase("<size=12>ABC</size>", -35.0f, -34.2046509f, -34.2046509f, -33.4093018f, -33.4093018f, -32.5441856f)]
        [TestCase("<size=-6>ABC</size>", -35.0f, -34.2046509f, -34.2046509f, -33.4093018f, -33.4093018f, -32.5441856f)]
        [TestCase("<size=+6>ABC</size>", -35.0f, -33.4093018f, -33.4093018f, -31.8186054f, -31.8186054f, -30.0883713f)]
        [TestCase("<size=50%>ABC</size>", -35.0f,  -34.4034882f, -34.4034882f, -33.8069763f, -33.8069763f, -33.1581383f)]
        [TestCase("<size=150%>DEF</size>", -35.0f, -33.0534897f, -33.0534897f, -31.2639542f, -31.2639542f, -29.6000004f)]
        [TestCase("<size=0.5em>ABC</size>", -35.0f,  -34.4034882f, -34.4034882f, -33.8069763f, -33.8069763f, -33.1581383f)]
        [TestCase("<size=1.5em>DEF</size>", -35.0f, -33.0534897f, -33.0534897f, -31.2639542f, -31.2639542f, -29.6000004f)]
        public void MarkupTag_Size(string sourceText, float origin1, float advance1, float origin2, float advance2, float origin3, float advance3)
        {
            m_TextComponent.text = sourceText;
            m_TextComponent.textWrappingMode = TextWrappingModes.Normal;
            m_TextComponent.alignment = TextAlignmentOptions.TopLeft;

            // Size the RectTransform
            m_TextComponent.rectTransform.sizeDelta = new Vector2(70, 35);

            // Force text generation to populate the TextInfo data structure.
            m_TextComponent.ForceMeshUpdate();

            Assert.AreEqual(origin1, m_TextComponent.textInfo.characterInfo[0].origin);
            Assert.AreEqual(advance1, m_TextComponent.textInfo.characterInfo[0].xAdvance);
            Assert.AreEqual(origin2, m_TextComponent.textInfo.characterInfo[1].origin);
            Assert.AreEqual(advance2, m_TextComponent.textInfo.characterInfo[1].xAdvance);
            Assert.AreEqual(origin3, m_TextComponent.textInfo.characterInfo[2].origin);
            Assert.AreEqual(advance3, m_TextComponent.textInfo.characterInfo[2].xAdvance);
        }

        [Test]
        [TestCase("<cspace=30>ABC</cspace>", -35.0f, -30.8069763f, -30.8069763f, -26.6139526f, -26.6139526f, -25.3162804f)]
        [TestCase("<cspace=2em>ABC</cspace>", -35.0f, -30.2069759f, -30.2069759f, -25.4139519f, -25.4139519f, -24.1162796f)]
        public void MarkupTag_Cspace(string sourceText, float origin1, float advance1, float origin2, float advance2, float origin3, float advance3)
        {
            m_TextComponent.text = sourceText;
            m_TextComponent.textWrappingMode = TextWrappingModes.Normal;
            m_TextComponent.alignment = TextAlignmentOptions.TopLeft;

            // Size the RectTransform
            m_TextComponent.rectTransform.sizeDelta = new Vector2(70, 35);

            // Force text generation to populate the TextInfo data structure.
            m_TextComponent.ForceMeshUpdate();

            Assert.AreEqual(origin1, m_TextComponent.textInfo.characterInfo[0].origin);
            Assert.AreEqual(advance1, m_TextComponent.textInfo.characterInfo[0].xAdvance);
            Assert.AreEqual(origin2, m_TextComponent.textInfo.characterInfo[1].origin);
            Assert.AreEqual(advance2, m_TextComponent.textInfo.characterInfo[1].xAdvance);
            Assert.AreEqual(origin3, m_TextComponent.textInfo.characterInfo[2].origin);
            Assert.AreEqual(advance3, m_TextComponent.textInfo.characterInfo[2].xAdvance);

        }

        [Test]
        [TestCase("<mspace=30>ABC</mspace>", -34.0965118f, -32.0f, -31.1279068f, -29.0f, -28.1593018f, -26.0f)]
        [TestCase("<mspace=2em>ABC</mspace>", -33.7965126f, -31.3999996f, -30.2279072f, -27.7999992f, -26.6593018f, -24.2000008f)]
        public void MarkupTag_Mspace(string sourceText, float origin1, float advance1, float origin2, float advance2, float origin3, float advance3)
        {
            m_TextComponent.text = sourceText;
            m_TextComponent.textWrappingMode = TextWrappingModes.Normal;
            m_TextComponent.alignment = TextAlignmentOptions.TopLeft;

            // Size the RectTransform
            m_TextComponent.rectTransform.sizeDelta = new Vector2(70, 35);

            // Force text generation to populate the TextInfo data structure.
            m_TextComponent.ForceMeshUpdate();

            Assert.AreEqual(origin1, m_TextComponent.textInfo.characterInfo[0].origin);
            Assert.AreEqual(advance1, m_TextComponent.textInfo.characterInfo[0].xAdvance);
            Assert.AreEqual(origin2, m_TextComponent.textInfo.characterInfo[1].origin);
            Assert.AreEqual(advance2, m_TextComponent.textInfo.characterInfo[1].xAdvance);
            Assert.AreEqual(origin3, m_TextComponent.textInfo.characterInfo[2].origin);
            Assert.AreEqual(advance3, m_TextComponent.textInfo.characterInfo[2].xAdvance);
        }

        [Test]
        [TestCase("A<space=18>B<space=18>C", -35.0f, -33.8069763f, -32.0069771f, -30.8139534f, -29.0139542f, -27.7162781f)]
        [TestCase("A<space=1em>B<space=1em>C", -35.0f, -33.8069763f, -32.0069771f, -30.8139534f, -29.0139542f, -27.7162781f)]
        public void MarkupTag_Space(string sourceText, float origin1, float advance1, float origin2, float advance2, float origin3, float advance3)
        {
            m_TextComponent.text = sourceText;
            m_TextComponent.textWrappingMode = TextWrappingModes.Normal;
            m_TextComponent.alignment = TextAlignmentOptions.TopLeft;

            // Size the RectTransform
            m_TextComponent.rectTransform.sizeDelta = new Vector2(70, 35);

            // Force text generation to populate the TextInfo data structure.
            m_TextComponent.ForceMeshUpdate();

            Assert.AreEqual(origin1, m_TextComponent.textInfo.characterInfo[0].origin);
            Assert.AreEqual(advance1, m_TextComponent.textInfo.characterInfo[0].xAdvance);
            Assert.AreEqual(origin2, m_TextComponent.textInfo.characterInfo[1].origin);
            Assert.AreEqual(advance2, m_TextComponent.textInfo.characterInfo[1].xAdvance);
            Assert.AreEqual(origin3, m_TextComponent.textInfo.characterInfo[2].origin);
            Assert.AreEqual(advance3, m_TextComponent.textInfo.characterInfo[2].xAdvance);
        }

        [Test]
        [TestCase("A<pos=10%>B<pos=20%>C", -35.0f, -33.8069763f, -28.0f, -26.8069763f, -21.0f, -19.7023258f)]
        [TestCase("A<pos=70>B<pos=140>C", -35.0f, -33.8069763f, -28.0f, -26.8069763f, -21.0f, -19.7023258f)]
        [TestCase("A<pos=1.5em>B<pos=3em>CC", -35.0f, -33.8069763f, -32.2999992f, -31.1069775f, -29.6000004f, -28.3023262f)]
        public void MarkupTag_Pos(string sourceText, float origin1, float advance1, float origin2, float advance2, float origin3, float advance3)
        {
            m_TextComponent.text = sourceText;
            m_TextComponent.textWrappingMode = TextWrappingModes.Normal;
            m_TextComponent.alignment = TextAlignmentOptions.TopLeft;

            // Size the RectTransform
            m_TextComponent.rectTransform.sizeDelta = new Vector2(70, 35);

            // Force text generation to populate the TextInfo data structure.
            m_TextComponent.ForceMeshUpdate();

            Assert.AreEqual(origin1, m_TextComponent.textInfo.characterInfo[0].origin);
            Assert.AreEqual(advance1, m_TextComponent.textInfo.characterInfo[0].xAdvance);
            Assert.AreEqual(origin2, m_TextComponent.textInfo.characterInfo[1].origin);
            Assert.AreEqual(advance2, m_TextComponent.textInfo.characterInfo[1].xAdvance);
            Assert.AreEqual(origin3, m_TextComponent.textInfo.characterInfo[2].origin);
            Assert.AreEqual(advance3, m_TextComponent.textInfo.characterInfo[2].xAdvance);
        }

        [Test]
        [TestCase("<indent=18>ABC", -33.2000008f, -32.0069771f, -32.0069771f, -30.8139534f, -30.8139534f, -29.5162792f)]
        [TestCase("<indent=1em>ABC", -33.2000008f, -32.0069771f, -32.0069771f, -30.8139534f, -30.8139534f, -29.5162792f)]
        [TestCase("<indent=2.5%>ABC", -33.25f, -32.0569763f, -32.0569763f, -30.8639526f, -30.8639526f, -29.5662804f)]
        public void MarkupTag_Indent(string sourceText, float origin1, float advance1, float origin2, float advance2, float origin3, float advance3)
        {
            m_TextComponent.text = sourceText;
            m_TextComponent.textWrappingMode = TextWrappingModes.Normal;
            m_TextComponent.alignment = TextAlignmentOptions.TopLeft;

            // Size the RectTransform
            m_TextComponent.rectTransform.sizeDelta = new Vector2(70, 35);

            // Force text generation to populate the TextInfo data structure.
            m_TextComponent.ForceMeshUpdate();

            Assert.AreEqual(origin1, m_TextComponent.textInfo.characterInfo[0].origin);
            Assert.AreEqual(advance1, m_TextComponent.textInfo.characterInfo[0].xAdvance);
            Assert.AreEqual(origin2, m_TextComponent.textInfo.characterInfo[1].origin);
            Assert.AreEqual(advance2, m_TextComponent.textInfo.characterInfo[1].xAdvance);
            Assert.AreEqual(origin3, m_TextComponent.textInfo.characterInfo[2].origin);
            Assert.AreEqual(advance3, m_TextComponent.textInfo.characterInfo[2].xAdvance);
        }

#if TMP_TEST_RESOURCES
        const string k_SpriteAssetPath = "Assets/TextMesh Pro/Resources/Sprite Assets/MixedIndexTest.asset";
        [Test]
        public void SpriteAssetIndexAreValidAfterReordering()
        {
            var spriteAsset = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(k_SpriteAssetPath);
            if (spriteAsset == null)
            {
                Debug.LogError("Failed to load Sprite Asset at path: " + k_SpriteAssetPath);
                return;
            }

            string text = $"<sprite name=\"cta_obsidianjade\">";
            m_TextComponent.spriteAsset = spriteAsset;
            m_TextComponent.text = text;
            m_TextComponent.ForceMeshUpdate();

            Assert.AreEqual(203, m_TextComponent.textInfo.characterInfo[0].textElement.glyphIndex, $"Mismatch between sprite index. Expected 203 but was {m_TextComponent.textInfo.characterInfo[0].textElement.glyphIndex}");
        }
#endif


        // Add tests that check position of individual characters in a complex block of text.
        // These test also use the data contained inside the TMP_TextInfo class.


        //[OneTimeTearDown]
        //public void Cleanup()
        //{
        //    // Remove TMP Essential Resources if they were imported in the project as a result of running tests.
        //    if (TMPro_EventManager.temporaryResourcesImported == true)
        //    {
        //        if (Directory.Exists(Path.GetFullPath("Assets/TextMesh Pro")))
        //        {
        //            AssetDatabase.DeleteAsset("Assets/TextMesh Pro");
        //            TMPro_EventManager.temporaryResourcesImported = false;
        //        }
        //    }
        //}

    }
}
