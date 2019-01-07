using System.Collections.Generic;
using UnityEngine.UI.Collections;

namespace UnityEngine.UI
{
    /// <summary>
    ///   Registry which maps a Graphic to the canvas it belongs to
    /// </summary>
    public class GraphicRegistry
    {
        private static GraphicRegistry s_Instance;

        private readonly Dictionary<Canvas, IndexedSet<Graphic>> m_Graphics = new Dictionary<Canvas, IndexedSet<Graphic>>();

        protected GraphicRegistry()
        {
            // Avoid runtime generation of these types. Some platforms are AOT only and do not support
            // JIT. What's more we actually create a instance of the required types instead of
            // just declaring an unused variable which may be optimized away by some compilers (Mono vs MS).

            // See: 877060

            System.GC.KeepAlive(new Dictionary<Graphic, int>());
            System.GC.KeepAlive(new Dictionary<ICanvasElement, int>());
            System.GC.KeepAlive(new Dictionary<IClipper, int>());
        }

        /// <summary>
        /// The singleton instance of the GraphicRegistry. Creates new if non exist.
        /// </summary>
        public static GraphicRegistry instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = new GraphicRegistry();
                return s_Instance;
            }
        }

        /// <summary>
        /// Store a link between the given canvas and graphic in the registry.
        /// </summary>
        /// <param name="c">The canvas the graphic will be associated to</param>
        /// <param name="graphic">The graphic in question.</param>
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

        /// <summary>
        /// Deregister the given Graphic from a Canvas.
        /// </summary>
        /// <param name="c">The canvas that should be associated with the graphic</param>
        /// <param name="graphic">The graphic to remove.</param>
        public static void UnregisterGraphicForCanvas(Canvas c, Graphic graphic)
        {
            if (c == null)
                return;

            IndexedSet<Graphic> graphics;
            if (instance.m_Graphics.TryGetValue(c, out graphics))
            {
                graphics.Remove(graphic);

                if (graphics.Count == 0)
                    instance.m_Graphics.Remove(c);
            }
        }

        private static readonly List<Graphic> s_EmptyList = new List<Graphic>();

        /// <summary>
        /// Get the list of associated graphics that are registered to a canvas.
        /// </summary>
        /// <param name="canvas">The canvas whose Graphics we are looking for</param>
        /// <returns>The list of all Graphics for the given Canvas.</returns>
        public static IList<Graphic> GetGraphicsForCanvas(Canvas canvas)
        {
            IndexedSet<Graphic> graphics;
            if (instance.m_Graphics.TryGetValue(canvas, out graphics))
                return graphics;

            return s_EmptyList;
        }
    }
}
