# Changelog

## [3.0.0-exp.4] - 2024-04-17
### TextMeshPro
- Fix Incorrect cursor placement when selecting text and typing if text is center-aligned.
- Setting the game object layer for the Dropdown blocker game object to match the Canvas value.
- Fixed the virtual keyboard of InputField not closing upon Enter/Newline on Android.
- Fixed Ideographic Variation Selector.
- Ensure Dynamic FontAsset have a readable Atlas
- Fixed compile error on TMP_PostBuildProcessorHandler.cs when building for iOS with "install into source code"
- Fixed missing help documentation for SpriteAsset component in TextCore.
- Fix Dropdown creation causing a crash after undoing.
- Addressed issue surrounding dropdown not closing correctly in certain situations
- Resolves issue in editor where a null mesh may be set with still present submesh data, not having the canvas update.
- Ensure enabling and disabling Canvases does not cause a regeneration of the text.
- Fixed un-detected sprite asset changes after adding new sprites.
- Ensure Kerning is not applied to Sprites
- Fixed TMP_InputField line limit behavior to mean unlimited when the value is set to zero or negative (UUM-57192)
- Fixed custom validator ignores the returned character from the validate function (UUM-42147)
- Fixed editing a textfield on mobile and then submitting throws an exception (UUM-37282)
- Addressed issue surrounding dropdown not closing correctly in certain situations(UUM-33691)
- Ensure Sprites can be reordered within a SpriteAsset. (UUM-49349)
- Added missing grey and lightblue tags (UUM-54820)
- Fix underline when use at end of text. (UUM-55135)
- Add support for Visions OS keyboard.
 Fix TextMeshPro component does not perform linear color conversion when the VertexColorAlwaysGammaSpace option is enabled. Case #UUM-36113
 Addressed issue surrounding dropdown not closing correctly in certain situations. Case #UUM-33691
- Fixed Multi Line Newline input field from not accepting any new line past the set line limit. Case #UUM-42585
## [3.0.0-exp.3] - 2023-07-07
### TextMeshPro
- Fixed TextMeshPro crash when upgrading materials. Case #TMPB-187
- Ensured PreferredHeight handles various line heights correctly in TextMeshPro. Case #TMPB-165
- Set FaceInfo setter to public in TextMeshPro. Case #TMPB-182
- Ensured FontCreationSettings are not exported in a build in TextMeshPro. Case #TMPB-202
- Ensured sprites used correct indexes in TextMeshPro. Case #TMPB-200
- Made Maskable now propagates to SubMesh in TextMeshPro. Case #TMPB-191
- Added missing _ScaleRatioA to HDRP and URP shaders in TextMeshPro. Case #TMPB-169
- Fixed TextCore crash when upgrading materials. Case #UUM-32513

## [3.0.0-exp.1] - 2023-03-31
Added asset migration to TextCore, enabling the use of the same assets between UI Toolkit and UGUI.

## [2.0.0] - 2023-03-08
Merge of the com.unity.textmeshpro package.

## [1.0.0] - 2019-01-08
This is the first release of Unity UI as a built in package.
