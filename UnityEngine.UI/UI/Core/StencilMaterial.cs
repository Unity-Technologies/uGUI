using System.Collections.Generic;
using UnityEngine.Rendering;

namespace UnityEngine.UI
{
    /// <summary>
    /// Dynamic material class makes it possible to create custom materials on the fly on a per-Graphic basis,
    /// and still have them get cleaned up correctly.
    /// </summary>
    public static class StencilMaterial
    {
        private class MatEntry
        {
            public Material baseMat;
            public Material customMat;
            public int count = 0;
            public int stencilID = 0;
        }

        private static List<MatEntry> m_List = new List<MatEntry>();

        /// <summary>
        /// Add a new material using the specified base and stencil ID.
        /// </summary>
        public static Material Add(Material baseMat, int stencilID)
        {
            if (stencilID <= 0 || baseMat == null)
                return null;

            if (!baseMat.HasProperty("_Stencil"))
            {
                Debug.LogWarning("Material " + baseMat.name + " doesn't have stencil properties", baseMat);
                return null;
            }

            for (int i = 0; i < m_List.Count; ++i)
            {
                MatEntry ent = m_List[i];

                if (ent.baseMat == baseMat && ent.stencilID == stencilID)
                {
                    ++ent.count;
                    return ent.customMat;
                }
            }

            var newEnt = new MatEntry();
            newEnt.count = 1;
            newEnt.baseMat = baseMat;
            newEnt.customMat = new Material(baseMat);
            newEnt.customMat.name = "Stencil " + stencilID + " (" + baseMat.name + ")";
            newEnt.customMat.hideFlags = HideFlags.HideAndDontSave;
            newEnt.stencilID = stencilID;

            if (baseMat.HasProperty("_StencilComp"))
                newEnt.customMat.SetInt("_StencilComp", (int)CompareFunction.Equal);

            newEnt.customMat.SetInt("_Stencil", stencilID);
            m_List.Add(newEnt);
            return newEnt.customMat;
        }

        /// <summary>
        /// Remove an existing material, automatically cleaning it up if it's no longer in use.
        /// </summary>
        public static void Remove(Material customMat)
        {
            if (customMat == null)
                return;

            for (int i = 0; i < m_List.Count; ++i)
            {
                MatEntry ent = m_List[i];

                if (ent.customMat != customMat)
                    continue;

                if (--ent.count == 0)
                {
                    Misc.DestroyImmediate(ent.customMat);
                    ent.baseMat = null;
                    m_List.RemoveAt(i);
                }
                return;
            }
        }
    }
}
