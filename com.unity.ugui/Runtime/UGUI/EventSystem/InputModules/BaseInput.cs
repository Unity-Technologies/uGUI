using UnityEngine.UI;

namespace UnityEngine.EventSystems
{
    /// <summary>
    /// Interface to the Input system used by the BaseInputModule. With this it is possible to bypass the Input system with your own but still use the same InputModule. For example this can be used to feed fake input into the UI or interface with a different input system.
    /// </summary>
    public class BaseInput : UIBehaviour
    {
        /// <summary>
        /// Interface to Input.compositionString. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public virtual string compositionString
        {
            get { return Input.compositionString; }
        }

        /// <summary>
        /// Interface to Input.imeCompositionMode. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public virtual IMECompositionMode imeCompositionMode
        {
            get { return Input.imeCompositionMode; }
            set { Input.imeCompositionMode = value; }
        }

        /// <summary>
        /// Interface to Input.compositionCursorPos. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public virtual Vector2 compositionCursorPos
        {
            get { return Input.compositionCursorPos; }
            set { Input.compositionCursorPos = value; }
        }

        /// <summary>
        /// Interface to Input.mousePresent. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public virtual bool mousePresent
        {
            get { return Input.mousePresent; }
        }

        /// <summary>
        /// Checks whether the user pressed the specified mouse button during this frame.
        /// </summary>
        /// <remarks>
        /// Override this method in a custom input module to supply your own mouse
        /// button logic. The default implementation forwards to Unity's
        /// <see cref="Input.GetMouseButtonDown"/>. Use this when implementing custom
        /// input or testing without device input.
        /// </remarks>
        /// <param name="button">The mouse button index to check, where 0 is left, 1 is right, and 2 is middle.</param>
        /// <returns>true if the user pressed the specified mouse button during this
        /// frame; otherwise, false.</returns>
        /// <example>
        /// <para>Override to supply custom mouse button input (for example, for testing).
        /// The following example simulates a left click when the user presses <b>Space</b>.</para>
        /// <code><![CDATA[
        /// public override bool GetMouseButtonDown(int button)
        /// {
        ///     // For example, simulate a left click when the user presses the Space key.
        ///     if (button == 0 && Input.GetKeyDown(KeyCode.Space))
        ///         return true;
        ///     return Input.GetMouseButtonDown(button);
        /// }
        /// ]]></code>
        /// </example>
        public virtual bool GetMouseButtonDown(int button)
        {
            return Input.GetMouseButtonDown(button);
        }

        /// <summary>
        /// Interface to Input.GetMouseButtonUp. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public virtual bool GetMouseButtonUp(int button)
        {
            return Input.GetMouseButtonUp(button);
        }

        /// <summary>
        /// Interface to Input.GetMouseButton. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public virtual bool GetMouseButton(int button)
        {
            return Input.GetMouseButton(button);
        }

        /// <summary>
        /// Interface to Input.mousePosition. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public virtual Vector2 mousePosition
        {
            get { return Input.mousePosition; }
        }

        /// <summary>
        /// Interface to Input.mouseScrollDelta. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public virtual Vector2 mouseScrollDelta
        {
            get { return Input.mouseScrollDelta; }
        }

        /// <summary>
        /// The magnitude of mouseScrollDelta that corresponds to exactly one tick of the scroll wheel.
        /// </summary>
        public virtual float mouseScrollDeltaPerTick
        {
            get { return 1.0f; }
        }

        /// <summary>
        /// Interface to Input.touchSupported. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public virtual bool touchSupported
        {
            get { return Input.touchSupported; }
        }

        /// <summary>
        /// Interface to Input.touchCount. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public virtual int touchCount
        {
            get { return Input.touchCount; }
        }

        /// <summary>
        /// Interface to Input.GetTouch. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        /// <param name="index">Touch index to get</param>
        public virtual Touch GetTouch(int index)
        {
            return Input.GetTouch(index);
        }

        /// <summary>
        /// Interface to Input.GetAxisRaw. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        /// <param name="axisName">Axis name to check</param>
        public virtual float GetAxisRaw(string axisName)
        {
            return Input.GetAxisRaw(axisName);
        }

        /// <summary>
        /// Interface to Input.GetButtonDown. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        /// <param name="buttonName">Button name to get</param>
        public virtual bool GetButtonDown(string buttonName)
        {
            return Input.GetButtonDown(buttonName);
        }
    }
}
