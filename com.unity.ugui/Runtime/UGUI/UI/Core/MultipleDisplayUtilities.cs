using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    internal static class MultipleDisplayUtilities
    {
        /// <summary>
        /// Converts the current drag position into a relative position for the display.
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="position"></param>
        /// <returns>Returns true except when the drag operation is not on the same display as it originated</returns>
        public static bool GetRelativeMousePositionForDrag(PointerEventData eventData, ref Vector2 position)
        {
            #if UNITY_EDITOR
            position = eventData.position;
            #else
            int pressDisplayIndex = eventData.pointerPressRaycast.displayIndex;
            var relativePosition = RelativeMouseAtScaled(eventData.position, eventData.displayIndex);
            int currentDisplayIndex = (int)relativePosition.z;

            // Discard events on a different display.
            if (currentDisplayIndex != pressDisplayIndex)
                return false;

            // If we are not on the main display then we must use the relative position.
            position = pressDisplayIndex != 0 ? (Vector2)relativePosition : eventData.position;
            #endif
            return true;
        }

        internal static Vector3 GetRelativeMousePositionForRaycast(PointerEventData eventData)
        {
            // The multiple display system is not supported on all platforms, when it is not supported the returned position
            // will be all zeros so when the returned index is 0 we will default to the event data to be safe.
            Vector3 eventPosition = RelativeMouseAtScaled(eventData.position, eventData.displayIndex);
            if (eventPosition == Vector3.zero)
            {
                eventPosition = eventData.position;
#if UNITY_EDITOR
                eventPosition.z = Display.activeEditorGameViewTarget;
#endif
                // We don't really know in which display the event occurred. We will process the event assuming it occurred in our display.
            }

            // We support multiple display on some platforms. When supported:
            //  - InputSystem will set eventData.displayIndex
            //  - Old Input System will set eventPosition.z
            //
            // If the event is on the main display, both displayIndex and eventPosition.z
            // will be 0 so in that case we can leave the eventPosition untouched (see UUM-47650).
#if ENABLE_INPUT_SYSTEM && PACKAGE_INPUTSYSTEM
            if (eventData.displayIndex > 0)
                eventPosition.z = eventData.displayIndex;
#endif

            return eventPosition;
        }

        /// <summary>
        /// A version of Display.RelativeMouseAt that scales the position when the main display has a different rendering resolution to the system resolution.
        /// By default, the mouse position is relative to the main render area, we need to adjust this so it is relative to the system resolution
        /// in order to correctly determine the position on other displays.
        /// </summary>
        /// <returns></returns>
        public static Vector3 RelativeMouseAtScaled(Vector2 position, int displayIndex)
        {
            #if !UNITY_EDITOR && !UNITY_WSA
            // For most platforms, if the main display is not the same resolution as the system then we will have to scale the mouse position. (case 1141732)
            var display = Display.main;
#if ENABLE_INPUT_SYSTEM && PACKAGE_INPUTSYSTEM
            if (displayIndex >= Display.displays.Length)
                displayIndex = 0;

            // With the new input system, passed positions are always relative to a surface and scaled accordingly to the rendering resolution.
            display = Display.displays[displayIndex];
            // So, if not in fullscreen, assume UaaL multi-view multi-screen multi-touch scenario, where the position is already in the correct scaled coordinates for the displayIndex
            if (!Screen.fullScreen)
            {
                return new Vector3(position.x, position.y, displayIndex);
            }
            // Otherwise, in full screen, we add some padding if rendering and system resolution differs, as for other platforms' main display. (So behavior is unchanged for Android main display, untested for non-main displays)
#endif
            if (display.renderingWidth != display.systemWidth || display.renderingHeight != display.systemHeight)
            {
                // The system will add padding when in full-screen and using a non-native aspect ratio. (case UUM-7893)
                // For example Rendering 1920x1080 with a systeem resolution of 3440x1440 would create black bars on each side that are 330 pixels wide.
                // we need to account for this or it will offset our coordinates when we are not on the main display.
                var systemAspectRatio = display.systemWidth / (float)display.systemHeight;

                var sizePlusPadding = new Vector2(display.renderingWidth, display.renderingHeight);
                var padding = Vector2.zero;
                if (Screen.fullScreen)
                {
                    var aspectRatio = Screen.width / (float)Screen.height; // This assumes aspectRatio is the same for all displays
                    if (display.systemHeight * aspectRatio < display.systemWidth)
                    {
                        // Horizontal padding
                        sizePlusPadding.x = display.renderingHeight * systemAspectRatio;
                        padding.x = (sizePlusPadding.x - display.renderingWidth) * 0.5f;
                    }
                    else
                    {
                        // Vertical padding
                        sizePlusPadding.y = display.renderingWidth / systemAspectRatio;
                        padding.y = (sizePlusPadding.y - display.renderingHeight) * 0.5f;
                    }
                }

                var sizePlusPositivePadding = sizePlusPadding - padding;

                // If we are not inside of the main display then we must adjust the mouse position so it is scaled by
                // the main display and adjusted for any padding that may have been added due to different aspect ratios.
                if (position.y < -padding.y || position.y > sizePlusPositivePadding.y ||
                     position.x < -padding.x || position.x > sizePlusPositivePadding.x)
                {
                    var adjustedPosition = position;

                    if (!Screen.fullScreen)
                    {
                        // When in windowed mode, the window will be centered with the 0,0 coordinate at the top left, we need to adjust so it is relative to the screen instead.
                        adjustedPosition.x -= (display.renderingWidth - display.systemWidth) * 0.5f;
                        adjustedPosition.y -= (display.renderingHeight - display.systemHeight) * 0.5f;
                    }
                    else
                    {
                        // Scale the mouse position to account for the black bars when in a non-native aspect ratio.
                        adjustedPosition += padding;
                        adjustedPosition.x *= display.systemWidth / sizePlusPadding.x;
                        adjustedPosition.y *= display.systemHeight / sizePlusPadding.y;
                    }

                    // fix for UUM-63551: Use the display index provided to this method.  Display.RelativeMouseAt( ) no longer works starting with 2021 LTS and new input system
                    // as the Pointer position is reported in Window coordinates rather than relative to the primary window as Display.RelativeMouseAt( ) expects.
#if ENABLE_INPUT_SYSTEM && PACKAGE_INPUTSYSTEM && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_ANDROID || UNITY_EMBEDDED_LINUX || UNITY_QNX)
                    var relativePos = new Vector3(adjustedPosition.x, adjustedPosition.y, displayIndex);
#else
                    var relativePos = Display.RelativeMouseAt(adjustedPosition);
#endif

                    // If we are not on the main display then return the adjusted position.
                    if (relativePos.z != 0)
                        return relativePos;
                }

                // We are using the main display.
#if ENABLE_INPUT_SYSTEM && PACKAGE_INPUTSYSTEM && (UNITY_ANDROID || UNITY_EMBEDDED_LINUX || UNITY_QNX)
                // On Android, in all cases, it is a surface associated to a given displayIndex, so we need to use the display index
                return new Vector3(position.x, position.y, displayIndex);
#else
                return new Vector3(position.x, position.y, 0);
#endif
            }
#endif

            // fix for UUM-63551: Use the display index provided to this method.  Display.RelativeMouseAt( ) no longer works starting with 2021 LTS and new input system
            // as the Pointer position is reported in Window coordinates rather than relative to the primary window as Display.RelativeMouseAt( ) expects.
#if ENABLE_INPUT_SYSTEM && PACKAGE_INPUTSYSTEM && (UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_ANDROID || UNITY_EMBEDDED_LINUX || UNITY_QNX)
            return new Vector3(position.x, position.y, displayIndex);
#else
            return Display.RelativeMouseAt(position);
#endif
        }
    }
}
