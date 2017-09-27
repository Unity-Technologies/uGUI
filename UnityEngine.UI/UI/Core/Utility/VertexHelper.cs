using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public class VertexHelper : IDisposable
    {
        private List<Vector3> m_Positions = ListPool<Vector3>.Get();
        private List<Color32> m_Colors = ListPool<Color32>.Get();
        private List<Vector2> m_Uv0S = ListPool<Vector2>.Get();
        private List<Vector2> m_Uv1S = ListPool<Vector2>.Get();
        private List<Vector2> m_Uv2S = ListPool<Vector2>.Get();
        private List<Vector2> m_Uv3S = ListPool<Vector2>.Get();
        private List<Vector3> m_Normals = ListPool<Vector3>.Get();
        private List<Vector4> m_Tangents = ListPool<Vector4>.Get();
        private List<int> m_Indices = ListPool<int>.Get();

        private static readonly Vector4 s_DefaultTangent = new Vector4(1.0f, 0.0f, 0.0f, -1.0f);
        private static readonly Vector3 s_DefaultNormal = Vector3.back;

        public VertexHelper()
        {}

        public VertexHelper(Mesh m)
        {
            m_Positions.AddRange(m.vertices);
            m_Colors.AddRange(m.colors32);
            m_Uv0S.AddRange(m.uv);
            m_Uv1S.AddRange(m.uv2);
            m_Uv2S.AddRange(m.uv3);
            m_Uv3S.AddRange(m.uv4);
            m_Normals.AddRange(m.normals);
            m_Tangents.AddRange(m.tangents);
            m_Indices.AddRange(m.GetIndices(0));
        }

        public void Clear()
        {
            m_Positions.Clear();
            m_Colors.Clear();
            m_Uv0S.Clear();
            m_Uv1S.Clear();
            m_Uv2S.Clear();
            m_Uv3S.Clear();
            m_Normals.Clear();
            m_Tangents.Clear();
            m_Indices.Clear();
        }

        public int currentVertCount
        {
            get { return m_Positions.Count; }
        }
        public int currentIndexCount
        {
            get { return m_Indices.Count; }
        }

        public void PopulateUIVertex(ref UIVertex vertex, int i)
        {
            vertex.position = m_Positions[i];
            vertex.color = m_Colors[i];
            vertex.uv0 = m_Uv0S[i];
            vertex.uv1 = m_Uv1S[i];
            vertex.uv2 = m_Uv2S[i];
            vertex.uv3 = m_Uv3S[i];
            vertex.normal = m_Normals[i];
            vertex.tangent = m_Tangents[i];
        }

        public void SetUIVertex(UIVertex vertex, int i)
        {
            m_Positions[i] = vertex.position;
            m_Colors[i] = vertex.color;
            m_Uv0S[i] = vertex.uv0;
            m_Uv1S[i] = vertex.uv1;
            m_Uv2S[i] = vertex.uv2;
            m_Uv3S[i] = vertex.uv3;
            m_Normals[i] = vertex.normal;
            m_Tangents[i] = vertex.tangent;
        }

        public void FillMesh(Mesh mesh)
        {
            mesh.Clear();

            if (m_Positions.Count >= 65000)
                throw new ArgumentException("Mesh can not have more than 65000 vertices");

            mesh.SetVertices(m_Positions);
            mesh.SetColors(m_Colors);
            mesh.SetUVs(0, m_Uv0S);
            mesh.SetUVs(1, m_Uv1S);
            mesh.SetUVs(2, m_Uv2S);
            mesh.SetUVs(3, m_Uv3S);
            mesh.SetNormals(m_Normals);
            mesh.SetTangents(m_Tangents);
            mesh.SetTriangles(m_Indices, 0);
            mesh.RecalculateBounds();
        }

        public void Dispose()
        {
            ListPool<Vector3>.Release(m_Positions);
            ListPool<Color32>.Release(m_Colors);
            ListPool<Vector2>.Release(m_Uv0S);
            ListPool<Vector2>.Release(m_Uv1S);
            ListPool<Vector2>.Release(m_Uv2S);
            ListPool<Vector2>.Release(m_Uv3S);
            ListPool<Vector3>.Release(m_Normals);
            ListPool<Vector4>.Release(m_Tangents);
            ListPool<int>.Release(m_Indices);

            m_Positions = null;
            m_Colors = null;
            m_Uv0S = null;
            m_Uv1S = null;
            m_Uv2S = null;
            m_Uv3S = null;
            m_Normals = null;
            m_Tangents = null;
            m_Indices = null;
        }

        public void AddVert(Vector3 position, Color32 color, Vector2 uv0, Vector2 uv1, Vector3 normal, Vector4 tangent)
        {
            m_Positions.Add(position);
            m_Colors.Add(color);
            m_Uv0S.Add(uv0);
            m_Uv1S.Add(uv1);
            m_Uv2S.Add(Vector2.zero);
            m_Uv3S.Add(Vector2.zero);
            m_Normals.Add(normal);
            m_Tangents.Add(tangent);
        }

        public void AddVert(Vector3 position, Color32 color, Vector2 uv0)
        {
            AddVert(position, color, uv0, Vector2.zero, s_DefaultNormal, s_DefaultTangent);
        }

        public void AddVert(UIVertex v)
        {
            AddVert(v.position, v.color, v.uv0, v.uv1, v.normal, v.tangent);
        }

        public void AddTriangle(int idx0, int idx1, int idx2)
        {
            m_Indices.Add(idx0);
            m_Indices.Add(idx1);
            m_Indices.Add(idx2);
        }

        public void AddUIVertexQuad(UIVertex[] verts)
        {
            int startIndex = currentVertCount;

            for (int i = 0; i < 4; i++)
                AddVert(verts[i].position, verts[i].color, verts[i].uv0, verts[i].uv1, verts[i].normal, verts[i].tangent);

            AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

        public void AddUIVertexStream(List<UIVertex> verts, List<int> indices)
        {
            if (verts != null)
            {
                CanvasRenderer.AddUIVertexStream(verts, m_Positions, m_Colors, m_Uv0S, m_Uv1S, m_Uv2S, m_Uv3S, m_Normals, m_Tangents);
            }

            if (indices != null)
            {
                m_Indices.AddRange(indices);
            }
        }

        public void AddUIVertexTriangleStream(List<UIVertex> verts)
        {
            if (verts == null)
                return;
            CanvasRenderer.SplitUIVertexStreams(verts, m_Positions, m_Colors, m_Uv0S, m_Uv1S, m_Uv2S, m_Uv3S, m_Normals, m_Tangents, m_Indices);
        }

        public void GetUIVertexStream(List<UIVertex> stream)
        {
            if (stream == null)
                return;

            CanvasRenderer.CreateUIVertexStream(stream, m_Positions, m_Colors, m_Uv0S, m_Uv1S, m_Uv2S, m_Uv3S, m_Normals, m_Tangents, m_Indices);
        }
    }
}
