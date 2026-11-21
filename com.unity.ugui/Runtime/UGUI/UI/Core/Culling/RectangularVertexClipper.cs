namespace UnityEngine.UI
{
    internal class RectangularVertexClipper
    {
        /// <summary>
        /// Get a RectTransform's rect in the canvas's local space.
        /// </summary>
        /// <param name="rectTransform">The RectTransform to project.</param>
        /// <param name="canvas">The canvas whose space to project into.</param>
        /// <returns>The rect in canvas space.</returns>
        /// <remarks>The size is negative on an axis where the transform mirrors the rect, such as past 90 degrees or with a negative scale.</remarks>
        public Rect GetCanvasRect(RectTransform rectTransform, Canvas canvas)
        {
            return GetCanvasRect(rectTransform, canvas, rectTransform.rect);
        }

        /// <summary>
        /// Get a rect given in a transform's local space, in the canvas's local space.
        /// </summary>
        /// <param name="transform">The transform whose local space the rect is in.</param>
        /// <param name="canvas">The canvas whose space to project into.</param>
        /// <param name="localRect">The rect, in the transform's local space.</param>
        /// <returns>The rect in canvas space.</returns>
        /// <remarks>The size is negative on an axis where the transform mirrors the rect, such as past 90 degrees or with a negative scale.</remarks>
        public Rect GetCanvasRect(Transform transform, Canvas canvas, Rect localRect)
        {
            // The canvas can be null, such as for a prefab with no parent Canvas.
            if (canvas == null)
                return new Rect();

            var canvasTransform = canvas.GetComponent<Transform>();
            Vector3 min = canvasTransform.InverseTransformPoint(transform.TransformPoint(localRect.min));
            Vector3 max = canvasTransform.InverseTransformPoint(transform.TransformPoint(localRect.max));
            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        /// <summary>
        /// Get how many canvas units one of a RectTransform's own units spans on each axis.
        /// </summary>
        /// <param name="rectTransform">The RectTransform to measure.</param>
        /// <param name="canvas">The canvas whose space to measure in.</param>
        /// <returns>The span per unit on each axis, or 0 on an axis where the rect is empty.</returns>
        public Vector2 GetCanvasScale(RectTransform rectTransform, Canvas canvas)
        {
            Rect rect = rectTransform.rect;
            Rect canvasRect = GetCanvasRect(rectTransform, canvas, rect);
            return new Vector2(
                rect.width != 0 ? Mathf.Abs(canvasRect.width / rect.width) : 0,
                rect.height != 0 ? Mathf.Abs(canvasRect.height / rect.height) : 0);
        }
    }
}
