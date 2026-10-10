namespace UnityEngine.UI
{
    static class PhysicsModuleErrors
    {
        static bool s_LoggedMissingPhysics;
        static bool s_LoggedMissingPhysics2D;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void ResetStaticsOnLoad()
        {
            s_LoggedMissingPhysics = false;
            s_LoggedMissingPhysics2D = false;
        }
#endif

        // Raycasters call this every frame, so the error is logged once per session.
        public static void LogPhysicsModuleNotPresent()
        {
            if (s_LoggedMissingPhysics)
                return;

            s_LoggedMissingPhysics = true;
            Debug.LogError("The Physics module is not present. 3D physics raycasts requested by the UI event system will not be performed.");
        }

        public static void LogPhysics2DModuleNotPresent()
        {
            if (s_LoggedMissingPhysics2D)
                return;

            s_LoggedMissingPhysics2D = true;
            Debug.LogError("The Physics2D module is not present. 2D physics raycasts requested by the UI event system will not be performed.");
        }
    }
}
