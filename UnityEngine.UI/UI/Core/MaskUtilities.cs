using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    public class MaskUtilities
    {
        public static void Notify2DMaskStateChanged(Component mask)
        {
            var components = ListPool<Component>.Get();
            mask.GetComponentsInChildren(components);
            for (var i = 0; i < components.Count; i++)
            {
                if (components[i] == null || components[i].gameObject == mask.gameObject)
                    continue;

                var toNotify = components[i] as IClippable;
                if (toNotify != null)
                    toNotify.RecalculateClipping();
            }
            ListPool<Component>.Release(components);
        }

        public static void NotifyStencilStateChanged(Component mask)
        {
            var components = ListPool<Component>.Get();
            mask.GetComponentsInChildren(components);
            for (var i = 0; i < components.Count; i++)
            {
                if (components[i] == null || components[i].gameObject == mask.gameObject)
                    continue;

                var toNotify = components[i] as IMaskable;
                if (toNotify != null)
                    toNotify.RecalculateMasking();
            }
            ListPool<Component>.Release(components);
        }

        public static Transform FindRootSortOverrideCanvas(Transform start)
        {
            var canvasList = ListPool<Canvas>.Get();
            start.GetComponentsInParent(false, canvasList);
            Canvas canvas = null;

            for (int i = 0; i < canvasList.Count; ++i)
            {
                canvas = canvasList[i];

                // We found the canvas we want to use break
                if (canvas.overrideSorting)
                    break;
            }
            ListPool<Canvas>.Release(canvasList);

            return canvas != null ? canvas.transform : null;
        }

        public static int GetStencilDepth(Transform transform, Transform stopAfter)
        {
            var depth = 0;
            if (transform == stopAfter)
                return depth;

            var t = transform.parent;
            var components = ListPool<Mask>.Get();
            while (t != null)
            {
                t.GetComponents<Mask>(components);
                for (var i = 0; i < components.Count; ++i)
                {
                    if (components[i] != null && components[i].IsActive() && components[i].graphic != null && components[i].graphic.IsActive())
                    {
                        ++depth;
                        break;
                    }
                }

                if (t == stopAfter)
                    break;

                t = t.parent;
            }
            ListPool<Mask>.Release(components);
            return depth;
        }

        public static RectMask2D GetRectMaskForClippable(IClippable transform)
        {
            List<RectMask2D> rectMaskComponents = ListPool<RectMask2D>.Get();
            RectMask2D componentToReturn = null;

            transform.rectTransform.GetComponentsInParent<RectMask2D>(false, rectMaskComponents);

            if (rectMaskComponents.Count > 0)
                componentToReturn = rectMaskComponents[0];

            ListPool<RectMask2D>.Release(rectMaskComponents);

            return componentToReturn;
        }

        public static void GetRectMasksForClip(RectMask2D clipper, List<RectMask2D> masks)
        {
            masks.Clear();
            clipper.transform.GetComponentsInParent<RectMask2D>(false, masks);
        }
    }
}
