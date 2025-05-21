# Mark

The `<mark>` tag adds an overlay on top of the text. You can use it to highlight portions of your text.

Because markings are overlaid on the text, you have to give them a semitransparent color for the text to show through. You can do this by specifying the color using a hex value that includes Alpha.

You cannot combine marks. Each tag affects the text between itself and the next `<mark>` tag or a closing `</mark>` tag.

**Example:**

```
Text <mark=#ffff00aa>can be marked with</mark> an overlay.
```

![Example image](../images/TMP_RichTextMark.png)<br/>
_Marked text_
