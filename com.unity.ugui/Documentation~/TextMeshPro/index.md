# TextMesh Pro Documentation

TextMesh Pro is a set of Unity tools for 2D and 3D text.

TextMesh Pro provides better control over text formatting and layout than to Unity's UI Text & Text Mesh systems. It includes features such as:

* Character, word, line, and paragraph spacing.
* Kerning.
* Justified text.
* Links.
* More than thirty rich text tags.
* Support for multiple fonts.
* Support for sprites.
* Custom styles.
* Advanced text rendering using custom [shaders](Shaders.md).

## Getting started

The TextMesh Pro package is included in the Unity Editor. You do not need to install it.

### Customizing the TMP Settings

By default, TextMesh Pro uses the TMP Settings shipped with the package. To customize them for your project, open **Edit > Project Settings > TextMesh Pro** and select **Create TMP Settings**. This adds a **TMP Settings** asset to the **Assets/TextMesh Pro/Resources** folder of your project, which then replaces the built-in settings. A project can only have one TMP Settings asset.

If your project does not use TextMesh Pro, disable **Include Default Settings In Builds** on the same page. Player builds then leave out the default TMP Settings, font asset and shaders.

### Importing examples and additional resources

TextMesh Pro also includes additional resources and examples to help you learn about various features.

To import them, open the **Package Manager** window, select the **uGUI** package, and import the **Examples & Extras** sample from the **Samples** tab.
