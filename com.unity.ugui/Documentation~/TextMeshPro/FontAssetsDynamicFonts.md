
# Dynamic fonts assets
Normally when you generate a font Asset using the Font Asset Creator, you choose which characters to include, and bake them into a Font Atlas texture.

Dynamic font assets work the other way around. Instead of baking characters into an atlas in advance, you start with an empty atlas to which characters are added automatically as you use them.

This makes dynamic fonts assets more flexible, but that flexibility comes at a cost.

* Dynamic fonts require more computational resources than static fonts.

* Dynamic font assets maintain a link to the original font file used to create them. That means:

    * During development, you must keep the font file in the project. You cannot delete it as you can the source fonts of static font assets.
    * Source fonts of any dynamic font assets in your game are included in builds, which can increase build size.


This has several uses, for example:

* Use dynamic fonts during development to capture characters you forgot to include in your baked font assets.

* Use dynamic fonts in runtime when you don't know in advance which characters the user will enter in a text field.

## Creating a dynamic font Asset

Empty font assets are dynamic by default. To create one:

* From Unity's main menu, choose **Assets > Create > TextMeshPro > Font Asset** or press **Ctrl/Cmd + Shift + F12**.

To make an existing font Asset dynamic:

1. Select Asset and open it in the Inspector.

1. Set the **Generation Settings > Atlas Population Mode** property to **Dynamic**.

## Resetting a dynamic font Asset

You reset TextMesh Pro dynamic font assets, the same way you reset other components: by choosing **Reset** from the gear icon menu or context menu in the Inspector.

[IMAGE]

However, instead of resetting all of the Asset's properties to their default values, the command affects only:

* The Font Atlas
* The Character Table
* The Glyph Table
* The Glyph Adjustment Table (kerning)

These are reset to include only the characters/glyphs used by TextMesh Pro text objects that use the font Asset.

If the Asset is currently unused, TextMesh Pro resizes the atlas texture to 0 x 0 pixels.

**NOTE:** Resetting a static font Asset leaves the atlas texture as-is, but empties the character-, glyph-, and glyph adjustment tables. 