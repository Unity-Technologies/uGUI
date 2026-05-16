# Style Sheets

Use style sheets to create custom text styles that you can apply to text using the [`<style>` rich text tag](RichTextStyle.md).

A custom style can include opening and closing rich text tags, as well as leading and trailing text.

For example, you might want headings in your text to be big, red, bold, with an asterisk to either side and a line break at the end.

![Example image](../images/TMP_StyleSheets_ExampleHeading_Render.png)

Instead of typing this for every heading:

`<font-weight=700><size=2em><color=#FF0000>*Heading*</color></size></font-weight><br>`

You can create a style, called `H1` that includes all of that formatting:

![Example image](../images/TMP_StyleSheets_ExampleHeading_Inspector.png)

You can then format all of your headings with a single `<style>` tag:

`<style="H1">Heading</style>`


## The default style sheet

The default style sheet is the style sheet that every TextMesh Pro object in your

TextMesh Pro ships with a default style sheet stored in the **TextMesh Pro > Resources > Style Sheets** folder, but you can set any style sheet to be the default.

To change the default style sheet, set the **Default Style Sheet > Default Style Sheet** option in the [TextMesh Pro settings](Settings.md).

## Per-object style sheets



## Creating custom style sheets

To create a new style sheet, choose **Assets > Create  > TextMesh Pro > Style Sheet** from the menu.

This adds a new TextMesh Pro style sheet to the Project. Open it in the Inspector to add custom styles.
