# Raycasters

A Raycaster is a component that determines what objects are under a specific screen space position, such as the location of a mouse click or a touch. It works by projecting a ray from the screen into the scene and identifying objects that intersect with that ray. Raycasters are essential for detecting user interactions with UI elements, 2D objects, or 3D objects.

Different types of Raycasters are used for different types of objects:

- [Graphic Raycaster](script-GraphicRaycaster.md): Detects UI elements on a Canvas.
- [Physics 2D Raycaster](script-Physics2DRaycaster.md): Detects 2D physics elements.
- [Physics Raycaster](script-PhysicsRaycaster.md): Detects 3D physics elements.

The Event System uses Raycasters to determine where to send input events. When a Raycaster is present and enabled in the scene, the Event System uses it to determine which object is closest to the screen at a given screen space position.  If multiple Raycasters are active, the system will cast against all of them and sort the results by distance.