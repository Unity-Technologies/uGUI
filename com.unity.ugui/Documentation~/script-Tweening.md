# Tweening

The Tweening system (`UnityEngine.UI.Tweening`) is an internal animation utility used by uGUI to drive smooth value transitions — for example, the color fades and alpha animations that `Graphic` and `Selectable` trigger in response to user interaction. It is not intended for direct use in game code; use Unity's `Animator` or `DOTween`/`LeanTween` for general-purpose tweening.

## Overview

The system includes the following types:

| Type | Role |
|:---|:---|
| `ColorTween` | Interpolates a `Color` value and dispatches the result via a callback. |
| `FloatTween` | Interpolates a `float` value and dispatches the result via a callback. |
| `TweenRunner<T>` | Manages the lifetime of a single active tween for one owner object. |
| `TweenManager` | Singleton that drives all active `TweenRunner` instances once per render frame. |


## ColorTween

`ColorTween` interpolates between two `Color` values. The `ColorTweenMode` enum controls which channels are animated:

| Mode | Behaviour |
|:---|:---|
| `All` | All four channels (R, G, B, A) are lerped. |
| `RGB` | Only R, G, B are lerped; alpha is held at the start value. |
| `Alpha` | Only alpha is lerped; R, G, B are held at the start values. |

### Properties

| Property | Type | Description |
|:---|:---|:---|
| `startColor` | `Color` | The color at progress 0. |
| `targetColor` | `Color` | The color at progress 1. |
| `tweenMode` | `ColorTweenMode` | Which channels to animate (default: `All`). |
| `duration` | `float` | Duration in seconds. |
| `ignoreTimeScale` | `bool` | When `true`, uses `Time.unscaledDeltaTime`. |

### Methods

| Method | Description |
|:---|:---|
| `SetChangedCallback(Action<Color>)` | Registers the callback invoked on every tick. Replaces any previously registered callback. |
| `TweenValue(float)` | Computes the interpolated color for the given progress and invokes the callback. |
| `ValidTarget()` | Returns `true` if a callback has been registered. |

## FloatTween

`FloatTween` interpolates a single `float` between `startValue` and `targetValue`.

### Properties

| Property | Type | Description |
|:---|:---|:---|
| `startValue` | `float` | The value at progress 0. |
| `targetValue` | `float` | The value at progress 1. |
| `duration` | `float` | Duration in seconds. |
| `ignoreTimeScale` | `bool` | When `true`, uses `Time.unscaledDeltaTime`. |

### Methods

| Method | Description |
|:---|:---|
| `SetChangedCallback(Action<float>)` | Registers the callback invoked on every tick. Replaces any previously registered callback. |
| `TweenValue(float)` | Computes the interpolated float for the given progress and invokes the callback. |
| `ValidTarget()` | Returns `true` if a callback has been registered. |

## TweenRunner&lt;T&gt;

`TweenRunner<T>` manages one tween at a time for a given `MonoBehaviour` owner. Starting a new tween automatically stops any tween that is already running.

```csharp
var runner = new TweenRunner<ColorTween>();
runner.Init(this);          // call once, typically in OnEnable

var tween = new ColorTween
{
    startColor  = Color.white,
    targetColor = Color.red,
    duration    = 0.15f,
    ignoreTimeScale = true
};
tween.SetChangedCallback(c => image.color = c);
runner.StartTween(tween);
```

### Methods

| Method | Description |
|:---|:---|
| `Init(MonoBehaviour)` | Associates the runner with a `MonoBehaviour`. Must be called before `StartTween`. |
| `StartTween(T)` | Stops any running tween and starts the new one. If the owner's `GameObject` is inactive, `TweenValue(1f)` is called immediately and no tick registration occurs. |
| `StopTween()` | Cancels the active tween. Safe to call when no tween is running. |

> [!NOTE]
> If `Init` has not been called, `StartTween` logs a warning and returns without starting the tween.

## TweenManager

`TweenManager` is an internal singleton that ticks all registered `TweenRunner` instances once per frame, just before canvases are rendered (`Canvas.willRenderCanvases`). You do not interact with it directly; `TweenRunner` calls `Register` and `Unregister` on your behalf.

### Tick scheduling

Ticks are driven by `Canvas.willRenderCanvases`, which fires after all scripts have run for the frame but before the canvas batch is built. This ensures that color and alpha changes made during a frame are reflected in the same canvas rebuild.

### Safe mutation during callbacks

`TweenManager` defers registrations and unregistrations that occur while iteration is in progress (for example, a tween callback that starts or stops another tween). Deferred operations are flushed immediately after the update loop finishes, so all side-effects take place within the same frame.

### Completion

A tween completes when the elapsed time meets or exceeds its duration. On that tick, `TweenValue(1f)` is called exactly once and the runner is unregistered. The final value is always the exact `targetColor` or `targetValue`, regardless of frame-time variance.

## Additional resources

- [Animation Integration](UIAnimationIntegration.md) — how uGUI uses the Animator for state-based transitions.
- [Visual Components](UIVisualComponents.md) — `Graphic` and related classes that consume `ColorTween`.
- [Interaction Components](UIInteractionComponents.md) — `Selectable` and the highlight/press/disabled color transitions.
