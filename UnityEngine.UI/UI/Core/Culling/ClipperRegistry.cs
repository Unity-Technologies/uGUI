using System.Collections.Generic;
using UnityEngine.UI.Collections;

namespace UnityEngine.UI
{
    public class ClipperRegistry
    {
        static ClipperRegistry s_Instance;

        readonly IndexedSet<IClipper> m_Clippers = new IndexedSet<IClipper>();

        protected ClipperRegistry()
        {
            // This is needed for AOT platforms. Without it the compile doesn't get the definition of the Dictionarys
#pragma warning disable 168
            Dictionary<IClipper, int> emptyIClipperDic;
#pragma warning restore 168
        }

        public static ClipperRegistry instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = new ClipperRegistry();
                return s_Instance;
            }
        }

        public void Cull()
        {
            for (var i = 0; i < m_Clippers.Count; ++i)
            {
                m_Clippers[i].PerformClipping();
            }
        }

        public static void Register(IClipper c)
        {
            if (c == null)
                return;
            instance.m_Clippers.AddUnique(c);
        }

        public static void Unregister(IClipper c)
        {
            instance.m_Clippers.Remove(c);
        }
    }
}
