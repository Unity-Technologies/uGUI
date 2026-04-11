# Text Width

Use the `<width>` tag adjust the horizontal size of text area. The change takes effect on the current line, after the tag. Typically, you place the tag at the start of a paragraph.

If you add more than one `,width>` tag to a line, the last one takes precedence over the others.

You can specify the width in either pixels, font units, or percentages. The adjusted width cannot exceed the TextMesh Pro object's original width.

The closing `</width>` tag reverts to the original width.

**Example:**

```
I remember when we had lots of space for text.
<width=60%>But those days are long gone.
```
![Example image](../images/TMP_RichTextWidth.png)<br/>
_Adjusting text area width_
