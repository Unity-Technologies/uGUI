# Migrate static font assets to the Advanced Text Generator

The Advanced Text Generator doesn't support static font assets. If you enable the **Use Advanced Text** option in the [TextMesh Pro settings](Settings.md), migrate the static font assets in your project so your text components render correctly.

The recommended way to migrate is to convert the static font assets to dynamic with the **Font Asset Migration** window. The conversion happens in place, so all references to the font asset keep working, and the source font is [subset](FontAssetsSubsetting.md) so only the baked characters ship in builds. You can optionally keep the baked atlas.

## Convert static font assets to dynamic

The **Font Asset Migration** window lists the static font assets in your project and converts them to dynamic. To open it, do one of the following:

- From the menu, select **Window** > **TextMeshPro** > **Font Asset Migration**.
- In the **Project** window, select a static font asset, then in the **Inspector** window, select **Open Font Asset Migration Window**. The button is displayed only when **Use Advanced Text** is enabled in the TextMesh Pro settings.

To convert static font assets:

1. In the **Font Asset Migration** window, select a font asset from the list to review its source font, baked glyphs, and atlas.
2. Choose what the conversion keeps. By default, SDFAA font assets use **Drop Everything** and the Advanced Text Generator rasterizes glyphs on demand into a fresh atlas. To avoid rasterization at startup, select **Keep Atlas**. If the asset is still used by the standard TextMesh Pro text generation, select **Keep Atlas and Legacy Tables** to also keep the character table and font feature table.
3. Select **Convert Font Asset** to convert the selected asset, or **Convert All** to convert every asset in the list.
4. The first time you convert, Unity displays a font license notice. Converting embeds subset font data in your builds, which static font assets didn't do. Confirm that the font licenses permit embedding, then select **Convert**.

Each converted font asset switches to the **Dynamic** atlas population mode and keeps its references. Unity subsets the source font to the characters baked into the atlas, so builds don't grow by the size of the full font file. You can review or change the subset in the [**Font Subsetting**](FontAssetsSubsetting.md) section of the Inspector window.

Some font assets need attention before conversion:

- If a font asset has baked glyphs but no reference to its source font file, it can't be converted. Assign the source font in the **Inspector** window first.
- If a font asset has no baked glyphs and no source font reference, Unity converts it to an empty dynamic font asset. Assign a source font in the **Inspector** window to populate it.

## Migrate manually

You can also create dynamic font assets yourself instead of converting in place:

1. Create a [dynamic font asset](FontAssetsDynamicFonts.md) from the original font file.
2. To ship a smaller character set, [subset the source font](FontAssetsSubsetting.md). To avoid glyph rasterization at startup, pre-populate the atlas with the [Font Asset Creator](FontAssetsCreator.md).
3. Replace all references to the static font asset.

## Additional resources

- [Font Assets](FontAssets.md)
- [Dynamic Fonts](FontAssetsDynamicFonts.md)
- [Optimize font files with font subsetting](FontAssetsSubsetting.md)
