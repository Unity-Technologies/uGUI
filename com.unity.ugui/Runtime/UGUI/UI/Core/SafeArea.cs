using System;
using System.Runtime.CompilerServices;

namespace UnityEngine.UI
{
    /// <summary>
    /// A component that drives a RectTransform to fit within the device's safe area.
    /// </summary>
    /// <remarks>
    /// Inset edges, alignment centering, and reference orientation are configurable. Edge and alignment
    /// directions are authored in the reference orientation and remapped to the current device orientation at runtime.
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("UI (Canvas)/Safe Area")]
    [ExecuteAlways]
    [UGUIHelpURL("SafeArea")]
    public class SafeArea : MonoBehaviour
    {
        private RectTransform m_RectTransform;
        private DrivenRectTransformTracker m_Tracker;

        /// <summary>
        /// Whether to respect the safe area of the device. Relative to Reference Orientation.
        /// </summary>
        [Flags]
        public enum SafeAreaMode
        {
            /// <summary>
            /// Inset the top edge to avoid the device's top safe area (e.g. notch, status bar).
            /// </summary>
            Top = 1 << 0,
            /// <summary>
            /// Inset the right edge to avoid the device's right safe area.
            /// </summary>
            Right = 1 << 1,
            /// <summary>
            /// Inset the bottom edge to avoid the device's bottom safe area (e.g. home indicator).
            /// </summary>
            Bottom = 1 << 2,
            /// <summary>
            /// Inset the left edge to avoid the device's left safe area.
            /// </summary>
            Left = 1 << 3
        }

        /// <summary>
        /// Directions to align the inset area to center the UI. Relative to Reference Orientation.
        /// </summary>
        [Flags]
        public enum AlignmentMode
        {
            /// <summary>
            /// Center the inset area horizontally by mirroring the larger horizontal inset onto the opposite edge.
            /// </summary>
            CenterHorizontally = 1 << 0,
            /// <summary>
            /// Center the inset area vertically by mirroring the larger vertical inset onto the opposite edge.
            /// </summary>
            CenterVertically = 1 << 1,
        }


        private enum ScreenOrientation
        {
            Portrait = 0,
            LandscapeLeft = 1,
            PortraitUpsideDown = 2,
            LandscapeRight = 3,
        }

        // Rotates the 4-bit flag value by shift steps (supports negative shifts via wrap-around)
        internal static SafeAreaMode RotateFlag(SafeAreaMode mode, int shift)
        {
            int bits = (int)mode & 0b1111;

            // Normalize shift to [0, 3] — handles negatives and values > 3
            int normalizedShift = (shift % 4 + 4) % 4;
            int inverseShift = 4 - normalizedShift;

            // Circular bit rotation within a 4-bit space
            int shiftedLeft  = bits << normalizedShift;       // bits that stay in range
            int shiftedRight = bits >> inverseShift;          // bits that wrap around to low end
            int rotated      = (shiftedLeft | shiftedRight) & 0b1111; // merge and clamp to 4 bits

            return (SafeAreaMode)rotated;
        }

        /// <summary>
        /// Rotations needed to map a current-space direction back to reference/authoring space.
        /// </summary>
        private int RotationsFromCurrentToReference => ((int)CurrentOrientation - (int)m_ReferenceOrientation + 4) % 4;

        /// <summary>
        /// Rotations needed to map a reference/authoring-space direction to current screen space.
        /// </summary>
        private int RotationsFromReferenceToCurrent => -RotationsFromCurrentToReference;

        /// <summary>
        /// The current orientation in local enum.
        /// </summary>
        private ScreenOrientation CurrentOrientation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ToLocalScreenOrientation(Screen.orientation);
        }

        /// <summary>
        /// Whether this UI component respects (inset to) the safe area on the screen's left edge
        /// for the current device orientation.
        /// </summary>
        public bool RespectSafeAreaScreenLeft
            => HasFlag(m_Edges, RotateFlag(SafeAreaMode.Left, RotationsFromCurrentToReference));

        /// <summary>
        /// Whether this UI component respects (inset to) the safe area on the screen's right edge
        /// for the current device orientation.
        /// </summary>
        public bool RespectSafeAreaScreenRight
            => HasFlag(m_Edges, RotateFlag(SafeAreaMode.Right, RotationsFromCurrentToReference));

        /// <summary>
        /// Whether this UI component respects (inset to) the safe area on the screen's bottom edge
        /// for the current device orientation.
        /// </summary>
        public bool RespectSafeAreaScreenBottom
            => HasFlag(m_Edges, RotateFlag(SafeAreaMode.Bottom, RotationsFromCurrentToReference));

        /// <summary>
        /// Whether this UI component respects (inset to) the safe area on the screen's top edge
        /// for the current device orientation.
        /// </summary>
        public bool RespectSafeAreaScreenTop
            => HasFlag(m_Edges, RotateFlag(SafeAreaMode.Top, RotationsFromCurrentToReference));


        [Tooltip("The orientation that the Edges and Alignment directions are authored against. At runtime, the component remaps those directions to match the current device orientation.")]
        [SerializeField] private ScreenOrientation m_ReferenceOrientation;

        [Tooltip("Edges to inset to respect the safe area. Directions are with respect to the reference orientation.")]
        [SerializeField] private SafeAreaMode m_Edges;

        [Tooltip("Align the inset to center the UI area. Directions are with respect to the reference orientation.")]
        [SerializeField] private AlignmentMode m_Alignment;

        [NonSerialized] private ScreenOrientation m_PreviousReferenceOrientation;
        [NonSerialized] private SafeAreaMode m_PreviousEdges;
        [NonSerialized] private AlignmentMode m_PreviousAlignment;
        [NonSerialized] private Rect m_PreviousSafeArea;
        [NonSerialized] private Vector2Int m_PreviousResolution;
        [NonSerialized] private UnityEngine.ScreenOrientation m_PreviousOrientation;

        /// <summary>
        /// The reference orientation to determine edges to respect safe area
        /// </summary>
        public UnityEngine.ScreenOrientation ReferenceOrientation
        {
            get => ToUnityScreenOrientation(m_ReferenceOrientation);
            set => m_ReferenceOrientation = ToLocalScreenOrientation(value);
        }

        /// <summary>
        /// Controls which edges of the device to inset to respect the safe area
        /// </summary>
        public SafeAreaMode Edges
        {
            get { return m_Edges; }
            set { m_Edges = value; }
        }

        /// <summary>
        /// Controls which axis to center the rect by equalizing insets on both sides
        /// </summary>
        public AlignmentMode Alignment
        {
            get { return m_Alignment; }
            set { m_Alignment = value; }
        }


        private void Awake()
        {
            m_RectTransform = transform as RectTransform;
            m_Tracker = new DrivenRectTransformTracker();
        }

        private void OnEnable()
        {
            if (m_RectTransform == null)
            {
                m_RectTransform = transform as RectTransform;
            }

            if (m_RectTransform.drivenByObject == null || m_RectTransform.drivenByObject == this)
            {
                ClaimRectTransformDrivenOwnership();
                ApplySafeArea();
            }
        }

        private void OnDisable()
        {
            SafeClearDrivenRectTransformTracker();
        }

        private void OnDestroy()
        {
            SafeClearDrivenRectTransformTracker();
        }

        private void SafeClearDrivenRectTransformTracker()
        {
            if (m_RectTransform == null)
            {
                m_RectTransform = transform as RectTransform;
            }
            if (m_RectTransform != null && m_RectTransform.drivenByObject == this)
            {
                m_Tracker.Clear();
            }
        }

        private void Update()
        {
            if (m_RectTransform.drivenByObject == null)
            {
                ClaimRectTransformDrivenOwnership();
                ApplySafeArea();
                return;
            }
            // Yield and warn about conflicting rectTransform drivers (In custom Editor)
            if (m_RectTransform.drivenByObject != this)
                return;


            if (Screen.safeArea != m_PreviousSafeArea ||
                Screen.width != m_PreviousResolution.x ||
                Screen.height != m_PreviousResolution.y ||
                Screen.orientation != m_PreviousOrientation ||
                m_PreviousReferenceOrientation != m_ReferenceOrientation ||
                m_PreviousEdges != m_Edges ||
                m_PreviousAlignment != m_Alignment ||
                HasNaNDrivenValues())
            {
                ApplySafeArea();
            }
        }

        private void ClaimRectTransformDrivenOwnership()
        {
            m_Tracker.Add(this, m_RectTransform,
                DrivenTransformProperties.AnchorMax | DrivenTransformProperties.AnchorMin |
                DrivenTransformProperties.SizeDelta | DrivenTransformProperties.AnchoredPosition);
        }

        private void ApplySafeArea()
        {
            var safe = Screen.safeArea;
            if (safe.width == 0 || safe.height == 0 || Screen.width == 0 || Screen.height == 0)
                return; //Initialization guard. Safe area will be performed on the next frame.

            UpdatePreviousDataCache();

            bool isLandscape = m_PreviousOrientation is UnityEngine.ScreenOrientation.LandscapeLeft
                or UnityEngine.ScreenOrientation.LandscapeRight;
            bool isReferenceLandscape =
                m_ReferenceOrientation is ScreenOrientation.LandscapeLeft or ScreenOrientation.LandscapeRight;

            var respectSafeAreaScreenEdges = RotateFlag(m_Edges, RotationsFromReferenceToCurrent);
            var isAlignmentFlipped = isLandscape != isReferenceLandscape;

            var (min, max) = CalculateAnchors(m_PreviousResolution.x, m_PreviousResolution.y, m_PreviousSafeArea, respectSafeAreaScreenEdges, Alignment, isAlignmentFlipped);

            m_RectTransform.anchorMin = min;
            m_RectTransform.anchorMax = max;
            m_RectTransform.offsetMin = Vector2.zero;
            m_RectTransform.offsetMax = Vector2.zero;

        }

        internal static (Vector2 min, Vector2 max) CalculateAnchors(int screenWidth, int screenHeight, Rect safeArea, SafeAreaMode respectSafeAreaScreenEdges, AlignmentMode alignmentMode, bool isAlignmentFlipped)
        {
            Vector2 min = safeArea.position;
            Vector2 max = safeArea.position + safeArea.size;

            var horizontalAlignmentMode = isAlignmentFlipped
                ? AlignmentMode.CenterVertically
                : AlignmentMode.CenterHorizontally;
            var verticalAlignmentMode = isAlignmentFlipped
                ? AlignmentMode.CenterHorizontally
                : AlignmentMode.CenterVertically;

            // X axis
            if (!HasFlag(respectSafeAreaScreenEdges, SafeAreaMode.Left))
                min.x = 0f;
            if (!HasFlag(respectSafeAreaScreenEdges, SafeAreaMode.Right))
                max.x = screenWidth;
            if (HasFlag(alignmentMode, horizontalAlignmentMode))
            {
                var maxOffsetX = Mathf.Max(min.x, screenWidth - max.x);
                min.x = maxOffsetX;
                max.x = screenWidth - maxOffsetX;
            }

            // Y axis
            if (!HasFlag(respectSafeAreaScreenEdges, SafeAreaMode.Bottom))
                min.y = 0f;
            if (!HasFlag(respectSafeAreaScreenEdges, SafeAreaMode.Top))
                max.y = screenHeight;
            if (HasFlag(alignmentMode, verticalAlignmentMode))
            {
                var maxOffsetY = Mathf.Max(min.y, screenHeight - max.y);
                min.y = maxOffsetY;
                max.y = screenHeight - maxOffsetY;
            }

            min.x /= screenWidth;
            min.y /= screenHeight;
            max.x /= screenWidth;
            max.y /= screenHeight;

            return (min, max);
        }

        private void UpdatePreviousDataCache()
        {
            m_PreviousSafeArea = Screen.safeArea;
            m_PreviousResolution = new Vector2Int(Screen.width, Screen.height);
            m_PreviousOrientation = Screen.orientation;
            m_PreviousReferenceOrientation = m_ReferenceOrientation;
            m_PreviousAlignment = m_Alignment;
            m_PreviousEdges = m_Edges;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (m_RectTransform == null)
                m_RectTransform = transform as RectTransform;

            if (TryGetComponent<ILayoutController>(out var layout) && layout is Behaviour behaviour && behaviour.enabled)
            {
                Debug.LogWarning(
                    $"'{GetType().Name}' conflicts with '{layout.GetType().Name}' on '{name}'.",
                    this);
            }
            var driver = m_RectTransform == null ? null : m_RectTransform.drivenByObject;
            if (driver != this && driver != null)
            {
                var component = (Component)driver;
                Debug.LogWarning(
                    $"'{GetType().Name}' conflicts with '{driver.GetType().Name}' on '{component.gameObject.name}'.",
                    this);
            }
        }
#endif


        /// <summary>
        /// Converts Unity's <see cref="UnityEngine.ScreenOrientation"/> to SafeArea's <see cref="ScreenOrientation"/>.
        /// </summary>
        private static ScreenOrientation ToLocalScreenOrientation(UnityEngine.ScreenOrientation orientation) =>
            orientation switch
            {
                UnityEngine.ScreenOrientation.Portrait => ScreenOrientation.Portrait,
                UnityEngine.ScreenOrientation.PortraitUpsideDown => ScreenOrientation.PortraitUpsideDown,
                UnityEngine.ScreenOrientation.LandscapeLeft => ScreenOrientation.LandscapeLeft,
                UnityEngine.ScreenOrientation.LandscapeRight => ScreenOrientation.LandscapeRight,
                _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation,
                    "Unsupported Unity screen orientation.")
            };

        /// <summary>
        /// Converts SafeArea's <see cref="ScreenOrientation"/> to Unity's <see cref="UnityEngine.ScreenOrientation"/>.
        /// </summary>
        private static UnityEngine.ScreenOrientation ToUnityScreenOrientation(ScreenOrientation orientation) =>
            orientation switch
            {
                ScreenOrientation.Portrait => UnityEngine.ScreenOrientation.Portrait,
                ScreenOrientation.PortraitUpsideDown => UnityEngine.ScreenOrientation.PortraitUpsideDown,
                ScreenOrientation.LandscapeLeft => UnityEngine.ScreenOrientation.LandscapeLeft,
                ScreenOrientation.LandscapeRight => UnityEngine.ScreenOrientation.LandscapeRight,
                _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation,
                    "Unsupported SafeArea screen orientation.")
            };

        /// <summary>
        /// Calculate and return the orientation that any given reference orientation direction is mapped to.
        /// </summary>
        /// <param name="referenceOrientationDirection">The direction in reference orientation space to map.</param>
        /// <returns>The direction in screen space for the current device orientation.</returns>
        public SafeAreaMode GetReferenceOrientationMappedDirection(SafeAreaMode referenceOrientationDirection) =>
            RotateFlag(referenceOrientationDirection, RotationsFromReferenceToCurrent);

        private bool HasNaNDrivenValues()
        {
            return float.IsNaN(m_RectTransform.anchorMin.x) || float.IsNaN(m_RectTransform.anchorMin.y) ||
                   float.IsNaN(m_RectTransform.anchorMax.x) || float.IsNaN(m_RectTransform.anchorMax.y);
        }

        private static bool HasFlag(SafeAreaMode value, SafeAreaMode flag)
        {
            return (value & flag) == flag;
        }
        private static bool HasFlag(AlignmentMode value, AlignmentMode flag)
        {
            return (value & flag) == flag;
        }
    }
}
