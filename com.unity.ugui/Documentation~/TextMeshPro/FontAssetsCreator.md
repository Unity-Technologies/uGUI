## Font Asset Creator

The Font Asset Creator converts [Unity font assets](FontAssets.md) into TextMesh Pro font assets. You can use it to create both Signed [Distance Field (SDF)](FontAssetsSDF.md) fonts and bitmap fonts.

When you create a new font Asset, TextMesh Pro generates the Asset itself, as well as the atlas texture and material for the font.

After you create a TextMesh Pro font Asset, you can delete the Unity font Asset you used as a source, although you may want to keep it in the Scene in case you need to regenerate the TextMesh Pro font Asset.

## Creating a font Asset

Before you start, make sure that you've already imported the font (usually a TrueType .ttf file) you want to use into the project. For more information about importing fonts into Unity, see the documentation on [Fonts](https://docs.unity3d.com/Manual/class-Font.html) in the Unity manual.

**To create a TextMesh Pro font Asset:**

1. From the menu, choose: **Window > TextMesh Pro > Font Asset Creator** to open the Font Asset Creator.

1. Choose a **Source Font File**. This the Unity font Asset that you want to convert into a TextMesh Pro font Asset.

1. Adjust the **[Font Settings](#FontAssetCreatorSettings)** as needed, then click **Generate Font Atlas** to create the atlas texture<br/><br/>The atlas, and information about the font Asset appear in the texture preview area.<br/><br/>IMAGE

1. Continue adjusting the settings and regenerating the atlas until you're satisfied with the result.

1. Click **Save** or **Save as...** to save the font Asset to your project.<br/><br/>You must save the Asset to a **Resources** folder  to make it accessible to TextMesh Pro.

<a name="FontAssetCreatorSettings"></a>
## Font Asset Creator Settings:

|Property:||Function:|
|-|-|-|
|**Source Font File**||Select a font from which to generate a Text Mesh Pro font Asset.<br/><br/>This font is not included in project builds, unless you use it elsewhere in the project, or put it in a Resources folder.<br/><br/>You can use one of the default TextMesh Pro font assets, or [import your own](https://docs.unity3d.com/Manual/class-Font.html).|
|**Sampling Point Size**||Set the font size, in points, used to generate the font texture.|
|**Auto Sizing**||Use the largest point size possible while still fitting all characters on the texture.<br/><br/>This is the usual setting for SDF fonts.|
|**Custom Size**||Use a custom point size. Enter the desired size in the text box.<br/><br/>Use this setting to achieve pixel-accurate control over bitmap-only fonts.|
|**Padding**||Specify the space, in pixels, between characters in the font texture.<br/><br/>Padding provides the space required to render character separately, and to generate the SDF gradient (See the documentation on [Font Assets](FontAssetsSDF.md) for details).<br/><br/>The larger the padding, the smoother the transition, which allows for higher-quality rendering and larger effects, like thick outlines.<br/><br/>A padding of 5 is often fine for a 512x512 texture.|
|**Packing Method**||Specify how to fit the characters into the font texture.|
||Optimum|Finds the largest possible automatic font size that still fits all characters in the texture.<br/><br/>Use this setting to generate the final font texture.
||Fast|Computes character packing more quickly, but may   use a smaller font size than Optimum mode.<br/><br/>Use this setting when testing out font Asset creation settings.|
|**Atlas Resolution**||Set the size width and height of the font texture, in pixels.<br/><br/>A resolution of 512 x 512 is fine for most fonts, as long as you are only including ASCII characters. Fonts with more characters may require larger resolutions, or multiple atlases. <br/><br/>When using an SDF font, a higher resolution produces finer gradients, and therefore higher quality text.|
|**Character Set**||The characters in a font file aren't included in the font Asset automatically. You have to specify which ones you need. You can select a predefined character set, provide a list of characters to include, or include all of the characters in an existing font Asset or text Asset.|
||ASCII|Includes the visible characters in the ASCII character set.|
||Extended ASCII|Includes the visible characters in the extended ASCII character set.|
||ASCII Lowercase|Includes only visible lower-case characters from the ASCII character set.|
||ASCII Uppercase|Includes only visible upper-case characters from the ASCII character set.|
||Numbers + Symbols|Includes only the visible numbers and symbols from the ASCII character set.|
||Custom Range|Includes a range of characters that you define.<br/><br/>Enter a sequence of decimal values, or ranges of values, to specify which characters to include.<br/><br/>Use a hyphen to separate the first and last values of a range. Use commas to separate values and ranges (for example `32-126,160,8230`).<br/><br/>You can also choose an existing font Asset to include the characters in that Asset.|
||Unicode Range (Hex)|Includes a range of characters that you define.<br/><br/>Enter a sequence of unicode hexadecimal values, or ranges of values, to specify which characters to include.<br/><br/>Use a hyphen to separate the first and last values of a range. Use commas to separate values and ranges (for example `20-7E,A0,2026`).<br/><br/>You can also choose an existing font Asset to include the characters in that Asset.|
||Custom Characters|Includes a range of characters that you define.<br/><br/>Enter a sequence of characters to specify which characters to include.<br/><br/>Enter characters one after the other, with no spaces or delimiting characters in between (for example `abc123*#%`).<br/><br/>You can also choose an existing font Asset to include the characters in that Asset.|
||Characters from File|Includes all the characters in a text Asset that you specify.<br/><br/>Use this option when you want to save your character set.|
|**Font Style**||Apply basic font styling when creating a bitmap-only font Asset.<br/><br/>For SDF fonts, you configure the styling in the shader rather than the font Asset.|
||Normal|Generates characters with no styling.|
||Bold, Italic, Bold_Italic|Generates the font Asset with bold characters, italicized characters, or both.<br/><br/>With these settings, you can set a strength value that applied to bolding and italicization|
||Outline|Generates the font Asset with outline characters.|
||Bold_Sim|Generates the font Asset with a simulated bold.|
|**Render Mode**||Specify the render mode to use when outputting the font atlas.|
||SMOOTH|Renders the atlas to an antialiased bitmap.|
||RASTER|Renders the atlas to a non-antialiased bitmap.|
||SMOOTH_HINTED|Renders the atlas to an antialiased bitmap, and aligns character pixels with texture pixels for a crisper result.|
||RASTER_HINTED|Renders the atlas to a non-antialiased bitmap and aligns character pixels with texture pixels for a crisper result.|
|   |SDF| Renders the atlas using a slower, but more accurate SDF generation mode, and  no oversampling.   |
|   |SDFAA| Renders the atlas using a faster, but less accurate SDF generation mode. It produces font atlases that are sufficient for most situations.|
|   |SDFAA_HINTED| Renders the atlas using a faster, but less accurate SDF generation mode, and aligns character pixels with texture pixels for a crisper result.. It produces font atlases that are sufficient for most situations  |
|   |SDF8|  Renders the atlas using a slower, but more accurate SDF generation mode, and  8x oversampling. |
|   |SDF16| Renders the atlas using a slower, but more accurate SDF generation mode, and  16x oversampling.  |
|   |SDF32|  Renders the atlas using a slower, but more accurate SDF generation mode, and  32x oversampling. Use this setting for fonts with complex or small characters. |
|**Get Kerning Pairs**||Enable this option to copy the kerning data from the font.<br/><br/>Kerning data is used to adjust the spacing between specific character pairs to produce a more visually pleasing result.<br/><br/>**Note:** It isn't always possible to import kerning data. Some fonts store kerning pairs in their glyph positioning (GPOS) table, which is not supported by FreeType, the font engine used by TextMesh Pro.   Other fonts do not store kerning pairs at all.|
|**Generate Font Atlas**||Generate the font atlas texture.|
|**Save**||Save the current font atlas.|
|**Save As**||Save the current font atlas as a new font Asset.|

## Tips for creating font assets










Characters in the font texture need some padding between them so they can be rendered separately. This padding is specified in pixels.
Padding also creates room for the SDF gradient. The larger the padding, the smoother the transition, which allows for higher-quality rendering and larger effects, like thick outlines. A padding of 5 is often fine for a 512x512 texture.


For most fonts, a 512x512 texture resolution is fine when including all ASCII characters.
When you need to support thousands of character, you will have to use large textures. But even at maximum resolution, you might not be able to fit everything. In that case, you can split the characters by creating multiple font assets. Put the most often used characters in a main font Asset, and the others in a fallback font assets.
