using UnityEngine.Pool;

namespace UnityEngine.UI
{
    /// <summary>
    /// Utility functions for querying layout elements for their minimum, preferred, and flexible sizes.
    /// </summary>
    public static class LayoutUtility
    {
        // Cached property accessors — eliminates per-call delegate allocations
        private static readonly System.Func<ILayoutElement, float> k_GetMinWidth = e => e.minWidth;
        private static readonly System.Func<ILayoutElement, float> k_GetMaxWidth = e => e.maxWidth;
        private static readonly System.Func<ILayoutElement, float> k_GetPreferredWidth = e => e.preferredWidth;
        private static readonly System.Func<ILayoutElement, float> k_GetFlexibleWidth = e => e.flexibleWidth;
        private static readonly System.Func<ILayoutElement, float> k_GetMinHeight = e => e.minHeight;
        private static readonly System.Func<ILayoutElement, float> k_GetMaxHeight = e => e.maxHeight;
        private static readonly System.Func<ILayoutElement, float> k_GetPreferredHeight = e => e.preferredHeight;
        private static readonly System.Func<ILayoutElement, float> k_GetFlexibleHeight = e => e.flexibleHeight;

        // Cached predicates
        private static readonly System.Func<float, float, bool> k_GreaterThan = (a, b) => a > b;
        private static readonly System.Func<float, float, bool> k_LessThan = (a, b) => a < b;

        /// <summary>
        /// The default maximum size used when no layout element provides an explicit maximum constraint.
        /// </summary>
        public const float DefaultMaxSize = float.PositiveInfinity;

        /// <summary>
        /// Returns the minimum size of the layout element.
        /// </summary>
        /// <param name="rect">The RectTransform of the layout element to query.</param>
        /// <param name="axis">The axis to query. This can be 0 or 1.</param>
        /// <remarks>All components on the GameObject that implement the ILayoutElement are queried. The one with the highest priority which has a value for this setting is used. If multiple components have this setting and have the same priority, the maximum value out of those is used.</remarks>
        /// <returns>The minimum size for the specified axis.</returns>
        public static float GetMinSize(RectTransform rect, int axis)
        {
            return axis == 0 ? GetMinWidth(rect) : GetMinHeight(rect);
        }

        /// <summary>
        /// Returns the maximum size of the layout element.
        /// </summary>
        /// <param name="rect">The RectTransform of the layout element to query.</param>
        /// <param name="axis">The axis to query. This can be 0 or 1.</param>
        /// <remarks>All components on the GameObject that implement the ILayoutElement are queried. The one with the highest priority which has a value for this setting is used. If multiple components have this setting and have the same priority, the minimum value out of those is used.</remarks>
        /// <returns>The maximum size for the specified axis.</returns>
        public static float GetMaxSize(RectTransform rect, int axis)
        {
            return axis == 0 ? GetMaxWidth(rect) : GetMaxHeight(rect);
        }

        /// <summary>
        /// Returns the preferred size of the layout element.
        /// </summary>
        /// <param name="rect">The RectTransform of the layout element to query.</param>
        /// <param name="axis">The axis to query. This can be 0 or 1.</param>
        /// <remarks>
        /// All components on the GameObject that implement the ILayoutElement are queried. The one with the highest priority which has a value for this setting is used. If multiple components have this setting and have the same priority, the maximum value out of those is used.
        /// </remarks>
        /// <returns>The preferred size for the specified axis.</returns>
        public static float GetPreferredSize(RectTransform rect, int axis)
        {
            return axis == 0 ? GetPreferredWidth(rect) : GetPreferredHeight(rect);
        }

        /// <summary>
        /// Returns the flexible size of the layout element.
        /// </summary>
        /// <remarks>
        /// All components on the GameObject that implement the ILayoutElement are queried. The one with the highest priority which has a value for this setting is used. If multiple components have this setting and have the same priority, the maximum value out of those is used.
        /// </remarks>
        /// <param name="rect">The RectTransform of the layout element to query.</param>
        /// <param name="axis">The axis to query. This can be 0 or 1.</param>
        /// <returns>The flexible size for the specified axis.</returns>
        public static float GetFlexibleSize(RectTransform rect, int axis)
        {
            return axis == 0 ? GetFlexibleWidth(rect) : GetFlexibleHeight(rect);
        }

        /// <summary>
        /// Returns the minimum width of the layout element.
        /// </summary>
        /// <param name="rect">The RectTransform of the layout element to query.</param>
        /// <remarks>
        /// All components on the GameObject that implement the ILayoutElement are queried. The one with the highest priority which has a value for this setting is used. If multiple components have this setting and have the same priority, the maximum value out of those is used.
        /// </remarks>
        /// <returns>The minimum width of the layout element.</returns>
        public static float GetMinWidth(RectTransform rect)
        {
            return GetMinWidth(rect, out _);
        }

        internal static float GetMinWidth(RectTransform rect, out ILayoutElement source)
        {
            return GetLayoutProperty(rect, k_GetMinWidth, k_GreaterThan, 0, out source);
        }


        /// <summary>
        /// Returns the maximum width of the layout element.
        /// </summary>
        /// <param name="rect">The RectTransform of the layout element to query.</param>
        /// <remarks>
        /// All components on the GameObject that implement the ILayoutElement are queried. The one with the highest priority which has a value for this setting is used. If multiple components have this setting and have the same priority, the minimum value out of those is used.
        /// </remarks>
        /// <returns>The maximum width of the layout element.</returns>
        public static float GetMaxWidth(RectTransform rect)
        {
            return GetMaxWidth(rect, out _);
        }

        internal static float GetMaxWidth(RectTransform rect, out ILayoutElement source)
        {
            return GetLayoutProperty(rect, k_GetMaxWidth, k_LessThan, DefaultMaxSize, out source);
        }

        /// <summary>
        /// Returns the preferred width of the layout element.
        /// </summary>
        /// <param name="rect">The RectTransform of the layout element to query.</param>
        /// <remarks>
        /// All components on the GameObject that implement the ILayoutElement are queried. The one with the highest priority which has a value for this setting is used. If multiple components have this setting and have the same priority, the maximum value out of those is used. The final value is clamped to the available minimum and maximum widths.
        /// </remarks>
        /// <returns>The preferred width of the layout element.</returns>
        public static float GetPreferredWidth(RectTransform rect)
        {
            return GetPreferredWidth(rect, out _);
        }

        internal static float GetPreferredWidth(RectTransform rect, out ILayoutElement source)
        {
            return GetPreferredLayoutProperty(rect, k_GetMinWidth, k_GetPreferredWidth, k_GetMaxWidth, 0, out source);
        }

        /// <summary>
        /// Returns the flexible width of the layout element.
        /// </summary>
        /// <remarks>
        /// All components on the GameObject that implement the ILayoutElement are queried. The one with the highest priority which has a value for this setting is used. If multiple components have this setting and have the same priority, the maximum value out of those is used
        /// </remarks>
        /// <param name="rect">The RectTransform of the layout element to query.</param>
        /// <returns>The flexible width of the layout element.</returns>
        public static float GetFlexibleWidth(RectTransform rect)
        {
            return GetFlexibleWidth(rect, out _);
        }

        internal static float GetFlexibleWidth(RectTransform rect, out ILayoutElement source)
        {
            return GetLayoutProperty(rect, k_GetFlexibleWidth, k_GreaterThan, 0, out source);
        }

        /// <summary>
        /// Returns the minimum height of the layout element.
        /// </summary>
        /// <param name="rect">The RectTransform of the layout element to query.</param>
        /// <remarks>
        /// All components on the GameObject that implement the ILayoutElement are queried. The one with the highest priority which has a value for this setting is used. If multiple components have this setting and have the same priority, the maximum value out of those is used.
        /// </remarks>
        /// <returns>The minimum height of the layout element.</returns>
        public static float GetMinHeight(RectTransform rect)
        {
            return GetMinHeight(rect, out _);
        }

        internal static float GetMinHeight(RectTransform rect, out ILayoutElement source)
        {
            return GetLayoutProperty(rect, k_GetMinHeight, k_GreaterThan, 0, out source);
        }

        /// <summary>
        /// Returns the maximum height of the layout element.
        /// </summary>
        /// <param name="rect">The RectTransform of the layout element to query.</param>
        /// <remarks>
        /// All components on the GameObject that implement the ILayoutElement are queried. The one with the highest priority which has a value for this setting is used. If multiple components have this setting and have the same priority, the minimum value out of those is used. The final value is clamped to the available minimum and maximum heights.
        /// </remarks>
        /// <returns>The maximum height of the layout element.</returns>
        public static float GetMaxHeight(RectTransform rect)
        {
            return GetMaxHeight(rect, out _);
        }

        internal static float GetMaxHeight(RectTransform rect, out ILayoutElement source)
        {
            return GetLayoutProperty(rect, k_GetMaxHeight, k_LessThan, DefaultMaxSize, out source);
        }

        /// <summary>
        /// Returns the preferred height of the layout element.
        /// </summary>
        /// <param name="rect">The RectTransform of the layout element to query.</param>
        /// <remarks>
        /// All components on the GameObject that implement the ILayoutElement are queried. The one with the highest priority which has a value for this setting is used. If multiple components have this setting and have the same priority, the maximum value out of those is used.
        /// </remarks>
        /// <returns>The preferred height of the layout element.</returns>
        public static float GetPreferredHeight(RectTransform rect)
        {
            return GetPreferredHeight(rect, out _);
        }

        internal static float GetPreferredHeight(RectTransform rect, out ILayoutElement source)
        {
            return GetPreferredLayoutProperty(rect, k_GetMinHeight, k_GetPreferredHeight, k_GetMaxHeight, 0, out source);
        }

        /// <summary>
        /// Returns the flexible height of the layout element.
        /// </summary>
        /// <remarks>
        /// All components on the GameObject that implement the ILayoutElement are queried. The one with the highest priority which has a value for this setting is used. If multiple components have this setting and have the same priority, the maximum value out of those is used.
        /// </remarks>
        /// <param name="rect">The RectTransform of the layout element to query.</param>
        /// <returns>The flexible height of the layout element.</returns>
        public static float GetFlexibleHeight(RectTransform rect)
        {
            return GetFlexibleHeight(rect, out _);
        }

        internal static float GetFlexibleHeight(RectTransform rect, out ILayoutElement source)
        {
            return GetLayoutProperty(rect, k_GetFlexibleHeight, k_GreaterThan, 0, out source);
        }

        /// <summary>
        /// Gets a calculated layout property for the layout element with the given RectTransform.
        /// </summary>
        /// <param name="rect">The RectTransform of the layout element to get a property for.</param>
        /// <param name="property">The property to calculate.</param>
        /// <param name="defaultValue">The default value to use if no component on the layout element supplies the given property</param>
        /// <returns>The calculated value of the layout property.</returns>
        public static float GetLayoutProperty(RectTransform rect, System.Func<ILayoutElement, float> property, float defaultValue)
        {
            return GetLayoutProperty(rect, property, defaultValue, out _);
        }

        /// <summary>
        /// Gets a calculated layout property for the layout element with the given RectTransform.
        /// </summary>
        /// <param name="rect">The RectTransform of the layout element to get a property for.</param>
        /// <param name="property">The property to calculate.</param>
        /// <param name="defaultValue">The default value to use if no component on the layout element supplies the given property</param>
        /// <param name="source">Optional out parameter to get the component that supplied the calculated value.</param>
        /// <returns>The calculated value of the layout property.</returns>
        public static float GetLayoutProperty(RectTransform rect, System.Func<ILayoutElement, float> property, float defaultValue, out ILayoutElement source)
        {
            return GetLayoutProperty(rect, property, k_GreaterThan, defaultValue, out source);
        }

        /// <summary>
        /// Gets a calculated layout property for the layout element with the given RectTransform.
        /// </summary>
        /// <param name="rect">The RectTransform of the layout element to get a property for.</param>
        /// <param name="property">The property to calculate.</param>
        /// <param name="predicate">The predicate used to compare potential values at the same priority level.</param>
        /// <param name="defaultValue">The default value to use if no component on the layout element supplies the given property</param>
        /// <param name="source">Optional out parameter to get the component that supplied the calculated value.</param>
        /// <returns>The calculated value of the layout property.</returns>
        public static float GetLayoutProperty(RectTransform rect, System.Func<ILayoutElement, float> property, System.Func<float, float,  bool> predicate, float defaultValue, out ILayoutElement source)
        {
            source = null;
            if (rect == null)
                return 0;
            float currentValue = defaultValue;
            int maxPriority = System.Int32.MinValue;

            using var _ = ListPool<Component>.Get(out var components);
            rect.GetComponents(typeof(ILayoutElement), components);

            var componentsCount = components.Count;
            for (int i = 0; i < componentsCount; i++)
            {
                var layoutComp = components[i] as ILayoutElement;
                if (layoutComp is Behaviour b && !b.isActiveAndEnabled)
                    continue;

                int priority = layoutComp.layoutPriority;
                // If this layout components has lower priority than a previously used, ignore it.
                if (priority < maxPriority)
                    continue;
                float prop = property(layoutComp);
                // If this layout property is set to a negative value, it means it should be ignored.
                if (prop < 0 || float.IsNaN(prop))
                    continue;

                // If this layout component has higher priority than all previous ones,
                // overwrite with this one's value.
                if (priority > maxPriority)
                {
                    currentValue = prop;
                    maxPriority = priority;
                    source = layoutComp;
                }
                // If the layout component has the same priority as a previously used,
                // use the largest of the values with the same priority.
                else if (predicate(prop, currentValue))
                {
                    currentValue = prop;
                    source = layoutComp;
                }
            }

            return currentValue;
        }

        internal static bool TryGetMinMaxConflict(
            RectTransform rect,
            int axis,
            out float min,
            out ILayoutElement minSource,
            out float max,
            out ILayoutElement maxSource
        )
        {
            if (rect == null)
            {
                min = 0;
                minSource = null;
                max = DefaultMaxSize;
                maxSource = null;
                return false;
            }

            min = GetLayoutProperty(rect, axis == 0 ? k_GetMinWidth : k_GetMinHeight, k_GreaterThan, 0, out minSource);
            max = GetLayoutProperty(rect, axis == 0 ? k_GetMaxWidth : k_GetMaxHeight, k_LessThan, DefaultMaxSize, out maxSource);
            return min > max;
        }

        /// <summary>
        /// Single-pass equivalent of Mathf.Clamp(Mathf.Max(GetLayoutProperty(min), GetLayoutProperty(preferred)), min, GetLayoutProperty(max)).
        /// More performant to use call this than calling GetLayoutProperty for min/preferred/max individually.
        /// </summary>
        private static float GetPreferredLayoutProperty(RectTransform rect, System.Func<ILayoutElement, float> minProperty, System.Func<ILayoutElement, float> preferredProperty, System.Func<ILayoutElement, float> maxProperty, float defaultValue, out ILayoutElement source)
        {
            source = null;
            if (rect == null)
                return 0;

            float currentMin = defaultValue;
            float currentPreferred = defaultValue;
            float currentMax = float.MaxValue;
            int highestPriorityMin = System.Int32.MinValue;
            int highestPriorityPreferred = System.Int32.MinValue;
            int highestPriorityMax = System.Int32.MinValue;
            ILayoutElement minSource = null;
            ILayoutElement preferredSource = null;
            ILayoutElement maxSource = null;

            using var _ = ListPool<Component>.Get(out var components);
            rect.GetComponents(typeof(ILayoutElement), components);
            int componentsCount = components.Count;
            for (int i = 0; i < componentsCount; i++)
            {
                var layoutComp = components[i] as ILayoutElement;
                if (layoutComp is Behaviour b && !b.isActiveAndEnabled)
                    continue;

                int priority = layoutComp.layoutPriority;

                // Get Min
                if (priority >= highestPriorityMin)
                {
                    float prop = minProperty(layoutComp);
                    if (prop >= 0)
                    {
                        if (priority > highestPriorityMin)
                        {
                            currentMin = prop;
                            highestPriorityMin = priority;
                            minSource = layoutComp;
                        }
                        else if (prop > currentMin)
                        {
                            currentMin = prop;
                            minSource = layoutComp;
                        }
                    }
                }

                // Get Preferred
                if (priority >= highestPriorityPreferred)
                {
                    float prop = preferredProperty(layoutComp);
                    if (prop >= 0)
                    {
                        if (priority > highestPriorityPreferred)
                        {
                            currentPreferred = prop;
                            highestPriorityPreferred = priority;
                            preferredSource = layoutComp;
                        }
                        else if (prop > currentPreferred)
                        {
                            currentPreferred = prop;
                            preferredSource = layoutComp;
                        }
                    }
                }

                // Get Max
                if (priority >= highestPriorityMax)
                {
                    float prop = maxProperty(layoutComp);
                    if (prop >= 0)
                    {
                        if (priority > highestPriorityMax)
                        {
                            currentMax = prop;
                            highestPriorityMax = priority;
                            maxSource = layoutComp;
                        }
                        else if (prop < currentMax)
                        {
                            currentMax = prop;
                            maxSource = layoutComp;
                        }
                    }
                }
            }

            // Resolve: clamp the effective preferred (which is at least min) to the max.
            // Same branches as Mathf.Clamp, so the source is the element whose value won.
            if (currentPreferred < currentMin)
            {
                source = minSource;
                return currentMin;
            }
            if (currentPreferred > currentMax)
            {
                source = maxSource;
                return currentMax;
            }
            source = preferredSource;
            return currentPreferred;
        }
    }
}
