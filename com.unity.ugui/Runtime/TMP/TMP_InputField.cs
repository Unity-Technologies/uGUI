//#define TMP_DEBUG_MODE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;


namespace TMPro
{
    /// <summary>
    /// Editable text input field.
    /// </summary>
    [AddComponentMenu("UI/TextMeshPro - Input Field", 11)]
        #if UNITY_2023_2_OR_NEWER
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/TextMeshPro/index.html")]
    #else
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.2")]
    #endif
    public class TMP_InputField : Selectable,
        IUpdateSelectedHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IPointerClickHandler,
        ISubmitHandler,
        ICancelHandler,
        ICanvasElement,
        ILayoutElement,
        IScrollHandler
    {

        // Setting the content type acts as a shortcut for setting a combination of InputType, CharacterValidation, LineType, and TouchScreenKeyboardType
        public enum ContentType
        {
            Standard,
            Autocorrected,
            IntegerNumber,
            DecimalNumber,
            Alphanumeric,
            Name,
            EmailAddress,
            Password,
            Pin,
            Custom
        }

        public enum InputType
        {
            Standard,
            AutoCorrect,
            Password,
        }

        public enum CharacterValidation
        {
            None,
            Digit,
            Integer,
            Decimal,
            Alphanumeric,
            Name,
            Regex,
            EmailAddress,
            CustomValidator
        }

        public enum LineType
        {
            SingleLine,
            MultiLineSubmit,
            MultiLineNewline
        }

        public delegate char OnValidateInput(string text, int charIndex, char addedChar);

        [Serializable]
        public class SubmitEvent : UnityEvent<string> { }

        [Serializable]
        public class OnChangeEvent : UnityEvent<string> { }

        [Serializable]
        public class SelectionEvent : UnityEvent<string> { }

        [Serializable]
        public class TextSelectionEvent : UnityEvent<string, int, int> { }

        [Serializable]
        public class TouchScreenKeyboardEvent : UnityEvent<TouchScreenKeyboard.Status> { }

        protected TouchScreenKeyboard m_SoftKeyboard;
        static private readonly char[] kSeparators = { ' ', '.', ',', '\t', '\r', '\n' };

    #if UNITY_ANDROID
        static private bool s_IsQuestDeviceEvaluated = false;
    #endif // if UNITY_ANDROID

        static private bool s_IsQuestDevice = false;

        #region Exposed properties
        /// <summary>
        /// Text Text used to display the input's value.
        /// </summary>

        protected RectTransform m_RectTransform;

        [SerializeField]
        protected RectTransform m_TextViewport;

        protected RectMask2D m_TextComponentRectMask;

        protected RectMask2D m_TextViewportRectMask;
        //private Rect m_CachedViewportRect;

        [SerializeField]
        protected TMP_Text m_TextComponent;

        protected RectTransform m_TextComponentRectTransform;

        [SerializeField]
        protected Graphic m_Placeholder;

        [SerializeField]
        protected Scrollbar m_VerticalScrollbar;

        [SerializeField]
        protected TMP_ScrollbarEventHandler m_VerticalScrollbarEventHandler;
        //private bool m_ForceDeactivation;

        private bool m_IsDrivenByLayoutComponents = false;
        [SerializeField]
        private LayoutGroup m_LayoutGroup;

        private IScrollHandler m_IScrollHandlerParent;

        /// <summary>
        /// Used to keep track of scroll position
        /// </summary>
        private float m_ScrollPosition;

        /// <summary>
        ///
        /// </summary>
        [SerializeField]
        protected float m_ScrollSensitivity = 1.0f;

        //[SerializeField]
        //protected TMP_Text m_PlaceholderTextComponent;

        [SerializeField]
        private ContentType m_ContentType = ContentType.Standard;

        /// <summary>
        /// Type of data expected by the input field.
        /// </summary>
        [SerializeField]
        private InputType m_InputType = InputType.Standard;

        /// <summary>
        /// The character used to hide text in password field.
        /// </summary>
        [SerializeField]
        private char m_AsteriskChar = '*';

        /// <summary>
        /// Keyboard type applies to mobile keyboards that get shown.
        /// </summary>
        [SerializeField]
        private TouchScreenKeyboardType m_KeyboardType = TouchScreenKeyboardType.Default;

        [SerializeField]
        private LineType m_LineType = LineType.SingleLine;

        /// <summary>
        /// Should hide mobile input field part of the virtual keyboard.
        /// </summary>
        [SerializeField]
        private bool m_HideMobileInput = false;

        /// <summary>
        /// Should hide soft / virtual keyboard.
        /// </summary>
        [SerializeField]
        private bool m_HideSoftKeyboard = false;

        /// <summary>
        /// What kind of validation to use with the input field's data.
        /// </summary>
        [SerializeField]
        private CharacterValidation m_CharacterValidation = CharacterValidation.None;

        /// <summary>
        /// The Regex expression used for validating the text input.
        /// </summary>
        [SerializeField]
        private string m_RegexValue = string.Empty;

        /// <summary>
        /// The point sized used by the placeholder and input text object.
        /// </summary>
        [SerializeField]
        private float m_GlobalPointSize = 14;

        /// <summary>
        /// Maximum number of characters allowed before input no longer works.
        /// </summary>
        [SerializeField]
        private int m_CharacterLimit = 0;

        /// <summary>
        /// Event delegates triggered when the input field submits its data.
        /// </summary>
        [SerializeField]
        private SubmitEvent m_OnEndEdit = new SubmitEvent();

        /// <summary>
        /// Event delegates triggered when the input field submits its data.
        /// </summary>
        [SerializeField]
        private SubmitEvent m_OnSubmit = new SubmitEvent();

        /// <summary>
        /// Event delegates triggered when the input field is focused.
        /// </summary>
        [SerializeField]
        private SelectionEvent m_OnSelect = new SelectionEvent();

        /// <summary>
        /// Event delegates triggered when the input field focus is lost.
        /// </summary>
        [SerializeField]
        private SelectionEvent m_OnDeselect = new SelectionEvent();

        /// <summary>
        /// Event delegates triggered when the text is selected / highlighted.
        /// </summary>
        [SerializeField]
        private TextSelectionEvent m_OnTextSelection = new TextSelectionEvent();

        /// <summary>
        /// Event delegates triggered when text is no longer select / highlighted.
        /// </summary>
        [SerializeField]
        private TextSelectionEvent m_OnEndTextSelection = new TextSelectionEvent();

        /// <summary>
        /// Event delegates triggered when the input field changes its data.
        /// </summary>
        [SerializeField]
        private OnChangeEvent m_OnValueChanged = new OnChangeEvent();

        /// <summary>
        /// Event delegates triggered when the status of the TouchScreenKeyboard changes.
        /// </summary>
        [SerializeField]
        private TouchScreenKeyboardEvent m_OnTouchScreenKeyboardStatusChanged = new TouchScreenKeyboardEvent();

        /// <summary>
        /// Custom validation callback.
        /// </summary>
        [SerializeField]
        private OnValidateInput m_OnValidateInput;

        [SerializeField]
        private Color m_CaretColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

        [SerializeField]
        private bool m_CustomCaretColor = false;

        [SerializeField]
        private Color m_SelectionColor = new Color(168f / 255f, 206f / 255f, 255f / 255f, 192f / 255f);

        /// <summary>
        /// Input field's value.
        /// </summary>

        [SerializeField]
        [TextArea(5, 10)]
        protected string m_Text = string.Empty;

        [SerializeField]
        [Range(0f, 4f)]
        private float m_CaretBlinkRate = 0.85f;

        [SerializeField]
        [Range(1, 5)]
        private int m_CaretWidth = 1;

        [SerializeField]
        private bool m_ReadOnly = false;

        [SerializeField]
        private bool m_RichText = true;

        #endregion

        protected int m_StringPosition = 0;
        protected int m_StringSelectPosition = 0;
        protected int m_CaretPosition = 0;
        protected int m_CaretSelectPosition = 0;

        private RectTransform caretRectTrans = null;
        protected UIVertex[] m_CursorVerts = null;
        private CanvasRenderer m_CachedInputRenderer;
        private Vector2 m_LastPosition;

        [NonSerialized]
        protected Mesh m_Mesh;
        private bool m_AllowInput = false;
        //bool m_HasLostFocus = false;
        private bool m_ShouldActivateNextUpdate = false;
        private bool m_UpdateDrag = false;
        private bool m_DragPositionOutOfBounds = false;
        private const float kHScrollSpeed = 0.05f;
        private const float kVScrollSpeed = 0.10f;
        protected bool m_CaretVisible;
        private Coroutine m_BlinkCoroutine = null;
        private float m_BlinkStartTime = 0.0f;
        private Coroutine m_DragCoroutine = null;
        private string m_OriginalText = "";
        private bool m_WasCanceled = false;
        private bool m_HasDoneFocusTransition = false;
        private WaitForSecondsRealtime m_WaitForSecondsRealtime;
        private bool m_PreventCallback = false;

        private bool m_TouchKeyboardAllowsInPlaceEditing = false;

        private bool m_IsTextComponentUpdateRequired = false;

        private bool m_HasTextBeenRemoved = false;
        private float m_PointerDownClickStartTime;
        private float m_KeyDownStartTime;
        private float m_DoubleClickDelay = 0.5f;

        private bool m_IsApplePlatform = false;

        // Doesn't include dot and @ on purpose! See usage for details.
        const string kEmailSpecialCharacters = "!#$%&'*+-/=?^_`{|}~";
        const string kOculusQuestDeviceModel = "Oculus Quest";

        private BaseInput inputSystem
        {
            get
            {
                if (EventSystem.current && EventSystem.current.currentInputModule)
                    return EventSystem.current.currentInputModule.input;
                return null;
            }
        }

        private string compositionString
        {
            get { return inputSystem != null ? inputSystem.compositionString : Input.compositionString; }
        }
        private bool m_IsCompositionActive = false;
        private bool m_ShouldUpdateIMEWindowPosition = false;
        private int m_PreviousIMEInsertionLine = 0;

        private int compositionLength
        {
            get
            {
                if (m_ReadOnly)
                    return 0;

                return compositionString.Length;
            }
        }



        protected TMP_InputField()
        {
            SetTextComponentWrapMode();
        }

        protected Mesh mesh
        {
            get
            {
                if (m_Mesh == null)
                    m_Mesh = new Mesh();
                return m_Mesh;
            }
        }

        /// <summary>
        /// Should the inputfield be automatically activated upon selection.
        /// </summary>
        public virtual bool shouldActivateOnSelect
        {
            set
            {
                m_ShouldActivateOnSelect = value;
            }
            get
            {
                return m_ShouldActivateOnSelect && Application.platform != RuntimePlatform.tvOS;
            }
        }

        /// <summary>
        /// Should the mobile keyboard input be hidden.
        /// </summary>
        public bool shouldHideMobileInput
        {
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.tvOS:
                    #if UNITY_2022_1_OR_NEWER
                    case RuntimePlatform.WebGLPlayer:
                    #endif
                        return m_HideMobileInput;
                    default:
                        return true;
                }
            }

            set
            {
                switch(Application.platform)
                {
                    case RuntimePlatform.Android:
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.tvOS:
                    #if UNITY_2022_1_OR_NEWER
                    case RuntimePlatform.WebGLPlayer:
                    #endif
                        SetPropertyUtility.SetStruct(ref m_HideMobileInput, value);
                        break;
                    default:
                        m_HideMobileInput = true;
                        break;
                }
            }
        }

        public bool shouldHideSoftKeyboard
        {
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.tvOS:
                    #if UNITY_XR_VISIONOS_SUPPORTED
                    case RuntimePlatform.VisionOS:
                    #endif
                    case RuntimePlatform.WSAPlayerX86:
                    case RuntimePlatform.WSAPlayerX64:
                    case RuntimePlatform.WSAPlayerARM:
                    #if UNITY_2020_2_OR_NEWER
                    case RuntimePlatform.PS4:
                        #if !(UNITY_2020_2_1 || UNITY_2020_2_2)
                        case RuntimePlatform.PS5:
                        #endif
                    #endif
                    #if UNITY_2019_4_OR_NEWER
                    case RuntimePlatform.GameCoreXboxOne:
                    case RuntimePlatform.GameCoreXboxSeries:
                    #endif
                    case RuntimePlatform.Switch:
                    #if UNITY_2022_1_OR_NEWER
                    case RuntimePlatform.WebGLPlayer:
                    #endif
                        return m_HideSoftKeyboard;
                    default:
                        return true;
                }
            }

            set
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.tvOS:
                    #if UNITY_XR_VISIONOS_SUPPORTED
                    case RuntimePlatform.VisionOS:
                    #endif
                    case RuntimePlatform.WSAPlayerX86:
                    case RuntimePlatform.WSAPlayerX64:
                    case RuntimePlatform.WSAPlayerARM:
                    #if UNITY_2020_2_OR_NEWER
                    case RuntimePlatform.PS4:
                        #if !(UNITY_2020_2_1 || UNITY_2020_2_2)
                        case RuntimePlatform.PS5:
                        #endif
                    #endif
                    #if UNITY_2019_4_OR_NEWER
                    case RuntimePlatform.GameCoreXboxOne:
                    case RuntimePlatform.GameCoreXboxSeries:
                    #endif
                    case RuntimePlatform.Switch:
                    #if UNITY_2022_1_OR_NEWER
                    case RuntimePlatform.WebGLPlayer:
                    #endif
                        SetPropertyUtility.SetStruct(ref m_HideSoftKeyboard, value);
                        break;
                    default:
                        m_HideSoftKeyboard = true;
                        break;
                }

                if (m_HideSoftKeyboard == true && m_SoftKeyboard != null && TouchScreenKeyboard.isSupported && m_SoftKeyboard.active)
                {
                    m_SoftKeyboard.active = false;
                    m_SoftKeyboard = null;
                }
            }
        }

        private bool isKeyboardUsingEvents()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return InPlaceEditing() && m_HideSoftKeyboard;
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.tvOS:
                #if UNITY_XR_VISIONOS_SUPPORTED
                case RuntimePlatform.VisionOS:
                #endif
                    return m_HideSoftKeyboard;
                #if UNITY_2020_2_OR_NEWER
                case RuntimePlatform.PS4:
                    #if !(UNITY_2020_2_1 || UNITY_2020_2_2)
                    case RuntimePlatform.PS5:
                    #endif
                #endif
                #if UNITY_2019_4_OR_NEWER
                case RuntimePlatform.GameCoreXboxOne:
                case RuntimePlatform.GameCoreXboxSeries:
                #endif
                case RuntimePlatform.Switch:
                    return false;
                #if UNITY_2022_1_OR_NEWER
                case RuntimePlatform.WebGLPlayer:
                    return m_SoftKeyboard == null || !m_SoftKeyboard.active;
                #endif
                default:
                    return true;
            }
        }

        private bool isUWP()
        {
            return Application.platform == RuntimePlatform.WSAPlayerX86 || Application.platform == RuntimePlatform.WSAPlayerX64 || Application.platform == RuntimePlatform.WSAPlayerARM;
        }

        /// <summary>
        /// Input field's current text value. This is not necessarily the same as what is visible on screen.
        /// </summary>
        /// <remarks>
        /// Note that null is invalid value  for InputField.text.
        /// </remarks>
        /// <example>
        /// <code>
        /// using UnityEngine;
        /// using System.Collections;
        /// using UnityEngine.UI; // Required when Using UI elements.
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     public InputField mainInputField;
        ///
        ///     public void Start()
        ///     {
        ///         mainInputField.text = "Enter Text Here...";
        ///     }
        /// }
        /// </code>
        /// </example>
        public string text
        {
            get
            {
                return m_Text;
            }
            set
            {
                SetText(value);
            }
        }

        /// <summary>
        /// Set Input field's current text value without invoke onValueChanged. This is not necessarily the same as what is visible on screen.
        /// </summary>
        public void SetTextWithoutNotify(string input)
        {
            SetText(input, false);
        }

        void SetText(string value, bool sendCallback = true)
        {
            if (this.text == value)
                return;

            if (value == null)
                value = "";

            value = value.Replace("\0", string.Empty); // remove embedded nulls

            m_Text = value;

            /*
            if (m_LineType == LineType.SingleLine)
                value = value.Replace("\n", "").Replace("\t", "");

            // If we have an input validator, validate the input and apply the character limit at the same time.
            if (onValidateInput != null || characterValidation != CharacterValidation.None)
            {
                m_Text = "";
                OnValidateInput validatorMethod = onValidateInput ?? Validate;
                m_CaretPosition = m_CaretSelectPosition = value.Length;
                int charactersToCheck = characterLimit > 0 ? Math.Min(characterLimit, value.Length) : value.Length;
                for (int i = 0; i < charactersToCheck; ++i)
                {
                    char c = validatorMethod(m_Text, m_Text.Length, value[i]);
                    if (c != 0)
                        m_Text += c;
                }
            }
            else
            {
                m_Text = characterLimit > 0 && value.Length > characterLimit ? value.Substring(0, characterLimit) : value;
            }
            */

            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                SendOnValueChangedAndUpdateLabel();
                return;
            }
            #endif

            if (m_SoftKeyboard != null)
                m_SoftKeyboard.text = m_Text;

            if (m_StringPosition > m_Text.Length)
                m_StringPosition = m_StringSelectPosition = m_Text.Length;
            else if (m_StringSelectPosition > m_Text.Length)
                m_StringSelectPosition = m_Text.Length;

            m_forceRectTransformAdjustment = true;

            m_IsTextComponentUpdateRequired = true;
            UpdateLabel();

            if (sendCallback)
                SendOnValueChanged();
        }


        public bool isFocused
        {
            get { return m_AllowInput; }
        }

        public float caretBlinkRate
        {
            get { return m_CaretBlinkRate; }
            set
            {
                if (SetPropertyUtility.SetStruct(ref m_CaretBlinkRate, value))
                {
                    if (m_AllowInput)
                        SetCaretActive();
                }
            }
        }

        public int caretWidth { get { return m_CaretWidth; } set { if (SetPropertyUtility.SetStruct(ref m_CaretWidth, value)) MarkGeometryAsDirty(); } }

        public RectTransform textViewport { get { return m_TextViewport; } set { SetPropertyUtility.SetClass(ref m_TextViewport, value); } }

        public TMP_Text textComponent
        {
            get { return m_TextComponent; }
            set
            {
                if (SetPropertyUtility.SetClass(ref m_TextComponent, value))
                {
                    SetTextComponentWrapMode();
                }
            }
        }

        //public TMP_Text placeholderTextComponent { get { return m_PlaceholderTextComponent; } set { SetPropertyUtility.SetClass(ref m_PlaceholderTextComponent, value); } }

        public Graphic placeholder { get { return m_Placeholder; } set { SetPropertyUtility.SetClass(ref m_Placeholder, value); } }

        public Scrollbar verticalScrollbar
        {
            get { return m_VerticalScrollbar; }
            set
            {
                if (m_VerticalScrollbar != null)
                    m_VerticalScrollbar.onValueChanged.RemoveListener(OnScrollbarValueChange);

                SetPropertyUtility.SetClass(ref m_VerticalScrollbar, value);

                if (m_VerticalScrollbar)
                {
                    m_VerticalScrollbar.onValueChanged.AddListener(OnScrollbarValueChange);

                }
            }
        }

        public float scrollSensitivity { get { return m_ScrollSensitivity; } set { if (SetPropertyUtility.SetStruct(ref m_ScrollSensitivity, value)) MarkGeometryAsDirty(); } }

        public Color caretColor { get { return customCaretColor ? m_CaretColor : textComponent.color; } set { if (SetPropertyUtility.SetColor(ref m_CaretColor, value)) MarkGeometryAsDirty(); } }

        public bool customCaretColor { get { return m_CustomCaretColor; } set { if (m_CustomCaretColor != value) { m_CustomCaretColor = value; MarkGeometryAsDirty(); } } }

        public Color selectionColor { get { return m_SelectionColor; } set { if (SetPropertyUtility.SetColor(ref m_SelectionColor, value)) MarkGeometryAsDirty(); } }

        public SubmitEvent onEndEdit { get { return m_OnEndEdit; } set { SetPropertyUtility.SetClass(ref m_OnEndEdit, value); } }

        public SubmitEvent onSubmit { get { return m_OnSubmit; } set { SetPropertyUtility.SetClass(ref m_OnSubmit, value); } }

        public SelectionEvent onSelect { get { return m_OnSelect; } set { SetPropertyUtility.SetClass(ref m_OnSelect, value); } }

        public SelectionEvent onDeselect { get { return m_OnDeselect; } set { SetPropertyUtility.SetClass(ref m_OnDeselect, value); } }

        public TextSelectionEvent onTextSelection { get { return m_OnTextSelection; } set { SetPropertyUtility.SetClass(ref m_OnTextSelection, value); } }

        public TextSelectionEvent onEndTextSelection { get { return m_OnEndTextSelection; } set { SetPropertyUtility.SetClass(ref m_OnEndTextSelection, value); } }

        public OnChangeEvent onValueChanged { get { return m_OnValueChanged; } set { SetPropertyUtility.SetClass(ref m_OnValueChanged, value); } }

        public TouchScreenKeyboardEvent onTouchScreenKeyboardStatusChanged { get { return m_OnTouchScreenKeyboardStatusChanged; } set { SetPropertyUtility.SetClass(ref m_OnTouchScreenKeyboardStatusChanged, value); } }

        public OnValidateInput onValidateInput { get { return m_OnValidateInput; } set { SetPropertyUtility.SetClass(ref m_OnValidateInput, value); } }

        public int characterLimit
        {
            get { return m_CharacterLimit; }
            set
            {
                if (SetPropertyUtility.SetStruct(ref m_CharacterLimit, Math.Max(0, value)))
                {
                    UpdateLabel();
                    if (m_SoftKeyboard != null)
                        m_SoftKeyboard.characterLimit = value;
                }
            }
        }

        //public bool isInteractableControl { set { if ( } }

        /// <summary>
        /// Set the point size on both Placeholder and Input text object.
        /// </summary>
        public float pointSize
        {
            get { return m_GlobalPointSize; }
            set
            {
                if (SetPropertyUtility.SetStruct(ref m_GlobalPointSize, Math.Max(0, value)))
                {
                    SetGlobalPointSize(m_GlobalPointSize);
                    UpdateLabel();
                }
            }
        }

        /// <summary>
        /// Sets the Font Asset on both Placeholder and Input child objects.
        /// </summary>
        public TMP_FontAsset fontAsset
        {
            get { return m_GlobalFontAsset; }
            set
            {
                if (SetPropertyUtility.SetClass(ref m_GlobalFontAsset, value))
                {
                    SetGlobalFontAsset(m_GlobalFontAsset);
                    UpdateLabel();
                }
            }
        }
        [SerializeField]
        protected TMP_FontAsset m_GlobalFontAsset;

        /// <summary>
        /// Determines if the whole text will be selected when focused.
        /// </summary>
        public bool onFocusSelectAll
        {
            get { return m_OnFocusSelectAll; }
            set { m_OnFocusSelectAll = value; }
        }
        [SerializeField]
        protected bool m_OnFocusSelectAll = true;
        protected bool m_isSelectAll;

        /// <summary>
        /// Determines if the text and caret position as well as selection will be reset when the input field is deactivated.
        /// </summary>
        public bool resetOnDeActivation
        {
            get { return m_ResetOnDeActivation; }
            set { m_ResetOnDeActivation = value; }
        }
        [SerializeField]
        protected bool m_ResetOnDeActivation = true;
        private bool m_SelectionStillActive = false;
        private bool m_ReleaseSelection = false;
        private KeyCode m_LastKeyCode;

        private GameObject m_PreviouslySelectedObject;

        /// <summary>
        /// Determines if the text selection will remain visible when the input field looses focus and is deactivated.
        /// </summary>
        public bool keepTextSelectionVisible
        {
            get { return m_KeepTextSelectionVisible; }
            set { m_KeepTextSelectionVisible = value; }
        }

        [SerializeField]
        private bool m_KeepTextSelectionVisible;

        /// <summary>
        /// Controls whether the original text is restored when pressing "ESC".
        /// </summary>
        public bool restoreOriginalTextOnEscape
        {
            get { return m_RestoreOriginalTextOnEscape; }
            set { m_RestoreOriginalTextOnEscape = value; }
        }
        [SerializeField]
        private bool m_RestoreOriginalTextOnEscape = true;

        /// <summary>
        /// Is Rich Text editing allowed?
        /// </summary>
        public bool isRichTextEditingAllowed
        {
            get { return m_isRichTextEditingAllowed; }
            set { m_isRichTextEditingAllowed = value; }
        }
        [SerializeField]
        protected bool m_isRichTextEditingAllowed = false;


        // Content Type related
        public ContentType contentType { get { return m_ContentType; } set { if (SetPropertyUtility.SetStruct(ref m_ContentType, value)) EnforceContentType(); } }

        public LineType lineType
        {
            get { return m_LineType; }
            set
            {
                if (SetPropertyUtility.SetStruct(ref m_LineType, value))
                {
                    SetToCustomIfContentTypeIsNot(ContentType.Standard, ContentType.Autocorrected);
                    SetTextComponentWrapMode();
                }
            }
        }

        /// <summary>
        /// Limits the number of lines of text in the Input Field.
        /// </summary>
        public int lineLimit
        {
            get { return m_LineLimit; }
            set
            {
                if (m_LineType == LineType.SingleLine)
                    m_LineLimit = 1;
                else
                    SetPropertyUtility.SetStruct(ref m_LineLimit, value);

            }
        }
        [SerializeField]
        protected int m_LineLimit = 0;

        /// <summary>
        /// The type of input expected. See InputField.InputType.
        /// </summary>
        public InputType inputType { get { return m_InputType; } set { if (SetPropertyUtility.SetStruct(ref m_InputType, value)) SetToCustom(); } }

        /// <summary>
        /// The TouchScreenKeyboard being used to edit the Input Field.
        /// </summary>
        public TouchScreenKeyboard touchScreenKeyboard { get { return m_SoftKeyboard; } }

        /// <summary>
        /// They type of mobile keyboard that will be used.
        /// </summary>
        public TouchScreenKeyboardType keyboardType
        {
            get { return m_KeyboardType; }
            set
            {
                if (SetPropertyUtility.SetStruct(ref m_KeyboardType, value))
                    SetToCustom();
            }
        }

        /// <summary>
        /// Determines if the keyboard is opened in alert mode.
        /// </summary>
        public bool isAlert;

        /// <summary>
        /// The type of validation to perform on a character
        /// </summary>
        public CharacterValidation characterValidation { get { return m_CharacterValidation; } set { if (SetPropertyUtility.SetStruct(ref m_CharacterValidation, value)) SetToCustom(); } }

        /// <summary>
        /// Sets the Input Validation to use a Custom Input Validation script.
        /// </summary>
        public TMP_InputValidator inputValidator
        {
            get { return m_InputValidator; }
            set {  if (SetPropertyUtility.SetClass(ref m_InputValidator, value)) SetToCustom(CharacterValidation.CustomValidator); }
        }
        [SerializeField]
        protected TMP_InputValidator m_InputValidator = null;

        public bool readOnly { get { return m_ReadOnly; } set { m_ReadOnly = value; } }

        [SerializeField]
        private bool m_ShouldActivateOnSelect = true;

        public bool richText { get { return m_RichText; } set { m_RichText = value; SetTextComponentRichTextMode(); } }

        // Derived property
        public bool multiLine { get { return m_LineType == LineType.MultiLineNewline || lineType == LineType.MultiLineSubmit; } }
        // Not shown in Inspector.
        public char asteriskChar { get { return m_AsteriskChar; } set { if (SetPropertyUtility.SetStruct(ref m_AsteriskChar, value)) UpdateLabel(); } }
        public bool wasCanceled { get { return m_WasCanceled; } }


        protected void ClampStringPos(ref int pos)
        {
            if (pos <= 0)
                pos = 0;
            else if (pos > text.Length)
                pos = text.Length;
        }

        protected void ClampCaretPos(ref int pos)
        {
            if (pos > m_TextComponent.textInfo.characterCount - 1)
                pos = m_TextComponent.textInfo.characterCount - 1;

            if (pos <= 0)
                pos = 0;
        }

        int ClampArrayIndex(int index)
        {
            if (index < 0)
                return 0;

            return index;
        }

        /// <summary>
        /// Current position of the cursor.
        /// Getters are public Setters are protected
        /// </summary>

        protected int caretPositionInternal { get { return m_CaretPosition + compositionLength; } set { m_CaretPosition = value; ClampCaretPos(ref m_CaretPosition); } }
        protected int stringPositionInternal { get { return m_StringPosition + compositionLength; } set { m_StringPosition = value; ClampStringPos(ref m_StringPosition); } }

        protected int caretSelectPositionInternal { get { return m_CaretSelectPosition + compositionLength; } set { m_CaretSelectPosition = value; ClampCaretPos(ref m_CaretSelectPosition); } }
        protected int stringSelectPositionInternal { get { return m_StringSelectPosition + compositionLength; } set { m_StringSelectPosition = value; ClampStringPos(ref m_StringSelectPosition); } }

        private bool hasSelection { get { return stringPositionInternal != stringSelectPositionInternal; } }
        private bool m_isSelected;
        private bool m_IsStringPositionDirty;
        private bool m_IsCaretPositionDirty;
        private bool m_forceRectTransformAdjustment;

		// Primary to track when an user presses on the X to close the keyboard in the HoloLens
		private bool m_IsKeyboardBeingClosedInHoloLens = false;

        /// <summary>
        /// Get: Returns the focus position as thats the position that moves around even during selection.
        /// Set: Set both the anchor and focus position such that a selection doesn't happen
        /// </summary>
        public int caretPosition
        {
            get => caretSelectPositionInternal;
            set { selectionAnchorPosition = value; selectionFocusPosition = value; UpdateStringIndexFromCaretPosition(); }
        }

        /// <summary>
        /// Get: Returns the fixed position of selection
        /// Set: If compositionString is 0 set the fixed position
        /// </summary>
        public int selectionAnchorPosition
        {
            get
            {
                return caretPositionInternal;
            }

            set
            {
                if (compositionLength != 0)
                    return;

                caretPositionInternal = value;
                m_IsStringPositionDirty = true;
            }
        }

        /// <summary>
        /// Get: Returns the variable position of selection
        /// Set: If compositionString is 0 set the variable position
        /// </summary>
        public int selectionFocusPosition
        {
            get
            {
                return caretSelectPositionInternal;
            }
            set
            {
                if (compositionLength != 0)
                    return;

                caretSelectPositionInternal = value;
                m_IsStringPositionDirty = true;
            }
        }


        /// <summary>
        ///
        /// </summary>
        public int stringPosition
        {
            get => stringSelectPositionInternal;
            set { selectionStringAnchorPosition = value; selectionStringFocusPosition = value; UpdateCaretPositionFromStringIndex(); }
        }


        /// <summary>
        /// The fixed position of the selection in the raw string which may contains rich text.
        /// </summary>
        public int selectionStringAnchorPosition
        {
            get
            {
                return stringPositionInternal;
            }

            set
            {
                if (compositionLength != 0)
                    return;

                stringPositionInternal = value;
                m_IsCaretPositionDirty = true;
            }
        }


        /// <summary>
        /// The variable position of the selection in the raw string which may contains rich text.
        /// </summary>
        public int selectionStringFocusPosition
        {
            get
            {
                return stringSelectPositionInternal;
            }
            set
            {
                if (compositionLength != 0)
                    return;

                stringSelectPositionInternal = value;
                m_IsCaretPositionDirty = true;
            }
        }


        #if UNITY_EDITOR
        // Remember: This is NOT related to text validation!
        // This is Unity's own OnValidate method which is invoked when changing values in the Inspector.
        protected override void OnValidate()
        {
            base.OnValidate();
            EnforceContentType();

            m_CharacterLimit = Math.Max(0, m_CharacterLimit);

            //This can be invoked before OnEnabled is called. So we shouldn't be accessing other objects, before OnEnable is called.
            if (!IsActive())
                return;

            SetTextComponentRichTextMode();

            UpdateLabel();

            if (m_AllowInput)
                SetCaretActive();
        }
        #endif // if UNITY_EDITOR

    	#if UNITY_ANDROID
        protected override void Awake()
        {
            base.Awake();

            if (s_IsQuestDeviceEvaluated)
                return;

            // Used for Oculus Quest 1 and 2 software keyboard regression.
            // TouchScreenKeyboard.isInPlaceEditingAllowed is always returning true in these devices and would prevent the software keyboard from showing up if that value was used.
            s_IsQuestDevice = SystemInfo.deviceModel == kOculusQuestDeviceModel;
            s_IsQuestDeviceEvaluated = true;
        }
    	#endif // if UNITY_ANDROID


        protected override void OnEnable()
        {
            //Debug.Log("*** OnEnable() *** - " + this.name);

            base.OnEnable();

            if (m_Text == null)
                m_Text = string.Empty;

            m_IsApplePlatform = SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX || SystemInfo.operatingSystem.Contains("iOS") || SystemInfo.operatingSystem.Contains("tvOS");

            // Check if Input Field is driven by any layout components
            ILayoutController layoutController = GetComponent<ILayoutController>();

            if (layoutController != null)
            {
                m_IsDrivenByLayoutComponents = true;
                m_LayoutGroup = GetComponent<LayoutGroup>();
            }
            else
                m_IsDrivenByLayoutComponents = false;

            if (Application.isPlaying)
            {
                if (m_CachedInputRenderer == null && m_TextComponent != null)
                {
                    GameObject go = new GameObject("Caret", typeof(TMP_SelectionCaret));

                    go.hideFlags = HideFlags.DontSave;
                    go.transform.SetParent(m_TextComponent.transform.parent);
                    go.transform.SetAsFirstSibling();
                    go.layer = gameObject.layer;

                    caretRectTrans = go.GetComponent<RectTransform>();
                    m_CachedInputRenderer = go.GetComponent<CanvasRenderer>();
                    m_CachedInputRenderer.SetMaterial(Graphic.defaultGraphicMaterial, Texture2D.whiteTexture);

                    // Needed as if any layout is present we want the caret to always be the same as the text area.
                    go.AddComponent<LayoutElement>().ignoreLayout = true;

                    AssignPositioningIfNeeded();
                }
            }

            m_RectTransform = GetComponent<RectTransform>();

            // Check if parent component has IScrollHandler
            IScrollHandler[] scrollHandlers = GetComponentsInParent<IScrollHandler>();
            if (scrollHandlers.Length > 1)
                m_IScrollHandlerParent = scrollHandlers[1] as ScrollRect;

            // Get a reference to the RectMask 2D on the Viewport Text Area object.
            if (m_TextViewport != null)
            {
                m_TextViewportRectMask = m_TextViewport.GetComponent<RectMask2D>();

                UpdateMaskRegions();
            }

            // If we have a cached renderer then we had OnDisable called so just restore the material.
            if (m_CachedInputRenderer != null)
                m_CachedInputRenderer.SetMaterial(Graphic.defaultGraphicMaterial, Texture2D.whiteTexture);

            if (m_TextComponent != null)
            {
                m_TextComponent.RegisterDirtyVerticesCallback(MarkGeometryAsDirty);
                m_TextComponent.RegisterDirtyVerticesCallback(UpdateLabel);

                // Cache reference to Vertical Scrollbar RectTransform and add listener.
                if (m_VerticalScrollbar != null)
                {
                    m_VerticalScrollbar.onValueChanged.AddListener(OnScrollbarValueChange);
                }

                UpdateLabel();
            }

            #if UNITY_2019_1_OR_NEWER
            m_TouchKeyboardAllowsInPlaceEditing = TouchScreenKeyboard.isInPlaceEditingAllowed;
            #endif

            // Subscribe to event fired when text object has been regenerated.
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
        }

        protected override void OnDisable()
        {
            // the coroutine will be terminated, so this will ensure it restarts when we are next activated
            m_BlinkCoroutine = null;

            DeactivateInputField();
            if (m_TextComponent != null)
            {
                m_TextComponent.UnregisterDirtyVerticesCallback(MarkGeometryAsDirty);
                m_TextComponent.UnregisterDirtyVerticesCallback(UpdateLabel);

                if (m_VerticalScrollbar != null)
                    m_VerticalScrollbar.onValueChanged.RemoveListener(OnScrollbarValueChange);

            }
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            // Clear needs to be called otherwise sync never happens as the object is disabled.
            if (m_CachedInputRenderer != null)
                m_CachedInputRenderer.Clear();

            if (m_Mesh != null)
                DestroyImmediate(m_Mesh);

            m_Mesh = null;

            // Unsubscribe to event triggered when text object has been regenerated
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);

            base.OnDisable();
        }


        /// <summary>
        /// Method used to update the tracking of the caret position when the text object has been regenerated.
        /// </summary>
        /// <param name="obj"></param>
        private void ON_TEXT_CHANGED(UnityEngine.Object obj)
        {
            bool isThisObject = obj == m_TextComponent;

            if (isThisObject && !m_IsStringPositionDirty)
            {
                if (Application.isPlaying && compositionLength == 0)
                {
                    UpdateCaretPositionFromStringIndex();

                    #if TMP_DEBUG_MODE
                    Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
                    #endif
                }

                if (m_VerticalScrollbar)
                    UpdateScrollbar();
            }
        }


        IEnumerator CaretBlink()
        {
            // Always ensure caret is initially visible since it can otherwise be confusing for a moment.
            m_CaretVisible = true;
            yield return null;

            while ((isFocused || m_SelectionStillActive) && m_CaretBlinkRate > 0)
            {
                // the blink rate is expressed as a frequency
                float blinkPeriod = 1f / m_CaretBlinkRate;

                // the caret should be ON if we are in the first half of the blink period
                bool blinkState = (Time.unscaledTime - m_BlinkStartTime) % blinkPeriod < blinkPeriod / 2;
                if (m_CaretVisible != blinkState)
                {
                    m_CaretVisible = blinkState;
                    if (!hasSelection)
                        MarkGeometryAsDirty();
                }

                // Then wait again.
                yield return null;
            }
            m_BlinkCoroutine = null;
        }

        void SetCaretVisible()
        {
            if (!m_AllowInput)
                return;

            m_CaretVisible = true;
            m_BlinkStartTime = Time.unscaledTime;
            SetCaretActive();
        }

        // SetCaretActive will not set the caret immediately visible - it will wait for the next time to blink.
        // However, it will handle things correctly if the blink speed changed from zero to non-zero or non-zero to zero.
        void SetCaretActive()
        {
            if (!m_AllowInput)
                return;

            if (m_CaretBlinkRate > 0.0f)
            {
                if (m_BlinkCoroutine == null)
                    m_BlinkCoroutine = StartCoroutine(CaretBlink());
            }
            else
            {
                m_CaretVisible = true;
            }
        }

        protected void OnFocus()
        {
            if (m_OnFocusSelectAll)
                SelectAll();
        }

        protected void SelectAll()
        {
            m_isSelectAll = true;
            stringPositionInternal = text.Length;
            stringSelectPositionInternal = 0;
        }

        /// <summary>
        /// Move to the end of the text.
        /// </summary>
        /// <param name="shift"></param>
        public void MoveTextEnd(bool shift)
        {
            if (m_isRichTextEditingAllowed)
            {
                int position = text.Length;

                if (shift)
                {
                    stringSelectPositionInternal = position;
                }
                else
                {
                    stringPositionInternal = position;
                    stringSelectPositionInternal = stringPositionInternal;
                }
            }
            else
            {
                int position = m_TextComponent.textInfo.characterCount - 1;

                if (shift)
                {
                    caretSelectPositionInternal = position;
                    stringSelectPositionInternal = GetStringIndexFromCaretPosition(position);
                }
                else
                {
                    caretPositionInternal = caretSelectPositionInternal = position;
                    stringSelectPositionInternal = stringPositionInternal = GetStringIndexFromCaretPosition(position);
                }
            }

            UpdateLabel();
        }

        /// <summary>
        /// Move to the start of the text.
        /// </summary>
        /// <param name="shift"></param>
        public void MoveTextStart(bool shift)
        {
            if (m_isRichTextEditingAllowed)
            {
                int position = 0;

                if (shift)
                {
                    stringSelectPositionInternal = position;
                }
                else
                {
                    stringPositionInternal = position;
                    stringSelectPositionInternal = stringPositionInternal;
                }
            }
            else
            {
                int position = 0;

                if (shift)
                {
                    caretSelectPositionInternal = position;
                    stringSelectPositionInternal = GetStringIndexFromCaretPosition(position);
                }
                else
                {
                    caretPositionInternal = caretSelectPositionInternal = position;
                    stringSelectPositionInternal = stringPositionInternal = GetStringIndexFromCaretPosition(position);
                }
            }

            UpdateLabel();
        }


        /// <summary>
        /// Move to the end of the current line of text.
        /// </summary>
        /// <param name="shift"></param>
        public void MoveToEndOfLine(bool shift, bool ctrl)
        {
            // Get the line the caret is currently located on.
            int currentLine = m_TextComponent.textInfo.characterInfo[caretPositionInternal].lineNumber;

            // Get the last character of the given line.
            int characterIndex = ctrl == true ? m_TextComponent.textInfo.characterCount - 1 : m_TextComponent.textInfo.lineInfo[currentLine].lastCharacterIndex;

            int position = m_TextComponent.textInfo.characterInfo[characterIndex].index;

            if (shift)
            {
                stringSelectPositionInternal = position;

                caretSelectPositionInternal = characterIndex;
            }
            else
            {
                stringPositionInternal = position;
                stringSelectPositionInternal = stringPositionInternal;

                caretSelectPositionInternal = caretPositionInternal = characterIndex;
            }

            UpdateLabel();
        }

        /// <summary>
        /// Move to the start of the current line of text.
        /// </summary>
        /// <param name="shift"></param>
        public void MoveToStartOfLine(bool shift, bool ctrl)
        {
            // Get the line the caret is currently located on.
            int currentLine = m_TextComponent.textInfo.characterInfo[caretPositionInternal].lineNumber;

            // Get the first character of the given line.
            int characterIndex = ctrl == true ? 0 : m_TextComponent.textInfo.lineInfo[currentLine].firstCharacterIndex;

            int position = 0;
            if (characterIndex > 0)
                position = m_TextComponent.textInfo.characterInfo[characterIndex - 1].index + m_TextComponent.textInfo.characterInfo[characterIndex - 1].stringLength;

            if (shift)
            {
                stringSelectPositionInternal = position;

                caretSelectPositionInternal = characterIndex;
            }
            else
            {
                stringPositionInternal = position;
                stringSelectPositionInternal = stringPositionInternal;

                caretSelectPositionInternal = caretPositionInternal = characterIndex;
            }

            UpdateLabel();
        }


        static string clipboard
        {
            get
            {
                return GUIUtility.systemCopyBuffer;
            }
            set
            {
                GUIUtility.systemCopyBuffer = value;
            }
        }

        private bool InPlaceEditing()
        {
            if (m_TouchKeyboardAllowsInPlaceEditing)
                return true;

            if (isUWP())
                return !TouchScreenKeyboard.isSupported;

            if (TouchScreenKeyboard.isSupported && shouldHideSoftKeyboard)
                return true;

            if (TouchScreenKeyboard.isSupported && shouldHideSoftKeyboard == false && shouldHideMobileInput == false)
                return false;

            return true;
        }

        // In-place editing can change state if a hardware keyboard becomes available or is hidden while the input field is activated.
        private bool InPlaceEditingChanged()
        {
                return !s_IsQuestDevice && m_TouchKeyboardAllowsInPlaceEditing != TouchScreenKeyboard.isInPlaceEditingAllowed;
        }

        // Returns true if the TouchScreenKeyboard should be used. On Android and Chrome OS, we only want to use the
        // TouchScreenKeyboard if in-place editing is not allowed (i.e. when we do not have a hardware keyboard available).
        private bool TouchScreenKeyboardShouldBeUsed()
        {
            RuntimePlatform platform = Application.platform;
            switch (platform)
            {
                case RuntimePlatform.Android:
                case RuntimePlatform.WebGLPlayer:
                    if (s_IsQuestDevice)
                        return TouchScreenKeyboard.isSupported;

                    return !TouchScreenKeyboard.isInPlaceEditingAllowed;
                default:
                    return TouchScreenKeyboard.isSupported;
            }
        }

        void UpdateKeyboardStringPosition()
        {
            // On iOS/tvOS we only update SoftKeyboard selection when we know that it might have changed by touch/pointer interactions with InputField
            // Setting the TouchScreenKeyboard selection here instead of LateUpdate so that we wouldn't override
            // TouchScreenKeyboard selection when it's changed with cmd+a/ctrl+a/arrow/etc. in the TouchScreenKeyboard
            // This is only applicable for iOS/tvOS as we have instance of TouchScreenKeyboard even when external keyboard is connected
            if (m_HideMobileInput && m_SoftKeyboard != null && m_SoftKeyboard.canSetSelection &&
                (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS))
            {
                var selectionStart = Mathf.Min(stringSelectPositionInternal, stringPositionInternal);
                var selectionLength = Mathf.Abs(stringSelectPositionInternal - stringPositionInternal);
                m_SoftKeyboard.selection = new RangeInt(selectionStart, selectionLength);
            }
        }

        void UpdateStringPositionFromKeyboard()
        {
            // TODO: Might want to add null check here.
            var selectionRange = m_SoftKeyboard.selection;

            //if (selectionRange.start == 0 && selectionRange.length == 0)
            //    return;

            var selectionStart = selectionRange.start;
            var selectionEnd = selectionRange.end;

            var stringPositionChanged = false;

            if (stringPositionInternal != selectionStart)
            {
                stringPositionChanged = true;
                stringPositionInternal = selectionStart;

                caretPositionInternal = GetCaretPositionFromStringIndex(stringPositionInternal);
            }

            if (stringSelectPositionInternal != selectionEnd)
            {
                stringSelectPositionInternal = selectionEnd;
                stringPositionChanged = true;

                caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
            }

            if (stringPositionChanged)
            {
                m_BlinkStartTime = Time.unscaledTime;

                UpdateLabel();
            }
        }

        /// <summary>
        /// Update the text based on input.
        /// </summary>
        // TODO: Make LateUpdate a coroutine instead. Allows us to control the update to only be when the field is active.
        protected virtual void LateUpdate()
        {
            // Only activate if we are not already activated.
            if (m_ShouldActivateNextUpdate)
            {
                if (!isFocused)
                {
                    ActivateInputFieldInternal();
                    m_ShouldActivateNextUpdate = false;
                    return;
                }

                // Reset as we are already activated.
                m_ShouldActivateNextUpdate = false;
            }

            // If the device's state changed in a way that affects whether we should use a touchscreen keyboard or not,
            // then deactivate the input field.
            if (isFocused && InPlaceEditingChanged())
                DeactivateInputField();

            // Handle double click to reset / deselect Input Field when ResetOnActivation is false.
            if (!isFocused && m_SelectionStillActive)
            {
                GameObject selectedObject = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;

                if (selectedObject == null && m_ResetOnDeActivation)
                {
                    ReleaseSelection();
                    return;
                }

                if (selectedObject != null && selectedObject != this.gameObject)
                {
                    if (selectedObject == m_PreviouslySelectedObject)
                        return;

                    m_PreviouslySelectedObject = selectedObject;

                    // Special handling for Vertical Scrollbar
                    if (m_VerticalScrollbar && selectedObject == m_VerticalScrollbar.gameObject)
                    {
                        // Do not release selection
                        return;
                    }

                    // Release selection for all objects when ResetOnDeActivation is true
                    if (m_ResetOnDeActivation)
                    {
                        ReleaseSelection();
                        return;
                    }

                    // Release current selection of selected object is another Input Field
                    if (m_KeepTextSelectionVisible == false && selectedObject.GetComponent<TMP_InputField>() != null)
                        ReleaseSelection();

                    return;
                }

                #if ENABLE_INPUT_SYSTEM
                if (m_ProcessingEvent != null && m_ProcessingEvent.rawType == EventType.MouseDown && m_ProcessingEvent.button == 0)
                {
                    // Check for Double Click
                    bool isDoubleClick = false;
                    float timeStamp = Time.unscaledTime;

                    if (m_KeyDownStartTime + m_DoubleClickDelay > timeStamp)
                        isDoubleClick = true;

                    m_KeyDownStartTime = timeStamp;

                    if (isDoubleClick)
                    {
                        //m_StringPosition = m_StringSelectPosition = 0;
                        //m_CaretPosition = m_CaretSelectPosition = 0;
                        //m_TextComponent.rectTransform.localPosition = m_DefaultTransformPosition;

                        //if (caretRectTrans != null)
                        //    caretRectTrans.localPosition = Vector3.zero;

                        ReleaseSelection();

                        return;
                    }
                }
                #else
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    // Check for Double Click
                    bool isDoubleClick = false;
                    float timeStamp = Time.unscaledTime;

                    if (m_KeyDownStartTime + m_DoubleClickDelay > timeStamp)
                        isDoubleClick = true;

                    m_KeyDownStartTime = timeStamp;

                    if (isDoubleClick)
                    {
                        //m_StringPosition = m_StringSelectPosition = 0;
                        //m_CaretPosition = m_CaretSelectPosition = 0;
                        //m_TextComponent.rectTransform.localPosition = m_DefaultTransformPosition;

                        //if (caretRectTrans != null)
                        //    caretRectTrans.localPosition = Vector3.zero;

                        ReleaseSelection();

                        return;
                    }
                }
                #endif
            }

            UpdateMaskRegions();

            if (InPlaceEditing() && isKeyboardUsingEvents() || !isFocused)
            {
                return;
            }

            AssignPositioningIfNeeded();

            if (m_SoftKeyboard == null || m_SoftKeyboard.status != TouchScreenKeyboard.Status.Visible)
            {
                if (m_SoftKeyboard != null)
                {
                    if (!m_ReadOnly)
                        text = m_SoftKeyboard.text;

                    TouchScreenKeyboard.Status status = m_SoftKeyboard.status;

                    // Special handling for UWP - Hololens which does not support Canceled status
                    if (m_LastKeyCode != KeyCode.Return && status == TouchScreenKeyboard.Status.Done && isUWP())
					{
                        status = TouchScreenKeyboard.Status.Canceled;
                        // The HoloLen's X button will not be acting as an ESC Key (TMBP-98)
						m_IsKeyboardBeingClosedInHoloLens = true;
					}

                    switch (status)
                    {
                        case TouchScreenKeyboard.Status.LostFocus:
                            SendTouchScreenKeyboardStatusChanged();
                            break;
                        case TouchScreenKeyboard.Status.Canceled:
                            m_ReleaseSelection = true;
                            m_WasCanceled = true;
                            SendTouchScreenKeyboardStatusChanged();
                            break;
                        case TouchScreenKeyboard.Status.Done:
                            m_ReleaseSelection = true;
                            SendTouchScreenKeyboardStatusChanged();
                            OnSubmit(null);
                            break;
                    }
                }

                OnDeselect(null);
                return;
            }

            string val = m_SoftKeyboard.text;

            if (m_Text != val)
            {
                if (m_ReadOnly)
                {
                    m_SoftKeyboard.text = m_Text;
                }
                else
                {
                    m_Text = "";

                    for (int i = 0; i < val.Length; ++i)
                    {
                        char c = val[i];
						bool hasValidateUpdatedText = false;

                        if (c == '\r' || c == 3)
                            c = '\n';

                        if (onValidateInput != null)
                            c = onValidateInput(m_Text, m_Text.Length, c);
                        else if (characterValidation != CharacterValidation.None)
						{
							string textBeforeValidate = m_Text;
                            c = Validate(m_Text, m_Text.Length, c);
                            hasValidateUpdatedText = textBeforeValidate != m_Text;
						}

                        if (lineType != LineType.MultiLineNewline && c == '\n')
                        {
                            UpdateLabel();

                            OnSubmit(null);
                            OnDeselect(null);
                            return;
                        }

                        // In the case of a Custom Validator, the user is expected to modify the m_Text where as such we do not append c.
                        // However we will append c if the user did not modify the m_Text (UUM-42147)
                        if (c != 0 && (characterValidation != CharacterValidation.CustomValidator || !hasValidateUpdatedText))
                            m_Text += c;
                    }

                    if (characterLimit > 0 && m_Text.Length > characterLimit)
                        m_Text = m_Text.Substring(0, characterLimit);

                    UpdateStringPositionFromKeyboard();

                    // Set keyboard text before updating label, as we might have changed it with validation
                    // and update label will take the old value from keyboard if we don't change it here
                    if (m_Text != val)
                        m_SoftKeyboard.text = m_Text;

                    SendOnValueChangedAndUpdateLabel();
                }
            }
            // On iOS/tvOS we always have TouchScreenKeyboard instance even when using external keyboard
            // so we keep track of the caret position there
            else if (m_HideMobileInput && m_SoftKeyboard != null && m_SoftKeyboard.canSetSelection &&
                     Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.tvOS)
            {
                var selectionStart = Mathf.Min(stringSelectPositionInternal, stringPositionInternal);
                var selectionLength = Mathf.Abs(stringSelectPositionInternal - stringPositionInternal);
                m_SoftKeyboard.selection = new RangeInt(selectionStart, selectionLength);
            }
            else if (m_HideMobileInput && Application.platform == RuntimePlatform.Android ||
                     m_SoftKeyboard.canSetSelection && (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS))
            {
                UpdateStringPositionFromKeyboard();
            }

            //else if (m_HideMobileInput) // m_Keyboard.canSetSelection
            //{
            //    int length = stringPositionInternal < stringSelectPositionInternal ? stringSelectPositionInternal - stringPositionInternal : stringPositionInternal - stringSelectPositionInternal;
            //    m_SoftKeyboard.selection = new RangeInt(stringPositionInternal < stringSelectPositionInternal ? stringPositionInternal : stringSelectPositionInternal, length);
            //}
            //else if (!m_HideMobileInput) // m_Keyboard.canGetSelection)
            //{
            //    UpdateStringPositionFromKeyboard();
            //}

            if (m_SoftKeyboard != null && m_SoftKeyboard.status != TouchScreenKeyboard.Status.Visible)
            {
                if (m_SoftKeyboard.status == TouchScreenKeyboard.Status.Canceled)
                    m_WasCanceled = true;

                OnDeselect(null);
            }
        }

        private bool MayDrag(PointerEventData eventData)
        {
            return IsActive() &&
                   IsInteractable() &&
                   eventData.button == PointerEventData.InputButton.Left &&
                   m_TextComponent != null &&
                   (m_SoftKeyboard == null || shouldHideSoftKeyboard || shouldHideMobileInput);
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            m_UpdateDrag = true;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            CaretPosition insertionSide;

            int insertionIndex = TMP_TextUtilities.GetCursorIndexFromPosition(m_TextComponent, eventData.position, eventData.pressEventCamera, out insertionSide);

            if (m_isRichTextEditingAllowed)
            {
                if (insertionSide == CaretPosition.Left)
                {
                    stringSelectPositionInternal = m_TextComponent.textInfo.characterInfo[insertionIndex].index;
                }
                else if (insertionSide == CaretPosition.Right)
                {
                    stringSelectPositionInternal = m_TextComponent.textInfo.characterInfo[insertionIndex].index + m_TextComponent.textInfo.characterInfo[insertionIndex].stringLength;
                }
            }
            else
            {
                if (insertionSide == CaretPosition.Left)
                {
                    stringSelectPositionInternal = insertionIndex == 0
                        ? m_TextComponent.textInfo.characterInfo[0].index
                        : m_TextComponent.textInfo.characterInfo[insertionIndex - 1].index + m_TextComponent.textInfo.characterInfo[insertionIndex - 1].stringLength;
                }
                else if (insertionSide == CaretPosition.Right)
                {
                    stringSelectPositionInternal = m_TextComponent.textInfo.characterInfo[insertionIndex].index + m_TextComponent.textInfo.characterInfo[insertionIndex].stringLength;
                }
            }

            caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);

            MarkGeometryAsDirty();

            m_DragPositionOutOfBounds = !RectTransformUtility.RectangleContainsScreenPoint(textViewport, eventData.position, eventData.pressEventCamera);
            if (m_DragPositionOutOfBounds && m_DragCoroutine == null)
                m_DragCoroutine = StartCoroutine(MouseDragOutsideRect(eventData));

            UpdateKeyboardStringPosition();
            eventData.Use();

            #if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
            #endif
        }

        IEnumerator MouseDragOutsideRect(PointerEventData eventData)
        {
            while (m_UpdateDrag && m_DragPositionOutOfBounds)
            {
                Vector2 localMousePos;

                RectTransformUtility.ScreenPointToLocalPointInRectangle(textViewport, eventData.position, eventData.pressEventCamera, out localMousePos);

                Rect rect = textViewport.rect;

                if (multiLine)
                {
                    if (localMousePos.y > rect.yMax)
                        MoveUp(true, true);
                    else if (localMousePos.y < rect.yMin)
                        MoveDown(true, true);
                }
                else
                {
                    if (localMousePos.x < rect.xMin)
                        MoveLeft(true, false);
                    else if (localMousePos.x > rect.xMax)
                        MoveRight(true, false);
                }

                UpdateLabel();

                float delay = multiLine ? kVScrollSpeed : kHScrollSpeed;

                if (m_WaitForSecondsRealtime == null)
                    m_WaitForSecondsRealtime = new WaitForSecondsRealtime(delay);
                else
                    m_WaitForSecondsRealtime.waitTime = delay;

                yield return m_WaitForSecondsRealtime;
            }
            m_DragCoroutine = null;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            m_UpdateDrag = false;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            EventSystem.current.SetSelectedGameObject(gameObject, eventData);

            bool hadFocusBefore = m_AllowInput;
            base.OnPointerDown(eventData);

            if (InPlaceEditing() == false)
            {
                if (m_SoftKeyboard == null || !m_SoftKeyboard.active)
                {
                    OnSelect(eventData);
                    return;
                }
            }

            #if ENABLE_INPUT_SYSTEM
            Event.PopEvent(m_ProcessingEvent);
            bool shift = m_ProcessingEvent != null && (m_ProcessingEvent.modifiers & EventModifiers.Shift) != 0;
            #else
            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            #endif

            // Check for Double Click
            bool isDoubleClick = false;
            float timeStamp = Time.unscaledTime;

            if (m_PointerDownClickStartTime + m_DoubleClickDelay > timeStamp)
                isDoubleClick = true;

            m_PointerDownClickStartTime = timeStamp;

            // Only set caret position if we didn't just get focus now.
            // Otherwise it will overwrite the select all on focus.
            if (hadFocusBefore || !m_OnFocusSelectAll)
            {
                CaretPosition insertionSide;

                int insertionIndex = TMP_TextUtilities.GetCursorIndexFromPosition(m_TextComponent, eventData.position, eventData.pressEventCamera, out insertionSide);

                if (shift)
                {
                    if (m_isRichTextEditingAllowed)
                    {
                        if (insertionSide == CaretPosition.Left)
                        {
                            stringSelectPositionInternal = m_TextComponent.textInfo.characterInfo[insertionIndex].index;
                        }
                        else if (insertionSide == CaretPosition.Right)
                        {
                            stringSelectPositionInternal = m_TextComponent.textInfo.characterInfo[insertionIndex].index + m_TextComponent.textInfo.characterInfo[insertionIndex].stringLength;
                        }
                    }
                    else
                    {
                        if (insertionSide == CaretPosition.Left)
                        {
                            stringSelectPositionInternal = insertionIndex == 0
                                ? m_TextComponent.textInfo.characterInfo[0].index
                                : m_TextComponent.textInfo.characterInfo[insertionIndex - 1].index + m_TextComponent.textInfo.characterInfo[insertionIndex - 1].stringLength;
                        }
                        else if (insertionSide == CaretPosition.Right)
                        {
                            stringSelectPositionInternal = m_TextComponent.textInfo.characterInfo[insertionIndex].index + m_TextComponent.textInfo.characterInfo[insertionIndex].stringLength;
                        }
                    }
                }
                else
                {
                    if (m_isRichTextEditingAllowed)
                    {
                        if (insertionSide == CaretPosition.Left)
                        {
                            stringPositionInternal = stringSelectPositionInternal = m_TextComponent.textInfo.characterInfo[insertionIndex].index;
                        }
                        else if (insertionSide == CaretPosition.Right)
                        {
                            stringPositionInternal = stringSelectPositionInternal = m_TextComponent.textInfo.characterInfo[insertionIndex].index + m_TextComponent.textInfo.characterInfo[insertionIndex].stringLength;
                        }
                    }
                    else
                    {
                        if (insertionSide == CaretPosition.Left)
                        {
                            stringPositionInternal = stringSelectPositionInternal = insertionIndex == 0
                                ? m_TextComponent.textInfo.characterInfo[0].index
                                : m_TextComponent.textInfo.characterInfo[insertionIndex - 1].index + m_TextComponent.textInfo.characterInfo[insertionIndex - 1].stringLength;
                        }
                        else if (insertionSide == CaretPosition.Right)
                        {
                            stringPositionInternal = stringSelectPositionInternal = m_TextComponent.textInfo.characterInfo[insertionIndex].index + m_TextComponent.textInfo.characterInfo[insertionIndex].stringLength;
                        }
                    }
                }


                if (isDoubleClick)
                {
                    int wordIndex = TMP_TextUtilities.FindIntersectingWord(m_TextComponent, eventData.position, eventData.pressEventCamera);

                    if (wordIndex != -1)
                    {
                        // TODO: Should behavior be different if rich text editing is enabled or not?

                        // Select current word
                        caretPositionInternal = m_TextComponent.textInfo.wordInfo[wordIndex].firstCharacterIndex;
                        caretSelectPositionInternal = m_TextComponent.textInfo.wordInfo[wordIndex].lastCharacterIndex + 1;

                        stringPositionInternal = m_TextComponent.textInfo.characterInfo[caretPositionInternal].index;
                        stringSelectPositionInternal = m_TextComponent.textInfo.characterInfo[caretSelectPositionInternal - 1].index + m_TextComponent.textInfo.characterInfo[caretSelectPositionInternal - 1].stringLength;
                    }
                    else
                    {
                        // Select current character
                        caretPositionInternal = insertionIndex;
                        caretSelectPositionInternal = caretPositionInternal + 1;

                        stringPositionInternal = m_TextComponent.textInfo.characterInfo[insertionIndex].index;
                        stringSelectPositionInternal = stringPositionInternal + m_TextComponent.textInfo.characterInfo[insertionIndex].stringLength;
                    }
                }
                else
                {
                    caretPositionInternal = caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringPositionInternal);
                }

                m_isSelectAll = false;
            }

            UpdateLabel();
            UpdateKeyboardStringPosition();
            eventData.Use();

            #if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
            #endif
        }

        protected enum EditState
        {
            Continue,
            Finish
        }

        protected EditState KeyPressed(Event evt)
        {
            var currentEventModifiers = evt.modifiers;
            bool ctrl = m_IsApplePlatform ? (currentEventModifiers & EventModifiers.Command) != 0 : (currentEventModifiers & EventModifiers.Control) != 0;
            bool shift = (currentEventModifiers & EventModifiers.Shift) != 0;
            bool alt = (currentEventModifiers & EventModifiers.Alt) != 0;
            bool ctrlOnly = ctrl && !alt && !shift;
            m_LastKeyCode = evt.keyCode;

            switch (evt.keyCode)
            {
                case KeyCode.Backspace:
                    {
                        Backspace();
                        return EditState.Continue;
                    }

                case KeyCode.Delete:
                    {
                        DeleteKey();
                        return EditState.Continue;
                    }

                case KeyCode.Home:
                    {
                        MoveToStartOfLine(shift, ctrl);
                        return EditState.Continue;
                    }

                case KeyCode.End:
                    {
                        MoveToEndOfLine(shift, ctrl);
                        return EditState.Continue;
                    }

                // Select All
                case KeyCode.A:
                    {
                        if (ctrlOnly)
                        {
                            SelectAll();
                            return EditState.Continue;
                        }
                        break;
                    }

                // Copy
                case KeyCode.C:
                    {
                        if (ctrlOnly)
                        {
                            if (inputType != InputType.Password)
                                clipboard = GetSelectedString();
                            else
                                clipboard = "";
                            return EditState.Continue;
                        }
                        break;
                    }

                // Paste
                case KeyCode.V:
                    {
                        if (ctrlOnly)
                        {
                            Append(clipboard);
                            return EditState.Continue;
                        }
                        break;
                    }

                // Cut
                case KeyCode.X:
                    {
                        if (ctrlOnly)
                        {
                            if (inputType != InputType.Password)
                                clipboard = GetSelectedString();
                            else
                                clipboard = "";
                            Delete();
                            UpdateTouchKeyboardFromEditChanges();
                            SendOnValueChangedAndUpdateLabel();
                            return EditState.Continue;
                        }
                        break;
                    }

                case KeyCode.LeftArrow:
                    {
                        MoveLeft(shift, ctrl);
                        return EditState.Continue;
                    }

                case KeyCode.RightArrow:
                    {
                        MoveRight(shift, ctrl);
                        return EditState.Continue;
                    }

                case KeyCode.UpArrow:
                    {
                        MoveUp(shift);
                        return EditState.Continue;
                    }

                case KeyCode.DownArrow:
                    {
                        MoveDown(shift);
                        return EditState.Continue;
                    }

                case KeyCode.PageUp:
                    {
                        MovePageUp(shift);
                        return EditState.Continue;
                    }

                case KeyCode.PageDown:
                    {
                        MovePageDown(shift);
                        return EditState.Continue;
                    }

                // Submit
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    {
                        if (lineType != LineType.MultiLineNewline)
                        {
                            m_ReleaseSelection = true;
                            return EditState.Finish;
                        }
                        else
                        {
                            TMP_TextInfo textInfo = m_TextComponent.textInfo;

                            if (m_LineLimit > 0 && textInfo != null && textInfo.lineCount >= m_LineLimit)
                            {
                                m_ReleaseSelection = true;
                                return EditState.Finish;
                            }
                        }
                        break;
                    }

                case KeyCode.Escape:
                    {
                        m_ReleaseSelection = true;
                        m_WasCanceled = true;
                        return EditState.Finish;
                    }
            }

            char c = evt.character;

            // Don't allow return chars or tabulator key to be entered into single line fields.
            if (!multiLine && (c == '\t' || c == '\r' || c == '\n'))
                return EditState.Continue;

            // Convert carriage return and end-of-text characters to newline.
            if (c == '\r' || c == 3)
                c = '\n';

            // Convert Shift Enter to Vertical tab
            if (shift && c == '\n')
                c = '\v';

            if (IsValidChar(c))
            {
                Append(c);
            }

            if (c == 0)
            {
                if (compositionLength > 0)
                {
                    UpdateLabel();
                }
            }
            return EditState.Continue;
        }

        protected virtual bool IsValidChar(char c)
        {
            // Delete key on mac
            if (c == 127)
                return false;

            // Accept newline and tab
            if (c == '\t' || c == '\n')
                return true;

            // Control characters (not printable)
            if (c < 32)
                return false;

            return true;

            // With the addition of Dynamic support, I think this will best be handled by the text component.
            //return m_TextComponent.font.HasCharacter(c, true);
        }

        /// <summary>
        /// Handle the specified event.
        /// </summary>
        private Event m_ProcessingEvent = new Event();

        public void ProcessEvent(Event e)
        {
            KeyPressed(e);
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnUpdateSelected(BaseEventData eventData)
        {
            if (!isFocused)
                return;

            bool consumedEvent = false;
            EditState editState = EditState.Continue;

            while (Event.PopEvent(m_ProcessingEvent))
            {
                //Debug.Log("Event: " + m_ProcessingEvent.ToString() + "  IsCompositionActive= " + m_IsCompositionActive + "  Composition Length: " + compositionLength);

                EventType eventType = m_ProcessingEvent.rawType;

                if (eventType == EventType.KeyUp)
                    continue;

                if (eventType == EventType.KeyDown)
                {
                    consumedEvent = true;

                    // Special handling on OSX which produces more events which need to be suppressed.
                    if (m_IsCompositionActive && compositionLength == 0)
                    {
                        // Suppress other events related to navigation or termination of composition sequence.
                        if (m_ProcessingEvent.character == 0 && m_ProcessingEvent.modifiers == EventModifiers.None)
                            continue;
                    }

                    editState = KeyPressed(m_ProcessingEvent);
                    if (editState == EditState.Finish)
                    {
                        if (!m_WasCanceled)
                            SendOnSubmit();

                        DeactivateInputField();
                        break;
                    }

                    m_IsTextComponentUpdateRequired = true;
                    UpdateLabel();

                    continue;
                }

                switch (eventType)
                {
                    case EventType.ValidateCommand:
                    case EventType.ExecuteCommand:
                        switch (m_ProcessingEvent.commandName)
                        {
                            case "SelectAll":
                                SelectAll();
                                consumedEvent = true;
                                break;
                        }
                        break;
                }
            }

            // We must also consume events when IME is active to prevent them from being passed to the text field. // UUM-100552
            if (consumedEvent || (m_IsCompositionActive && compositionLength > 0))
            {
                UpdateLabel();
                eventData.Use();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnScroll(PointerEventData eventData)
        {
            // Return if Single Line
            if (m_LineType == LineType.SingleLine)
            {
                if (m_IScrollHandlerParent != null)
                    m_IScrollHandlerParent.OnScroll(eventData);

                return;
            }

            if (m_TextComponent.preferredHeight < m_TextViewport.rect.height)
                return;

            float scrollDirection = -eventData.scrollDelta.y;

            // Determine the current scroll position of the text within the viewport
            m_ScrollPosition = GetScrollPositionRelativeToViewport();

            m_ScrollPosition += (1f / m_TextComponent.textInfo.lineCount) * scrollDirection * m_ScrollSensitivity;

            m_ScrollPosition = Mathf.Clamp01(m_ScrollPosition);

            AdjustTextPositionRelativeToViewport(m_ScrollPosition);

            if (m_VerticalScrollbar)
            {
                m_VerticalScrollbar.value = m_ScrollPosition;
            }

            //Debug.Log(GetInstanceID() + "- Scroll Position:" + m_ScrollPosition);
        }

        float GetScrollPositionRelativeToViewport()
        {
            // Determine the current scroll position of the text within the viewport
            Rect viewportRect = m_TextViewport.rect;

            float scrollPosition = (m_TextComponent.textInfo.lineInfo[0].ascender + m_TextComponent.margin.y + m_TextComponent.margin.w - viewportRect.yMax + m_TextComponent.rectTransform.anchoredPosition.y) / ( m_TextComponent.preferredHeight - viewportRect.height);

            scrollPosition = (int)((scrollPosition * 1000) + 0.5f) / 1000.0f;

            return scrollPosition;
        }

        private string GetSelectedString()
        {
            if (!hasSelection)
                return "";

            int startPos = stringPositionInternal;
            int endPos = stringSelectPositionInternal;

            // Ensure pos is always less then selPos to make the code simpler
            if (startPos > endPos)
            {
                int temp = startPos;
                startPos = endPos;
                endPos = temp;
            }

            //for (int i = m_CaretPosition; i < m_CaretSelectPosition; i++)
            //{
            //    Debug.Log("Character [" + m_TextComponent.textInfo.characterInfo[i].character + "] using Style [" + m_TextComponent.textInfo.characterInfo[i].style + "] has been selected.");
            //}


            return text.Substring(startPos, endPos - startPos);
        }

        private int FindNextWordBegin()
        {
            if (stringSelectPositionInternal + 1 >= text.Length)
                return text.Length;

            int spaceLoc = text.IndexOfAny(kSeparators, stringSelectPositionInternal + 1);

            if (spaceLoc == -1)
                spaceLoc = text.Length;
            else
                spaceLoc++;

            return spaceLoc;
        }

        private void MoveRight(bool shift, bool ctrl)
        {
            if (hasSelection && !shift)
            {
                // By convention, if we have a selection and move right without holding shift,
                // we just place the cursor at the end.
                stringPositionInternal = stringSelectPositionInternal = Mathf.Max(stringPositionInternal, stringSelectPositionInternal);
                caretPositionInternal = caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);

                #if TMP_DEBUG_MODE
                    Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
                #endif
                return;
            }

            int position;
            if (ctrl)
                position = FindNextWordBegin();
            else
            {
                if (m_isRichTextEditingAllowed)
                {
                    // Special handling for Surrogate pairs and Diacritical marks.
                    if (stringSelectPositionInternal < text.Length && char.IsHighSurrogate(text[stringSelectPositionInternal]))
                        position = stringSelectPositionInternal + 2;
                    else
                        position = stringSelectPositionInternal + 1;
                }
                else
                {
                    // Special handling for <CR><LF>
                    if (m_TextComponent.textInfo.characterInfo[caretSelectPositionInternal].character == '\r' && m_TextComponent.textInfo.characterInfo[caretSelectPositionInternal + 1].character == '\n')
                        position = m_TextComponent.textInfo.characterInfo[caretSelectPositionInternal + 1].index + m_TextComponent.textInfo.characterInfo[caretSelectPositionInternal + 1].stringLength;
                    else
                        position = m_TextComponent.textInfo.characterInfo[caretSelectPositionInternal].index + m_TextComponent.textInfo.characterInfo[caretSelectPositionInternal].stringLength;
                }

            }

            if (shift)
            {
                stringSelectPositionInternal = position;
                caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
            }
            else
            {
                stringSelectPositionInternal = stringPositionInternal = position;

                // Only increase caret position as we cross character boundary.
                if (stringPositionInternal >= m_TextComponent.textInfo.characterInfo[caretPositionInternal].index + m_TextComponent.textInfo.characterInfo[caretPositionInternal].stringLength)
                    caretSelectPositionInternal = caretPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
            }

            #if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + "  Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + "  String Select Position: " + stringSelectPositionInternal);
            #endif
        }

        private int FindPrevWordBegin()
        {
            if (stringSelectPositionInternal - 2 < 0)
                return 0;

            int spaceLoc = text.LastIndexOfAny(kSeparators, stringSelectPositionInternal - 2);

            if (spaceLoc == -1)
                spaceLoc = 0;
            else
                spaceLoc++;

            return spaceLoc;
        }

        private void MoveLeft(bool shift, bool ctrl)
        {
            if (hasSelection && !shift)
            {
                // By convention, if we have a selection and move left without holding shift,
                // we just place the cursor at the start.
                stringPositionInternal = stringSelectPositionInternal = Mathf.Min(stringPositionInternal, stringSelectPositionInternal);
                caretPositionInternal = caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);

                #if TMP_DEBUG_MODE
                    Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
                #endif
                return;
            }

            int position;
            if (ctrl)
                position = FindPrevWordBegin();
            else
            {
                if (m_isRichTextEditingAllowed)
                {
                    // Special handling for Surrogate pairs and Diacritical marks.
                    if (stringSelectPositionInternal > 0 && char.IsLowSurrogate(text[stringSelectPositionInternal - 1]))
                        position = stringSelectPositionInternal - 2;
                    else
                        position =  stringSelectPositionInternal - 1;
                }
                else
                {
                    position = caretSelectPositionInternal < 1
                        ? m_TextComponent.textInfo.characterInfo[0].index
                        : m_TextComponent.textInfo.characterInfo[caretSelectPositionInternal - 1].index;

                    // Special handling for <CR><LF>
                    if (position > 0 && m_TextComponent.textInfo.characterInfo[caretSelectPositionInternal - 1].character == '\n' && m_TextComponent.textInfo.characterInfo[caretSelectPositionInternal - 2].character == '\r')
                        position = m_TextComponent.textInfo.characterInfo[caretSelectPositionInternal - 2].index;
                }
            }

            if (shift)
            {
                stringSelectPositionInternal = position;
                caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
            }
            else
            {
                stringSelectPositionInternal = stringPositionInternal = position;

                // Only decrease caret position as we cross character boundary.
                if (caretPositionInternal > 0 && stringPositionInternal <= m_TextComponent.textInfo.characterInfo[caretPositionInternal - 1].index)
                    caretSelectPositionInternal = caretPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
            }

            #if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + "  Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + "  String Select Position: " + stringSelectPositionInternal);
            #endif
        }


        private int LineUpCharacterPosition(int originalPos, bool goToFirstChar)
        {
            if (originalPos >= m_TextComponent.textInfo.characterCount)
                originalPos -= 1;

            TMP_CharacterInfo originChar = m_TextComponent.textInfo.characterInfo[originalPos];
            int originLine = originChar.lineNumber;

            // We are on the first line return first character
            if (originLine - 1 < 0)
                return goToFirstChar ? 0 : originalPos;

            int endCharIdx = m_TextComponent.textInfo.lineInfo[originLine].firstCharacterIndex - 1;

            int closest = -1;
            float distance = TMP_Math.FLOAT_MAX;
            float range = 0;

            for (int i = m_TextComponent.textInfo.lineInfo[originLine - 1].firstCharacterIndex; i < endCharIdx; ++i)
            {
                TMP_CharacterInfo currentChar = m_TextComponent.textInfo.characterInfo[i];

                float d = originChar.origin - currentChar.origin;
                float r = d / (currentChar.xAdvance - currentChar.origin);

                if (r >= 0 && r <= 1)
                {
                    if (r < 0.5f)
                        return i;
                    else
                        return i + 1;
                }

                d = Mathf.Abs(d);

                if (d < distance)
                {
                    closest = i;
                    distance = d;
                    range = r;
                }
            }

            if (closest == -1) return endCharIdx;

            //Debug.Log("Returning nearest character with Range = " + range);

            if (range < 0.5f)
                return closest;
            else
                return closest + 1;
        }


        private int LineDownCharacterPosition(int originalPos, bool goToLastChar)
        {
            if (originalPos >= m_TextComponent.textInfo.characterCount)
                return m_TextComponent.textInfo.characterCount - 1; // text.Length;

            TMP_CharacterInfo originChar = m_TextComponent.textInfo.characterInfo[originalPos];
            int originLine = originChar.lineNumber;

            //// We are on the last line return last character
            if (originLine + 1 >= m_TextComponent.textInfo.lineCount)
                return goToLastChar ? m_TextComponent.textInfo.characterCount - 1 : originalPos;

            // Need to determine end line for next line.
            int endCharIdx = m_TextComponent.textInfo.lineInfo[originLine + 1].lastCharacterIndex;

            int closest = -1;
            float distance = TMP_Math.FLOAT_MAX;
            float range = 0;

            for (int i = m_TextComponent.textInfo.lineInfo[originLine + 1].firstCharacterIndex; i < endCharIdx; ++i)
            {
                TMP_CharacterInfo currentChar = m_TextComponent.textInfo.characterInfo[i];

                float d = originChar.origin - currentChar.origin;
                float r = d / (currentChar.xAdvance - currentChar.origin);

                if (r >= 0 && r <= 1)
                {
                    if (r < 0.5f)
                        return i;
                    else
                        return i + 1;
                }

                d = Mathf.Abs(d);

                if (d < distance)
                {
                    closest = i;
                    distance = d;
                    range = r;
                }
            }

            if (closest == -1) return endCharIdx;

            //Debug.Log("Returning nearest character with Range = " + range);

            if (range < 0.5f)
                return closest;
            else
                return closest + 1;
        }


         private int PageUpCharacterPosition(int originalPos, bool goToFirstChar)
        {
            if (originalPos >= m_TextComponent.textInfo.characterCount)
                originalPos -= 1;

            TMP_CharacterInfo originChar = m_TextComponent.textInfo.characterInfo[originalPos];
            int originLine = originChar.lineNumber;

            // We are on the first line return first character
            if (originLine - 1 < 0)
                return goToFirstChar ? 0 : originalPos;

            float viewportHeight = m_TextViewport.rect.height;

            int newLine = originLine - 1;
            // Iterate through each subsequent line to find the first baseline that is not visible in the viewport.
            for (; newLine > 0; newLine--)
            {
                if (m_TextComponent.textInfo.lineInfo[newLine].baseline > m_TextComponent.textInfo.lineInfo[originLine].baseline + viewportHeight)
                    break;
            }

            int endCharIdx = m_TextComponent.textInfo.lineInfo[newLine].lastCharacterIndex;

            int closest = -1;
            float distance = TMP_Math.FLOAT_MAX;
            float range = 0;

            for (int i = m_TextComponent.textInfo.lineInfo[newLine].firstCharacterIndex; i < endCharIdx; ++i)
            {
                TMP_CharacterInfo currentChar = m_TextComponent.textInfo.characterInfo[i];

                float d = originChar.origin - currentChar.origin;
                float r = d / (currentChar.xAdvance - currentChar.origin);

                if (r >= 0 && r <= 1)
                {
                    if (r < 0.5f)
                        return i;
                    else
                        return i + 1;
                }

                d = Mathf.Abs(d);

                if (d < distance)
                {
                    closest = i;
                    distance = d;
                    range = r;
                }
            }

            if (closest == -1) return endCharIdx;

            //Debug.Log("Returning nearest character with Range = " + range);

            if (range < 0.5f)
                return closest;
            else
                return closest + 1;
        }


         private int PageDownCharacterPosition(int originalPos, bool goToLastChar)
        {
            if (originalPos >= m_TextComponent.textInfo.characterCount)
                return m_TextComponent.textInfo.characterCount - 1;

            TMP_CharacterInfo originChar = m_TextComponent.textInfo.characterInfo[originalPos];
            int originLine = originChar.lineNumber;

            // We are on the last line return last character
            if (originLine + 1 >= m_TextComponent.textInfo.lineCount)
                return goToLastChar ? m_TextComponent.textInfo.characterCount - 1 : originalPos;

            float viewportHeight = m_TextViewport.rect.height;

            int newLine = originLine + 1;
            // Iterate through each subsequent line to find the first baseline that is not visible in the viewport.
            for (; newLine < m_TextComponent.textInfo.lineCount - 1; newLine++)
            {
                if (m_TextComponent.textInfo.lineInfo[newLine].baseline < m_TextComponent.textInfo.lineInfo[originLine].baseline - viewportHeight)
                    break;
            }

            // Need to determine end line for next line.
            int endCharIdx = m_TextComponent.textInfo.lineInfo[newLine].lastCharacterIndex;

            int closest = -1;
            float distance = TMP_Math.FLOAT_MAX;
            float range = 0;

            for (int i = m_TextComponent.textInfo.lineInfo[newLine].firstCharacterIndex; i < endCharIdx; ++i)
            {
                TMP_CharacterInfo currentChar = m_TextComponent.textInfo.characterInfo[i];

                float d = originChar.origin - currentChar.origin;
                float r = d / (currentChar.xAdvance - currentChar.origin);

                if (r >= 0 && r <= 1)
                {
                    if (r < 0.5f)
                        return i;
                    else
                        return i + 1;
                }

                d = Mathf.Abs(d);

                if (d < distance)
                {
                    closest = i;
                    distance = d;
                    range = r;
                }
            }

            if (closest == -1) return endCharIdx;

            if (range < 0.5f)
                return closest;
            else
                return closest + 1;
        }


        private void MoveDown(bool shift)
        {
            MoveDown(shift, true);
        }


        private void MoveDown(bool shift, bool goToLastChar)
        {
            if (hasSelection && !shift)
            {
                // If we have a selection and press down without shift,
                // set caret to end of selection before we move it down.
                caretPositionInternal = caretSelectPositionInternal = Mathf.Max(caretPositionInternal, caretSelectPositionInternal);
            }

            int position = multiLine ? LineDownCharacterPosition(caretSelectPositionInternal, goToLastChar) : m_TextComponent.textInfo.characterCount - 1; // text.Length;

            if (shift)
            {
                caretSelectPositionInternal = position;
                stringSelectPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
            }
            else
            {
                caretSelectPositionInternal = caretPositionInternal = position;
                stringSelectPositionInternal = stringPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
            }

            #if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
            #endif
        }

        private void MoveUp(bool shift)
        {
            MoveUp(shift, true);
        }


        private void MoveUp(bool shift, bool goToFirstChar)
        {
            if (hasSelection && !shift)
            {
                // If we have a selection and press up without shift,
                // set caret position to start of selection before we move it up.
                caretPositionInternal = caretSelectPositionInternal = Mathf.Min(caretPositionInternal, caretSelectPositionInternal);
            }

            int position = multiLine ? LineUpCharacterPosition(caretSelectPositionInternal, goToFirstChar) : 0;

            if (shift)
            {
                caretSelectPositionInternal = position;
                stringSelectPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
            }
            else
            {
                caretSelectPositionInternal = caretPositionInternal = position;
                stringSelectPositionInternal = stringPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
            }

            #if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
            #endif
        }


        private void MovePageUp(bool shift)
        {
            MovePageUp(shift, true);
        }

        private void MovePageUp(bool shift, bool goToFirstChar)
        {
            if (hasSelection && !shift)
            {
                // If we have a selection and press up without shift,
                // set caret position to start of selection before we move it up.
                caretPositionInternal = caretSelectPositionInternal = Mathf.Min(caretPositionInternal, caretSelectPositionInternal);
            }

            int position = multiLine ? PageUpCharacterPosition(caretSelectPositionInternal, goToFirstChar) : 0;

            if (shift)
            {
                caretSelectPositionInternal = position;
                stringSelectPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
            }
            else
            {
                caretSelectPositionInternal = caretPositionInternal = position;
                stringSelectPositionInternal = stringPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
            }


            // Scroll to top of viewport
            //int currentLine = m_TextComponent.textInfo.characterInfo[position].lineNumber;
            //float lineAscender = m_TextComponent.textInfo.lineInfo[currentLine].ascender;

            // Adjust text area up or down if not in single line mode.
            if (m_LineType != LineType.SingleLine)
            {
                float offset = m_TextViewport.rect.height; // m_TextViewport.rect.yMax - (m_TextComponent.rectTransform.anchoredPosition.y + lineAscender);

                float topTextBounds = m_TextComponent.rectTransform.position.y + m_TextComponent.textBounds.max.y;
                float topViewportBounds = m_TextViewport.position.y + m_TextViewport.rect.yMax;

                offset = topViewportBounds > topTextBounds + offset ? offset : topViewportBounds - topTextBounds;

                m_TextComponent.rectTransform.anchoredPosition += new Vector2(0, offset);
                AssignPositioningIfNeeded();
            }

            #if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
            #endif

        }


        private void MovePageDown(bool shift)
        {
            MovePageDown(shift, true);
        }

        private void MovePageDown(bool shift, bool goToLastChar)
        {
             if (hasSelection && !shift)
            {
                // If we have a selection and press down without shift,
                // set caret to end of selection before we move it down.
                caretPositionInternal = caretSelectPositionInternal = Mathf.Max(caretPositionInternal, caretSelectPositionInternal);
            }

            int position = multiLine ? PageDownCharacterPosition(caretSelectPositionInternal, goToLastChar) : m_TextComponent.textInfo.characterCount - 1;

            if (shift)
            {
                caretSelectPositionInternal = position;
                stringSelectPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
            }
            else
            {
                caretSelectPositionInternal = caretPositionInternal = position;
                stringSelectPositionInternal = stringPositionInternal = GetStringIndexFromCaretPosition(caretSelectPositionInternal);
            }

            // Scroll to top of viewport
            //int currentLine = m_TextComponent.textInfo.characterInfo[position].lineNumber;
            //float lineAscender = m_TextComponent.textInfo.lineInfo[currentLine].ascender;

            // Adjust text area up or down if not in single line mode.
            if (m_LineType != LineType.SingleLine)
            {
                float offset = m_TextViewport.rect.height; // m_TextViewport.rect.yMax - (m_TextComponent.rectTransform.anchoredPosition.y + lineAscender);

                float bottomTextBounds = m_TextComponent.rectTransform.position.y + m_TextComponent.textBounds.min.y;
                float bottomViewportBounds = m_TextViewport.position.y + m_TextViewport.rect.yMin;

                offset = bottomViewportBounds > bottomTextBounds + offset ? offset : bottomViewportBounds - bottomTextBounds;

                m_TextComponent.rectTransform.anchoredPosition += new Vector2(0, offset);
                AssignPositioningIfNeeded();
            }

            #if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
            #endif

        }

        private void Delete()
        {
            if (m_ReadOnly)
                return;

            if (m_StringPosition == m_StringSelectPosition)
                return;

            if (m_isRichTextEditingAllowed || m_isSelectAll)
            {
                // Handling of Delete when Rich Text is allowed.
                if (m_StringPosition < m_StringSelectPosition)
                {
                    m_Text = text.Remove(m_StringPosition, m_StringSelectPosition - m_StringPosition);
                    m_StringSelectPosition = m_StringPosition;
                }
                else
                {
                    m_Text = text.Remove(m_StringSelectPosition, m_StringPosition - m_StringSelectPosition);
                    m_StringPosition = m_StringSelectPosition;
                }

                if (m_isSelectAll)
                {
                    m_CaretPosition = m_CaretSelectPosition = 0;
                    m_isSelectAll = false;
                }
            }
            else
            {
                if (m_CaretPosition < m_CaretSelectPosition)
                {
                    int index = ClampArrayIndex(m_CaretSelectPosition - 1);
                    m_StringPosition = m_TextComponent.textInfo.characterInfo[m_CaretPosition].index;
                    m_StringSelectPosition = m_TextComponent.textInfo.characterInfo[index].index + m_TextComponent.textInfo.characterInfo[index].stringLength;

                    m_Text = text.Remove(m_StringPosition, m_StringSelectPosition - m_StringPosition);

                    m_StringSelectPosition = m_StringPosition;
                    m_CaretSelectPosition = m_CaretPosition;
                }
                else
                {
                    int index = ClampArrayIndex(m_CaretPosition - 1);
                    m_StringPosition = m_TextComponent.textInfo.characterInfo[index].index + m_TextComponent.textInfo.characterInfo[index].stringLength;
                    m_StringSelectPosition = m_TextComponent.textInfo.characterInfo[m_CaretSelectPosition].index;

                    m_Text = text.Remove(m_StringSelectPosition, m_StringPosition - m_StringSelectPosition);

                    m_StringPosition = m_StringSelectPosition;
                    m_CaretPosition = m_CaretSelectPosition;
                }
            }

            #if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
            #endif
        }

        /// <summary>
        /// Handling of DEL key
        /// </summary>
        private void DeleteKey()
        {
            if (m_ReadOnly)
                return;

            if (hasSelection)
            {
                m_HasTextBeenRemoved = true;

                Delete();
                UpdateTouchKeyboardFromEditChanges();
                SendOnValueChangedAndUpdateLabel();
            }
            else
            {
                if (m_isRichTextEditingAllowed)
                {
                    if (stringPositionInternal < text.Length)
                    {
                        // Special handling for Surrogate Pairs
                        if (char.IsHighSurrogate(text[stringPositionInternal]))
                            m_Text = text.Remove(stringPositionInternal, 2);
                        else
                            m_Text = text.Remove(stringPositionInternal, 1);

                        m_HasTextBeenRemoved = true;

                        UpdateTouchKeyboardFromEditChanges();
                        SendOnValueChangedAndUpdateLabel();
                    }
                }
                else
                {
                    if (caretPositionInternal < m_TextComponent.textInfo.characterCount - 1)
                    {
                        int numberOfCharactersToRemove = m_TextComponent.textInfo.characterInfo[caretPositionInternal].stringLength;

                        // Special handling for <CR><LF>
                        if (m_TextComponent.textInfo.characterInfo[caretPositionInternal].character == '\r' && m_TextComponent.textInfo.characterInfo[caretPositionInternal + 1].character == '\n')
                            numberOfCharactersToRemove += m_TextComponent.textInfo.characterInfo[caretPositionInternal + 1].stringLength;

                        // Adjust string position to skip any potential rich text tags.
                        int nextCharacterStringPosition = m_TextComponent.textInfo.characterInfo[caretPositionInternal].index;

                        m_Text = text.Remove(nextCharacterStringPosition, numberOfCharactersToRemove);

                        m_HasTextBeenRemoved = true;

                        SendOnValueChangedAndUpdateLabel();
                    }
                }
            }

            #if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
            #endif
        }

        /// <summary>
        /// Handling of Backspace key
        /// </summary>
        private void Backspace()
        {
            if (m_ReadOnly)
                return;

            if (hasSelection)
            {
                m_HasTextBeenRemoved = true;

                Delete();
                UpdateTouchKeyboardFromEditChanges();
                SendOnValueChangedAndUpdateLabel();
            }
            else
            {
                if (m_isRichTextEditingAllowed)
                {
                    if (stringPositionInternal > 0)
                    {
                        int numberOfCharactersToRemove = 1;

                        // Special handling for Surrogate pairs and Diacritical marks
                        if (char.IsLowSurrogate(text[stringPositionInternal - 1]))
                            numberOfCharactersToRemove = 2;

                        stringSelectPositionInternal = stringPositionInternal = stringPositionInternal - numberOfCharactersToRemove;

                        m_Text = text.Remove(stringPositionInternal, numberOfCharactersToRemove);

                        caretSelectPositionInternal = caretPositionInternal = caretPositionInternal - 1;

                        m_HasTextBeenRemoved = true;

                        UpdateTouchKeyboardFromEditChanges();
                        SendOnValueChangedAndUpdateLabel();
                    }
                }
                else
                {
                    if (caretPositionInternal > 0)
                    {
                        int caretPositionIndex = caretPositionInternal - 1;
                        int numberOfCharactersToRemove = m_TextComponent.textInfo.characterInfo[caretPositionIndex].stringLength;

                        // Special handling for <CR><LR>
                        if (caretPositionIndex > 0 && m_TextComponent.textInfo.characterInfo[caretPositionIndex].character == '\n' && m_TextComponent.textInfo.characterInfo[caretPositionIndex - 1].character == '\r')
                        {
                            numberOfCharactersToRemove += m_TextComponent.textInfo.characterInfo[caretPositionIndex - 1].stringLength;
                            caretPositionIndex -= 1;
                        }

                        // Delete the previous character
                        m_Text = text.Remove(m_TextComponent.textInfo.characterInfo[caretPositionIndex].index, numberOfCharactersToRemove);

                        // Get new adjusted string position
                        stringSelectPositionInternal = stringPositionInternal = caretPositionInternal < 1
                            ? m_TextComponent.textInfo.characterInfo[0].index
                            : m_TextComponent.textInfo.characterInfo[caretPositionIndex].index;

                        caretSelectPositionInternal = caretPositionInternal = caretPositionIndex;
                    }

                    m_HasTextBeenRemoved = true;

                    UpdateTouchKeyboardFromEditChanges();
                    SendOnValueChangedAndUpdateLabel();
                }

            }

            #if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
            #endif
        }


        /// <summary>
        /// Append the specified text to the end of the current.
        /// </summary>
        protected virtual void Append(string input)
        {
            if (m_ReadOnly)
                return;

            if (InPlaceEditing() == false)
                return;

            for (int i = 0, imax = input.Length; i < imax; ++i)
            {
                char c = input[i];

                if (c >= ' ' || c == '\t' || c == '\r' || c == '\n')
                {
                    Append(c);
                }
            }
        }

        protected virtual void Append(char input)
        {
            if (m_ReadOnly)
                return;

            if (InPlaceEditing() == false)
                return;

            // If we have an input validator, validate the input first
            int insertionPosition = Mathf.Min(stringPositionInternal, stringSelectPositionInternal);

            //Get the text based on selection for validation instead of whole text(case 1253193).
            var validateText = text;

            if (selectionFocusPosition != selectionAnchorPosition)
            {

                m_HasTextBeenRemoved = true;

                if (m_isRichTextEditingAllowed || m_isSelectAll)
                {
                    // Handling of Delete when Rich Text is allowed.
                    if (m_StringPosition < m_StringSelectPosition)
                        validateText = text.Remove(m_StringPosition, m_StringSelectPosition - m_StringPosition);
                    else
                        validateText = text.Remove(m_StringSelectPosition, m_StringPosition - m_StringSelectPosition);
                }
                else
                {
                    if (m_CaretPosition < m_CaretSelectPosition)
                    {
                        m_StringPosition = m_TextComponent.textInfo.characterInfo[m_CaretPosition].index;
                        m_StringSelectPosition = m_TextComponent.textInfo.characterInfo[m_CaretSelectPosition - 1].index + m_TextComponent.textInfo.characterInfo[m_CaretSelectPosition - 1].stringLength;

                        validateText = text.Remove(m_StringPosition, m_StringSelectPosition - m_StringPosition);
                    }
                    else
                    {
                        m_StringPosition = m_TextComponent.textInfo.characterInfo[m_CaretPosition - 1].index + m_TextComponent.textInfo.characterInfo[m_CaretPosition - 1].stringLength;
                        m_StringSelectPosition = m_TextComponent.textInfo.characterInfo[m_CaretSelectPosition].index;

                        validateText = text.Remove(m_StringSelectPosition, m_StringPosition - m_StringSelectPosition);
                    }
                }
            }

            if (onValidateInput != null)
            {
                input = onValidateInput(validateText, insertionPosition, input);
            }
            else if (characterValidation == CharacterValidation.CustomValidator)
            {
                input = Validate(validateText, insertionPosition, input);

                if (input == 0) return;

                if (!char.IsHighSurrogate(input))
                    m_CaretSelectPosition = m_CaretPosition += 1;

                SendOnValueChanged();
                UpdateLabel();

                return;
            }
            else if (characterValidation != CharacterValidation.None)
            {
                input = Validate(validateText, insertionPosition, input);
            }

            // If the input is invalid, skip it
            if (input == 0)
                return;

            // Append the character and update the label
            Insert(input);
        }


        // Insert the character and update the label.
        private void Insert(char c)
        {
            if (m_ReadOnly)
                return;

            //Debug.Log("Inserting character " + m_IsCompositionActive);

            string replaceString = c.ToString();
            Delete();

            // Can't go past the character limit
            if (characterLimit > 0 && text.Length >= characterLimit)
                return;

            m_Text = text.Insert(m_StringPosition, replaceString);

            if (!char.IsHighSurrogate(c))
                m_CaretSelectPosition = m_CaretPosition += 1;

            m_StringSelectPosition = m_StringPosition += 1;

            UpdateTouchKeyboardFromEditChanges();
            SendOnValueChanged();

            #if TMP_DEBUG_MODE
                Debug.Log("Caret Position: " + caretPositionInternal + " Selection Position: " + caretSelectPositionInternal + "  String Position: " + stringPositionInternal + " String Select Position: " + stringSelectPositionInternal);
            #endif
        }

        private void UpdateTouchKeyboardFromEditChanges()
        {
            // Update the TouchKeyboard's text from edit changes
            // if in-place editing is allowed
            if (m_SoftKeyboard != null && InPlaceEditing())
            {
                m_SoftKeyboard.text = m_Text;
            }
        }

        private void SendOnValueChangedAndUpdateLabel()
        {
            UpdateLabel();
            SendOnValueChanged();
        }

        private void SendOnValueChanged()
        {
            if (onValueChanged != null)
                onValueChanged.Invoke(text);
        }

        /// <summary>
        /// Submit the input field's text.
        /// </summary>

        protected void SendOnEndEdit()
        {
            if (onEndEdit != null)
                onEndEdit.Invoke(m_Text);
        }

        protected void SendOnSubmit()
        {
            if (onSubmit != null)
                onSubmit.Invoke(m_Text);
        }

        protected void SendOnFocus()
        {
            if (onSelect != null)
                onSelect.Invoke(m_Text);
        }

        protected void SendOnFocusLost()
        {
            if (onDeselect != null)
                onDeselect.Invoke(m_Text);
        }

        protected void SendOnTextSelection()
        {
            m_isSelected = true;

            if (onTextSelection != null)
                onTextSelection.Invoke(m_Text, stringPositionInternal, stringSelectPositionInternal);
        }

        protected void SendOnEndTextSelection()
        {
            if (!m_isSelected) return;

            if (onEndTextSelection != null)
                onEndTextSelection.Invoke(m_Text, stringPositionInternal, stringSelectPositionInternal);

            m_isSelected = false;
        }

        protected void SendTouchScreenKeyboardStatusChanged()
        {
            if (m_SoftKeyboard != null && onTouchScreenKeyboardStatusChanged != null)
                onTouchScreenKeyboardStatusChanged.Invoke(m_SoftKeyboard.status);
        }


        /// <summary>
        /// Update the visual text Text.
        /// </summary>

        protected void UpdateLabel()
        {
            if (m_TextComponent != null && m_TextComponent.font != null && m_PreventCallback == false)
            {
                // Prevent callback from the text component as we assign new text. This is to prevent a recursive call.
                m_PreventCallback = true;

                string fullText;
                if (compositionLength > 0 && m_ReadOnly == false)
                {
                    //Input.imeCompositionMode = IMECompositionMode.On;

                    // Handle selections
                    Delete();

                    if (m_RichText)
                        fullText = text.Substring(0, m_StringPosition) +  "<u>" + compositionString + "</u>" + text.Substring(m_StringPosition);
                    else
                        fullText = text.Substring(0, m_StringPosition) +  compositionString + text.Substring(m_StringPosition);

                    m_IsCompositionActive = true;

                    //Debug.Log("[" + Time.frameCount + "] Handling IME Input");
                }
                else
                {
                    fullText = text;
                    m_IsCompositionActive = false;
                    m_ShouldUpdateIMEWindowPosition = true;

                }

                //Debug.Log("Handling IME Input... [" + compositionString + "] of length [" + compositionLength + "] at StringPosition [" + m_StringPosition + "]  IsActive [" + m_IsCompositionActive + "]");

                string processed;
                if (inputType == InputType.Password)
                    processed = new string(asteriskChar, fullText.Length);
                else
                    processed = fullText;

                bool isEmpty = string.IsNullOrEmpty(fullText);

                if (m_Placeholder != null)
                    m_Placeholder.enabled = isEmpty;

                if (!isEmpty && m_ReadOnly == false)
                {
                    SetCaretVisible();
                }

                m_TextComponent.text = processed + "\u200B"; // Extra space is added for Caret tracking.

                // Rebuild layout if using Layout components.
                if (m_IsDrivenByLayoutComponents)
                    LayoutRebuilder.MarkLayoutForRebuild(m_RectTransform);

                // Special handling to limit the number of lines of text in the Input Field.
                if (m_LineLimit > 0)
                {
                    m_TextComponent.ForceMeshUpdate();

                    TMP_TextInfo textInfo = m_TextComponent.textInfo;

                    // Check if text exceeds maximum number of lines.
                    if (textInfo != null && textInfo.lineCount > m_LineLimit)
                    {
                        int lastValidCharacterIndex = textInfo.lineInfo[m_LineLimit - 1].lastCharacterIndex;
                        int characterStringIndex = textInfo.characterInfo[lastValidCharacterIndex].index + textInfo.characterInfo[lastValidCharacterIndex].stringLength;
                        text = processed.Remove(characterStringIndex, processed.Length - characterStringIndex);
                        m_TextComponent.text = text + "\u200B";
                    }
                }

                if (m_IsTextComponentUpdateRequired || m_VerticalScrollbar && !(m_IsCaretPositionDirty && m_IsStringPositionDirty))
                {
                    m_IsTextComponentUpdateRequired = false;
                    m_TextComponent.ForceMeshUpdate();
                }

                MarkGeometryAsDirty();

                m_PreventCallback = false;
            }
        }


        void UpdateScrollbar()
        {
            // Update Scrollbar
            if (m_VerticalScrollbar)
            {
                Rect viewportRect = m_TextViewport.rect;

                float size = viewportRect.height / m_TextComponent.preferredHeight;

                m_VerticalScrollbar.size = size;

                m_VerticalScrollbar.value = GetScrollPositionRelativeToViewport();

                //Debug.Log(GetInstanceID() + "- UpdateScrollbar() - Updating Scrollbar... Value: " + m_VerticalScrollbar.value);
            }
        }


        /// <summary>
        /// Function to update the vertical position of the text container when OnValueChanged event is received from the Scrollbar.
        /// </summary>
        /// <param name="value"></param>
        void OnScrollbarValueChange(float value)
        {
            //if (m_IsUpdatingScrollbarValues)
            //{
            //    m_IsUpdatingScrollbarValues = false;
            //    return;
            //}

            if (value < 0 || value > 1) return;

            AdjustTextPositionRelativeToViewport(value);

            m_ScrollPosition = value;

            //Debug.Log(GetInstanceID() + "- OnScrollbarValueChange() - Scrollbar value is: " + value + "  Transform POS: " + m_TextComponent.rectTransform.anchoredPosition);
        }

        void UpdateMaskRegions()
        {
            // TODO: Figure out a better way to handle adding an offset to the masking region
            // This region is defined by the RectTransform of the GameObject that contains the RectMask2D component.
            /*
            // Update Masking Region
            if (m_TextViewportRectMask != null)
            {
                Rect viewportRect = m_TextViewportRectMask.canvasRect;

                if (viewportRect != m_CachedViewportRect)
                {
                    m_CachedViewportRect = viewportRect;

                    viewportRect.min -= m_TextViewport.offsetMin * 0.5f;
                    viewportRect.max -= m_TextViewport.offsetMax * 0.5f;

                    if (m_CachedInputRenderer != null)
                        m_CachedInputRenderer.EnableRectClipping(viewportRect);

                    if (m_TextComponent.canvasRenderer != null)
                        m_TextComponent.canvasRenderer.EnableRectClipping(viewportRect);

                    if (m_Placeholder != null && m_Placeholder.enabled)
                        m_Placeholder.canvasRenderer.EnableRectClipping(viewportRect);
                }
            }
            */
        }

        /// <summary>
        /// Adjusts the relative position of the body of the text relative to the viewport.
        /// </summary>
        /// <param name="relativePosition"></param>
        void AdjustTextPositionRelativeToViewport (float relativePosition)
        {
            if (m_TextViewport == null)
                return;

            TMP_TextInfo textInfo = m_TextComponent.textInfo;

            // Check to make sure we have valid data and lines to query.
            if (textInfo == null || textInfo.lineInfo == null || textInfo.lineCount == 0 || textInfo.lineCount > textInfo.lineInfo.Length) return;

            float verticalAlignmentOffset = 0;
            float textHeight = m_TextComponent.preferredHeight;

            switch (m_TextComponent.verticalAlignment)
            {
                case VerticalAlignmentOptions.Top:
                    verticalAlignmentOffset = 0;
                    break;
                case VerticalAlignmentOptions.Middle:
                    verticalAlignmentOffset = 0.5f;
                    break;
                case VerticalAlignmentOptions.Bottom:
                    verticalAlignmentOffset = 1.0f;
                    break;
                case VerticalAlignmentOptions.Baseline:
                    break;
                case VerticalAlignmentOptions.Geometry:
                    verticalAlignmentOffset = 0.5f;
                    textHeight = m_TextComponent.bounds.size.y;
                    break;
                case VerticalAlignmentOptions.Capline:
                    verticalAlignmentOffset = 0.5f;
                    break;
            }

            m_TextComponent.rectTransform.anchoredPosition = new Vector2(m_TextComponent.rectTransform.anchoredPosition.x, (textHeight - m_TextViewport.rect.height) * (relativePosition - verticalAlignmentOffset));

            AssignPositioningIfNeeded();

            //Debug.Log("Text height: " + m_TextComponent.preferredHeight + "  Viewport height: " + m_TextViewport.rect.height + "  Adjusted RectTransform anchordedPosition:" + m_TextComponent.rectTransform.anchoredPosition + "  Text Bounds: " + m_TextComponent.bounds.ToString("f3"));
        }


        private int GetCaretPositionFromStringIndex(int stringIndex)
        {
            int count = m_TextComponent.textInfo.characterCount;

            for (int i = 0; i < count; i++)
            {
                if (m_TextComponent.textInfo.characterInfo[i].index >= stringIndex)
                    return i;
            }

            return count;
        }

        /// <summary>
        /// Returns / places the caret before the given character at the string index.
        /// </summary>
        /// <param name="stringIndex"></param>
        /// <returns></returns>
        private int GetMinCaretPositionFromStringIndex(int stringIndex)
        {
            int count = m_TextComponent.textInfo.characterCount;

            for (int i = 0; i < count; i++)
            {
                if (stringIndex < m_TextComponent.textInfo.characterInfo[i].index + m_TextComponent.textInfo.characterInfo[i].stringLength)
                    return i;
            }

            return count;
        }

        /// <summary>
        /// Returns / places the caret after the given character at the string index.
        /// </summary>
        /// <param name="stringIndex"></param>
        /// <returns></returns>
        private int GetMaxCaretPositionFromStringIndex(int stringIndex)
        {
            int count = m_TextComponent.textInfo.characterCount;

            for (int i = 0; i < count; i++)
            {
                if (m_TextComponent.textInfo.characterInfo[i].index >= stringIndex)
                    return i;
            }

            return count;
        }

        private int GetStringIndexFromCaretPosition(int caretPosition)
        {
            // Clamp values between 0 and character count.
            ClampCaretPos(ref caretPosition);

            return m_TextComponent.textInfo.characterInfo[caretPosition].index;
        }

        void UpdateStringIndexFromCaretPosition()
        {
            stringPositionInternal = GetStringIndexFromCaretPosition(m_CaretPosition);
            stringSelectPositionInternal = GetStringIndexFromCaretPosition(m_CaretSelectPosition);
            m_IsStringPositionDirty = false;
        }

        void UpdateCaretPositionFromStringIndex()
        {
            caretPositionInternal = GetCaretPositionFromStringIndex(stringPositionInternal);
            caretSelectPositionInternal = GetCaretPositionFromStringIndex(stringSelectPositionInternal);
            m_IsCaretPositionDirty = false;
        }


        public void ForceLabelUpdate()
        {
            UpdateLabel();
        }

        private void MarkGeometryAsDirty()
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying || UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this))
                return;
            #endif

            CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
        }

        public virtual void Rebuild(CanvasUpdate update)
        {
            switch (update)
            {
                case CanvasUpdate.LatePreRender:
                    UpdateGeometry();
                    break;
            }
        }

        public virtual void LayoutComplete()
        { }

        public virtual void GraphicUpdateComplete()
        { }

        private void UpdateGeometry()
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
            #endif

            // No need to draw a cursor on mobile as its handled by the devices keyboard with the exception of UWP.
            if (InPlaceEditing() == false && isUWP() == false)
                return;

            if (m_CachedInputRenderer == null)
                return;

            OnFillVBO(mesh);

            m_CachedInputRenderer.SetMesh(mesh);
        }


        /// <summary>
        /// Method to keep the Caret RectTransform properties in sync with the text object's RectTransform
        /// </summary>
        private void AssignPositioningIfNeeded()
        {
            if (m_TextComponent != null && caretRectTrans != null &&
                (caretRectTrans.localPosition != m_TextComponent.rectTransform.localPosition ||
                 caretRectTrans.localRotation != m_TextComponent.rectTransform.localRotation ||
                 caretRectTrans.localScale != m_TextComponent.rectTransform.localScale ||
                 caretRectTrans.anchorMin != m_TextComponent.rectTransform.anchorMin ||
                 caretRectTrans.anchorMax != m_TextComponent.rectTransform.anchorMax ||
                 caretRectTrans.anchoredPosition != m_TextComponent.rectTransform.anchoredPosition ||
                 caretRectTrans.sizeDelta != m_TextComponent.rectTransform.sizeDelta ||
                 caretRectTrans.pivot != m_TextComponent.rectTransform.pivot))
            {
                caretRectTrans.localPosition = m_TextComponent.rectTransform.localPosition;
                caretRectTrans.localRotation = m_TextComponent.rectTransform.localRotation;
                caretRectTrans.localScale = m_TextComponent.rectTransform.localScale;
                caretRectTrans.anchorMin = m_TextComponent.rectTransform.anchorMin;
                caretRectTrans.anchorMax = m_TextComponent.rectTransform.anchorMax;
                caretRectTrans.anchoredPosition = m_TextComponent.rectTransform.anchoredPosition;
                caretRectTrans.sizeDelta = m_TextComponent.rectTransform.sizeDelta;
                caretRectTrans.pivot = m_TextComponent.rectTransform.pivot;
            }
        }


        private void OnFillVBO(Mesh vbo)
        {
            using (var helper = new VertexHelper())
            {
                if (!isFocused && !m_SelectionStillActive)
                {
                    helper.FillMesh(vbo);
                    return;
                }

                if (m_IsStringPositionDirty)
                    UpdateStringIndexFromCaretPosition();

                if (m_IsCaretPositionDirty)
                    UpdateCaretPositionFromStringIndex();

                if (!hasSelection)
                {
                    GenerateCaret(helper, Vector2.zero);
                    SendOnEndTextSelection();
                }
                else
                {
                    GenerateHighlight(helper, Vector2.zero);
                    SendOnTextSelection();
                }

                helper.FillMesh(vbo);
            }
        }


        private void GenerateCaret(VertexHelper vbo, Vector2 roundingOffset)
        {
            if (m_CaretVisible == false || m_TextComponent.canvas == null || m_ReadOnly)
                return;

            if (m_CursorVerts == null)
            {
                CreateCursorVerts();
            }

            // TODO: Optimize to only update the caret position when needed.

            Vector2 startPosition = Vector2.zero;
            float height = 0;
            TMP_CharacterInfo currentCharacter;

            // Make sure caret position does not exceed characterInfo array size or less than zero.
            if (caretPositionInternal >= m_TextComponent.textInfo.characterInfo.Length || caretPositionInternal < 0)
                return;

            int currentLine = m_TextComponent.textInfo.characterInfo[caretPositionInternal].lineNumber;

            // Caret is positioned at the origin for the first character of each lines and at the advance for subsequent characters.
            if (caretPositionInternal == m_TextComponent.textInfo.lineInfo[currentLine].firstCharacterIndex)
            {
                currentCharacter = m_TextComponent.textInfo.characterInfo[caretPositionInternal];
                height = currentCharacter.ascender - currentCharacter.descender;

                if (m_TextComponent.verticalAlignment == VerticalAlignmentOptions.Geometry)
                    startPosition = new Vector2(currentCharacter.origin, 0 - height / 2);
                else
                    startPosition = new Vector2(currentCharacter.origin, currentCharacter.descender);
            }
            else
            {
                currentCharacter = m_TextComponent.textInfo.characterInfo[caretPositionInternal - 1];
                height = currentCharacter.ascender - currentCharacter.descender;

                if (m_TextComponent.verticalAlignment == VerticalAlignmentOptions.Geometry)
                    startPosition = new Vector2(currentCharacter.xAdvance, 0 - height / 2);
                else
                    startPosition = new Vector2(currentCharacter.xAdvance, currentCharacter.descender);

            }

            if (m_SoftKeyboard != null && compositionLength == 0)
            {
                int selectionStart = m_StringPosition;
                int softKeyboardStringLength = m_SoftKeyboard.text == null ? 0 : m_SoftKeyboard.text.Length;

                if (selectionStart < 0)
                    selectionStart = 0;

                if (selectionStart > softKeyboardStringLength)
                    selectionStart = softKeyboardStringLength;

                m_SoftKeyboard.selection = new RangeInt(selectionStart, 0);
            }

            // Adjust the position of the RectTransform based on the caret position in the viewport (only if we have focus).
            if (isFocused && startPosition != m_LastPosition || m_forceRectTransformAdjustment || m_HasTextBeenRemoved)
                AdjustRectTransformRelativeToViewport(startPosition, height, currentCharacter.isVisible);

            m_LastPosition = startPosition;

            // Clamp Caret height
            float top = startPosition.y + height;
            float bottom = top - height;

            // Compute the width of the caret which is based on the line height of the primary font asset.
            //float width = m_CaretWidth;
            TMP_FontAsset fontAsset = m_TextComponent.font;
            float baseScale = (m_TextComponent.fontSize / fontAsset.m_FaceInfo.pointSize * fontAsset.m_FaceInfo.scale);
            float width = m_CaretWidth * fontAsset.faceInfo.lineHeight * baseScale * 0.05f;

            m_CursorVerts[0].position = new Vector3(startPosition.x, bottom, 0.0f);
            m_CursorVerts[1].position = new Vector3(startPosition.x, top, 0.0f);
            m_CursorVerts[2].position = new Vector3(startPosition.x + width, top, 0.0f);
            m_CursorVerts[3].position = new Vector3(startPosition.x + width, bottom, 0.0f);

            // Set Vertex Color for the caret color.
            m_CursorVerts[0].color = caretColor;
            m_CursorVerts[1].color = caretColor;
            m_CursorVerts[2].color = caretColor;
            m_CursorVerts[3].color = caretColor;

            vbo.AddUIVertexQuad(m_CursorVerts);

            // Update position of IME window when necessary.
            if (m_ShouldUpdateIMEWindowPosition || currentLine != m_PreviousIMEInsertionLine)
            {
                m_ShouldUpdateIMEWindowPosition = false;
                m_PreviousIMEInsertionLine = currentLine;

                // Calculate position of IME Window in screen space.
                Camera cameraRef;
                if (m_TextComponent.canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    cameraRef = null;
                else
                {
                    cameraRef = m_TextComponent.canvas.worldCamera;

                    if (cameraRef == null)
                        cameraRef = Camera.current;
                }

                Vector3 cursorPosition = m_CachedInputRenderer.gameObject.transform.TransformPoint(m_CursorVerts[0].position);
                Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(cameraRef, cursorPosition);
                screenPosition.y = Screen.height - screenPosition.y;

                if (inputSystem != null)
                    inputSystem.compositionCursorPos = screenPosition;

                //Debug.Log("[" + Time.frameCount + "] Updating IME Window position  Cursor Pos: (" + cursorPosition + ")  Screen Pos: (" + screenPosition + ") with Composition Length: " + compositionLength);
            }

            //#if TMP_DEBUG_MODE
            //Debug.Log("Caret position updated at frame: " + Time.frameCount);
            //#endif
        }


        private void CreateCursorVerts()
        {
            m_CursorVerts = new UIVertex[4];

            for (int i = 0; i < m_CursorVerts.Length; i++)
            {
                m_CursorVerts[i] = UIVertex.simpleVert;
                m_CursorVerts[i].uv0 = Vector2.zero;
            }
        }


        private void GenerateHighlight(VertexHelper vbo, Vector2 roundingOffset)
        {
            // Update Masking Region
            UpdateMaskRegions();

            // Make sure caret position does not exceed characterInfo array size.
            //if (caretSelectPositionInternal >= m_TextComponent.textInfo.characterInfo.Length)
            //    return;

            TMP_TextInfo textInfo = m_TextComponent.textInfo;

            // Return if character count is zero as there is nothing to highlight.
            if (textInfo.characterCount == 0)
                return;

            m_CaretPosition = GetCaretPositionFromStringIndex(stringPositionInternal);
            m_CaretSelectPosition = GetCaretPositionFromStringIndex(stringSelectPositionInternal);

            if (m_SoftKeyboard != null && compositionLength == 0)
            {
                int stringPosition = m_CaretPosition < m_CaretSelectPosition ? textInfo.characterInfo[m_CaretPosition].index : textInfo.characterInfo[m_CaretSelectPosition].index;
                int length = m_CaretPosition < m_CaretSelectPosition ? stringSelectPositionInternal - stringPosition : stringPositionInternal - stringPosition;
                m_SoftKeyboard.selection = new RangeInt(stringPosition, length);
            }

            // Adjust text RectTranform position to make sure it is visible in viewport.
            Vector2 caretPosition;
            float height = 0;
            if (m_CaretSelectPosition < textInfo.characterCount)
            {
                caretPosition = new Vector2(textInfo.characterInfo[m_CaretSelectPosition].origin, textInfo.characterInfo[m_CaretSelectPosition].descender);
                height = textInfo.characterInfo[m_CaretSelectPosition].ascender - textInfo.characterInfo[m_CaretSelectPosition].descender;
            }
            else
            {
                caretPosition = new Vector2(textInfo.characterInfo[m_CaretSelectPosition - 1].xAdvance, textInfo.characterInfo[m_CaretSelectPosition - 1].descender);
                height = textInfo.characterInfo[m_CaretSelectPosition - 1].ascender - textInfo.characterInfo[m_CaretSelectPosition - 1].descender;
            }

            // TODO: Don't adjust the position of the RectTransform if Reset On Deactivation is disabled
            // and we just selected the Input Field again.
            AdjustRectTransformRelativeToViewport(caretPosition, height, true);

            int startChar = Mathf.Max(0, m_CaretPosition);
            int endChar = Mathf.Max(0, m_CaretSelectPosition);

            // Ensure pos is always less then selPos to make the code simpler
            if (startChar > endChar)
            {
                int temp = startChar;
                startChar = endChar;
                endChar = temp;
            }

            endChar -= 1;

            //Debug.Log("Updating Highlight... Caret Position: " + startChar + " Caret Select POS: " + endChar);


            int currentLineIndex = textInfo.characterInfo[startChar].lineNumber;
            int nextLineStartIdx = textInfo.lineInfo[currentLineIndex].lastCharacterIndex;

            UIVertex vert = UIVertex.simpleVert;
            vert.uv0 = Vector2.zero;
            vert.color = selectionColor;

            int currentChar = startChar;
            while (currentChar <= endChar && currentChar < textInfo.characterCount)
            {
                if (currentChar == nextLineStartIdx || currentChar == endChar)
                {
                    TMP_CharacterInfo startCharInfo = textInfo.characterInfo[startChar];
                    TMP_CharacterInfo endCharInfo = textInfo.characterInfo[currentChar];

                    // Extra check to handle Carriage Return
                    if (currentChar > 0 && endCharInfo.character == '\n' && textInfo.characterInfo[currentChar - 1].character == '\r')
                        endCharInfo = textInfo.characterInfo[currentChar - 1];

                    Vector2 startPosition = new Vector2(startCharInfo.origin, textInfo.lineInfo[currentLineIndex].ascender);
                    Vector2 endPosition = new Vector2(endCharInfo.xAdvance, textInfo.lineInfo[currentLineIndex].descender);

                    var startIndex = vbo.currentVertCount;
                    vert.position = new Vector3(startPosition.x, endPosition.y, 0.0f);
                    vbo.AddVert(vert);

                    vert.position = new Vector3(endPosition.x, endPosition.y, 0.0f);
                    vbo.AddVert(vert);

                    vert.position = new Vector3(endPosition.x, startPosition.y, 0.0f);
                    vbo.AddVert(vert);

                    vert.position = new Vector3(startPosition.x, startPosition.y, 0.0f);
                    vbo.AddVert(vert);

                    vbo.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
                    vbo.AddTriangle(startIndex + 2, startIndex + 3, startIndex + 0);

                    startChar = currentChar + 1;
                    currentLineIndex++;

                    if (currentLineIndex < textInfo.lineCount)
                        nextLineStartIdx = textInfo.lineInfo[currentLineIndex].lastCharacterIndex;
                }
                currentChar++;
            }

            //#if TMP_DEBUG_MODE
            //    Debug.Log("Text selection updated at frame: " + Time.frameCount);
            //#endif
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="height"></param>
        /// <param name="isCharVisible"></param>
        private void AdjustRectTransformRelativeToViewport(Vector2 startPosition, float height, bool isCharVisible)
        {
            //Debug.Log("Adjusting transform position relative to viewport.");

            if (m_TextViewport == null)
                return;

            Vector3 localPosition = transform.localPosition;
            Vector3 textComponentLocalPosition = m_TextComponent.rectTransform.localPosition;
            Vector3 textViewportLocalPosition = m_TextViewport.localPosition;
            Rect textViewportRect = m_TextViewport.rect;

            Vector2 caretPosition = new Vector2(startPosition.x + textComponentLocalPosition.x + textViewportLocalPosition.x + localPosition.x, startPosition.y + textComponentLocalPosition.y + textViewportLocalPosition.y + localPosition.y);
            Rect viewportWSRect = new Rect(localPosition.x + textViewportLocalPosition.x + textViewportRect.x, localPosition.y + textViewportLocalPosition.y + textViewportRect.y, textViewportRect.width, textViewportRect.height);

            // Adjust the position of the RectTransform based on the caret position in the viewport.
            float rightOffset = viewportWSRect.xMax - (caretPosition.x + m_TextComponent.margin.z + m_CaretWidth);
            if (rightOffset < 0f)
            {
                if (!multiLine || (multiLine && isCharVisible))
                {
                    //Debug.Log("Shifting text to the LEFT by " + rightOffset.ToString("f3"));
                    m_TextComponent.rectTransform.anchoredPosition += new Vector2(rightOffset, 0);

                    AssignPositioningIfNeeded();
                }
            }

            float leftOffset = (caretPosition.x - m_TextComponent.margin.x) - viewportWSRect.xMin;
            if (leftOffset < 0f)
            {
                //Debug.Log("Shifting text to the RIGHT by " + leftOffset.ToString("f3"));
                m_TextComponent.rectTransform.anchoredPosition += new Vector2(-leftOffset, 0);
                AssignPositioningIfNeeded();
            }

            // Adjust text area up or down if not in single line mode.
            if (m_LineType != LineType.SingleLine)
            {
                float topOffset = viewportWSRect.yMax - (caretPosition.y + height);
                if (topOffset < -0.0001f)
                {
                    //Debug.Log("Shifting text to Up " + topOffset.ToString("f3"));
                    m_TextComponent.rectTransform.anchoredPosition += new Vector2(0, topOffset);
                    AssignPositioningIfNeeded();
                }

                float bottomOffset = caretPosition.y - viewportWSRect.yMin;
                if (bottomOffset < 0f)
                {
                    //Debug.Log("Shifting text to Down " + bottomOffset.ToString("f3"));
                    m_TextComponent.rectTransform.anchoredPosition -= new Vector2(0, bottomOffset);
                    AssignPositioningIfNeeded();
                }
            }

            // Special handling of backspace/text being removed
            if (m_HasTextBeenRemoved)
            {
                float anchoredPositionX = m_TextComponent.rectTransform.anchoredPosition.x;

                float firstCharPosition = localPosition.x + textViewportLocalPosition.x + textComponentLocalPosition.x + m_TextComponent.textInfo.characterInfo[0].origin - m_TextComponent.margin.x;
                int lastCharacterIndex = ClampArrayIndex(m_TextComponent.textInfo.characterCount - 1);
                float lastCharPosition = localPosition.x + textViewportLocalPosition.x + textComponentLocalPosition.x + m_TextComponent.textInfo.characterInfo[lastCharacterIndex].origin + m_TextComponent.margin.z + m_CaretWidth;

                if (anchoredPositionX > 0.0001f && firstCharPosition > viewportWSRect.xMin)
                {
                    float offset = viewportWSRect.xMin - firstCharPosition;

                    if (anchoredPositionX < -offset)
                        offset = -anchoredPositionX;

                    m_TextComponent.rectTransform.anchoredPosition += new Vector2(offset, 0);
                    AssignPositioningIfNeeded();
                }
                else if (anchoredPositionX < -0.0001f && lastCharPosition < viewportWSRect.xMax)
                {
                    float offset = viewportWSRect.xMax - lastCharPosition;

                    if (-anchoredPositionX < offset)
                        offset = -anchoredPositionX;

                    m_TextComponent.rectTransform.anchoredPosition += new Vector2(offset, 0);
                    AssignPositioningIfNeeded();
                }

                m_HasTextBeenRemoved = false;
            }

            m_forceRectTransformAdjustment = false;
        }

        /// <summary>
        /// Validate the specified input.
        /// </summary>
        protected char Validate(string text, int pos, char ch)
        {
            // Validation is disabled
            if (characterValidation == CharacterValidation.None || !enabled)
                return ch;

            if (characterValidation == CharacterValidation.Integer || characterValidation == CharacterValidation.Decimal)
            {
                // Integer and decimal
                bool cursorBeforeDash = (pos == 0 && text.Length > 0 && text[0] == '-');
                bool selectionAtStart = stringPositionInternal == 0 || stringSelectPositionInternal == 0;
                if (!cursorBeforeDash)
                {
                    if (ch >= '0' && ch <= '9') return ch;
                    if (ch == '-' && (pos == 0 || selectionAtStart) && !text.Contains('-')) return ch;

                    var separator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                    if (ch == Convert.ToChar(separator) && characterValidation == CharacterValidation.Decimal && !text.Contains(separator)) return ch;

                    //Some keyboards including Samsung require double tapping a . to get a - this allows these keyboards to input negative integers
                    if (characterValidation == CharacterValidation.Integer && ch == '.' && (pos == 0 || selectionAtStart) && !text.Contains('-')) return '-';
                }

            }
            else if (characterValidation == CharacterValidation.Digit)
            {
                if (ch >= '0' && ch <= '9') return ch;
            }
            else if (characterValidation == CharacterValidation.Alphanumeric)
            {
                // All alphanumeric characters
                if (ch >= 'A' && ch <= 'Z') return ch;
                if (ch >= 'a' && ch <= 'z') return ch;
                if (ch >= '0' && ch <= '9') return ch;
            }
            else if (characterValidation == CharacterValidation.Name)
            {
                char prevChar = (text.Length > 0) ? text[Mathf.Clamp(pos - 1, 0, text.Length - 1)] : ' ';
                char lastChar = (text.Length > 0) ? text[Mathf.Clamp(pos, 0, text.Length - 1)] : ' ';
                char nextChar = (text.Length > 0) ? text[Mathf.Clamp(pos + 1, 0, text.Length - 1)] : '\n';

                if (char.IsLetter(ch))
                {
                    // First letter is always capitalized
                    if (char.IsLower(ch) && pos == 0)
                        return char.ToUpper(ch);

                    // Letter following a space or hyphen is always capitalized
                    if (char.IsLower(ch) && (prevChar == ' ' || prevChar == '-'))
                        return char.ToUpper(ch);

                    // Uppercase letters are only allowed after spaces, apostrophes, hyphens or lowercase letter
                    if (char.IsUpper(ch) && pos > 0 && prevChar != ' ' && prevChar != '\'' && prevChar != '-' && !char.IsLower(prevChar))
                        return char.ToLower(ch);

                    // Do not allow uppercase characters to be inserted before another uppercase character
                    if (char.IsUpper(ch) && char.IsUpper(lastChar))
                        return (char)0;

                    // If character was already in correct case, return it as-is.
                    // Also, letters that are neither upper nor lower case are always allowed.
                    return ch;
                }
                else if (ch == '\'')
                {
                    // Don't allow more than one apostrophe
                    if (lastChar != ' ' && lastChar != '\'' && nextChar != '\'' && !text.Contains("'"))
                        return ch;
                }

                // Allow inserting a hyphen after a character
                if (char.IsLetter(prevChar) && ch == '-' && lastChar != '-')
                {
                    return ch;
                }

                if ((ch == ' ' || ch == '-') && pos != 0)
                {
                    // Don't allow more than one space in a row
                    if (prevChar != ' ' && prevChar != '\'' && prevChar != '-' &&
                        lastChar != ' ' && lastChar != '\'' && lastChar != '-' &&
                        nextChar != ' ' && nextChar != '\'' && nextChar != '-')
                        return ch;
                }
            }
            else if (characterValidation == CharacterValidation.EmailAddress)
            {
                // From StackOverflow about allowed characters in email addresses:
                // Uppercase and lowercase English letters (a-z, A-Z)
                // Digits 0 to 9
                // Characters ! # $ % & ' * + - / = ? ^ _ ` { | } ~
                // Character . (dot, period, full stop) provided that it is not the first or last character,
                // and provided also that it does not appear two or more times consecutively.

                if (ch >= 'A' && ch <= 'Z') return ch;
                if (ch >= 'a' && ch <= 'z') return ch;
                if (ch >= '0' && ch <= '9') return ch;
                if (ch == '@' && text.IndexOf('@') == -1) return ch;
                if (kEmailSpecialCharacters.IndexOf(ch) != -1) return ch;
                if (ch == '.')
                {
                    char lastChar = (text.Length > 0) ? text[Mathf.Clamp(pos, 0, text.Length - 1)] : ' ';
                    char nextChar = (text.Length > 0) ? text[Mathf.Clamp(pos + 1, 0, text.Length - 1)] : '\n';
                    if (lastChar != '.' && nextChar != '.')
                        return ch;
                }
            }
            else if (characterValidation == CharacterValidation.Regex)
            {
                // Regex expression
                if (Regex.IsMatch(ch.ToString(), m_RegexValue))
                {
                    return ch;
                }
            }
            else if (characterValidation == CharacterValidation.CustomValidator)
            {
                if (m_InputValidator != null)
                {
                    char c = m_InputValidator.Validate(ref text, ref pos, ch);
                    m_Text = text;
                    stringSelectPositionInternal = stringPositionInternal = pos;
                    return c;
                }
            }
            return (char)0;
        }

        public void ActivateInputField()
        {
            if (m_TextComponent == null || m_TextComponent.font == null || !IsActive() || !IsInteractable())
                return;

            if (isFocused)
            {
                if (m_SoftKeyboard != null && !m_SoftKeyboard.active)
                {
                    m_SoftKeyboard.active = true;
                    m_SoftKeyboard.text = m_Text;
                }
            }

            m_ShouldActivateNextUpdate = true;
        }

        private void ActivateInputFieldInternal()
        {
            if (EventSystem.current == null)
                return;

            if (EventSystem.current.currentSelectedGameObject != gameObject)
                EventSystem.current.SetSelectedGameObject(gameObject);

            // Cache the value of isInPlaceEditingAllowed, because on UWP this involves calling into native code
            // The value only needs to be updated once when the TouchKeyboard is opened.
            m_TouchKeyboardAllowsInPlaceEditing = !s_IsQuestDevice && TouchScreenKeyboard.isInPlaceEditingAllowed;

            if (TouchScreenKeyboardShouldBeUsed() && shouldHideSoftKeyboard == false)
            {
                if (inputSystem != null && inputSystem.touchSupported)
                {
                    TouchScreenKeyboard.hideInput = shouldHideMobileInput;
                }

                if (shouldHideSoftKeyboard == false && m_ReadOnly == false)
                {
                    m_SoftKeyboard = (inputType == InputType.Password) ?
                        TouchScreenKeyboard.Open(m_Text, keyboardType, false, multiLine, true, isAlert, "", characterLimit) :
                        TouchScreenKeyboard.Open(m_Text, keyboardType, inputType == InputType.AutoCorrect, multiLine, false, isAlert, "", characterLimit);

                    OnFocus();

                    // Opening the soft keyboard sets its selection to the end of the text.
                    // As such, we set the selection to match the Input Field's internal selection.
                    if (m_SoftKeyboard != null && m_SoftKeyboard.canSetSelection)
                    {
                        int length = stringPositionInternal < stringSelectPositionInternal ? stringSelectPositionInternal - stringPositionInternal : stringPositionInternal - stringSelectPositionInternal;
                        m_SoftKeyboard.selection = new RangeInt(stringPositionInternal < stringSelectPositionInternal ? stringPositionInternal : stringSelectPositionInternal, length);
                    }
                    //}
                }
            }
            else
            {
                if (!TouchScreenKeyboardShouldBeUsed() && m_ReadOnly == false && inputSystem != null)
                    inputSystem.imeCompositionMode = IMECompositionMode.On;

                OnFocus();
            }

            m_AllowInput = true;
            m_OriginalText = text;
            m_WasCanceled = false;
            SetCaretVisible();
            UpdateLabel();
        }

        public override void OnSelect(BaseEventData eventData)
        {
            //Debug.Log("OnSelect()");

            base.OnSelect(eventData);
            SendOnFocus();

            if (shouldActivateOnSelect)
                ActivateInputField();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            //Debug.Log("Pointer Click Event...");

            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            ActivateInputField();
        }

        public void OnControlClick()
        {
            //Debug.Log("Input Field control click...");
        }

        public void ReleaseSelection()
        {
            m_SelectionStillActive = false;
            m_ReleaseSelection = false;
            m_PreviouslySelectedObject = null;

            MarkGeometryAsDirty();

            SendOnEndEdit();
            SendOnEndTextSelection();
        }

        public void DeactivateInputField(bool clearSelection = false)
        {
            //Debug.Log("Deactivate Input Field...");

            // Not activated do nothing.
            if (!m_AllowInput)
                return;

            m_HasDoneFocusTransition = false;
            m_AllowInput = false;

            if (m_Placeholder != null)
                m_Placeholder.enabled = string.IsNullOrEmpty(m_Text);

            if (m_TextComponent != null && IsInteractable())
            {
                if (m_WasCanceled && m_RestoreOriginalTextOnEscape && !m_IsKeyboardBeingClosedInHoloLens)
                    text = m_OriginalText;

                if (m_SoftKeyboard != null)
                {
                    m_SoftKeyboard.active = false;
                    m_SoftKeyboard = null;
                }

                m_SelectionStillActive = true;

                if (m_ResetOnDeActivation || m_ReleaseSelection || clearSelection)
                {
                    //m_StringPosition = m_StringSelectPosition = 0;
                    //m_CaretPosition = m_CaretSelectPosition = 0;
                    //m_TextComponent.rectTransform.localPosition = m_DefaultTransformPosition;

                    if (m_VerticalScrollbar == null)
                        ReleaseSelection();
                }

                if (inputSystem != null)
                    inputSystem.imeCompositionMode = IMECompositionMode.Auto;

				m_IsKeyboardBeingClosedInHoloLens = false;
            }

            MarkGeometryAsDirty();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            DeactivateInputField();

            base.OnDeselect(eventData);
            SendOnFocusLost();
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            //Debug.Log("OnSubmit()");

            if (!IsActive() || !IsInteractable())
                return;

            if (!isFocused)
                m_ShouldActivateNextUpdate = true;

            SendOnSubmit();
            DeactivateInputField();
            eventData?.Use();
        }

        public virtual void OnCancel(BaseEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
                return;

            if (!isFocused)
                m_ShouldActivateNextUpdate = true;

            m_WasCanceled = true;
            DeactivateInputField();
            eventData.Use();
        }

        public override void OnMove(AxisEventData eventData)
        {
            // Prevent UI navigation while text is being edited.
            if (!m_AllowInput)
                base.OnMove(eventData);
        }

        //public virtual void OnLostFocus(BaseEventData eventData)
        //{
        //    if (!IsActive() || !IsInteractable())
        //        return;
        //}

        private void EnforceContentType()
        {
            switch (contentType)
            {
                case ContentType.Standard:
                    {
                        // Don't enforce line type for this content type.
                        m_InputType = InputType.Standard;
                        m_KeyboardType = TouchScreenKeyboardType.Default;
                        m_CharacterValidation = CharacterValidation.None;
                        break;
                    }
                case ContentType.Autocorrected:
                    {
                        // Don't enforce line type for this content type.
                        m_InputType = InputType.AutoCorrect;
                        m_KeyboardType = TouchScreenKeyboardType.Default;
                        m_CharacterValidation = CharacterValidation.None;
                        break;
                    }
                case ContentType.IntegerNumber:
                    {
                        m_LineType = LineType.SingleLine;
                        m_InputType = InputType.Standard;
                        m_KeyboardType = TouchScreenKeyboardType.NumbersAndPunctuation;
                        m_CharacterValidation = CharacterValidation.Integer;
                        break;
                    }
                case ContentType.DecimalNumber:
                    {
                        m_LineType = LineType.SingleLine;
                        m_InputType = InputType.Standard;
                        m_KeyboardType = TouchScreenKeyboardType.NumbersAndPunctuation;
                        m_CharacterValidation = CharacterValidation.Decimal;
                        break;
                    }
                case ContentType.Alphanumeric:
                    {
                        m_LineType = LineType.SingleLine;
                        m_InputType = InputType.Standard;
                        m_KeyboardType = TouchScreenKeyboardType.ASCIICapable;
                        m_CharacterValidation = CharacterValidation.Alphanumeric;
                        break;
                    }
                case ContentType.Name:
                    {
                        m_LineType = LineType.SingleLine;
                        m_InputType = InputType.Standard;
                        m_KeyboardType = TouchScreenKeyboardType.Default;
                        m_CharacterValidation = CharacterValidation.Name;
                        break;
                    }
                case ContentType.EmailAddress:
                    {
                        m_LineType = LineType.SingleLine;
                        m_InputType = InputType.Standard;
                        m_KeyboardType = TouchScreenKeyboardType.EmailAddress;
                        m_CharacterValidation = CharacterValidation.EmailAddress;
                        break;
                    }
                case ContentType.Password:
                    {
                        m_LineType = LineType.SingleLine;
                        m_InputType = InputType.Password;
                        m_KeyboardType = TouchScreenKeyboardType.Default;
                        m_CharacterValidation = CharacterValidation.None;
                        break;
                    }
                case ContentType.Pin:
                    {
                        m_LineType = LineType.SingleLine;
                        m_InputType = InputType.Password;
                        m_KeyboardType = TouchScreenKeyboardType.NumberPad;
                        m_CharacterValidation = CharacterValidation.Digit;
                        break;
                    }
                default:
                    {
                        // Includes Custom type. Nothing should be enforced.
                        break;
                    }
            }

            SetTextComponentWrapMode();
        }

        void SetTextComponentWrapMode()
        {
            if (m_TextComponent == null)
                return;

            if (multiLine)
                m_TextComponent.textWrappingMode = TextWrappingModes.Normal;
            else
                m_TextComponent.textWrappingMode = TextWrappingModes.PreserveWhitespaceNoWrap;
        }

        // Control Rich Text option on the text component.
        void SetTextComponentRichTextMode()
        {
            if (m_TextComponent == null)
                return;

            m_TextComponent.richText = m_RichText;
        }

        void SetToCustomIfContentTypeIsNot(params ContentType[] allowedContentTypes)
        {
            if (contentType == ContentType.Custom)
                return;

            for (int i = 0; i < allowedContentTypes.Length; i++)
                if (contentType == allowedContentTypes[i])
                    return;

            contentType = ContentType.Custom;
        }

        void SetToCustom()
        {
            if (contentType == ContentType.Custom)
                return;

            contentType = ContentType.Custom;
        }

        void SetToCustom(CharacterValidation characterValidation)
        {
            if (contentType == ContentType.Custom)
            {
                characterValidation = CharacterValidation.CustomValidator;
                return;
            }

            contentType = ContentType.Custom;
            characterValidation = CharacterValidation.CustomValidator;
        }


        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (m_HasDoneFocusTransition)
                state = SelectionState.Selected;
            else if (state == SelectionState.Pressed)
                m_HasDoneFocusTransition = true;

            base.DoStateTransition(state, instant);
        }


        /// <summary>
        /// See ILayoutElement.CalculateLayoutInputHorizontal.
        /// </summary>
        public virtual void CalculateLayoutInputHorizontal()
        { }

        /// <summary>
        /// See ILayoutElement.CalculateLayoutInputVertical.
        /// </summary>
        public virtual void CalculateLayoutInputVertical()
        { }

        /// <summary>
        /// See ILayoutElement.minWidth.
        /// </summary>
        public virtual float minWidth { get { return 0; } }

        /// <summary>
        /// Get the displayed with of all input characters.
        /// </summary>
        public virtual float preferredWidth
        {
            get
            {
                if (textComponent == null)
                    return 0;

                float horizontalPadding = 0;

                if (m_LayoutGroup != null)
                    horizontalPadding = m_LayoutGroup.padding.horizontal;

                if (m_TextViewport != null)
                    horizontalPadding += m_TextViewport.offsetMin.x - m_TextViewport.offsetMax.x;

                return m_TextComponent.preferredWidth + horizontalPadding; // Should add some extra padding for caret
            }
        }

        /// <summary>
        /// See ILayoutElement.flexibleWidth.
        /// </summary>
        public virtual float flexibleWidth { get { return -1; } }

        /// <summary>
        /// See ILayoutElement.minHeight.
        /// </summary>
        public virtual float minHeight { get { return 0; } }

        /// <summary>
        /// Get the height of all the text if constrained to the height of the RectTransform.
        /// </summary>
        public virtual float preferredHeight
        {
            get
            {
                if (textComponent == null)
                    return 0;

                float verticalPadding = 0;

                if (m_LayoutGroup != null)
                    verticalPadding = m_LayoutGroup.padding.vertical;

                if (m_TextViewport != null)
                    verticalPadding += m_TextViewport.offsetMin.y - m_TextViewport.offsetMax.y;

                return m_TextComponent.preferredHeight + verticalPadding;
            }
        }

        /// <summary>
        /// See ILayoutElement.flexibleHeight.
        /// </summary>
        public virtual float flexibleHeight { get { return -1; } }

        /// <summary>
        /// See ILayoutElement.layoutPriority.
        /// </summary>
        public virtual int layoutPriority { get { return 1; } }


        /// <summary>
        /// Function to conveniently set the point size of both Placeholder and Input Field text object.
        /// </summary>
        /// <param name="pointSize"></param>
        public void SetGlobalPointSize(float pointSize)
        {
            TMP_Text placeholderTextComponent = m_Placeholder as TMP_Text;

            if (placeholderTextComponent != null)
                placeholderTextComponent.fontSize = pointSize;

            textComponent.fontSize = pointSize;
        }

        /// <summary>
        /// Function to conveniently set the Font Asset of both Placeholder and Input Field text object.
        /// </summary>
        /// <param name="fontAsset"></param>
        public void SetGlobalFontAsset(TMP_FontAsset fontAsset)
        {
            TMP_Text placeholderTextComponent = m_Placeholder as TMP_Text;

            if (placeholderTextComponent != null)
                placeholderTextComponent.font = fontAsset;

            textComponent.font = fontAsset;
        }

    }


    static class SetPropertyUtility
    {
        public static bool SetColor(ref Color currentValue, Color newValue)
        {
            if (currentValue.r == newValue.r && currentValue.g == newValue.g && currentValue.b == newValue.b && currentValue.a == newValue.a)
                return false;

            currentValue = newValue;
            return true;
        }

        public static bool SetEquatableStruct<T>(ref T currentValue, T newValue) where T : IEquatable<T>
        {
            if (currentValue.Equals(newValue))
                return false;

            currentValue = newValue;
            return true;
        }

        public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
        {
            if (currentValue.Equals(newValue))
                return false;

            currentValue = newValue;
            return true;
        }

        public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return false;

            currentValue = newValue;
            return true;
        }
    }
}
