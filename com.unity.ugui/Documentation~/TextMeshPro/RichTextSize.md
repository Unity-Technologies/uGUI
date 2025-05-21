# Font Size

Use the `<size>` tag to adjust the font size of your text.

You can specify the new size in pixels (`px`), font units (`em`), or percentages (`%`).

Pixel adjustments can be absolute (`5px`, `10px`, and so on) or relative (`+1` or `-1`, for example). Relative sizes are based on the original font size, so they're not cumulative.

Font unit adjustments are always relative to the original font size. For example, `<size=1em>` sets the font size to the original size, `<size=2em>` doubles the size, and `<size=0.5em>` halves it.

**Example:**

```
<size=100%>Echo <size=80%>Echo <size=60%>Echo <size=40%>Echo <size=20%>Echo
```

![Example image](../images/TMP_RichTextSize.png)<br/>
_Adjusting font size_
