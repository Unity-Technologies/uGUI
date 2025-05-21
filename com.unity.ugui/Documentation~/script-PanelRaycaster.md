# Panel Raycaster

A derived [Raycaster](Raycasters.md) to raycast against UI Toolkit panel instances at runtime.

During the Start method of the EventSystem, a PanelRaycaster is automatically added to the scene for each
active UI Document in the scene. To disable this behavior, call
EventSystem.SetUITookitEventSystemOverride before the Start method
executes.

You can use the Sort Order of the Panel Settings asset referenced by each document to configure
the priority of the raycast between multiple documents. You can also use the PanelRaycaster in combination
with a [GraphicRaycaster](script-GraphicRaycaster.md). In that case, the Sort Order of documents is compared to
the Sort Order of Canvases to determine overall priority.

For additional information about using input and event systems with UI Toolkit,
refer to [FAQ for event and input system](https://docs.unity3d.com/Manual/UIE-faq-event-and-input-system.html).

## Properties

|**Property:** |**Function:** |
|:---|:---|
|`panel`| The panel that this component relates to. If the panel is null, this component has no effect. It will automatically be set to `null` automatically if the panel is disposed of from an external source. |