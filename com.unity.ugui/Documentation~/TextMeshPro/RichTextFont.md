# Font

You can switch to a different font using `<font="fontAssetName">`.

The font you specify replaces the default font until you insert a closing `<font>` tag. Font tags can be nested.

You can also use the `material` attribute to switch between different materials for a single font.

You must place the font and material assets in the directory that is specified in the **TextMesh Settings > Default Font Asset > Path** field. The default path is `Fonts & Materials`, resolved inside a `Resources` folder of your project, for example `Assets/TextMesh Pro/Resources/Fonts & Materials`. Create the folder if your project does not have it yet.

To revert to the default font:
* Close all open font tags using  `</font>` tag
* Use another `<font>` tag and set the font Asset name to `default`

**Example:**

```
Would you like <font="Impact SDF">a different font?</font> or just <font="NotoSans" material="NotoSans Outline">a different material?
```

![Example image](../images/TMP_RichTextFont.png)<br/>
_Mixing fonts and materials_
