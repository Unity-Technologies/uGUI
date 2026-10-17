# Distance Field (URP Lit) Shader

The Distance Field (URP Lit) shader is a Shader Graph version of the Distance Field shader for the Universal Render Pipeline (URP). Unlike the Distance Field shader, it interacts with scene lighting. It replaces the Distance Field (Surface) shader, which was available only in the Built-In Render Pipeline.

To use this shader, select **TextMeshPro** > **SRP** > **TMP_SDF-URP Lit** in the **Shader** dropdown of the text's material. Your project must use URP.

## Properties

Edit Distance Field (URP Lit) shader properties in the material Inspector. Properties are divided into the following sections:

* **[Face](#Face):** Controls the appearance of the text face.
* **[Outline 1, 2 and 3](#Outline):** Control the appearance of up to three text outlines.
* **[Underlay](#Underlay):** Adds a second rendering of the text beneath the original, for example, to apply a drop shadow.
* **[Lighting](#Lighting):** Adds a bevel and controls the appearance of locally simulated lighting on the text.
* **[Debug Settings](#DebugSettings):** Exposes internal shader properties for troubleshooting.

<a name="Face"></a>
### Face

| Property:    | Description |
|--------------|-------------|
| **Color**    |Adjust the face color of the text.<br/><br/>The value set here is multiplied by the vertex `Colors` set in the TextMeshPro component.<br/><br/>Set this to white to use the original vertex colors.<br/><br/>Set this to black to cancel out the vertex colors.|
| **Texture**  |Apply a texture to the text face.<br/><br/>The texture is multiplied with the face **Color** and the vertex colors in the TextMesh Pro component to produce the final face color.<br/><br/>The **Horizontal Mapping** and **Vertical Mapping** properties in the TextMesh Pro component determine how TextMesh Pro fits the texture to the text face.|
| **Tiling**   |Increase these values to repeat the texture across the text surface, in accordance with the TextMesh Pro object's **Horizontal Mapping** and **Vertical Mapping** properties.|
| **Offset**   |Adjust these values to change the texture's relative position, horizontally or vertically, on the text surface.|
| **Speed**    |Animate the face texture by setting a value greater than **0**.<br/><br/>The resulting animation is a scrolling effect as the texture's UV coordinates change over time.<br/><br/>**Note:** To see this effect in the Scene view, you must enable **Animated Materials** from the Effects menu in the [Scene view control bar](https://docs.unity3d.com/Manual/ViewModes.html).|
| **Softness** |Adjust the softness of the face edges.<br/><br/>A value of `0` produces crisp, anti-aliased edges.<br/><br/>Values greater than `0` produce increasingly soft and blurry edges.|
| **Dilate**   |Adjust the position of the face contour in the font [distance field](FontAssetsSDF.md).<br/><br/>A value of `0` places the contour halfway, which corresponds to the contour of the original font.<br/><br/>Negative values thin the characters, while positive values thicken them.|

<a name="Outline"></a>
### Outline 1, 2 and 3

The shader renders up to three outlines beneath the face. Outline 1 is drawn directly beneath the face, Outline 2 beneath Outline 1, and Outline 3 beneath Outline 2. Each outline is visible only where it extends beyond the layer above it, so set a larger **Dilate** value for each outline than for the layer above it.

| Property:    | Description |
|--------------|-------------|
| **Color**    |Adjust the color of the outline.|
| **Texture**  |**Outline 1 only.** Apply a texture to the outline.<br/><br/>The texture is multiplied with the outline **Color** to produce the final outline color.<br/><br/>The **Horizontal Mapping** and **Vertical Mapping** properties in the TextMesh Pro component determine how TextMesh Pro fits the texture to the outline.|
| **Tiling**   |**Outline 1 only.** Increase these values to repeat the texture across the outline.|
| **Offset**   |**Outline 1 only.** Adjust these values to change the texture's relative position on the outline.|
| **Speed**    |**Outline 1 only.** Animate the outline texture by setting a value greater than **0**.<br/><br/>The resulting animation is a scrolling effect as the texture's UV coordinates change over time.|
| **Offset**   |Shift the outline horizontally (**X**) and vertically (**Y**) relative to the face, for example to create a hard drop shadow.<br/><br/>For Outline 3, this property is only available when **Outline Mode** is disabled.|
| **Softness** |Adjust the softness of the outline edges.<br/><br/>A value of `0` produces crisp, anti-aliased edges.<br/><br/>Values greater than `0` produce increasingly soft and blurry edges.|
| **Dilate**   |Adjust the position of the outline contour in the font [distance field](FontAssetsSDF.md).<br/><br/>A value of **0** places the contour on the contour of the original font. Positive values push the outline outward.|
| **Outline Mode** |**Outline 3 only.** When enabled, Outline 3 follows the combined shape of the face, Outline 1 and Outline 2 at their offset positions, instead of using its own **Offset**.|

<a name="Underlay"></a>
### Underlay

Underlay adds an additional rendering of the text underneath the original rendering. You can use it to add a drop-shadow effect.

| Property:    | Description |
|--------------|-------------|
| **Color**    |Adjust the color of the underlay.<br/><br/>The default color is semi-transparent black. Set the alpha to `0` to hide the underlay.|
| **Offset X** |Adjust the horizontal position of the underlay relative to the text.|
| **Offset Y** |Adjust the vertical position of the underlay relative to the text.|
| **Dilate**   |Adjust the position of the underlay contour in the font [distance field](FontAssetsSDF.md).<br/><br/>Negative values thin the underlay, while positive values thicken it.|
| **Softness** |Adjust the softness of the underlay edges.<br/><br/>A value of `0` produces crisp, anti-aliased edges.<br/><br/>Values greater than `0` produce increasingly soft and blurry edges. When using the underlay as a drop shadow, use this setting to make the shadow harder or softer.|

<a name="Lighting"></a>
### Lighting

The text reacts to scene lighting. The settings in this group add a bevel, and simulate local directional lighting on top of the scene lighting.

#### Bevel

A bevel adds the illusion of depth to your text.

| Property:    | Description |
|--------------|-------------|
| **Type**     |Choose the type of bevel to apply.<br/><br/>**Outer Bevel:** Produces raised lettering with sloped sides.<br/><br/>**Inner Bevel:** Produces text that looks raised along its outline.|
| **Amount**   |Adjust the steepness of the bevel.|
| **Offset**   |Offset the bevel from its usual position so it no longer matches the outline.|
| **Width**    |Adjust the bevel size.|
| **Roundness** |Increase this value to smooth out more angular areas of the bevel.|
| **Clamp**    |Set this value to limit the maximum height of the bevel.|

#### Local Lighting

| Property:    | Description |
|--------------|-------------|
| **Light Angle** |Adjust the angle of the simulated local light, in radians.|
| **Specular Color** |Set the tint for specular highlights.|
| **Specular Power** |Adjust the brightness of specular highlights.|
| **Reflectivity Power** |Adjust how shiny the bevel surface appears.|
| **Diffuse Shadow** |Adjust the overall shadow level.|
| **Ambient Shadow** |Adjust the ambient light level.<br/><br/>Lower values darken the text where the bevel slopes away from the viewer.|

<a name="DebugSettings"></a>
### Debug Settings

This section contains internal shader properties that can help you troubleshoot text-related issues.

| Property:    | Description |
|--------------|-------------|
| **Font Atlas** |Points to the atlas texture used by the font asset.|
| **Gradient Scale** |Represents the spread/range of the font's [signed distance field](FontAssetsSDF.md).<br/><br/>This value is equal to `Padding +1`, with `Padding` being the **Padding** value set when the font asset was created.<br/><br/>**Note:** This value is displayed for debugging purposes. Don't edit it manually.|

The bottom of the Inspector also shows the standard material settings: **Render Queue**, **Enable GPU Instancing**, **Double Sided Global Illumination**, and **Emission**.
