# Horizontal Space

The `<space>` tag inserts a horizontal offset, as if you inserted multiple spaces.

You can specify the offset in pixels or font units.

When the `<space>` tag touches adjacent text, it appends or prepends the offset to that text, which affects how the text wraps. If you do not want the offset to wrap independently of adjacent text, make sure to add a space character on either side of the `<space>` tag.

**Example:**

```
Give me some <space=5em> space.
```

![Example image](../images/TMP_RichTextSpace.png)<br/>
_Adding some space_
