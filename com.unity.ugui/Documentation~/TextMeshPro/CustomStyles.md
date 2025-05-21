# Styles

Use styles to apply additional formatting to some or all of the text in a TextMesh Pro object. A style is a combination of opening and closing [rich text tags](RichText.md), and can also include leading and trailing characters.

* To define styles, use a TextMesh Pro [style sheet](StyleSheets.md).

* To apply styles to your text, use the [`<style>` rich text tag](RichTextStyle.md) in the text editor.

## Custom styles example

Say you want headings in your text to be big, red, and bold with an asterisk to either side and a line break at the end.

![Example image](../images/TMP_StyleSheets_ExampleHeading_Render.png)

That requires several tags for each heading, which makes the formatting cumbersome to maintain, and the text more difficult to read in the editor.

`<font-weight=700><size=2em><color=#FF0000>*Heading*</color></size></font-weight><br>`

It's easier to put all of the markup in a style. The example below shows a  style called `H1`.

![Example image](../images/TMP_StyleSheets_ExampleHeading_Inspector.png)

Once you create the style you can format all of your headings with a single `<style>` tag.

`<style="H1">Heading</style>`

Not only does that make the text easier to read in the editor, you can now update all of the headings in your text just by changing the style.
