using System.Collections.Generic;

namespace UnityEngine.UI
{
    public interface IVertexModifier
    {
        void ModifyVertices(List<UIVertex> verts);
    }
}
