using System.Collections.Generic;

namespace UnityEngine.UI
{
    public class GraphicRegistry
    {
        private static GraphicRegistry s_Instance;

        private readonly Dictionary<Canvas, List<Graphic>> m_Graphics = new Dictionary<Canvas, List<Graphic>>();

        protected GraphicRegistry()
        { }

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

            List<Graphic> graphics;
            instance.m_Graphics.TryGetValue(c, out graphics);

            if (graphics != null)
            {
                if (!graphics.Contains(graphic))
                    graphics.Add(graphic);
                return;
            }

            graphics = new List<Graphic> {graphic};
            instance.m_Graphics.Add(c, graphics);
        }

        public static void UnregisterGraphicForCanvas(Canvas c, Graphic graphic)
        {
            if (c == null)
                return;

            List<Graphic> graphics;
            instance.m_Graphics.TryGetValue(c, out graphics);

            if (graphics != null)
            {
                graphics.Remove(graphic);
            }
        }

        private static readonly List<Graphic> s_EmptyList = new List<Graphic>();
        public static IList<Graphic> GetGraphicsForCanvas(Canvas canvas)
        {
            List<Graphic> graphics;
            instance.m_Graphics.TryGetValue(canvas, out graphics);
            return graphics ?? s_EmptyList;
        }
    }
}
