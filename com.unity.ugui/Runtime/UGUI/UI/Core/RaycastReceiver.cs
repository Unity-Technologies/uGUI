
namespace UnityEngine.UI
{
    /// <summary>
    /// Intercepts raycasts from a GraphicRaycaster.
    /// Use it to block objects or detect raycasts.
    /// </summary>
    [AddComponentMenu("UI (Canvas)/Raycast Receiver")]
    [RequireComponent(typeof(CanvasRenderer))]
    public class RaycastReceiver : Graphic
    {
        /// <summary>
        /// The material set by the user.
        /// </summary>
        /// <remarks>
        /// **Note: ** RaycastReceiver doesn't use a material.
        /// </remarks>
        public override Material material
        {
            get { return base.material; }
            set { base.material = value; }
        }

        /// <summary>
        /// Base color of the graphic.
        /// </summary>
        /// <remarks>
        /// RaycastReceiver doesn't use color. RaycastReceiver is an unseen graphic.
        /// </remarks>
        public override Color color
        {
            get { return base.color; }
            set { base.color = value; }
        }

        /// <summary>
        /// Generates the vertex buffer data for the UI element. Fills the vertex buffer data.
        /// </summary>
        /// <param name="vh">VertexHelper utility.</param>
        /// <remarks>
        /// RaycastReceiver clears this object's vertex buffer.
        /// This object will not be drawn as a result of this overridden function.
        /// </remarks>
        protected override void OnPopulateMesh(VertexHelper vh) { vh.Clear(); }
    }
}
