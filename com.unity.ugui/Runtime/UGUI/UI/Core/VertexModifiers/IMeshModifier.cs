using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    /// <summary>
    /// Interface which allows for the modification of verticies in a Graphic before they are passed to the CanvasRenderer.
    /// When a Graphic generates a list of vertices they are passed (in order) to any components on the GameObject that implement IMeshModifier. This component can modify the given Mesh.
    /// </summary>
    public interface IMeshModifier
    {
        /// <summary>Obsolete. Please use <see cref="ModifyMesh(VertexHelper)"/> instead.</summary>
        /// <param name="mesh">The <see cref="Mesh"/> to modify.</param>
        [Obsolete("Use IMeshModifier.ModifyMesh(VertexHelper verts) instead", true)]
        void ModifyMesh(Mesh mesh);

        /// <summary>
        /// Callback used for modifying the mesh. Place any custom mesh processing in this function.
        /// </summary>
        /// <param name="verts">The <see cref="VertexHelper"/> containing the mesh data to modify.</param>
        void ModifyMesh(VertexHelper verts);
    }
}
