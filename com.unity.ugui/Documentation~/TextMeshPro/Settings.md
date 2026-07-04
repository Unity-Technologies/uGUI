# Settings

TextMesh Pro stores its project-wide settings in a special **TMP Settings** asset. This asset must be stored in a `Resources` folder. By default it’s in the `Assets/TextMesh Pro` folder.

To edit the settings, either select the asset in the **Project window** or open the **Project Settings** window and select **TextMesh Pro** from the category list.

The settings appear under the following groups:

| **Group** | **Description** |
|:--|:--|
| **[Default Font Asset](#default-font-asset)** | Set the default font for text objects. |
| **[Fallback Font Assets](#fallback-font-assets)** | Choose font assets to search when TextMesh Pro can’t find a character in a text object’s main font asset. |
| **[Fallback Material Settings](#fallback-material-settings)** | Set style options for characters retrieved from fallback fonts. |
| **[Dynamic Font System Settings](#dynamic-font-system-settings)** | Set options for handling missing characters. |
| **[Text Container Default Settings](#text-container-default-settings)** | Control the size of the text container for new text objects. |
| **[Text Component Default Settings](#text-component-default-settings)** | Set the basic text formatting options for new text objects. |
| **[Default Sprite Asset](#default-sprite-asset)** | Choose a default Sprite asset to use for rich text sprite tags that don't specify an asset, and set other sprite-related options. |
| **[Fallback Emoji Text Assets](#fallback-emoji-text-assets)** | Manage the list of text assets to use to look up characters defined as emojis. |
| **[Default Style Sheet](#default-style-sheet)** | Choose a default style sheet. |
| **[Color Gradient Presets](#color-gradient-presets)** | Choose a location to store color gradient presets. |
| **[Line Breaking for Asian Languages](#line-breaking-for-asian-languages)** | Define leading and following characters to get proper line breaking when using Asian fonts. |
| **[Korean Language Options](#korean-language-options)** | Define whether to use Modern or Traditional line breaking rules for Korean text. |

## Default Font Asset

| **Property** | **Description** |
|:--|:--|
|**Default Font Asset**|Specify the default font used when you create a new text object.
|**Path**|Specify where to store font assets.<br/><br/>The **Path** must point to  a subfolder of the `Resources` folder.|

## Fallback Font Assets

When a text object contains a character that's missing from its font asset, TextMesh Pro searches these font assets for the glyph. If the object’s font asset has a local fallback font list, TextMesh Pro searches the fonts in that list first.

| **Property** | **Description** |
|:--|:--|
|**Fallback Font Assets List**|Manage the global fallback font assets.<br/><br/>Select **+** and **-** to add and remove font slots.<br/><br/>Select the circle icon next to a font to choose a font asset using the Object Picker.<br/><br/>Drag the handles on the left side of any font asset to reorder the list.|

## Fallback Material Settings

| **Property** | **Description** |
|:--|:--|
|**Match Material Presets**|Enable this setting to make glyphs from the fallback font match the style of the main font.<br/><br/>When TextMesh Pro uses a glyph from a fallback font, it creates a material with the same settings as the main font’s material.<br/><br/>This looks best when the main font and the fallback font are similar.|
|**Hide Sub Text Objects**|Determines whether sub text objects are hidden in the scene hierarchy.|

## Dynamic Font System Settings

These are project-wide settings for handling missing glyphs.

| **Property** | **Description** |
|:--|:--|
|**Missing Character Unicode**|Specify the ID of the character to use when TextMesh Pro can't find a missing glyph in any of the fallback fonts.<br/><br/>The default value of 0 produces the outline of a square.|
|**Disable warnings**|Enable this setting to prevent Unity from logging a warning for every missing glyph.|
|**Get Font Features at Runtime**  | Determines whether to retrieve OpenType Font features from the source font file at runtime. |
|**Clear Dynamic Data On Build**| Determines whether to set the Clear Dynamic Data on Build property to true or false on newly created dynamic font assets. |

## Text Container Default Settings

These settings define the default size for text containers in new text objects.

| **Property** | **Description** |
|:--|:--|
|**TextMeshPro**|Set the default size of text containers for new TextMesh Pro 3D GameObjects, in Unity units.|
|**TextMeshPro UI**|Set the default size of text containers for new TextMesh Pro UI GameObjects, in Unity units.|
|**Enable Raycast Target**  | Enable this option to make TextMesh Pro GameObjects targets for ray casting by default. <br/><br/> When you disable this option, the UI ignores TextMesh Pro GameObjects by default when determining what the cursor interacts with. |
|**Auto Size Text Container**|Enable this option to automatically size text containers to fit the text when creating new TextMesh Pro UI GameObjects.|
|**Is Object Scale Static**|Enable this option to disable internal updates. This can improve performance when the scale of the text object is static.|

## Text Component Default Settings

These settings define default values for new text objects. After adding a text object to the Scene, you can adjust these settings in the object's TextMesh Pro Inspector.

| **Property** | **Description** |
|:--|:--|
|**Default Font Size**|Set the default font size, in points.|
|**Text Auto Size Ratios**|Set the default **Min** to **Max** size ratio TextMesh Pro uses when it [sets font size automatically](TMPObjectUIText.md#font).|
|**Text Wrapping Mode**|Enable this option to turn text wrapping on for all new text objects.|
|**Font Features**|Defines which font features to enable by default on newly created text objects.|
|**Extra Padding**|Enable this option to add extra padding to character sprites.<br/><br/>TextMesh Pro creates sprites to fit the visible text, but the results aren't always perfect. This setting reduces the chances that glyphs are cut off at the boundaries of their sprites.|
|**Tint All Sprites**|By default, sprites aren't affected by the text's vertex colors. Enable Tint All Sprites changes this.|
|**Parse Escape Sequence**|Enable this option to make TextMesh Pro interpret backslash-escaped characters as special characters.<br/><br/>For example, TextMesh Pro interprets `\n` as a newline and `\t` as a tab.<br/><br/>**Note:** This applies to rendered text. In code, escaped characters are already parsed by the compiler.|

## Default Sprite Asset

| **Property** | **Description** |
|:--|:--|
|**Default Sprite Asset**  | Choose the [Sprite asset](Sprites.md) for TextMesh Pro GameObjects to use by default. |
|**Missing Sprite Unicode**  | Define the Unicode value of the sprite to use when the requested sprite is missing from the sprite asset and potential fallbacks. |
|**iOS Emoji Support**  | Toggle support for iOS emoji. |
|**Path** | Specify where to store Sprite assets.<br/><br/>The **Path** must point to a subfolder of the `Resources` folder. |

## Fallback Emoji Text Assets

When a text object contains a character that's an emoji, TextMesh Pro searches this list of text assets (font assets and sprite assets) to find characters defined as emojis.

| **Property** | **Description** |
|:--|:--|
|**Text Asset List**|Manage the list of fallback emoji text assets.<br/><br/>Select **+** and **-** to add and remove font slots.<br/><br/>Select the circle icon next to a font to choose a font asset using the Object Picker.<br/><br/>Drag the handles on the left side of any font asset to reorder the list.|

## Default Style Sheet

| **Property** | **Description** |
|:--|:--|
|**Default Style Sheet**|You can choose a single [style sheet](StyleSheets.md) asset to use for all text objects in the project.|
|**Path**|Specify where to store style sheets.<br/><br/>The **Path** must point to a subfolder of the `Resources` folder. |

## Color Gradient Presets

| **Property** | **Description** |
|:--|:--|
|**Path**|Specify where to store Sprite assets.<br/><br/>The **Path** must point to a subfolder of the `Resources` folder.|

## Line Breaking for Asian Languages

To correctly display line-breaks for Asian languages, you must specify which characters behave as leading and following characters:

| **Property** | **Description** |
|:--|:--|
|**Leading Characters**|Specify the text file that contains the list of leading characters.|
|**Following Characters**|Specify the text file that contains the list of following characters.|

## Korean Language Options

| **Property** | **Description** |
|:--|:--|
|**Use Modern Line Breaking**|Enable this option to use Modern line breaking rules for Korean text. Disable to use Traditional line breaking rules.|
