using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityEngine.UI
{
    internal class ReflectionMethodsCache
    {
        public delegate bool Raycast3DCallback (Ray r, out RaycastHit hit, float f, int i);
        public delegate RaycastHit2D Raycast2DCallback (Vector2 p1, Vector2 p2, float f, int i);
        public delegate RaycastHit[] RaycastAllCallback (Ray r, float f, int i);
        public delegate RaycastHit2D[] GetRayIntersectionAllCallback (Ray r, float f, int i);

        // We call Physics.Raycast and Physics2D.Raycast through reflection to avoid creating a hard dependency from
        // this class to the Physics/Physics2D modules, which would otherwise make it impossible to make content with UI
        // without force-including both modules.
        public ReflectionMethodsCache() 
        {
            var raycast3DMethodInfo = typeof(Physics).GetMethod("Raycast", new[] {typeof(Ray), typeof(RaycastHit).MakeByRefType(), typeof(float), typeof(int)});
            if (raycast3DMethodInfo != null)
                raycast3D = (Raycast3DCallback)UnityEngineInternal.ScriptingUtils.CreateDelegate(typeof(Raycast3DCallback), raycast3DMethodInfo);

            var raycast2DMethodInfo = typeof(Physics2D).GetMethod("Raycast", new[] {typeof(Vector2), typeof(Vector2), typeof(float), typeof(int)});
            if (raycast2DMethodInfo != null)
                raycast2D = (Raycast2DCallback)UnityEngineInternal.ScriptingUtils.CreateDelegate(typeof(Raycast2DCallback), raycast2DMethodInfo);

            var raycastAllMethodInfo = typeof(Physics).GetMethod("RaycastAll", new[] {typeof(Ray), typeof(float), typeof(int)});
            if (raycastAllMethodInfo != null)
                raycast3DAll = (RaycastAllCallback)UnityEngineInternal.ScriptingUtils.CreateDelegate(typeof(RaycastAllCallback), raycastAllMethodInfo);

            var getRayIntersectionAllMethodInfo = typeof(Physics2D).GetMethod("GetRayIntersectionAll", new[] {typeof(Ray), typeof(float), typeof(int)});
            if (getRayIntersectionAllMethodInfo != null)
                getRayIntersectionAll = (GetRayIntersectionAllCallback)UnityEngineInternal.ScriptingUtils.CreateDelegate(typeof(GetRayIntersectionAllCallback), getRayIntersectionAllMethodInfo);
        }

        public Raycast3DCallback raycast3D = null;
        public RaycastAllCallback raycast3DAll = null;
        public Raycast2DCallback raycast2D = null;            
        public GetRayIntersectionAllCallback getRayIntersectionAll = null;

        private static ReflectionMethodsCache s_ReflectionMethodsCache = null;

        public static ReflectionMethodsCache Singleton
        {
            get 
            {
                if (s_ReflectionMethodsCache == null)
                    s_ReflectionMethodsCache = new ReflectionMethodsCache();
                return s_ReflectionMethodsCache;
            }
        }        
    };
}
