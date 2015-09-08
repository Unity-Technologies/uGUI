using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityEngine.UI
{
    public static class LayoutUtility
    {
        public static float GetMinSize(RectTransform rect, int axis)
        {
            if (axis == 0)
                return GetMinWidth(rect);
            return GetMinHeight(rect);
        }

        public static float GetPreferredSize(RectTransform rect, int axis)
        {
            if (axis == 0)
                return GetPreferredWidth(rect);
            return GetPreferredHeight(rect);
        }

        public static float GetFlexibleSize(RectTransform rect, int axis)
        {
            if (axis == 0)
                return GetFlexibleWidth(rect);
            return GetFlexibleHeight(rect);
        }

        public static float GetMinWidth(RectTransform rect)
        {
            return GetLayoutProperty(rect, e => e.minWidth, 0);
        }

        public static float GetPreferredWidth(RectTransform rect)
        {
            return Mathf.Max(GetLayoutProperty(rect, e => e.minWidth, 0), GetLayoutProperty(rect, e => e.preferredWidth, 0));
        }

        public static float GetFlexibleWidth(RectTransform rect)
        {
            return GetLayoutProperty(rect, e => e.flexibleWidth, 0);
        }

        public static float GetMinHeight(RectTransform rect)
        {
            return GetLayoutProperty(rect, e => e.minHeight, 0);
        }

        public static float GetPreferredHeight(RectTransform rect)
        {
            return Mathf.Max(GetLayoutProperty(rect, e => e.minHeight, 0), GetLayoutProperty(rect, e => e.preferredHeight, 0));
        }

        public static float GetFlexibleHeight(RectTransform rect)
        {
            return GetLayoutProperty(rect, e => e.flexibleHeight, 0);
        }

        public static float GetLayoutProperty(RectTransform rect, System.Func<ILayoutElement, float> property, float defaultValue)
        {
            ILayoutElement dummy;
            return GetLayoutProperty(rect, property, defaultValue, out dummy);
        }

        public static float GetLayoutProperty(RectTransform rect, System.Func<ILayoutElement, float> property, float defaultValue, out ILayoutElement source)
        {
            source = null;
            if (rect == null)
                return 0;
            float min = defaultValue;
            int maxPriority = System.Int32.MinValue;
            var components = ListPool<Component>.Get();
            rect.GetComponents(typeof(ILayoutElement), components);

            for (int i = 0; i < components.Count; i++)
            {
                var layoutComp = components[i] as ILayoutElement;
                if (layoutComp is Behaviour && !((Behaviour)layoutComp).isActiveAndEnabled)
                    continue;

                int priority = layoutComp.layoutPriority;
                // If this layout components has lower priority than a previously used, ignore it.
                if (priority < maxPriority)
                    continue;
                float prop = property(layoutComp);
                // If this layout property is set to a negative value, it means it should be ignored.
                if (prop < 0)
                    continue;

                // If this layout component has higher priority than all previous ones,
                // overwrite with this one's value.
                if (priority > maxPriority)
                {
                    min = prop;
                    maxPriority = priority;
                    source = layoutComp;
                }
                // If the layout component has the same priority as a previously used,
                // use the largest of the values with the same priority.
                else if (prop > min)
                {
                    min = prop;
                    source = layoutComp;
                }
            }

            ListPool<Component>.Release(components);
            return min;
        }
    }
}
