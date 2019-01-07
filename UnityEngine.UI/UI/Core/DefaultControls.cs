using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    /// <summary>
    /// Utility class for creating default implementations of builtin UI controls.
    /// </summary>
    /// <remarks>
    /// The recommended workflow for using UI controls with the UI system is to create a prefab for each type of control and instantiate those when needed. This way changes can be made to the prefabs which immediately have effect on all used instances.
    ///
    /// However, in certain cases there can be reasons to create UI controls entirely from code. The DefaultControls class provide methods to create each of the builtin UI controls. The resulting objects are the same as are obtained from using the corresponding UI menu entries in the GameObject menu in the Editor.
    ///
    /// An example use of this is creating menu items for custom new UI controls that mimics the ones that are builtin in Unity. Some such UI controls may contain other UI controls. For example, a scroll view contains scrollbars.By using the DefaultControls methods to create those parts, it is ensured that they are identical in look and setup to the ones provided in the menu items builtin with Unity.
    ///
    /// Note that the details of the setup of the UI controls created by the methods in this class may change with later revisions of the UI system.As such, they are not guaranteed to be 100% backwards compatible. It is recommended not to rely on the specific hierarchies of the GameObjects created by these methods, and limit your code to only interface with the root GameObject created by each method.
    /// </remarks>
    public static class DefaultControls
    {
        /// <summary>
        /// Object used to pass resources to use for the default controls.
        /// </summary>
        public struct Resources
        {
            /// <summary>
            /// The primary sprite to be used for graphical UI elements, used by the button, toggle, and dropdown controls, among others.
            /// </summary>
            public Sprite standard;

            /// <summary>
            /// Sprite used for background elements.
            /// </summary>
            public Sprite background;

            /// <summary>
            /// Sprite used as background for input fields.
            /// </summary>
            public Sprite inputField;

            /// <summary>
            /// Sprite used for knobs that can be dragged, such as on a slider.
            /// </summary>
            public Sprite knob;

            /// <summary>
            /// Sprite used for representation of an "on" state when present, such as a checkmark.
            /// </summary>
            public Sprite checkmark;

            /// <summary>
            /// Sprite used to indicate that a button will open a dropdown when clicked.
            /// </summary>
            public Sprite dropdown;

            /// <summary>
            /// Sprite used for masking purposes, for example to be used for the viewport of a scroll view.
            /// </summary>
            public Sprite mask;
        }

        private const float  kWidth       = 160f;
        private const float  kThickHeight = 30f;
        private const float  kThinHeight  = 20f;
        private static Vector2 s_ThickElementSize       = new Vector2(kWidth, kThickHeight);
        private static Vector2 s_ThinElementSize        = new Vector2(kWidth, kThinHeight);
        private static Vector2 s_ImageElementSize       = new Vector2(100f, 100f);
        private static Color   s_DefaultSelectableColor = new Color(1f, 1f, 1f, 1f);
        private static Color   s_PanelColor             = new Color(1f, 1f, 1f, 0.392f);
        private static Color   s_TextColor              = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

        // Helper methods at top

        private static GameObject CreateUIElementRoot(string name, Vector2 size)
        {
            GameObject child = new GameObject(name);
            RectTransform rectTransform = child.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            return child;
        }

        static GameObject CreateUIObject(string name, GameObject parent)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<RectTransform>();
            SetParentAndAlign(go, parent);
            return go;
        }

        private static void SetDefaultTextValues(Text lbl)
        {
            // Set text values we want across UI elements in default controls.
            // Don't set values which are the same as the default values for the Text component,
            // since there's no point in that, and it's good to keep them as consistent as possible.
            lbl.color = s_TextColor;

            // Reset() is not called when playing. We still want the default font to be assigned
            lbl.AssignDefaultFont();
        }

        private static void SetDefaultColorTransitionValues(Selectable slider)
        {
            ColorBlock colors = slider.colors;
            colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
            colors.pressedColor     = new Color(0.698f, 0.698f, 0.698f);
            colors.disabledColor    = new Color(0.521f, 0.521f, 0.521f);
        }

        private static void SetParentAndAlign(GameObject child, GameObject parent)
        {
            if (parent == null)
                return;

            child.transform.SetParent(parent.transform, false);
            SetLayerRecursively(child, parent.layer);
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }

        /// <summary>
        /// Create the basic UI Panel.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     Image
        /// </remarks>
        /// <param name="resources">The resources to use for creation.</param>
        /// <returns>The root GameObject of the created element.</returns>
        public static GameObject CreatePanel(Resources resources)
        {
            GameObject panelRoot = CreateUIElementRoot("Panel", s_ThickElementSize);

            // Set RectTransform to stretch
            RectTransform rectTransform = panelRoot.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;

            Image image = panelRoot.AddComponent<Image>();
            image.sprite = resources.background;
            image.type = Image.Type.Sliced;
            image.color = s_PanelColor;

            return panelRoot;
        }

        /// <summary>
        /// Create the basic UI button.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     Button
        ///         -Text
        /// </remarks>
        /// <param name="resources">The resources to use for creation.</param>
        /// <returns>The root GameObject of the created element.</returns>
        public static GameObject CreateButton(Resources resources)
        {
            GameObject buttonRoot = CreateUIElementRoot("Button", s_ThickElementSize);

            GameObject childText = new GameObject("Text");
            childText.AddComponent<RectTransform>();
            SetParentAndAlign(childText, buttonRoot);

            Image image = buttonRoot.AddComponent<Image>();
            image.sprite = resources.standard;
            image.type = Image.Type.Sliced;
            image.color = s_DefaultSelectableColor;

            Button bt = buttonRoot.AddComponent<Button>();
            SetDefaultColorTransitionValues(bt);

            Text text = childText.AddComponent<Text>();
            text.text = "Button";
            text.alignment = TextAnchor.MiddleCenter;
            SetDefaultTextValues(text);

            RectTransform textRectTransform = childText.GetComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.sizeDelta = Vector2.zero;

            return buttonRoot;
        }

        /// <summary>
        /// Create the basic UI Text.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     Text
        /// </remarks>
        /// <param name="resources">The resources to use for creation.</param>
        /// <returns>The root GameObject of the created element.</returns>
        public static GameObject CreateText(Resources resources)
        {
            GameObject go = CreateUIElementRoot("Text", s_ThickElementSize);

            Text lbl = go.AddComponent<Text>();
            lbl.text = "New Text";
            SetDefaultTextValues(lbl);

            return go;
        }

        /// <summary>
        /// Create the basic UI Image.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     Image
        /// </remarks>
        /// <param name="resources">The resources to use for creation.</param>
        /// <returns>The root GameObject of the created element.</returns>
        public static GameObject CreateImage(Resources resources)
        {
            GameObject go = CreateUIElementRoot("Image", s_ImageElementSize);
            go.AddComponent<Image>();
            return go;
        }

        /// <summary>
        /// Create the basic UI RawImage.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     RawImage
        /// </remarks>
        /// <param name="resources">The resources to use for creation.</param>
        /// <returns>The root GameObject of the created element.</returns>
        public static GameObject CreateRawImage(Resources resources)
        {
            GameObject go = CreateUIElementRoot("RawImage", s_ImageElementSize);
            go.AddComponent<RawImage>();
            return go;
        }

        /// <summary>
        /// Create the basic UI Slider.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     Slider
        ///         - Background
        ///         - Fill Area
        ///             - Fill
        ///         - Handle Slide Area
        ///             - Handle
        /// </remarks>
        /// <param name="resources">The resources to use for creation.</param>
        /// <returns>The root GameObject of the created element.</returns>
        public static GameObject CreateSlider(Resources resources)
        {
            // Create GOs Hierarchy
            GameObject root = CreateUIElementRoot("Slider", s_ThinElementSize);

            GameObject background = CreateUIObject("Background", root);
            GameObject fillArea = CreateUIObject("Fill Area", root);
            GameObject fill = CreateUIObject("Fill", fillArea);
            GameObject handleArea = CreateUIObject("Handle Slide Area", root);
            GameObject handle = CreateUIObject("Handle", handleArea);

            // Background
            Image backgroundImage = background.AddComponent<Image>();
            backgroundImage.sprite = resources.background;
            backgroundImage.type = Image.Type.Sliced;
            backgroundImage.color = s_DefaultSelectableColor;
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0, 0.25f);
            backgroundRect.anchorMax = new Vector2(1, 0.75f);
            backgroundRect.sizeDelta = new Vector2(0, 0);

            // Fill Area
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1, 0.75f);
            fillAreaRect.anchoredPosition = new Vector2(-5, 0);
            fillAreaRect.sizeDelta = new Vector2(-20, 0);

            // Fill
            Image fillImage = fill.AddComponent<Image>();
            fillImage.sprite = resources.standard;
            fillImage.type = Image.Type.Sliced;
            fillImage.color = s_DefaultSelectableColor;

            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.sizeDelta = new Vector2(10, 0);

            // Handle Area
            RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.sizeDelta = new Vector2(-20, 0);
            handleAreaRect.anchorMin = new Vector2(0, 0);
            handleAreaRect.anchorMax = new Vector2(1, 1);

            // Handle
            Image handleImage = handle.AddComponent<Image>();
            handleImage.sprite = resources.knob;
            handleImage.color = s_DefaultSelectableColor;

            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 0);

            // Setup slider component
            Slider slider = root.AddComponent<Slider>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            SetDefaultColorTransitionValues(slider);

            return root;
        }

        /// <summary>
        /// Create the basic UI Scrollbar.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     Scrollbar
        ///         - Sliding Area
        ///             - Handle
        /// </remarks>
        /// <param name="resources">The resources to use for creation.</param>
        /// <returns>The root GameObject of the created element.</returns>
        public static GameObject CreateScrollbar(Resources resources)
        {
            // Create GOs Hierarchy
            GameObject scrollbarRoot = CreateUIElementRoot("Scrollbar", s_ThinElementSize);

            GameObject sliderArea = CreateUIObject("Sliding Area", scrollbarRoot);
            GameObject handle = CreateUIObject("Handle", sliderArea);

            Image bgImage = scrollbarRoot.AddComponent<Image>();
            bgImage.sprite = resources.background;
            bgImage.type = Image.Type.Sliced;
            bgImage.color = s_DefaultSelectableColor;

            Image handleImage = handle.AddComponent<Image>();
            handleImage.sprite = resources.standard;
            handleImage.type = Image.Type.Sliced;
            handleImage.color = s_DefaultSelectableColor;

            RectTransform sliderAreaRect = sliderArea.GetComponent<RectTransform>();
            sliderAreaRect.sizeDelta = new Vector2(-20, -20);
            sliderAreaRect.anchorMin = Vector2.zero;
            sliderAreaRect.anchorMax = Vector2.one;

            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 20);

            Scrollbar scrollbar = scrollbarRoot.AddComponent<Scrollbar>();
            scrollbar.handleRect = handleRect;
            scrollbar.targetGraphic = handleImage;
            SetDefaultColorTransitionValues(scrollbar);

            return scrollbarRoot;
        }

        /// <summary>
        /// Create the basic UI Toggle.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     Toggle
        ///         - Background
        ///             - Checkmark
        ///         - Label
        /// </remarks>
        /// <param name="resources">The resources to use for creation.</param>
        /// <returns>The root GameObject of the created element.</returns>
        public static GameObject CreateToggle(Resources resources)
        {
            // Set up hierarchy
            GameObject toggleRoot = CreateUIElementRoot("Toggle", s_ThinElementSize);

            GameObject background = CreateUIObject("Background", toggleRoot);
            GameObject checkmark = CreateUIObject("Checkmark", background);
            GameObject childLabel = CreateUIObject("Label", toggleRoot);

            // Set up components
            Toggle toggle = toggleRoot.AddComponent<Toggle>();
            toggle.isOn = true;

            Image bgImage = background.AddComponent<Image>();
            bgImage.sprite = resources.standard;
            bgImage.type = Image.Type.Sliced;
            bgImage.color = s_DefaultSelectableColor;

            Image checkmarkImage = checkmark.AddComponent<Image>();
            checkmarkImage.sprite = resources.checkmark;

            Text label = childLabel.AddComponent<Text>();
            label.text = "Toggle";
            SetDefaultTextValues(label);

            toggle.graphic = checkmarkImage;
            toggle.targetGraphic = bgImage;
            SetDefaultColorTransitionValues(toggle);

            RectTransform bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin        = new Vector2(0f, 1f);
            bgRect.anchorMax        = new Vector2(0f, 1f);
            bgRect.anchoredPosition = new Vector2(10f, -10f);
            bgRect.sizeDelta        = new Vector2(kThinHeight, kThinHeight);

            RectTransform checkmarkRect = checkmark.GetComponent<RectTransform>();
            checkmarkRect.anchorMin = new Vector2(0.5f, 0.5f);
            checkmarkRect.anchorMax = new Vector2(0.5f, 0.5f);
            checkmarkRect.anchoredPosition = Vector2.zero;
            checkmarkRect.sizeDelta = new Vector2(20f, 20f);

            RectTransform labelRect = childLabel.GetComponent<RectTransform>();
            labelRect.anchorMin     = new Vector2(0f, 0f);
            labelRect.anchorMax     = new Vector2(1f, 1f);
            labelRect.offsetMin     = new Vector2(23f, 1f);
            labelRect.offsetMax     = new Vector2(-5f, -2f);

            return toggleRoot;
        }

        /// <summary>
        /// Create the basic UI input field.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     InputField
        ///         - PlaceHolder
        ///         - Text
        /// </remarks>
        /// <param name="resources">The resources to use for creation.</param>
        /// <returns>The root GameObject of the created element.</returns>
        public static GameObject CreateInputField(Resources resources)
        {
            GameObject root = CreateUIElementRoot("InputField", s_ThickElementSize);

            GameObject childPlaceholder = CreateUIObject("Placeholder", root);
            GameObject childText = CreateUIObject("Text", root);

            Image image = root.AddComponent<Image>();
            image.sprite = resources.inputField;
            image.type = Image.Type.Sliced;
            image.color = s_DefaultSelectableColor;

            InputField inputField = root.AddComponent<InputField>();
            SetDefaultColorTransitionValues(inputField);

            Text text = childText.AddComponent<Text>();
            text.text = "";
            text.supportRichText = false;
            SetDefaultTextValues(text);

            Text placeholder = childPlaceholder.AddComponent<Text>();
            placeholder.text = "Enter text...";
            placeholder.fontStyle = FontStyle.Italic;
            // Make placeholder color half as opaque as normal text color.
            Color placeholderColor = text.color;
            placeholderColor.a *= 0.5f;
            placeholder.color = placeholderColor;

            RectTransform textRectTransform = childText.GetComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.sizeDelta = Vector2.zero;
            textRectTransform.offsetMin = new Vector2(10, 6);
            textRectTransform.offsetMax = new Vector2(-10, -7);

            RectTransform placeholderRectTransform = childPlaceholder.GetComponent<RectTransform>();
            placeholderRectTransform.anchorMin = Vector2.zero;
            placeholderRectTransform.anchorMax = Vector2.one;
            placeholderRectTransform.sizeDelta = Vector2.zero;
            placeholderRectTransform.offsetMin = new Vector2(10, 6);
            placeholderRectTransform.offsetMax = new Vector2(-10, -7);

            inputField.textComponent = text;
            inputField.placeholder = placeholder;

            return root;
        }

        /// <summary>
        /// Create the basic UI dropdown.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     Dropdown
        ///         - Label
        ///         - Arrow
        ///         - Template
        ///             - Viewport
        ///                 - Content
        ///                     - Item
        ///                         - Item Background
        ///                         - Item Checkmark
        ///                         - Item Label
        ///             - Scrollbar
        ///                 - Sliding Area
        ///                     - Handle
        /// </remarks>
        /// <param name="resources">The resources to use for creation.</param>
        /// <returns>The root GameObject of the created element.</returns>
        public static GameObject CreateDropdown(Resources resources)
        {
            GameObject root = CreateUIElementRoot("Dropdown", s_ThickElementSize);

            GameObject label = CreateUIObject("Label", root);
            GameObject arrow = CreateUIObject("Arrow", root);
            GameObject template = CreateUIObject("Template", root);
            GameObject viewport = CreateUIObject("Viewport", template);
            GameObject content = CreateUIObject("Content", viewport);
            GameObject item = CreateUIObject("Item", content);
            GameObject itemBackground = CreateUIObject("Item Background", item);
            GameObject itemCheckmark = CreateUIObject("Item Checkmark", item);
            GameObject itemLabel = CreateUIObject("Item Label", item);

            // Sub controls.

            GameObject scrollbar = CreateScrollbar(resources);
            scrollbar.name = "Scrollbar";
            SetParentAndAlign(scrollbar, template);

            Scrollbar scrollbarScrollbar = scrollbar.GetComponent<Scrollbar>();
            scrollbarScrollbar.SetDirection(Scrollbar.Direction.BottomToTop, true);

            RectTransform vScrollbarRT = scrollbar.GetComponent<RectTransform>();
            vScrollbarRT.anchorMin = Vector2.right;
            vScrollbarRT.anchorMax = Vector2.one;
            vScrollbarRT.pivot = Vector2.one;
            vScrollbarRT.sizeDelta = new Vector2(vScrollbarRT.sizeDelta.x, 0);

            // Setup item UI components.

            Text itemLabelText = itemLabel.AddComponent<Text>();
            SetDefaultTextValues(itemLabelText);
            itemLabelText.alignment = TextAnchor.MiddleLeft;

            Image itemBackgroundImage = itemBackground.AddComponent<Image>();
            itemBackgroundImage.color = new Color32(245, 245, 245, 255);

            Image itemCheckmarkImage = itemCheckmark.AddComponent<Image>();
            itemCheckmarkImage.sprite = resources.checkmark;

            Toggle itemToggle = item.AddComponent<Toggle>();
            itemToggle.targetGraphic = itemBackgroundImage;
            itemToggle.graphic = itemCheckmarkImage;
            itemToggle.isOn = true;

            // Setup template UI components.

            Image templateImage = template.AddComponent<Image>();
            templateImage.sprite = resources.standard;
            templateImage.type = Image.Type.Sliced;

            ScrollRect templateScrollRect = template.AddComponent<ScrollRect>();
            templateScrollRect.content = (RectTransform)content.transform;
            templateScrollRect.viewport = (RectTransform)viewport.transform;
            templateScrollRect.horizontal = false;
            templateScrollRect.movementType = ScrollRect.MovementType.Clamped;
            templateScrollRect.verticalScrollbar = scrollbarScrollbar;
            templateScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            templateScrollRect.verticalScrollbarSpacing = -3;

            Mask scrollRectMask = viewport.AddComponent<Mask>();
            scrollRectMask.showMaskGraphic = false;

            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.sprite = resources.mask;
            viewportImage.type = Image.Type.Sliced;

            // Setup dropdown UI components.

            Text labelText = label.AddComponent<Text>();
            SetDefaultTextValues(labelText);
            labelText.alignment = TextAnchor.MiddleLeft;

            Image arrowImage = arrow.AddComponent<Image>();
            arrowImage.sprite = resources.dropdown;

            Image backgroundImage = root.AddComponent<Image>();
            backgroundImage.sprite = resources.standard;
            backgroundImage.color = s_DefaultSelectableColor;
            backgroundImage.type = Image.Type.Sliced;

            Dropdown dropdown = root.AddComponent<Dropdown>();
            dropdown.targetGraphic = backgroundImage;
            SetDefaultColorTransitionValues(dropdown);
            dropdown.template = template.GetComponent<RectTransform>();
            dropdown.captionText = labelText;
            dropdown.itemText = itemLabelText;

            // Setting default Item list.
            itemLabelText.text = "Option A";
            dropdown.options.Add(new Dropdown.OptionData {text = "Option A"});
            dropdown.options.Add(new Dropdown.OptionData {text = "Option B"});
            dropdown.options.Add(new Dropdown.OptionData {text = "Option C"});
            dropdown.RefreshShownValue();

            // Set up RectTransforms.

            RectTransform labelRT = label.GetComponent<RectTransform>();
            labelRT.anchorMin           = Vector2.zero;
            labelRT.anchorMax           = Vector2.one;
            labelRT.offsetMin           = new Vector2(10, 6);
            labelRT.offsetMax           = new Vector2(-25, -7);

            RectTransform arrowRT = arrow.GetComponent<RectTransform>();
            arrowRT.anchorMin           = new Vector2(1, 0.5f);
            arrowRT.anchorMax           = new Vector2(1, 0.5f);
            arrowRT.sizeDelta           = new Vector2(20, 20);
            arrowRT.anchoredPosition    = new Vector2(-15, 0);

            RectTransform templateRT = template.GetComponent<RectTransform>();
            templateRT.anchorMin        = new Vector2(0, 0);
            templateRT.anchorMax        = new Vector2(1, 0);
            templateRT.pivot            = new Vector2(0.5f, 1);
            templateRT.anchoredPosition = new Vector2(0, 2);
            templateRT.sizeDelta        = new Vector2(0, 150);

            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            viewportRT.anchorMin        = new Vector2(0, 0);
            viewportRT.anchorMax        = new Vector2(1, 1);
            viewportRT.sizeDelta        = new Vector2(-18, 0);
            viewportRT.pivot            = new Vector2(0, 1);

            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin         = new Vector2(0f, 1);
            contentRT.anchorMax         = new Vector2(1f, 1);
            contentRT.pivot             = new Vector2(0.5f, 1);
            contentRT.anchoredPosition  = new Vector2(0, 0);
            contentRT.sizeDelta         = new Vector2(0, 28);

            RectTransform itemRT = item.GetComponent<RectTransform>();
            itemRT.anchorMin            = new Vector2(0, 0.5f);
            itemRT.anchorMax            = new Vector2(1, 0.5f);
            itemRT.sizeDelta            = new Vector2(0, 20);

            RectTransform itemBackgroundRT = itemBackground.GetComponent<RectTransform>();
            itemBackgroundRT.anchorMin  = Vector2.zero;
            itemBackgroundRT.anchorMax  = Vector2.one;
            itemBackgroundRT.sizeDelta  = Vector2.zero;

            RectTransform itemCheckmarkRT = itemCheckmark.GetComponent<RectTransform>();
            itemCheckmarkRT.anchorMin   = new Vector2(0, 0.5f);
            itemCheckmarkRT.anchorMax   = new Vector2(0, 0.5f);
            itemCheckmarkRT.sizeDelta   = new Vector2(20, 20);
            itemCheckmarkRT.anchoredPosition = new Vector2(10, 0);

            RectTransform itemLabelRT = itemLabel.GetComponent<RectTransform>();
            itemLabelRT.anchorMin       = Vector2.zero;
            itemLabelRT.anchorMax       = Vector2.one;
            itemLabelRT.offsetMin       = new Vector2(20, 1);
            itemLabelRT.offsetMax       = new Vector2(-10, -2);

            template.SetActive(false);

            return root;
        }

        /// <summary>
        /// Create the basic UI Scrollview.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     Scrollview
        ///         - Viewport
        ///             - Content
        ///         - Scrollbar Horizontal
        ///             - Sliding Area
        ///                 - Handle
        ///         - Scrollbar Vertical
        ///             - Sliding Area
        ///                 - Handle
        /// </remarks>
        /// <param name="resources">The resources to use for creation.</param>
        /// <returns>The root GameObject of the created element.</returns>
        public static GameObject CreateScrollView(Resources resources)
        {
            GameObject root = CreateUIElementRoot("Scroll View", new Vector2(200, 200));

            GameObject viewport = CreateUIObject("Viewport", root);
            GameObject content = CreateUIObject("Content", viewport);

            // Sub controls.

            GameObject hScrollbar = CreateScrollbar(resources);
            hScrollbar.name = "Scrollbar Horizontal";
            SetParentAndAlign(hScrollbar, root);
            RectTransform hScrollbarRT = hScrollbar.GetComponent<RectTransform>();
            hScrollbarRT.anchorMin = Vector2.zero;
            hScrollbarRT.anchorMax = Vector2.right;
            hScrollbarRT.pivot = Vector2.zero;
            hScrollbarRT.sizeDelta = new Vector2(0, hScrollbarRT.sizeDelta.y);

            GameObject vScrollbar = CreateScrollbar(resources);
            vScrollbar.name = "Scrollbar Vertical";
            SetParentAndAlign(vScrollbar, root);
            vScrollbar.GetComponent<Scrollbar>().SetDirection(Scrollbar.Direction.BottomToTop, true);
            RectTransform vScrollbarRT = vScrollbar.GetComponent<RectTransform>();
            vScrollbarRT.anchorMin = Vector2.right;
            vScrollbarRT.anchorMax = Vector2.one;
            vScrollbarRT.pivot = Vector2.one;
            vScrollbarRT.sizeDelta = new Vector2(vScrollbarRT.sizeDelta.x, 0);

            // Setup RectTransforms.

            // Make viewport fill entire scroll view.
            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.sizeDelta = Vector2.zero;
            viewportRT.pivot = Vector2.up;

            // Make context match viewpoprt width and be somewhat taller.
            // This will show the vertical scrollbar and not the horizontal one.
            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = Vector2.up;
            contentRT.anchorMax = Vector2.one;
            contentRT.sizeDelta = new Vector2(0, 300);
            contentRT.pivot = Vector2.up;

            // Setup UI components.

            ScrollRect scrollRect = root.AddComponent<ScrollRect>();
            scrollRect.content = contentRT;
            scrollRect.viewport = viewportRT;
            scrollRect.horizontalScrollbar = hScrollbar.GetComponent<Scrollbar>();
            scrollRect.verticalScrollbar = vScrollbar.GetComponent<Scrollbar>();
            scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.horizontalScrollbarSpacing = -3;
            scrollRect.verticalScrollbarSpacing = -3;

            Image rootImage = root.AddComponent<Image>();
            rootImage.sprite = resources.background;
            rootImage.type = Image.Type.Sliced;
            rootImage.color = s_PanelColor;

            Mask viewportMask = viewport.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.sprite = resources.mask;
            viewportImage.type = Image.Type.Sliced;

            return root;
        }
    }
}
