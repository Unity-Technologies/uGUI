using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
    internal static class CanvasListPool
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<List<Canvas>> s_CanvasListPool = new ObjectPool<List<Canvas>>(null, l => l.Clear());

        public static List<Canvas> Get()
        {
            return s_CanvasListPool.Get();
        }

        public static void Release(List<Canvas> toRelease)
        {
            s_CanvasListPool.Release(toRelease);
        }
    }
}
