# Fallback font assets

A font atlas, and by extension a font Asset, can only contain a certain number of glyphs. The exact number depends on the font, the size of the atlas texture, and the settings you use when generating the atlas. The fallback font system allows you to specify other font assets to search when TextMesh Pro can't find a glyph in a text object's font Asset.

This is useful in a variety of situations, including:
* Working with languages that have very large alphabets (Chinese, Korean, and Japanese, for example). Use fallback fonts to distribute an alphabet across several assets.

* Designing for mobile devices, where an imposed maximum texture size prevents you from fitting an entire set of glyphs in a single atlas of  sufficient quality.

* Including special characters from other alphabets in your text.

## Local and general fallback font assets

Every font Asset can have its own list of fallback font assets. You set these in the [font Asset properties](FontAssetsProperties.md).

You can also set general fallback font assets that apply to every TextMesh Pro font Asset in your project. You set these in the [TextMesh Pro settings](Settings.md).

## The fallback chain

In addition to a text object's fallback fonts, TextMesh Pro searches several other assets for missing glyphs. Together, these assets form the fallback chain.

The table below lists the assets in the fallback chain in the order in which they are searched.

|Position:| Asset: | Defined in:|Notes:|
|:-:|-|-||
|1   | TextMesh Pro object's primary **Font Asset**  | [Text object properties](TMPObjects.md) ||  
|2   | Primary font assets **Fallback Font Assets**  | [Font Asset properties](FontAssetsProperties.md)  |TexMesh Pro searches these assets in the order they're listed in the [font Asset properties](FontAssetsProperties.md). <br/><br/>The search is recursive, and includes each fallback Asset's fallback assets. |
|3   | Text object's **Sprite Asset**  | [Text object properties](TMPObjects.md)  |When searching sprite assets, TextMesh Pro looks for sprites with an assigned unicode value that matches the missing character's unicode value.|
|4   | General **Fallback Font Assets**  | [TextMesh Pro settings](Settings.md)  |TexMesh Pro searches these assets in the order they're listed in the [font Asset properties](FontAssetsProperties.md). <br/><br/>The search is recursive, and includes each fallback Asset's fallback assets. |
|5   | **Default Sprite Asset**  | [TextMesh Pro settings](Settings.md)  |When searching sprite assets, TextMesh Pro looks for sprites with an assigned unicode value that matches the missing character's unicode value.|
|6   | **Default Font Asset** | [TextMesh Pro settings](Settings.md)  | |
|7   | **Missing glyphs** character | [TextMesh Pro settings](Settings.md)  |  |

The fallback chain search is designed to detect circular references so each Asset in the chain is only searched once.
