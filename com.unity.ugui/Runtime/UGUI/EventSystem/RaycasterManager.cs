using System.Collections.Generic;

namespace UnityEngine.EventSystems
{
    /// <summary>
    /// Keeps track of the BaseRaycasters that the EventSystem raycasts with.
    /// </summary>
    /// <remarks>
    /// A <see cref="BaseRaycaster"/> registers itself when it becomes active and unregisters itself when it becomes inactive.
    /// <see cref="EventSystems.EventSystem.RaycastAll"/> raycasts with every registered raycaster.
    /// </remarks>
    public static class RaycasterManager
    {
        private static readonly List<BaseRaycaster> s_Raycasters = new List<BaseRaycaster>();

        internal static void AddRaycaster(BaseRaycaster baseRaycaster)
        {
            if (s_Raycasters.Contains(baseRaycaster))
                return;

            s_Raycasters.Add(baseRaycaster);
        }

        /// <summary>
        /// List of BaseRaycasters that have been registered.
        /// </summary>
        /// <returns>The registered raycasters.</returns>
        public static List<BaseRaycaster> GetRaycasters()
        {
            return s_Raycasters;
        }

        internal static void RemoveRaycasters(BaseRaycaster baseRaycaster)
        {
            if (!s_Raycasters.Contains(baseRaycaster))
                return;
            s_Raycasters.Remove(baseRaycaster);
        }
    }
}
