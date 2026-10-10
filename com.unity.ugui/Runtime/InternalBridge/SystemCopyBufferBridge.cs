namespace TMPro
{
    internal static class SystemCopyBufferBridge
    {
        // Exposes the system clipboard without going through GUIUtility, so TMP does not depend on the IMGUI module.
        public static string systemCopyBuffer
        {
            get => UnityEngine.SystemCopyBuffer.systemCopyBuffer;
            set => UnityEngine.SystemCopyBuffer.systemCopyBuffer = value;
        }
    }
}
