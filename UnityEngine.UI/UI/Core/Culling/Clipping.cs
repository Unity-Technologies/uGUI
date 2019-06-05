using System.Collections.Generic;

namespace UnityEngine.UI
{
    /// <summary>
    /// Utility class to help when clipping using IClipper.
    /// </summary>
    public static class Clipping
    {
        /// <summary>
        /// Find the Rect to use for clipping.
        /// Given the input RectMask2ds find a rectangle that is the overlap of all the inputs.
        /// </summary>
        /// <param name="rectMaskParents">RectMasks to build the overlap rect from.</param>
        /// <param name="validRect">Was there a valid Rect found.</param>
        /// <returns>The final compounded overlapping rect</returns>
        public static Rect FindCullAndClipWorldRect(List<RectMask2D> rectMaskParents, out bool validRect)
        {
            if (rectMaskParents.Count == 0)
            {
                validRect = false;
                return new Rect();
            }

            Rect current = rectMaskParents[0].canvasRect;
            float xMin = current.xMin;
            float xMax = current.xMax;
            float yMin = current.yMin;
            float yMax = current.yMax;
            for (var i = 1; i < rectMaskParents.Count; ++i)
            {
                current = rectMaskParents[i].canvasRect;
                if (xMin < current.xMin)
                    xMin = current.xMin;
                if (yMin < current.yMin)
                    yMin = current.yMin;
                if (xMax > current.xMax)
                    xMax = current.xMax;
                if (yMax > current.yMax)
                    yMax = current.yMax;
            }

            validRect = xMax > xMin && yMax > yMin;
            if (validRect)
                return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
            else
                return new Rect();
        }
    }
}
