# 3D Text GameObjects

By default, a TextMesh Pro 3D Text GameObject has the following components:

* **Rect Transform:** Controls the GameObject's position and size. For more information, see the [Rect Transform](https://docs.unity3d.com/Manual/class-RectTransform.html) documentation in the Unity Manual.

> [!NOTE]
> **Note:** If you want to use the Rect Transform component's anchoring system, the TextMesh Pro component's parent GameObject must also have a Rect Transform component.

* **Mesh Renderer:** Renders the GameObject. For more information, see the [Mesh Renderer](https://docs.unity3d.com/Manual/class-MeshRenderer.html) documentation in the Unity Manual.
* **TextMesh Pro UGUI (Script):** Contains the text to display, and the properties that control its appearance and behavior. These properties are described [below](#properties).
* **Material:** A Unity material that uses one of the TextMesh Pro shaders to further control the text's appearance. For more information see the [Shaders](Shaders.md) section.

## Properties Overview

![Example image](../images/TMP_Object_3DInspector.png)

[!include[](include-tmpobject-legend.md)]

[!include[](include-tmpobject-text.md)]

[!include[](include-tmpobject-main-settings.md)]

[!include[](include-tmpobject-font.md)]

[!include[](include-tmpobject-color.md)]

[!include[](include-tmpobject-spacing.md)]

[!include[](include-tmpobject-alignment.md)]

[!include[](include-tmpobject-wrapping.md)]

[!include[](include-tmpobject-uv-mapping.md)]

[!include[](include-tmpobject-extra-settings-3d.md)]













