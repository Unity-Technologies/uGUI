# Distance Field Masking Mobile Shader

The Distance Field and Distance Field Overlay shaders are two nearly-identical variants of the TextMesh Pro signed distance field (SDF)shader. The difference between the two is that the Distance Field Overlay variant always renders the TextMesh Pro object on top of everything else in the Scene, while the Distance Field variant renders the Scene normally—objects in front of the TextMesh Pro object are rendered on top of the text.

![Example image](../images/IMAGE.png)

Both of these variants are unlit, meaning they do not interact with Scene lighting. Instead, they can simulate local directional lighting effects.

## Properties

The Distance Field and Distance Field Overlay shaders have identical properties, which you can edit in the TextMesh Pro object Inspector.

Properties are divided into several sections, some of which you must enable in order to activate the property group.

![Example image](../images/TMP_Shader_DFMM_Inspector.png)

![Example image](../images/Letter_A_half.png) **[Face](#Face):** Controls the text's overall appearance.

![Example image](../images/Letter_B_half.png) **[Outline](#Outline):** Adds a colored and/or textured outline to the text.

![Example image](../images/Letter_C_half.png) **[Underlay](#Underlay):** Adds a second rendering of the text underneath the original rendering, for example to add a drop shadow.

![Example image](../images/Letter_D_half.png) **[Debug Settings](#DebugSettings):** Exposes internal shader properties that are sometimes useful for troubleshooting.

<a name="Face"></a>
### Face

The Face properties control the overall appearance of the text.

![Example image](../images/TMP_Shader_DFMM_Face.png)

| Property:    | Description |
|--------------|-------------|
| **Color**    |Adjust the face color of the text.<br/><br/>The value you set here is multiplied with the vertex **Colors** you set in the TextMeshPro component.<br/><br/>Set this to white to use the original vertex colors.<br/><br/>Set this to black to cancel out the vertex colors.<br/><br/>Similarly, setting the Alpha to **1** uses the original vertex-color alpha, while setting it to **0** removes any alpha set in the original vertex colors.|
| **Softness** |Adjust the softness of the text edges.<br/><br/>A value of **0** produces  crisp, anti-aliased edges.<br/><br/>Values greater than **0** produce increasingly soft/blurry edges.<br/><br/>This setting applies to both the text face and the outline.|
| **Dilate**   |Adjust the position of the text contour in the font [distance field](FontAssetsSDF.md).<br/><br/>A value of **0** places the contour halfway, which corresponds to the contour of the original font.<br/><br/>Negative values thin the characters, while positive values thicken them.|

<a name="Outline"></a>
### Outline

The outline properties allow you to add an outline to the text and control its appearance. The outline is not visible unless you set a **Thickness** value greater than **0**.

![Example image](../images/TMP_Shader_DFMM_Outline.png)

| Property:    |Description |
|--------------|------------|
| **Color** |Adjust the color for the text outline.|
| **Thickness** |Adjust the thickness of the outline. The higher the value, the thicker the line.<br/><br/>The outline is drawn on the text contour, with half its thickness inside the contour and half of it outside the contour.<br/><br/>You can pull it farther in or push it farther out  by adjusting the **Face > Dilate** property.|

<a name="Underlay"></a>
### Underlay

Underlay adds an additional rendering of the text underneath the original rendering. You can use it to add a drop-shadow effect.

![Example image](../images/TMP_Shader_DF_Underlay.png)

| Property:    |   | Description |
|--------------|---|-------------|
| **Underlay Type** |   |Choose the type of underlay to render.|
| | None  |No underlay.             |
| | Normal  |Renders the underlay underneath the original text.<br/><br/>This creates a standard drop-shadow style effect.|
| | Inner  |Inverts the underlay and masks it with the original text so it is only visible inside the outline of the original letters.<br/><br/>This creates the type of drop shadow you would see through a cutout of the text.<br/><br/>To see an **Inner** underlay, you must make the text face transparent by setting its Alpha to **0**.|
| **Color** |   |Set the color of the underlay text. The default is a semi-transparent black.|
| **Offset X/Offset Y** |   |Offset the underlay text horizontally and vertically  from the original text.<br/><br/>For example, if you’re using the underlay to create a drop shadow, you can position it to suggest a specific lighting direction.|
| **Dilate** |   |Adjust the position of the underlay text contour in the font's [distance field](FontAssetsSDF.md).<br/><br/>A value of **0** places the contour halfway, which corresponds to the contour of the original font.<br/><br/>Negative values thin the characters, while positive values thicken them.|
| **Softness** |   |Adjust the softness of the underlay text edges.<br/><br/>A value of **0** produces  crisp, anti-aliased edges.<br/><br/>Values greater than **0** produce increasingly soft/blurry edges.<br/><br/>When using the underlay to create a drop-shadow, you can use this setting to make the shadows harder or softer.|

<a name="DebugSettings"></a>
### Debug Settings

The debug section contains options for defining and controlling masking. It also exposes some of the shader’s internal properties, which can be helpful for troubleshooting.



![Example image](../images/TMP_Shader_DFMM_Debug.png)

| Property:                        |           | Description |
|----------------------------------|-----------|-------------|
| **Font Atlas**                   |           | Points to the atlas texture used by the font Asset. |
| **Gradient Scale**               |           |Represents the spread / range of the font’s [signed distance field](FontAssetsSDF.md).<br/><br/>This determines the effective range of material properties such as  **Outline > Width** and **Underlay > Offset X/Y**.<br/><br/>This value is equal to Padding +1, with Padding being the **Padding** value set when the font Asset was created.<br/><br/>**Note:** This value is displayed for debugging purposes. You should not edit it manually. |
| **Texture Width/Texture Height** |           | Displays the texture atlas width and height specified in the **Atlas Resolution** settings when the font Asset was created. |
| **Scale X/Scale X**              |           | Set multipliers for the SDF scale.<br/><br/>When set to **0**, characters are rendered as blocks.<br/><br/>Negative values soften the characters, while positive values make them appear sharper. |
| **Perspective Filter**           |           | When using a perspective camera, adjust this setting to make text look softer when viewed at sharp angles.<br/><br/>The default setting of **0.875** is adequate in most cases.<br/><br/>When using orthographic cameras, set this to **0**. |
| **Offset X/Offset Y**            |           | Offset the vertex positions of each character in X and Y.<br/><br/>You can change these values using a script to create simulated crawl or scrolling FX. |
| **Mask Texture**  |    | Choose a texture file to use as a mask. Black and white images work best. <br/><br/>By default, black regions of the image  mask the text, while white areas reveal it.  |
| **Inverse Mask**   |   | Invert the mask so that white regions of the image  mask the text, while black areas reveal it.  |
| **Edge Color**   |   | Tint the edge of the mask with a specific color. <br/><br/>The softer the edge, the larger the tinted region.  |
| **Edge Softness**   |   | Make the edges of the mask softer or harder. <br/><br/>A value of **0.5** applies the mask as-is. Higher values soften the edges. Lower values make them sharper.  |
| **Wipe Position**   |   | Control the extent to which the text is masked. <br/><br/> A value of **0.5** masks the text exactly as defined by the **Mask Texture**. <br/><br/>A value of **0** fully exposes the text (no masking at all). <br/><br/> A value of **1** hides the text (all of the text is masked).    |
| **Softness X/Softness Y**        |           | Apply soft masking to the text in either axis. <br/><br/>Increase the **X** value to add soft masking to the left and right sides of the text. Increase the **Y** value to add soft masking at the top and bottom. <br/><br/>This masking is added to any masking defined by the **Mask Texture**.|
| **Clip Rect**                    |           | Clip Rect defines the Left (**L**), Bottom (**B**), Right (**R**) and Top (**T**) world space coordinates of the masking rectangle.<br/><br/> This is normally set automatically by the **2D RectMask**. However when using a normal **TextMeshPro** component, this allows you to set / control the masking region. |
