namespace UnityEngine.UI
{
    public interface IClipper
    {
        void PerformClipping();
    }

    public interface IClippable
    {
        GameObject gameObject { get; }
        void RecalculateClipping();
        RectTransform rectTransform { get; }
        void Cull(Rect clipRect, bool validRect);
        void SetClipRect(Rect value, bool validRect);
    }
}
