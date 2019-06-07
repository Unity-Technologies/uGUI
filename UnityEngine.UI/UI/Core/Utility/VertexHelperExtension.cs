using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UnityEngine.Experimental.UI
{
    public static class VertexHelperExtension
    {
        public static void AddVert(this VertexHelper obj, Vector3 position, Color32 color, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector3 normal, Vector4 tangent)
        {
            obj.AddVert(position, color, uv0, uv1, uv2, uv3, normal, tangent);
        }
    }
}
