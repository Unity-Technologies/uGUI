# Optimize font files with font subsetting

Font subsetting is the process of reducing a font file's size by including only the specific characters or glyphs needed.

You can subset the source font of a [dynamic font asset](FontAssetsDynamicFonts.md) directly in its Inspector window. When a subset is active, builds that use the font asset include only the subset font data instead of the full font file. The subset is stored with the imported font file, so the original font file isn't modified and references to the font asset are unaffected.

Characters that aren't included in the subset behave like characters that are missing from the font: TextMesh Pro searches the [fallback font assets](FontAssetsFallback.md) for them, and renders them as missing glyphs if no fallback contains them.

For a description of each property in the **Font Subsetting** section, refer to [Font Asset Properties](FontAssetsProperties.md#font-subsetting).

## Prerequisites

The **Font Subsetting** section is displayed in the Inspector window only when the following conditions are met:

- The font asset's **Atlas Population Mode** is set to **Dynamic**.
- The font asset's **Source Font File** is a font file, such as `.ttf` or `.otf`, in your project. You can't subset system fonts, built-in fonts, or fonts in read-only packages.

## Subset the source font of a font asset

To subset the source font of a dynamic font asset:

1. In the **Project** window, select the font asset.
2. In the **Inspector** window, select **Font Subsetting** to expand the section.
3. Set **Preset** to a predefined character set, or set it to **Custom** and enter the characters to keep in the **Characters** field.

   The **Font File Size** line previews the size of the subset font data next to the size of the original font file, and the **Missing Characters** foldout lists the requested code points that the source font doesn't contain; they're left out of the subset.
4. Select **Update Subset**.

The **Font Subsetting** section header now displays **Active**. To change the subset later, edit the character set and select **Update Subset** again.

## Remove a subset

To remove a subset and include the full font file in builds again:

1. In the **Project** window, select the font asset.
2. In the **Inspector** window, expand the **Font Subsetting** section.
3. Select **Remove Subset**.

## Additional resources

- [Font assets](FontAssets.md)
- [Font Asset Properties](FontAssetsProperties.md#font-subsetting)
- [Dynamic font assets](FontAssetsDynamicFonts.md)
- [Fallback font assets](FontAssetsFallback.md)
