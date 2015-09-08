using System;

namespace UnityEngine.UI
{
    [Obsolete("Not supported anymore.", true)]
    public interface IMask
    {
        bool Enabled();
        RectTransform rectTransform { get; }
    }
}
