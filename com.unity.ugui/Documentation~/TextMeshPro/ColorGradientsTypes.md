# Color Gradient Types

You can apply the following types of gradients to text.

- **[Single](#single-color):** A single color that is TextMesh Pro multiplies with the text object's vertex color.

- **[Horizontal](#horizontal-gradients):** A two-color side-to-side gradient.

- **[Vertical](#vertical-gradients):** A two-color up-and-down gradient.

- **[Four Corner](#four-corner-gradients):** A four-color gradient. Each color radiates from one corner.

![Example image](../images/TMP_ColorGradientInspector.png)<br/>
_The TexMesh Pro color gradient settings_ <br/><br/>

The number of colors available in the **Colors** settings depends on the type of gradient you choose. Each swatch corresponds to the color's origin on a character sprite.

The image above shows a the settings for a four color gradient. Each color originates in the corresponding corner of the sprite (top-left, top-right, bottom-left, bottom-right). IT produces the following gradient:

![Example image](../images/TMP_ColorGradient_4-Corner-YBRG_half.png)


## Single Color

The **Single** gradient type applies a single color.

![Example image](../images/TMP_ColorGradient_Single-Y_half.png)

## Horizontal Gradients

The **Horizontal** gradient type applies two colors, and produces a side to side transition between them on each character.

![Example image](../images/TMP_ColorGradient_Horiz-YB_half.png)

![Example image](../images/TMP_ColorGradient_Horiz-BY_half.png)<br/><br/>

## Vertical Gradients

The **Vertical** gradient type consists of two colors, and produces an up and down transition between the two on each character.

![Example image](../images/TMP_ColorGradient_Vert-BY_half.png)<br/><br/>

![Example image](../images/TMP_ColorGradient_Vert-YB_half.png)<br/><br/>

## Four Corner Gradients

The **Four Corner** gradient type applies four colors. Each one radiates out from its assigned corner of each character.

![Example image](../images/TMP_ColorGradient_4-Corner-YBRG_half.png)<br/><br/>

![Example image](../images/TMP_ColorGradient_4-Corner-YBRO_half.png)

This is the most versatile gradient type. By varying some colors and keeping others identical, you can create different kinds of gradients. For example:

- Give three corners one color and the fourth a different color.

![Example image](../images/TMP_ColorGradient_1-Corner-BYYY_half.png)

- Give pairs of adjacent corners the same color to create horizontal  or vertical gradients.

![Example image](../images/TMP_ColorGradient_2-Corner-BYBY_half.png)<br/><br/>

![Example image](../images/TMP_ColorGradient_2-Corner-BBYY_half.png)

- Give pairs of diagonally opposite corners the same color to create diagonal gradients.

![Example image](../images/TMP_ColorGradient_2-Corner-BYYB_half.png)<br/><br/>

![Example image](../images/TMP_ColorGradient_2-Corner-YBBY_half.png)<br/><br/>

- Create horizontal and vertical 3-color gradients with a dominant color at one end and a transition between two colors at the other.

![Example image](../images/TMP_ColorGradient_3-Corner-YRYB_half.png)<br/><br/>

![Example image](../images/TMP_ColorGradient_3-Corner-YYRB_half.png)

- Give two diagonally opposite corners same color and give the other two corners different colors to create a diagonal stripe 3-color gradient.

![Example image](../images/TMP_ColorGradient_3-Corner-RYYB_half.png)<br/><br/>

![Example image](../images/TMP_ColorGradient_3-Corner-YBRY_half.png)
