# Base Input

The Base Input component is the interface between an [Input Module](InputModules.md) and the input system. Input modules such as the [Standalone Input Module](script-StandaloneInputModule.md) read mouse, touch, keyboard, controller, and IME state through this component instead of calling the `Input` class directly.

When an input module needs input and no Base Input component is on its GameObject, the module adds one automatically.

## Customizing input

To provide your own input, for example to inject simulated input for testing or to read from a different input system, derive a class from `BaseInput` and override the members you need. Then assign an instance of your class to the input module's `inputOverride` property. The input module uses the override instead of the default Base Input component.

## Properties

The Base Input component has no properties to configure in the Inspector.

For details of the members you can override, refer to the [BaseInput](../api/UnityEngine.EventSystems.BaseInput.html) scripting API.
