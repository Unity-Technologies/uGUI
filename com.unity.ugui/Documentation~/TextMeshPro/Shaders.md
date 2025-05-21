# Shaders

TextMesh Pro has been designed to take advantage of signed distance field (SDF) rendering and includes a collection of shaders for this purpose. There are also bitmap-only shaders, in case you don't want to use SDF rendering.

All shaders have a desktop and a mobile version. The mobile versions are less demanding and suitable for mobile devices, but support fewer effects. All shaders can be found in the shader menu under TextMeshPro and TextMeshPro / Mobile.

## SDF Shaders

There are three variants of the SDF shader, known as Distance Field, Distance Field (Surface), and Distance Field Overlay. The regular and overlay shaders are unlit, so they don't interact with the Scene lighting. They can support locally simulated lighting effects instead.

The surface shader versions do interact with the Scene lighting. They use Unity's surface shader framework and are quite flexible, but also more demanding on the GPU. They are not physically based shaders.

SDF shaders can use the distance data to generate special effects, like outlines, underlays, and bevels. These effects often increase the visual size of the text. When taken to their extremes, you might see artifacts appear around the edges of character sprites. If this happens, scale down the effects. For example, a soft dilated underlay with large offsets might take things too far.

The artifacts occur because data from adjacent characters in the font atlas will bleed into the current character. You can increase the padding when importing a font to give the effects more space.
