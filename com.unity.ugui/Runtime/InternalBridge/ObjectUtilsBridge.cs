using UnityEngine;
using System.ComponentModel;

namespace TMPro
{
    public static class ObjectUtilsBridge
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void MarkDirty(this UnityEngine.Object obj)
        {
            obj.MarkDirty();
        }
    }
}
