# Bitmap Custom Atlas Shader

The Bitmap shader is designed to use bitmap-only fonts. It treats the font atlas like a regular texture, displaying it directly, and does not support any text effects. Bitmap-textured text becomes blocky when you zoom in on it.

## Properties

![Example image](../images/TMP_Shader_BitmapCustomAtlas.png)

![Example image](../images/Letter_A_half.png) **[Face](#Face):** Controls the text's overall appearance.

![Example image](../images/Letter_B_half.png) **[Debug Settings](#DebugSettings):** Exposes internal shader properties that are sometimes useful for troubleshooting.

<a name="Face"></a>
### Face

Description


| Property:    ||Description |
|--------------|-------------|--|
| **Color**    ||Adjust the face color of the text.<br/><br/>The value you set here is multiplied with the vertex **Colors** you set in the TextMeshPro component.<br/><br/>Set this to white to use the original vertex colors.<br/><br/>Set this to black to cancel out the vertex colors.<br/><br/>Similarly, setting the Alpha to **1** uses the original vertex-color alpha, while setting it to **0** removes any alpha set in the original vertex colors.|
| **Texture**  ||Apply a texture to the text face.<br/><br/>The texture is multiplied with the face **Color** and the vertex colors in the TextMesh Pro component to produce the final face color.<br/><br/>The **Horizontal Mapping** and **Vertical Mapping** properties in the TextMesh Pro component determine how TextMesh Pro fits the texture to the text face.|
||**Tiling X/Y**   |Increase these values to repeat the texture across the text surface, in accordance with the TextMesh Pro object's **Horizontal Mapping** and **Vertical Mapping** properties.|
||**Offset X/Y**   |Adjust these values to change the texture's relative position, horizontally or vertically, on the text surface.             |

<a name="DebugSettings"></a>
### Debug Settings

The debug section exposes some of the shader’s internal properties. They can be helpful for troubleshooting problems you encounter with the shader.

| Property:                        |  Description |
|----------------------------------|--------------|
| **Font Atlas**                   | Points to the atlas texture used by the font Asset. |
| **Offset X/Offset Y**            | Offset the vertex positions of each character in X and Y.<br/><br/>You can change these values using a script to create simulated crawl or scrolling FX. |
| **Softness X/Softness Y**        | When **Mask** is set to **Soft**, set these to adjust the softness of the edge of the text. |
| **Clip Rect**                    | Clip Rect defines the Left (**L**), Bottom (**B**), Right (**R**) and Top (**T**) world space coordinates of the masking rectangle.<br/><br/> This is normally set automatically by the **2D RectMask**. However when using a normal **TextMeshPro** component, this allows you to set / control the masking region. |
| **Stencil ID**                   | The reference value. For more information, refer to [the `ref` parameter in ShaderLab command: Stencil](https://docs.unity3d.com/Manual/SL-Stencil.html). |     
| **Stencil Comp**                 | A comparison operation. For more information, refer to [the `comparisonOperation` parameter in ShaderLab command: Stencil](https://docs.unity3d.com/Manual/SL-Stencil.html). |         
