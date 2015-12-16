using System.Collections.Generic;
using UnityEngine.UI.Collections;

namespace UnityEngine.UI
{
    public class GraphicRegistry
    {
        private static GraphicRegistry s_Instance;

        private readonly Dictionary<Canvas, IndexedSet<Graphic>> m_Graphics = new Dictionary<Canvas, IndexedSet<Graphic>>();

        protected GraphicRegistry()
        {
            // This is needed for AOT on IOS. Without it the compile doesn't get the definition of the Dictionarys
#pragma warning disable 168
            Dictionary<Graphic, int> emptyGraphicDic;
            Dictionary<ICanvasElement, int> emptyElementDic;
#pragma warning restore 168
        }

        public static GraphicRegistry instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = new GraphicRegistry();
                return s_Instance;
            }
        }

        public static void RegisterGraphicForCanvas(Canvas c, Graphic graphic)
        {
            if (c == null)
                return;

            IndexedSet<Graphic> graphics;
            instance.m_Graphics.TryGetValue(c, out graphics);

            if (graphics != null)
            {
                graphics.AddUnique(graphic);
                return;
            }

            // Dont need to AddUnique as we know its the only item in the list
            graphics = new IndexedSet<Graphic>();
            graphics.Add(graphic);
            instance.m_Graphics.Add(c, graphics);
        }

        public static void UnregisterGraphicForCanvas(Canvas c, Graphic graphic)
        {
            if (c == null)
                return;

            IndexedSet<Graphic> graphics;
            if (instance.m_Graphics.TryGetValue(c, out graphics))
            {
                graphics.Remove(graphic);
            }
        }

        private static readonly List<Graphic> s_EmptyList = new List<Graphic>();
        public static IList<Graphic> GetGraphicsForCanvas(Canvas canvas)
        {
            IndexedSet<Graphic> graphics;
            if (instance.m_Graphics.TryGetValue(canvas, out graphics))
                return graphics;

            return s_EmptyList;
        }
    }
}
