using System.Collections.Generic;

namespace UnityEngine.UI
{
    internal static class ComponentListPool
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<List<Component>> s_ComponentListPool = new ObjectPool<List<Component>>(null, l => l.Clear());

        public static List<Component> Get()
        {
            return s_ComponentListPool.Get();
        }

        public static void Release(List<Component> toRelease)
        {
            s_ComponentListPool.Release(toRelease);
        }
    }
}
