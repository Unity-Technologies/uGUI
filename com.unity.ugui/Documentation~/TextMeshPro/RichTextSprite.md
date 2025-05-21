# Sprite

The `<sprite>` tag inserts images from a [Sprite Asset](Sprites.md) into your text. Sprite assets must be located in the folder specified in the [TextMesh Pro settings](Settings.md).

You can access sprites from the default sprite assets by index `<sprite index=1>` or by name `<sprite name="spriteName">`. When accessing a sprite from the default Asset by index, you can also use the index shorthand, `<sprite=1>`,

To use sprites from a different Asset, specify the Asset before accessing the sprites by index `<sprite="assetName" index=1>` or by name `<sprite="assetName" name="spriteName">`.

Adding the `tint=1` attribute to the tag tints the sprite with the [TextMesh Pro object's](TMPObjectUIText.md#Color) **Vertex Color**. You can choose a different color by adding a `color` attribute to the tag (`color=#FFFFFF`).

**Example:**

```
Sprites! <sprite=0> More sprites! <sprite index=3> And even more! <sprite name="Default Sprite Asset_4" color=#55FF55FF>
```

![Example image](../images/TMP_RichTextSprite.png)<br/>
_Inserting sprites from the default sprite asset_
