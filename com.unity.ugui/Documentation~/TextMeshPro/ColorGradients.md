# Color Gradients

You can apply gradients of up to four colors to TextMesh Pro GameObjects. When you add a gradient, TextMesh Pro applies it to each character in the text individually. It stores gradient colors as in each character sprite's vertex colors.  

![Example image](../images/TMP_ColorGradient_4-Corner-no-legend.png)

_TextMesh Pro text with a four-color gradient_

Because each character sprite consists of two triangles, gradients tend to have a dominant direction. This is most obvious in diagonal gradients.

For example, the dominant direction in gradient below favors the red and black colors in the bottom-left and top-right corners

![Example image](../images/TMP_ColorGradient_3-Corner-YBRY_half.png)

When you reverse the gradient colors, so both the top-right and bottom-left corners are yellow, the dominant color changes.

![Example image](../images/TMP_ColorGradient_3-Corner-RYYB_half.png)


TextMesh Pro multiplies gradient colors with the text's main vertex color (**Main Settings > Vertex Color** in the TextMesh Pro Inspector). If the main vertex color is white you see only the gradient colors. If it’s black you don’t see the gradient colors at all.

## Applying a Gradient

To apply a gradient to a TextMesh Pro GameObject, edit the [Gradient properties](TMPObjectUIText.md#color) in the Inspector.

> [!NOTE]
> - To apply a gradient to only a portion of the text, use the [gradient](RichTextGradient.md) rich text tag.
> - To apply a gradient to multiple text objects, use a [gradient preset](ColorGradientsPresets.md).

![Example image](../images/TMP_ColorGradientInspector.png)

**To apply a color gradient to a TextMesh Pro GameObject:**

1. Enable the **Main Settings > Color Gradient** property.

1. Set **Main Settings > Color Gradient > Color Mode** to the [type of gradient](ColorGradientsTypes.md) you want to apply.

1. Use the **Main Settings > Color Gradient > Colors** settings to choose colors for the gradient. For each color you can:

  - Click the color swatch to open a [Color Picker](https://docs.unity3d.com/Manual/EditingValueProperties.html).
  - Use the eyedropper to pick a color from anywhere on your screen.
  - Enter the color’s hexadecimal value directly in the text field.