namespace UnityEngine.EventSystems
{
    public struct RaycastResult
    {
        private GameObject m_GameObject; // Game object hit by the raycast

        public GameObject gameObject
        {
            get { return m_GameObject; }
            set { m_GameObject = value; }
        }

        public BaseRaycaster module; // Event system that hit this object
        public float distance; // The distance from the origin this hit was.
        public float index; // The index this element is in the raycastList (used for sorting)
        public int depth;
        public int sortingLayer;
        public int sortingOrder;

        public bool isValid
        {
            get { return module != null && gameObject != null; }
        }

        public void Clear()
        {
            gameObject = null;
            module = null;
            distance = 0;
            index = 0;
            depth = 0;
        }

        public override string ToString()
        {
            return "Name: " + gameObject.name + "\n" +
                   "module: " + module.camera + "\n" +
                   "distance: " + distance + "\n" +
                   "index: " + index + "\n" +
                   "depth: " + depth + "\n" +
                   "module.sortOrderPriority: " + module.sortOrderPriority + "\n" +
                   "module.renderOrderPriority: " + module.renderOrderPriority + "\n" +
                   "sortingLayer: " + sortingLayer + "\n" +
                   "sortingOrder: " + sortingOrder;
        }
    }
}
