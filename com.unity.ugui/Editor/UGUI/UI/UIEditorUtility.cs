using System;
using UnityEngine.UIElements;

namespace UnityEditor.UI
{
    /// <summary>
    /// Provides utility methods for creating and manipulating UI Toolkit elements in the Unity Editor.
    /// </summary>
    internal static class UIEditorUtility
    {
        // Unity's internal USS class for the hover state on draggable fields
        private const string k_draggerHoverClass = "unity-base-field__label--with-dragger";

        /// <summary>
        /// Configures a visual element to act as a drag zone for a value field, allowing users to change the field's
        /// value by dragging the mouse.
        /// </summary>
        /// <typeparam name="T">The type of the value held by the field.</typeparam>
        /// <param name="field">The target field to be updated by dragging. This field must implement IValueField&lt;T&gt;.</param>
        /// <param name="element">The visual element that will serve as the interactable drag zone.</param>
        /// <param name="canDrag">An optional function to dynamically determine if dragging is currently allowed. If null, dragging is always allowed.</param>
        /// <exception cref="ArgumentNullException">Thrown if either the <paramref name="field"/> or <paramref name="element"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="field"/> does not implement IValueField&lt;T&gt;.</exception>
        public static void SetupDragger<T>(this BaseField<T> field, VisualElement element, Func<bool> canDrag = null)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            if (element == null) throw new ArgumentNullException(nameof(element));

            // Must implement IValueField<T> for FieldMouseDragger to work
            if (!(field is IValueField<T>))
                throw new ArgumentException($"Field must implement IValueField<{typeof(T).Name}>", nameof(field));

            // Initialize dragger (requires casting to IValueField<T> used by UIElements)
            var dragger = new FieldMouseDragger<T>((IValueField<T>)field);
            dragger.SetDragZone(element);

            // Handle mouse over
            element.RegisterCallback<MouseOverEvent>(_ =>
            {
                if (canDrag == null || canDrag())
                    element.AddToClassList(k_draggerHoverClass);
                else
                    element.RemoveFromClassList(k_draggerHoverClass);
            });

            // Handle mouse leave
            element.RegisterCallback<MouseLeaveEvent>(_ =>
                element.RemoveFromClassList(k_draggerHoverClass));

            // Removal of dragging capabilities if the field gets disabled while the mouse hovers.
            field.RegisterCallback<CustomStyleResolvedEvent>(_ =>
            {
                if (canDrag == null || canDrag())
                    element.RemoveFromClassList(k_draggerHoverClass);
            });
        }
    }
}
