# Font Asset Properties

Properties appear in the following groups:

|**Group**|**Description**|
|:--|:--|
| **[Face Info](#face-info)** | Manage the font's line metrics. |
| **[Generation Settings](#generation-settings)** | View the values that the font asset generates. |
| **[Atlas & Material](#atlas-material)** | View the subassets that the font asset generates. |
| **[Font Subsetting](#font-subsetting)** | Subset the source font so builds include only the characters you select. |
| **[Font Weights](#font-weights)** | Control the appearance of bold and italicized text. |
| **[Fallback Font Assets](#fallback-font-assets)** | Manage the list of font assets to use as fallback for missing characters. |
| **[Character Table](#character-table)** | Manage the characters included in the font asset. |
| **[Glyph Table](#glyph-table)** | Adjust the attributes of individual glyphs when you need to correct problems from importing font data. |
| **[Glyph Adjustment Table](#glyph-adjustment-table)** | Control spacing between specific pairs of characters. |
| **[Ligature Table](#ligature-table)** | Manage substitution rules that replace multiple glyphs with a single ligature glyph. |
| **[Mark To Base Adjustment Table](#mark-to-base-adjustment-table)** | Manage positional adjustments between base glyphs and mark glyphs. |
| **[Mark To Mark Adjustment Table](#mark-to-mark-adjustment-table)** | Manage positional adjustments between pairs of mark glyphs. |

### Face Info

The Face Info properties control the font's line metrics. They also include read-only properties that the [Font Asset Creator](FontAssetsCreator.md) generates when you create the asset.

| **Property** | **Description** |
|:--|:--|
|**Update Texture Atlas**|Open the [Font Asset Creator](FontAssetsCreator.md) pre-configured to modify and regenerate this font asset.|
|**Family Name**|The name of the font used to create this font asset.<br/><br/>TextMesh Pro sets this value when you generate the font asset. You can't change it manually.|
|**Style Name**|The style of the font used to create this font asset. For example, **Regular**, **Bold**, or **Italic**.<br/><br/>TextMesh Pro sets this value when you generate the font asset. You can't change it manually.|
|**Point Size**|The font size in points.<br/><br/>TextMesh Pro bakes this value into the atlas texture when you generate the font asset. You can't change it manually.|
|**Scale**|Scales the font by this amount. For example, a value of **1.5** scales glyphs to 150% of their normal size.|
|**Line Height**|Controls the distance between the tops of consecutive lines.<br/><br/>If you set a line height greater than the sum of the **Ascent Line** and **Descent Line** values, it creates a gap between lines.<br/><br/>If you set a line height less than the sum of the **Ascent Line** and **Descent Line** values, characters on different lines might overlap.|
|**Ascent Line**|Controls the maximum distance that glyphs can extend above the baseline. It corresponds to the top of a line.|
|**Cap Line**|Controls the distance between the base line and the tops of uppercase glyphs.|
|**Mean Line**|Controls the maximum height for non-ascending lowercase glyphs (for example, `a` and `c`, but not `b` and `d`, which have ascenders).<br/><br/>The tops of rounded glyphs sometimes extend a slightly above the mean line.|
|**Baseline**|Controls the height of the baseline.<br/><br/>The baseline is the horizontal line that characters sit on.|
|**Descent Line**|Controls the maximum distance that glyphs can extend below the baseline.|
|**Underline Offset**|Controls the position of underlines relative to the baseline.|
|**Underline Thickness**   | Controls the thickness of underlines.  |
|**Strikethrough Offset**|Controls the position of strikethrough lines relative to the baseline.|
|**Superscript Offset**| Offsets superscript text from the baseline.|
|**Superscript Size**|Scales superscript text relative to the normal font size.|
|**Subscript Offset**| Offsets subscript text from the baseline.|
|**Subscript Size**|Scales subscript text relative to the normal font size.|
| **Tab Width**   | Specifies the width of a TAB character.  |

### Generation Settings

The [Font Asset Creator](FontAssetsCreator.md) generates these values when you generate the font asset.

> [!NOTE]
> When the **Atlas Population Mode** is **Dynamic**, you can change the atlas size without regenerating the atlas.

| **Property** | **Description** |
|:--|:--|
| **Source Font File** | Specifies the location of the font file to use as a source. |
| **Font Face** | Selects which font face to use (such as regular or bold). |
| **Atlas Population Mode** | Determines the type of font asset (Static, Dynamic, or Dynamic OS). |
| **Render Mode** | Defines the rendering modes used by the Font Engine to render glyphs. Refer to [Atlas Render Mode settings](#atlas-render-mode-settings) for supported values. |
| **Sampling Point Size** | Determines the size, in points, of characters in the font texture. |
| **Padding** | Defines the amount of padding between characters in the font atlas texture.<br/><br/>The font asset creator sets this when you generate the font asset, and it's not editable.|
| **Atlas Width/Height** | Determines the width and height of the font atlas texture.<br/><br/>For each dimension, select one of the available values from the drop-down menu.|
| **Multi Atlas Textures** | Determines whether the font asset needs to create additional atlas textures. |
| **Clear Dynamic Data On Build** | Determines whether to set the Clear Dynamic Data on Build property to true or false on newly created dynamic font assets. |
| **Get Font Features** | Determines whether to retrieve OpenType font features from the source font file as new characters and glyphs get dynamically added to the font asset. |

#### Atlas Render Mode settings

Valid values for the **Render Mode** property:

| **Setting** | **Description** |
|:--|:--|
|**SMOOTH**|Renders the atlas to an anti-aliased bitmap.|
|**RASTER**|Renders the atlas to a non-anti-aliased bitmap.|
|**SMOOTH_HINTED**|Renders the atlas to an anti-aliased bitmap, and aligns character pixels with texture pixels for a crisper result.|
|**RASTER_HINTED**|Renders the atlas to a non-anti-aliased bitmap and aligns character pixels with texture pixels for a crisper result.|
|**SDF**| Renders the atlas using a slower, but more accurate SDF generation mode, and  no oversampling.   |
|**SDFAA**| Renders the atlas using a faster, but less accurate SDF generation mode. It produces font atlases that are sufficient for most situations.|
|**SDFAA_HINTED**| Renders the atlas using a faster, but less accurate SDF generation mode, and aligns character pixels with texture pixels for a crisper result. It produces font atlases that are sufficient for most situations  |
|**SDF8**|  Renders the atlas using a slower, but more accurate SDF generation mode, and  8x oversampling. |
|**SDF16**| Renders the atlas using a slower, but more accurate SDF generation mode, and  16x oversampling.  |
|**SDF32**|  Renders the atlas using a slower, but more accurate SDF generation mode, and  32x oversampling. Use this setting for fonts with complex or small characters. |

### Atlas & Material

This section lists the subassets that the [Font Asset Creator](FontAssetsCreator.md) creates when you generate the asset. Don't edit these directly.

| **Property** | **Description** |
|:--|:--|
|**Font Atlas**|The font texture atlas created when you generated the font asset.|
|**Font Material**|The font material created when you generated the font asset.|

### Font Subsetting

Subset the source font so builds include only the characters you select. This section is displayed only for dynamic font assets whose source font is a font file in the project. For more information, refer to [Optimize font files with font subsetting](FontAssetsSubsetting.md).

| **Property** | **Description** |
|:--|:--|
|**Active Subset**|Number of code points in the applied subset. Displayed only when a subset is active.|
|**Preset**|Predefined character set that fills the **Characters** field. The presets match the [Font Asset Creator](FontAssetsCreator.md) character sets. Choose from **ASCII**, **Extended ASCII**, **ASCII Lowercase**, **ASCII Uppercase**, **Numbers + Symbols**, **From Baked Atlas** (a snapshot of the characters baked into the atlas), or **Custom** (the characters you enter yourself).|
|**Characters**|Characters to keep in the subset. All other glyphs are removed from the font data included in builds. Displayed only when **Preset** is set to **From Baked Atlas** or **Custom**.|
|**Missing Characters**|Code points that the source font doesn't contain; they're left out of the subset. Format and control characters are listed by code point only, because they have no visual form.|
|**Font File Size**|Size of the original font file and the size of the subset font data. While you edit the character set, the subset size is a preview of what **Update Subset** produces.|
|**Update Subset**|Apply the characters in the **Characters** field as the subset.|
|**Remove Subset**|Remove the applied subset so builds include the full font file.|

### Font Weights

Use one of the following options to control the appearance of bold and italicized text:

- Set references to the bold and italic variants of the font asset. For this method, you need to create your own bold and italic variants of the font asset for weights in these ranges:

    - **100 - Thin**
    - **200 - Extra-Light**
    - **300 - Light**
    - **400 - Regular** (italic only)
    - **500 - Medium** (current font asset — not editable)
    - **600 - Semi-Bold**
    - **700 - Bold**
    - **800 - Heavy**
    - **900 - Black**

- Set weight, spacing, slant, and tab values to control how TextMesh Pro simulates variants of the font asset:

    | **Setting** | **Description** |
    |:--|:--|
    |**Normal Weight**|Set the regular font weight to use when no font asset is available.|
    |**Bold Weight**|Set the bold font weight assumed when no font asset is available.|
    |**Spacing Offset**|Add space between characters when using the normal text style.|
    |**Bold Spacing**|Add space between characters when using the fake bold text style (meaning you haven’t specified a bold font asset).|
    |**Italic Style**|Set the amount of slant you want TextMesh Pro to apply to the Normal Style font asset to simulate an italic font.|
    |**Tab Multiple**|Set the tab size. TextMesh Pro multiplies this value by the width of the font's space character to calculate the tab size used.|

If you don't specify font assets, TextMesh Pro simulates bold and italicization using the values you set. Using simulated font weights limits you to regular and italic versions of normal and bold text (equivalent to weights of 400 and 700 respectively).

<a name="FallbackFontAssets"></a>

### Fallback Font Assets

Each font asset contains a limited number of characters. When you use a character that the current font asset doesn't contain, TextMesh Pro searches the fallback font list until it finds a font asset that includes it. The text object then uses that font to render the character.

You can use this feature to distribute fonts over multiple textures, or use different fonts for specific characters. Be aware that searching the list for missing characters requires extra computing resources, and that using additional fonts requires additional draw calls.

For more information about how fallback fonts work, refer to [Fallback font assets](FontAssetsFallback.md).

| **Property** | **Description** |
|:--|:--|
|**Fallback Font Asset list**|Manage the fallback fonts for this font asset.<br/><br/>Select **+** and **-** to add and remove font slots.<br/><br/>Select the circle icon next to a font to open an Object Picker where you can select a font asset.<br/><br/>Drag the handles on the left side of any font asset to reorder the list.|

### Character Table

#### Character Search

You can use **Character Search** to search the list by **Unicode** or **UTF16** value. 

Search results appear in ascending order by **Unicode**.

#### Previous and Next

Long character lists appear on multiple pages, which you can navigate using the **Previous Page** and **Next Page** buttons. 

These also appear at the bottom of the table.

#### Character information

Use this list to manage the information in this asset:

- Select a character glyph to make it active and enable the controls.
- Enter an unused Unicode (Hex) ID in the text field and select **Copy to** to duplicate this character glyph.
- Select **Remove** to remove this character glyph from the list.

| **Setting** | **Description** |
|:--|:--|
|**Unicode**|Unicode value of the character.|
|**Glyph ID**|A unique ID for the character, based on its position in the list.<br/><br/>Reordering the list updates the **Glyph ID**s of any affected characters.<br/><br/>To redefine its dimensions, select **Edit Glyph**.|

### Glyph Table

The glyph table contains information about each of the glyphs in the font asset. You can adjust the attributes of individual glyphs, which helps correct problems that can occur when TextMesh Pro imports font data.

#### Glyph Search

Search the glyph list by character, ASCII value, or Hex value. 

Search results appear in ascending order by ASCII value, lowest to highest.

#### Previous and Next

Long character lists appear on multiple pages, which you can navigate using the **Previous Page** and **Next Page** buttons. 

These also appear at the bottom of the table.

#### Glyph properties

Displays a single glyph’s properties. Each glyph has its own entry.

Select an entry to make it active. This allows you to:

- Enter an unused Unicode (Hex) ID in the text field and select **Copy to** to duplicate this glyph.
- Select **Remove** to remove this glyph from the list.
- Edit any of these settings:

    | **Setting** | **Description** |
    |:--|:--|
    |**X**, **Y**, **W**, **H** (**Glyph Rect**)|Define the position of a character within a font atlas.|
    |**W**, **H**, **BX**, **BY**, **AD** (**Glyph Metrics**)|Define the character's width, height, horizontal position from the left, vertical position from the top, and how far to advance along the baseline relative to its origin on the baseline.|
    |**Scale**|Change this scaling factor value to adjust the size of the character.|
    |**Atlas Index**|Set the index of the atlas texture that contains this character.|
    |**Class Type**|Select the [class definition type](xref:UnityEngine.TextCore.GlyphClassDefinitionType) for this glyph.|

### Ligature Table

The ligature table provides a list of ligature substitution records. Each of these records defines how to substitute multiple (component) glyphs using a single (ligature) glyph.

#### Ligature Search

Search the table by the index of the ligature glyph. 

#### Previous and Next

Long ligature tables appear on multiple pages, which you can navigate using the **Previous Page** and **Next Page** buttons. 

These also appear at the bottom of the table.

#### Ligature glyph records

Displays a list of ligature glyph records. Each record has its own entry.

- Select a record to make it active and enable the controls.
- Select **Up** or **Down** to move the record up or down in the list.
- Enter the number of **Component Glyphs** you want to specify for this record. An array of text boxes matching that number appear where you can enter the index for each glyph you want to substitute.
- Enter the index of the glyph you want to replace them with under **Ligature Glyph**.
- Select **+** to add a copy of the glyph to the list.
- Select **-** or **Remove** to remove the glyph from the list.

### Glyph Adjustment Table

The glyph adjustment table controls spacing between specific pairs of characters. Some fonts include kerning information, which TextMesh Pro imports automatically. You can add kerning pairs for fonts that don’t include them.

#### Adjustment Pair Search

Search the adjustment table by character or ASCII value. Search results include entries where either the left or right character matches the search string.

Search results appear in ascending order of the left character's ASCII value.

#### Previous and Next

Long adjustment tables appear on multiple pages, which you can navigate using the **Previous Page** and **Next Page** buttons. 

These also appear at the bottom of the table.

#### Glyph properties

Displays a single glyph’s properties. Each glyph has its own entry.

Select an entry to make it active. The left and right characters appear for the kerning pair.

You can adjust the following:

| **Setting** | **Description** |
|:--|:--|
|**OX**, **OY**|For each character in the kerning pair, set the horizontal (**X**) and vertical (**Y**) offset relative to the character's initial position.|
|**AX**|For each character in the kerning pair, specify how far to advance along the baseline before placing the next character.<br/><br/>The left **AX** value controls the distance between the characters in the kerning pair, while the right **AX** value controls the distance between the kerning pair and the next character.|
|**Options**|Select how you want TextMesh Pro to apply this glyph pair adjustment (such as ignoring spacing adjustments or ligatures).|

#### Add New Kerning Pair

Add a new entry to the Glyph Adjustment Table.

You can't duplicate an existing entry.

### Mark To Base Adjustment Table

The mark-to-base adjustment table provides a list of records that define the positional adjustment between a base glyph and a mark glyph.

#### Mark To Base Search

Search the table by the index of either the base glyph or the mark glyph. 

#### Previous and Next

Long mark-to-base adjustment tables appear on multiple pages, which you can navigate using the **Previous Page** and **Next Page** buttons. 

These also appear at the bottom of the table.

#### Mark To Base adjustment records

Displays a list of mark-to-base adjustment glyph records. Each record has its own entry.

- Select a record to make it active and enable the controls.
- Select **+** to add a copy of the glyph to the list.
- Select **-** or **Remove** to remove the glyph from the list.

When an entry is active, you can perform these actions:

- The mark and base glyphs each have an **ID**, and **X** and **Y** values. These pairs appear side by side on each record.
- Edit any of these settings:

    | **Setting** | **Description** |
    |:--|:--|
    |**X**, **Y**|For the base glyph, set the position of the anchor point of the base glyph.<br/>For the mark glyph, set the positional adjustment of the mark glyph relative to the anchor point of the base glyph.|
    |**ID**|Specify the index of the base glyph or the mark glyph.|

### Mark To Mark Adjustment Table

The mark-to-mark adjustment table provides a list of records that define the positional adjustment between two mark glyphs.

#### Mark To Mark Search

Search the table by the index of either the mark glyphs. 

#### Previous and Next

Long mark-to-mark adjustment tables appear on multiple pages, which you can navigate using the **Previous Page** and **Next Page** buttons. 

These also appear at the bottom of the table.

#### Mark To Mark adjustment records

Displays a list of mark-to-mark adjustment glyph records. Each record has its own entry.

- Select a record to make it active and enable the controls.
- Select **+** to add a copy of the glyph to the list.
- Select **-** or **Remove** to remove the glyph from the list.

When an entry is active, you can perform these actions:

- Both mark glyphs each have an **ID**, and **X** and **Y** values. These pairs appear side by side on each record.
- Edit any of these settings:

    | **Setting** | **Description** |
    |:--|:--|
    |**X**, **Y**|For the mark glyph you want to use as the base, set the position of its anchor point.<br/>For the adjustment mark glyph, set the positional adjustment of the glyph relative to the anchor point of the base mark glyph.|
    |**ID**|Specify the index of the two mark glyphs.|
