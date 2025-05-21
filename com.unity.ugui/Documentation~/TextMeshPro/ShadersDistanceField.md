# Distance Field / Distance Field Overlay Shaders

The Distance Field and Distance Field Overlay shaders are two nearly-identical variants of the TextMesh Pro signed distance field (SDF)shader. The difference between the two is that the Distance Field Overlay variant always renders the TextMesh Pro object on top of everything else in the Scene, while the Distance Field variant renders the Scene normally—objects in front of the TextMesh Pro object are rendered on top of the text.

Both of these variants are unlit, meaning they do not interact with Scene lighting. Instead, they can simulate local directional lighting effects.

## Properties

The Distance Field and Distance Field Overlay shaders have identical properties, which you can edit in the TextMesh Pro object Inspector.

Properties are divided into several sections, some of which you must enable in order to activate the property group.

![Example image](../images/TMP_Shader_DF_Inspector.png)

![Example image](../images/Letter_A_half.png) **[Face](#Face):** Controls the text's overall appearance.

![Example image](../images/Letter_B_half.png) **[Outline](#Outline):** Adds a colored and/or textured outline to the text.

![Example image](../images/Letter_C_half.png) **[Underlay](#Underlay):** Adds a second rendering of the text underneath the original rendering, for example to add a drop shadow.

![Example image](../images/Letter_D_half.png) **[Lighting](#Lighting):** Simulates local directional lighting on the text.

![Example image](../images/Letter_E_half.png) **[Glow](#Glow):** Adds a smooth outline to the text in order to simulate glow.

![Example image](../images/Letter_F_half.png) **[Debug Settings](#DebugSettings):** Exposes internal shader properties that are sometimes useful for troubleshooting.

<a name="Face"></a>
### Face

The Face properties control the overall appearance of the text.

![Example image](../images/TMP_Shader_DF_Face.png)

| Property:    || Description |
|--------------|---|-------------|
| **Color**    ||Adjust the face color of the text.<br/><br/>The value you set here is multiplied with the vertex **Colors** you set in the TextMeshPro component.<br/><br/>Set this to white to use the original vertex colors.<br/><br/>Set this to black to cancel out the vertex colors.<br/><br/>Similarly, setting the Alpha to **1** uses the original vertex-color alpha, while setting it to **0** removes any alpha set in the original vertex colors.|
| **Texture**  ||Apply a texture to the text face.<br/><br/>The texture is multiplied with the face **Color** and the vertex colors in the TextMesh Pro component to produce the final face color.<br/><br/>The **Horizontal Mapping** and **Vertical Mapping** properties in the TextMesh Pro component determine how TextMesh Pro fits the texture to the text face.|
| **Tiling X/Y**   ||Increase these values to repeat the texture across the text surface, in accordance with the TextMesh Pro object's **Horizontal Mapping** and **Vertical Mapping** properties.|
| **Offset X/Y**   ||Adjust these values to change the texture's relative position, horizontally or vertically, on the text surface.             |
| **Speed X/Y**    ||Animate the face texture by setting a value greater than **0**.<br/><br/>The resulting animation is a scrolling effect as the texture’s UV coordinates change over time.<br/><br/>**Note:** To see this effect in the Scene view, you must enable **Animated Materials** from the Effects menu in the [Scene view control bar](https://docs.unity3d.com/Manual/ViewModes.html).|
| **Softness** ||Adjust the softness of the text edges.<br/><br/>A value of **0** produces  crisp, anti-aliased edges.<br/><br/>Values greater than **0** produce increasingly soft/blurry edges.<br/><br/>This setting applies to both the text face and the outline.|
| **Dilate**   ||Adjust the position of the text contour in the font [distance field](FontAssetsSDF.md).<br/><br/>A value of **0** places the contour halfway, which corresponds to the contour of the original font.<br/><br/>Negative values thin the characters, while positive values thicken them.|

<a name="Outline"></a>
### Outline

The outline properties allow you to add an outline to the text and control its appearance. The outline is not visible unless you set a **Thickness** value greater than **0**.

![Example image](../images/TMP_Shader_DF_Outline.png)

| Property:    |Description |
|--------------|------------|
| **Color** |Adjust the color for the text outline.|
| **Texture** |Apply a texture to the text outline.<br/><br/>The texture is multiplied with the outline **Color** to produce the final outline color.<br/><br/>The **Horizontal Mapping** and **Vertical Mapping** properties in the TextMesh Pro component determine how TextMesh Pro fits the texture to the text outline.|
| **Tiling** |            |
| **Offset** |            |
| **Speed** |Animate the outline texture by setting a value greater than 0.<br/><br/>The resulting animation is a scrolling effect as the texture’s UV coordinates change over time.<br/><br/>**Note:** To see this effect in the Scene view, you must enable **Animated Materials** from the Effects menu in the [Scene view control bar](https://docs.unity3d.com/Manual/ViewModes.html).|
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

<a name="Lighting"></a>
### Lighting

The Distance Field shader does not react to Scene lighting. Instead, it uses the settings in this group to simulate local directional lighting, and light effects.

If you want your text to react to Scene lighting, use the [Distance Field Surface](ShaderDistanceFieldSurface.md) shader.

![Example image](../images/TMP_Shader_DF_Lighting.png)

The Lighting properties are grouped into the following sections

![Example image](../images/Letter_A_half.png) **[Bevel](#Bevel):**

![Example image](../images/Letter_B_half.png) **[Local Lighting](#LocalLighting):**

![Example image](../images/Letter_C_half.png) **[Bump Map](#BumpMap):**

![Example image](../images/Letter_D_half.png) **[Environment Map](#EnvironmentMap):**


<a name="Bevel"></a>
#### Bevel

A bevel adds the illusion of depth to your text. It works like a normal map, except that the shader calculates the bevel using the font’s signed distance field.

Bevels are prone to showing artifacts, especially when they are too pronounced. These artifacts are more obvious on some materials than on others. Sometimes, artifacts that are more obvious on a simple material are hardly noticeable on a more complex material.

Although bevels work best with text that has an outline, you can apply them to text with no outline. In that case, you must set a positive **Width**, and should set a negative **Offset** to ensure that the whole bevel is visible.

![Example image](../images/TMP_Shader_DF_LightingBevel.png)

| Property:    |             | Description |
|--------------|-------------|-------------|
| **Type** |             | Choose the type of bevel to apply            |
|              | Outer Bevel | Produces raised lettering with sloped sides.<br/><br/>The bevel starts at the outside of the outline and increases in height until it reaches the inside of the outline. |
|              | Inner Bevel | Produces text with a raised outline.<br/><br/>The bevel starts at the outside of the outline, increases in height until it reaches the middle of the outline, and decreases in height until it reaches the inside of the outline. |
| **Amount** |             | Adjust the steepness of the bevel.<br/><br/>This setting defines the apparent difference in height between low and high regions of the bevel. |
| **Offset** |             | Offset the bevel from its usual position so it no longer matches the outline.<br/><br/>Different offsets produce very different bevels.<br/><br/>This is especially useful when you apply a bevel to text with no outline. |
| **Width** |             | Adjust the bevel size.<br/><br/>Set a value of **0** to make the bevel fill the full thickness of the outline.<br/><br/>Set a positive value to make the bevel extend beyond both sides of the outline.<br/><br/>Set a negative value to shrink the bevel toward the middle of the outline.|
| **Roundness** |             | Increase this value to smooth out more angular regions of the bevel. The effect is often quite subtle. |
| **Clamp** |             | Set this value to limit the maximum height of the bevel.<br/><br/>Higher values mean the bevel reaches its maximum height sooner.<br/><br/>Clamped outer bevels end before reaching the inside edge of the outline.<br/><br/>Clamped inner bevels have a larger flat region in the middle of the outline. |

<a name="LocalLighting"></a>
#### Local Lighting

These settings control simulated local directional lighting. They work in combination with the Bevel, Bump Map, and Environment Map settings.


![Example image](../images/TMP_Shader_DF_LightingLocal.png)

| Property:    |Description |
|--------------|------------|
| **Light Angle** | Adjust the angle, in radians, of the simulated local light illuminating the text.<br/><br/>The default angle is approximately π (pi) radians, which positions the light above the text.|
| **Specular Color** | Set the tint for specular highlights.<br/><br/>These are the highlights you see when the text directly reflects the simulated local light source. |
| **Specular Power** | Adjust the appearance of specular highlights. Larger values produce larger and brighter highlights. |
| **Reflectivity Power** | Adjust the how much the **[Environment Map](#EnvironmentMap)** contributes to the final color of the text.<br/><br/>The higher the value, the more the text appears to reflect the environment map texture and color. |
| **Diffuse Shadow** | Adjust the overall shadow level.<br/><br/>Higher values produce stronger shadowing, and consequently fewer apparent light effects on the text. |
| **Ambient Shadow** | Adjust the ambient light level.<br/><br/>Settings lower than **1** darken the text color based on the slope of the text. This is a subtle effect that is only noticeable with strong bevels or normal maps. |

<a name="BumpMap"></a>
#### Bump Map

You can use a normal map as a bump map to add bumpiness to the text. The bump map affects both the text face and outline, but you can control how strongly it affects each one individually. If your text has both a bevel and a bump map, the two mix together.

![Example image](../images/TMP_Shader_DF_LightingBump.png)

| Property:    |Description |
|--------------|------------|
| **Texture** | Select a normal map texture to use as a bump map. |
| **Face** | Control how much the bump map affects the text face.<br/><br/>A value of **0** shows no effect while a value of **1** shows the full effect of the bump map. |
| **Outline** | Control how much the bump map affects the text outline.<br/><br/>A value of **0** shows nothing while a value of **1** shows the full effect of the bump map. |

<a name="EnvironmentMap"></a>
#### Environment Map

You can use an environment map to add a reflection effect to your text face or outline, or for special image effects. The environment texture must be a cubemap. You can provide a static cubemap or create one at run time via a script.

![Example image](../images/TMP_Shader_DF_LightingEnv.png)

| Property:    |Description |
|--------------|------------|
| **Face Color** | Choose a color to use to tint reflections on the text face.<br/><br/>This color is multiplied with the environment map before the reflectivity effect is applied to the text face.<br/><br/>When this color is set to black, the environment map has no effect on the text face.<br/><br/>When this color is set to white, the environment map is at full strength on the text face. |
| **Outline Color** | Choose a color to use to tint reflections on the text outline.<br/><br/>This color is multiplied with the environment map before the reflectivity effect is applied to the text outline.<br/><br/>When this color is set to black, the environment map has no effect on the text outline.<br/><br/>When this color is set to white, the environment map is at full strength on the text outline. |
| **Texture** | Choose a cubemap texture to use as an environment map. |
| **Rotation** | Rotate the environment map to control which parts of the texture are visible in the reflection. You can animate the rotation to create a sense of movement. |

<a name="Glow"></a>
### Glow

The **Glow** effect adds a smooth outline on top of other text effects, which is typically used to suggest a glow. The effect is additive, so it is more noticeable on dark backgrounds.

When the glow extends beyond the text boundary, the surface shader shades it as if it were part of the solid text, meaning that it gets simulated lighting effects such as specular highlights.

![Example image](../images/TMP_Shader_DF_Glow.png)

| Property:    |Description |
|--------------|------------|
| **Color** |Set the tint and strength of the glow effect by adjusting the **Color** and **Alpha** values respectively. |
| **Offset** | Adjust the center of the glow effect.<br/><br/>A value of **0** places the center of the glow effect right on the text contour.<br/><br/>Positive values move the center out from the contour. Negative values move it in toward the center of the text. |
| **Inner** | Control how far the glow effect extends inward from the its start point (text contour + **Offset**). |
| **Outer** | Control how far the glow effect extends outward  from the text contour (text contour + Offset). |
| **Power** | Control how the glow effect falls off from its center to its edges.<br/><br/>A value of **1** produces a strong, bright glow effect with a sharp linear falloff.<br/><br/>Lower values produce a glow effect with a quick drop in intensity followed by a more gradual falloff. |

<a name="DebugSettings"></a>
### Debug Settings

The debug section exposes some of the shader’s internal properties. They can be helpful for troubleshooting problems you encounter with the shader.


![Example image](../images/TMP_Shader_DF_Debug.png)

| Property:                        |           | Description |
|----------------------------------|-----------|-------------|
| **Font Atlas**                   |           | Points to the atlas texture used by the font Asset. |
| **Gradient Scale**               |           |Represents the spread / range of the font’s [signed distance field](FontAssetsSDF.md).<br/><br/>This determines the effective range of material properties such as  **Outline > Width** and **Underlay > Offset X/Y**.<br/><br/>This value is equal to Padding +1, with Padding being the **Padding** value set when the font Asset was created.<br/><br/>**Note:** This value is displayed for debugging purposes. You should not edit it manually. |
| **Texture Width/Texture Height** |           | Displays the texture atlas width and height specified in the **Atlas Resolution** settings when the font Asset was created. |
| **Scale X/Scale X**              |           | Set multipliers for the SDF scale.<br/><br/>When set to **0**, characters are rendered as blocks.<br/><br/>Negative values soften the characters, while positive values make them appear sharper. |
| **Perspective Filter**           |           | When using a perspective camera, adjust this setting to make text look softer when viewed at sharp angles.<br/><br/>The default setting of **0.875** is adequate in most cases.<br/><br/>When using orthographic cameras, set this to **0**. |
| **Offset X/Offset Y**            |           | Offset the vertex positions of each character in X and Y.<br/><br/>You can change these values using a script to create simulated crawl or scrolling FX. |
| **Mask**                         |           |             |
|                                  | Mask Off  |             |
|                                  | Mask Hard |             |
|                                  | Mask Soft |             |
| **Mask Bounds**                  |           |             |
| |**Softness X/Softness Y**                | When **Mask** is set to **Soft**, set these to adjust the softness of the edge of the text. |
| **Match Bounds Renderer**        |           |             |
| |**Clip Rect**                            | Clip Rect defines the Left (**L**), Bottom (**B**), Right (**R**) and Top (**T**) world space coordinates of the masking rectangle.<br/><br/> This is normally set automatically by the **2D RectMask**. However when using a normal **TextMeshPro** component, this allows you to set / control the masking region. |
