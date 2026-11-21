# Changelog

## [7.0.0] - 2026-09-09
uGUI 7.x is shipped alongside Unity 7.x as a [core package](https://docs.unity3d.com/Manual/pack-core.html).

The changelog for version 7.0.0 will remain under active development throughout the pre-release phases of Unity 7000.0.

### Added
- Added `RectMask2D.edgeSoftness` to set the softness of each edge of the mask separately. The **Softness** foldout in the RectMask2D Inspector sets it too.
- Added `IClippable.SetClipSoftness(Vector4)` and `MaskableGraphic.SetClipSoftness(Vector4)`, which receive the softness of each edge.

### Changed
- uGUI no longer depends on the IMGUI built-in package (`com.unity.modules.imgui`), so projects that use uGUI without UI Toolkit can disable the IMGUI module.
- TextMesh Pro no longer requires importing the TMP Essential Resources. The default font asset, materials, shaders, style sheet and TMP Settings now ship with the package and work out of the box. A project-specific TMP Settings asset can be created from **Project Settings > TextMesh Pro**; only one TMP Settings asset per project is supported. Projects that do not use TextMesh Pro can turn off **Include Default Settings In Builds** on that page to keep the default settings, font asset and shaders out of player builds.
- Moved the TMP Examples & Extras to the **TextMesh Pro Examples & Extras** package sample, available from the Package Manager window.
- Renamed the sample from **Examples & Extras**, its name in Unity 7000.0 pre-releases, to **TextMesh Pro Examples & Extras**. If you imported it with a pre-release, delete the `Assets/Samples/uGUI/<version>/Examples & Extras` folder before you import it again. The Package Manager does not detect a copy imported under the old name, so importing the renamed sample adds a second copy.
- The **Window > TextMeshPro > Project Files GUID Remapping Tool** now also redirects references to the TMP Essential Resources and TMP Examples & Extras imported by earlier Unity versions to the resources shipped with the package and its sample. Legacy resources still present in the project, such as a customized TMP Settings asset, keep being referenced.
- Replaced the **Enable RTL Editor** toggle in the TextMesh Pro Text Input section with a **Text Direction** property in **Extra Settings**, exposed to scripts as `TMP_Text.textDirection`. Text objects that had **Enable RTL Editor** enabled keep displaying right-to-left. The protected `TMP_Text.m_isRightToLeft` field was replaced by `m_TextDirection`.
- The TextMesh Pro shaders shipped with the package are named `TextMeshPro/Default/...`, for example `TextMeshPro/Default/Distance Field`, so they no longer share names with copies imported by earlier Unity versions. `Shader.Find` and shader `Fallback` with a former name, such as `TextMeshPro/Distance Field`, return a project shader with that name if there is one, and the package shader otherwise.

### Deprecated
- `RectMask2D.softness` is deprecated. Use `RectMask2D.edgeSoftness` instead.
- `IClippable.SetClipSoftness(Vector2)` and `MaskableGraphic.SetClipSoftness(Vector2)` are deprecated. Use `SetClipSoftness(Vector4)` instead.
- The `_UIMaskSoftnessX` and `_UIMaskSoftnessY` shader properties are deprecated. Custom UI shaders should read `_UIMaskSoftness`, which holds the softness of each edge: left, bottom, right, top.

### Removed
- Removed the TMP Essential Resources and TMP Examples & Extras importer windows and the **Window > TextMeshPro > Import ...** menu items, along with the `TMP_PackageResourceImporter`, `TMP_PackageResourceImporterWindow` and `TMP_PackageUtilities` classes.
- Removed the legacy serialized `fontWeights` field from `TMP_FontAsset`, along with its conversion when upgrading font assets created before TMP 1.1.0. This fixes the CS0414 warning raised when building with warnings as errors.

## [2.7.0] - 2026-07-27
uGUI 2.7 is shipped alongside Unity 6000.7 as a [core package](https://docs.unity3d.com/Manual/pack-core.html).

### Deprecated
- Marked the PositionAsUV1, TMP_TextElement_Legacy, FaceInfo_Legacy, TMP_Glyph, TMP_Sprite, KerningPair and KerningTable classes as deprecated (with a warning).
- Marked the GlyphValueRecord_Legacy struct as deprecated (with a warning).
- Marked the EventHandle enum as deprecated (with a warning).
- Marked TMP_SpriteAsset.spriteInfoList as deprecated (with a warning).
- Marked Scrollbar.ClickRepeat(PointerEventData eventData) deprecated (with a warning).
- Marked the TMP_FontUtilities class as obsolete (with an error).
- Marked the KerningPairKey struct as obsolete (with an error).
- Marked the EventSystem.SetUITookitEventSystemOverride method as obsolete (with an error).
- Marked the Graphic.m_CachedMesh, Graphic.m_CachedUvs and Graphic.useLegacyMeshGeneration members as obsolete (with an error).

### Removed
`BaseMeshEffect.ModifyMesh(Mesh mesh)`, `Graphic.OnPopulateMesh(Mesh m)`, `ShaderUtilities.isInitialized` and `ShaderUtilities.GetShaderPropertyIDs()` were marked obsolete in uGUI 2.6. Earlier versions didn't flag them, so if you upgrade from uGUI 2.5 or earlier, code that uses these APIs will fail to compile.

- BaseMeshEffect.ModifyMesh(Mesh mesh). Override `ModifyMesh(VertexHelper vh)` instead.
- BaseRaycaster.priority property.
- EventSystem.lastSelectedGameObject property.
- Graphic.OnPopulateMesh(Mesh m). Override `OnPopulateMesh(VertexHelper vh)` instead.
- Image.eventAlphaThreshold property.
- InputField.onValueChange property.
- InputField.ScreenToLocal(Vector2 screen).
- IGraphicEnabledDisabled interface.
- IMeshModifier.ModifyMesh(Mesh mesh).
- Mask.OnSiblingGraphicEnabledDisabled().
- PointerEventData.worldPosition and PointerEventData.worldNormal properties.
- RaycastResult.document property.
- Selectable.allSelectables property.
- ShaderUtilities.isInitialized and ShaderUtilities.GetShaderPropertyIDs(). `ShaderUtilities` initializes automatically, so you can remove these calls.
- StandaloneInputModule.InputMode enum.
- StandaloneInputModule.inputMode, StandaloneInputModule.allowActivationOnMobileDevice and StandaloneInputModule.forceModuleActive properties.
- StandaloneInputModule.ForceAutoSelect().
- TextMeshPro.textContainer property.
- TMP_FontAsset.fontInfo property.
- TMP_Text.enableWordWrapping and TMP_Text.enableKerning properties.
- TMP_Text.SetText(string sourceText, bool syncTextInputBox = true).
- TouchInputModule class.

## [2.6.0] - 2026-03-30
uGUI 2.6 is shipped alongside Unity 6000.6 as a [core package](https://docs.unity3d.com/Manual/pack-core.html).

### Added
- New Maximum Width and Height properties added to ILayoutElement.
- New Clamped FitMode added to ContentSizeFitter.

### Changed
- ContentSizeFitter's PreferredSize FitMode now respects Maximum Width and Height of ILayoutElement.
- GridLayoutGroup, VerticalLayoutGroup and HorizontalLayoutGroup calculate their maximum width and height based on their children's layout element properties.

### Added
- New SafeArea component that insets a RectTransform to respect the device's safe area. Supports per-edge control over which sides are inset, a configurable reference orientation so that edge assignments remain stable across device rotations, and a balance mode that symmetrically mirrors the inset on the opposite edge to keep the UI centered.

### Changed
The following APIs were marked obsolete in uGUI 2.6. Earlier versions didn't flag them, so if you upgrade from uGUI 2.5 or earlier, code that uses these APIs will fail to compile.

- Marked BaseMeshEffect.ModifyMesh(Mesh mesh) as obsolete. Override `ModifyMesh(VertexHelper vh)` instead.
- Marked Graphic.OnPopulateMesh(Mesh m) as obsolete. Override `OnPopulateMesh(VertexHelper vh)` instead.
- Marked ShaderUtilities.isInitialized and ShaderUtilities.GetShaderPropertyIDs() as obsolete. `ShaderUtilities` initializes automatically, so you can remove these calls.

## [2.5.0] - 2026-02-27
uGUI 2.5 is shipped alongside Unity 6000.5 as a [core package](https://docs.unity3d.com/Manual/pack-core.html).

### Added
- New RaycastReceiver component that serves as an intractable area which is not rendered.
- GridLayoutGroup has 2 new read-only properties that expose the row & column counts after the layout is calculated.

### Changed
- Made the IsPressed and IsHighlighted methods public in the Selectable class.
- The CanvasGroup inspector now offers a slider for adjusting the alpha value.

## [2.0.0] - 2023-03-08
Merge of the com.unity.textmeshpro package.

## [1.0.0] - 2019-01-08
This is the first release of Unity UI as a built in package.
