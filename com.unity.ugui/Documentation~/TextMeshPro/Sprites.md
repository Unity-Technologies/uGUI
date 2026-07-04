# Sprites

TextMesh Pro allows you to include sprites in your text via [rich text tags](RichTextSprite.md).

To use sprites in your Scene, you need a sprite asset. You create sprite assets from atlas textures that each contain a given set of sprites.

![Example image](../images/TMP_SpriteAtlas.png)<br/>
_A sprite atlas texture_

You can use as many sprite atlases and assets as you like, but keep in mind that using multiple atlases per text object results in multiple draw calls for that object, which consumes more system resources. As a rule, try to stick to one atlas per object.

**Note:** Sprites are regular bitmap textures, so make sure that their resolution is high enough to display correctly on your target platforms.

## Use sprite assets

To use a sprite asset in your project, put it in a `Resources/Sprites` folder. This allows TextMesh Pro to find it.

After you add or create your sprite assets, you can set one as the default source for sprites in the project. You set the default sprite asset in the [TextMesh Pro Settings](Settings.md#default-sprite-asset).

You can also choose sprite assets to use with specific text objects. Edit a [TextMesh Pro 3D](TMPObject3DText.md) or [TextMesh Pro UI](TMPObjectUIText.md) asset to specify a sprite asset to use with the font.

## Create a sprite asset

You create sprite assets from atlas textures. Although sprite assets and their source textures are separate entities, you must keep the source textures in the project after creating the sprite assets.

1. Select the texture you want to use for the Sprite Asset.
2. In the Inspector, change the following Texture Importer properties:
    * Set the **Texture Type** to **Sprite (2D and UI)**.
    * Set the **Sprite Mode** to **Multiple**.
3. Open the Sprite Editor from the Inspector, or select **Window > 2D > Sprite Editor** from the menu, and use it to divide the texture into individual sprites.
4. With the texture still selected, select **Asset > Create > TextMesh Pro > Sprite Asset** from the menu to create a new sprite asset.

After creating the sprite asset, you can revert the atlas texture's **Texture Type** to its original setting.

## Sprite Asset Properties

The sprite asset properties fall into the following groups:

- **[Face Info](#face-info):** Provides information about a given typeface at a specific point size.
- **[Atlas & Material](#atlas-_-material):** Provides references to the sprite asset's material and source texture.
- **[Fallback Sprite Assets](#fallback-sprite-assets):** Provides a list of fallback sprite assets.
- **[Sprite Character Table](#sprite-character-table):** Use this table to manage your sprite character's name, position in the list, and access the glyph information.
- **[Sprite Glyph Table](#sprite-glyph-table):** Use this table to manage your sprite glyph's position relative to the bounding box and its scaling factor.
- **[Global Offsets & Scale](#global-offsets-_-scale):** Provides global overrides for sprite position relative to the baseline in the asset.

### Face Info

| **Property** | **Description** |
|:--|:--|
|**Point Size**|Specifies the point size used for sampling the typeface.|
|**Scale**|Defines the relative scale of the typeface.|
|**Ascent Line**|Marks the top of the tallest glyph in the typeface.|
|**Baseline**|Defines the imaginary line upon which all glyphs rest.|
|**Descent Line**|Marks the bottom of the glyph with the lowest descender in the typeface.|

### Atlas & Material

| **Property** | **Description** |
|:--|:--|
|**Sprite Atlas**|References the sprite asset's source texture.|
|**Default Material**|References the sprite asset's material used to render sprites.|

### Fallback Sprite Assets

When TextMesh Pro can't find a glyph in this sprite asset, it searches the fallback sprite assets that you specify here.

| **Property** | **Description** |
|:--|:--|
|**Fallback Sprite Asset List**|Manage the fallback sprite assets.<br/><br/>Select **+** and **-** to add and remove font slots.<br/><br/>Select the circle icon next to a font to select a font asset using the Object Picker.<br/><br/>Drag the handles on the left side of any font asset to reorder the list.|

### Sprite Character Table

#### Sprite Search

You can use **Sprite Search** to search the sprite list by **ID** or **Name**. 

Search results appear in ascending order by **ID**.

#### Previous and Next

Long sprite lists appear on multiple pages, which you can navigate using the **Previous Page** and **Next Page** buttons. 

These also appear at the bottom of the table.

#### Sprite information

Use this list of sprites to manage their information in this asset:

- Select a sprite to make it active and enable the controls.
- Select **Up** or **Down** to move the sprite up or down in the list.
- Enter an **ID** in the text field and select **Goto** to move the sprite to that position in the list.

  **Note:** Moving a sprite updates its **ID** and the **ID**s of all preceding sprites.

- Select **+** to add a copy of the sprite to the list.
- Select **-** to remove the sprite from the list.

| **Setting** | **Description** |
|:--|:--|
|**Unicode**|Unicode value of the sprite character.|
|**Name**|A unique name for the sprite.<br/><br/>You can change this value, but it must be unique in the list<br/><br/>You can use this value in [rich text tags](RichTextSprite.md) to add this sprite to text.|
|**Glyph ID**|A unique ID for the sprite, based on its position in the list.<br/><br/>You can use this value in [rich text tags](RichTextSprite.md) to add this sprite to text.<br/><br/>Reordering the list updates the **Glyph ID**s of any affected sprites.<br/><br/>To redefine its dimensions, select **Edit Glyph**.|
|**X**, **Y**, **W**, **H** (**Glyph Rect**)|The position of a glyph within an atlas texture.|
|**W**, **H**, **BX**, **BY**, **AD** (**Glyph Metrics**)|The glyph's width, height, horizontal position from the left, vertical position from the top, and how far to advance along the baseline relative to its origin on the baseline.|
|**Scale**|Change this scaling factor value to adjust the size of the sprite.|

### Sprite Glyph Table

#### Sprite Search

You can use **Sprite Search** to search the sprite list by **ID**. 

Search results appear in ascending order by **ID**.

#### Previous and Next

Long sprite lists appear on multiple pages. To navigate, use the **Previous Page** and **Next Page** buttons. 

These also appear at the bottom of the table.

#### Glyph information

Use this list of glyphs to manage their information in this asset:

- Select a glyph to make it active and enable the controls.
- Select **Up** or **Down** to move the glyph up or down in the list.
- Enter an **ID** in the text field and select **Goto** to move the glyph to that position in the list.

  **Note:** Moving a glyph updates its **ID** and the **ID**s of all preceding glyphs.

- Select **+** to add a copy of the glyph to the list.
- Select **-** to remove the glyph from the list.

| **Setting** | **Description** |
|:--|:--|
|**X**, **Y**, **W**, **H** (**Glyph Rect**)|The position of a glyph within an atlas texture.|
|**W**, **H**, **BX**, **BY**, **AD** (**Glyph Metrics**)|The glyph's width, height, horizontal position from the left, vertical position from the top, and how far to advance along the baseline relative to its origin on the baseline.|
|**Scale**|Change this scaling factor value to adjust the size of the glyph.|
|**Atlas Index**|The index of the atlas texture that contains this glyph.|

### Global Offsets & Scale

Use these settings to override the following values for all sprites in the asset: 

| **Setting** | **Description** |
|:--|:--|
|**OX** and **OY**|Control the placement of the sprite, defined at its top-left corner relative to its origin on the baseline.|
|**ADV**|Specify how far to advance along the baseline before placing the next sprite.|
|**SF**|Change this scaling factor value to adjust the size of the sprite.|
