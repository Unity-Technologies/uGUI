# Changelog

## [2.7.0] - 2026-07-27

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
- BaseMeshEffect.ModifyMesh(Mesh mesh).
- BaseRaycaster.priority property.
- EventSystem.lastSelectedGameObject property.
- Graphic.OnPopulateMesh(Mesh m).
- Image.eventAlphaThreshold property.
- InputField.onValueChange property.
- InputField.ScreenToLocal(Vector2 screen).
- IGraphicEnabledDisabled interface.
- IMeshModifier.ModifyMesh(Mesh mesh).
- Mask.OnSiblingGraphicEnabledDisabled().
- PointerEventData.worldPosition and PointerEventData.worldNormal properties.
- RaycastResult.document property.
- Selectable.allSelectables property.
- ShaderUtilities.isInitialized and ShaderUtilities.GetShaderPropertyIDs().
- StandaloneInputModule.InputMode enum.
- StandaloneInputModule.inputMode, StandaloneInputModule.allowActivationOnMobileDevice and StandaloneInputModule.forceModuleActive properties.
- StandaloneInputModule.ForceAutoSelect().
- TextMeshPro.textContainer property.
- TMP_FontAsset.fontInfo property.
- TMP_Text.enableWordWrapping and TMP_Text.enableKerning properties.
- TMP_Text.SetText(string sourceText, bool syncTextInputBox = true).
- TouchInputModule class.

## [2.6.0] - 2026-03-30

### Added
- New Maximum Width and Height properties added to ILayoutElement.
- New Clamped FitMode added to ContentSizeFitter.

### Changed
- ContentSizeFitter's PreferredSize FitMode now respects Maximum Width and Height of ILayoutElement.
- GridLayoutGroup, VerticalLayoutGroup and HorizontalLayoutGroup calculate their maximum width and height based on their children's layout element properties.

### Added
- New SafeArea component that insets a RectTransform to respect the device's safe area. Supports per-edge control over which sides are inset, a configurable reference orientation so that edge assignments remain stable across device rotations, and a balance mode that symmetrically mirrors the inset on the opposite edge to keep the UI centered.

## [2.5.0] - 2026-02-27

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
