using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Rendering;

namespace UnityEngine.UI
{
    /// <summary>
    /// A utility class that can aid in the generation of meshes for the UI.
    /// </summary>
    /// <remarks>
    /// This class implements IDisposable to aid with memory management.
    /// </remarks>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// using UnityEngine;
    /// using UnityEngine.UI;
    ///
    /// public class ExampleClass : MonoBehaviour
    /// {
    ///     Mesh m;
    ///
    ///     void Start()
    ///     {
    ///         Color32 color32 = Color.red;
    ///         using (var vh = new VertexHelper())
    ///         {
    ///             vh.AddVert(new Vector3(0, 0), color32, new Vector2(0f, 0f));
    ///             vh.AddVert(new Vector3(0, 100), color32, new Vector2(0f, 1f));
    ///             vh.AddVert(new Vector3(100, 100), color32, new Vector2(1f, 1f));
    ///             vh.AddVert(new Vector3(100, 0), color32, new Vector2(1f, 0f));
    ///
    ///             vh.AddTriangle(0, 1, 2);
    ///             vh.AddTriangle(2, 3, 0);
    ///             vh.FillMesh(m);
    ///         }
    ///     }
    /// }
    /// ]]>
    ///</code>
    /// </example>
    public class VertexHelper : IDisposable
    {
        // Single interleaved AoS storage. UIVertex's field order matches the GPU vertex
        // layout below (Position, Normal, Tangent, Color, TexCoord0-3, TexCoord4 for
        // prevPosition), so one mesh.SetVertexBufferData<UIVertex> call uploads the lot.
        NativeArray<UIVertex> m_Verts;
        NativeArray<ushort>   m_Indices;

        int m_VertCount;
        int m_VertCapacity;
        int m_IndexCount;
        int m_IndexCapacity;

        Allocator m_Allocator;

        const int k_InitialVertCapacity  = 64;
        const int k_InitialIndexCapacity = 96;
        const int k_MaxVertCount = 65000;

        static readonly Vector4 s_DefaultTangent = new Vector4(1.0f, 0.0f, 0.0f, -1.0f);
        static readonly Vector3 s_DefaultNormal = Vector3.back;

        /// <summary>
        /// Construct an empty VertexHelper backed by persistent native storage.
        /// </summary>
        public VertexHelper() : this(Allocator.Persistent) { }

        internal VertexHelper(Allocator allocator)
        {
            m_Allocator = allocator;
        }

        /// <summary>
        /// Construct a VertexHelper seeded with the contents of an existing Mesh.
        /// </summary>
        /// <param name="m">Mesh whose vertices, colors, UV channels (0-4), normals, tangents, and submesh 0 indices are copied into the helper.</param>
        public VertexHelper(Mesh m) : this(Allocator.Persistent)
        {
            // Cold path — only used by user code constructing VertexHelper from an existing Mesh.
            // Read every present channel through the Mesh.MeshData API: each copy lands directly
            // in a NativeArray with no managed array allocations, and UVs avoid the slow
            // Mesh.GetUVs(int, List<Vector4>) marshalling path. Absent channels fall back to the
            // UI defaults below.
            using var meshDataArray = Mesh.AcquireReadOnlyMeshData(m);
            Mesh.MeshData data = meshDataArray[0];

            int vertexCount = data.vertexCount;
            int indexCount = data.subMeshCount > 0 ? data.GetSubMesh(0).indexCount : 0;
            InitializeIfRequired(vertexCount, indexCount);

            using var positions = new NativeArray<Vector3>(vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            data.GetVertices(positions);

            bool hasNormals = data.HasVertexAttribute(VertexAttribute.Normal);
            using var normals = hasNormals ? new NativeArray<Vector3>(vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory) : default;
            if (hasNormals)
                data.GetNormals(normals);

            bool hasTangents = data.HasVertexAttribute(VertexAttribute.Tangent);
            using var tangents = hasTangents ? new NativeArray<Vector4>(vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory) : default;
            if (hasTangents)
                data.GetTangents(tangents);

            bool hasColors = data.HasVertexAttribute(VertexAttribute.Color);
            using var colors = hasColors ? new NativeArray<Color32>(vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory) : default;
            if (hasColors)
                data.GetColors(colors);

            NativeArray<Vector4> ReadUVChannel(int channel, int vertexCount, Mesh.MeshData data)
            {
                if (!data.HasVertexAttribute(VertexAttribute.TexCoord0 + channel))
                    return default;
                var uvs = new NativeArray<Vector4>(vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                data.GetUVs(channel, uvs);
                return uvs;
            }

            // uv0-3 land in TexCoord0-3; uv4 (prevPosition) lands in TexCoord4.
            using var uv0 = ReadUVChannel(0, vertexCount, data);
            using var uv1 = ReadUVChannel(1, vertexCount, data);
            using var uv2 = ReadUVChannel(2, vertexCount, data);
            using var uv3 = ReadUVChannel(3, vertexCount, data);
            using var uv4 = ReadUVChannel(4, vertexCount, data);

            for (int i = 0; i < vertexCount; i++)
            {
                m_Verts[i] = new UIVertex
                {
                    position     = positions[i],
                    normal       = hasNormals  ? normals[i]  : s_DefaultNormal,
                    tangent      = hasTangents ? tangents[i] : s_DefaultTangent,
                    color        = hasColors   ? colors[i]   : (Color32)Color.white,
                    uv0          = uv0.IsCreated ? uv0[i] : Vector4.zero,
                    uv1          = uv1.IsCreated ? uv1[i] : Vector4.zero,
                    uv2          = uv2.IsCreated ? uv2[i] : Vector4.zero,
                    uv3          = uv3.IsCreated ? uv3[i] : Vector4.zero,
                    prevPosition = uv4.IsCreated ? uv4[i] : Vector4.zero,
                };
            }
            m_VertCount = vertexCount;

            // Submesh 0 indices, read straight into the (already capacity-sized) index buffer.
            if (indexCount > 0)
            {
                data.GetIndices(m_Indices, 0);
                m_IndexCount = indexCount;
            }
        }

        void InitializeIfRequired(int vertCapacityHint = k_InitialVertCapacity, int indexCapacityHint = k_InitialIndexCapacity)
        {
            // Only allocate on first use. For subsequent calls, defer to the Ensure* helpers
            // so we never silently overwrite an existing NativeArray (which would leak native
            // memory) and never reset m_*Capacity below the live array length (which would
            // turn every AddVert/AddTriangle past the initial capacity into an O(N) reallocation).
            if (!m_Verts.IsCreated)
            {
                m_VertCapacity = Math.Max(vertCapacityHint, k_InitialVertCapacity);
                m_Verts = new NativeArray<UIVertex>(m_VertCapacity, m_Allocator, NativeArrayOptions.UninitializedMemory);
            }
            else if (vertCapacityHint > m_VertCapacity)
            {
                EnsureVertCapacity(vertCapacityHint);
            }

            if (!m_Indices.IsCreated)
            {
                m_IndexCapacity = Math.Max(indexCapacityHint, k_InitialIndexCapacity);
                m_Indices = new NativeArray<ushort>(m_IndexCapacity, m_Allocator, NativeArrayOptions.UninitializedMemory);
            }
            else if (indexCapacityHint > m_IndexCapacity)
            {
                EnsureIndexCapacity(indexCapacityHint);
            }
        }

        void EnsureVertCapacity(int needed)
        {
            if (needed <= m_VertCapacity) return;
            var newCap = Mathf.NextPowerOfTwo(needed);
            GrowNativeArray(ref m_Verts, newCap, m_VertCount);
            m_VertCapacity = newCap;
        }

        void EnsureIndexCapacity(int needed)
        {
            if (needed <= m_IndexCapacity) return;
            var newCap = Mathf.NextPowerOfTwo(needed);
            GrowNativeArray(ref m_Indices, newCap, m_IndexCount);
            m_IndexCapacity = newCap;
        }

        void GrowNativeArray<T>(ref NativeArray<T> arr, int newCapacity, int liveCount) where T : struct
        {
            var newArr = new NativeArray<T>(newCapacity, m_Allocator, NativeArrayOptions.UninitializedMemory);
            if (liveCount > 0)
                NativeArray<T>.Copy(arr, 0, newArr, 0, liveCount);
            arr.Dispose();
            arr = newArr;
        }

        internal int ReserveVerts(int count)
        {
            InitializeIfRequired();
            int start = m_VertCount;
            EnsureVertCapacity(start + count);
            m_VertCount = start + count;
            return start;
        }

        internal int ReserveIndices(int count)
        {
            InitializeIfRequired();
            int start = m_IndexCount;
            EnsureIndexCapacity(start + count);
            m_IndexCount = start + count;
            return start;
        }

        // Append a quad (4 verts + 6 indices) in one call, writing straight into the
        // native buffers. Matches the field defaults of AddVert(position, color, uv0).
        internal void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Color32 color, Vector4 p0uv0, Vector4 p1uv0, Vector4 p2uv0, Vector4 p3uv0)
        {
            InitializeIfRequired();

            int v = m_VertCount;
            EnsureVertCapacity(v + 4);
            m_Verts[v + 0] = new UIVertex { position = p0, normal = s_DefaultNormal, tangent = s_DefaultTangent, color = color, uv0 = p0uv0 };
            m_Verts[v + 1] = new UIVertex { position = p1, normal = s_DefaultNormal, tangent = s_DefaultTangent, color = color, uv0 = p1uv0 };
            m_Verts[v + 2] = new UIVertex { position = p2, normal = s_DefaultNormal, tangent = s_DefaultTangent, color = color, uv0 = p2uv0 };
            m_Verts[v + 3] = new UIVertex { position = p3, normal = s_DefaultNormal, tangent = s_DefaultTangent, color = color, uv0 = p3uv0 };
            m_VertCount = v + 4;

            int t = m_IndexCount;
            EnsureIndexCapacity(t + 6);
            m_Indices[t] = (ushort)(v);
            m_Indices[t + 1] = (ushort)(v + 1);
            m_Indices[t + 2] = (ushort)(v + 2);
            m_Indices[t + 3] = (ushort)(v + 2);
            m_Indices[t + 4] = (ushort)(v + 3);
            m_Indices[t + 5] = (ushort)(v);
            m_IndexCount = t + 6;
        }

        // Append `count` indices copied from [srcStart, srcStart+count), each shifted by
        // vertexOffset. Caller must keep srcStart+count <= currentIndexCount: ReserveIndices
        // grows/copies the live [0, currentIndexCount) range before we read it, so a source
        // range inside it stays valid even when the append triggers a realloc.
        internal void AddShiftedIndices(int srcStart, int count, int vertexOffset)
        {
            int dst = ReserveIndices(count);
            for (int i = 0; i < count; i++)
                m_Indices[dst + i] = (ushort)(m_Indices[srcStart + i] + vertexOffset);
        }

        /// <summary>
        /// Cleanup allocated memory.
        /// </summary>
        public void Dispose()
        {
            if (m_Verts.IsCreated)   m_Verts.Dispose();
            if (m_Indices.IsCreated) m_Indices.Dispose();

            m_VertCount = 0;
            m_IndexCount = 0;
            m_VertCapacity = 0;
            m_IndexCapacity = 0;
        }

        /// <summary>
        /// Clear all vertices from the stream.
        /// </summary>
        public void Clear()
        {
            m_VertCount = 0;
            m_IndexCount = 0;
        }

        /// <summary>
        /// Current number of vertices in the buffer.
        /// </summary>
        public int currentVertCount => m_VertCount;

        /// <summary>
        /// Get the number of indices set on the VertexHelper.
        /// </summary>
        public int currentIndexCount => m_IndexCount;

        /// <summary>
        /// Fill a UIVertex with data from index i of the stream.
        /// </summary>
        /// <param name="vertex">Vertex to populate</param>
        /// <param name="i">Index to populate.</param>
        public void PopulateUIVertex(ref UIVertex vertex, int i)
        {
            InitializeIfRequired();
            vertex = m_Verts[i];
        }

        /// <summary>
        /// Set a UIVertex at the given index.
        /// </summary>
        /// <param name="vertex">The vertex to fill</param>
        /// <param name="i">the position in the current list to fill.</param>
        public void SetUIVertex(UIVertex vertex, int i)
        {
            InitializeIfRequired();
            m_Verts[i] = vertex;
        }

        // Cached single-stream layout — built once, reused for every FillMesh call.
        static readonly VertexAttributeDescriptor[] s_VertexLayout =
        {
            new VertexAttributeDescriptor(VertexAttribute.Position,  VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal,    VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Tangent,   VertexAttributeFormat.Float32, 4),
            new VertexAttributeDescriptor(VertexAttribute.Color,     VertexAttributeFormat.UNorm8,  4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord2, VertexAttributeFormat.Float32, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord3, VertexAttributeFormat.Float32, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord4, VertexAttributeFormat.Float32, 4),
        };

        /// <summary>
        /// Fill the given mesh with the stream data.
        /// </summary>
        /// <param name="mesh">The mesh to write the current vertex and index data into.</param>
        public void FillMesh(Mesh mesh)
        {
            InitializeIfRequired();

            if (m_VertCount >= k_MaxVertCount)
                throw new ArgumentException($"Mesh can not have more than {k_MaxVertCount} vertices");

            if (m_VertCount == 0 || m_IndexCount == 0)
            {
                mesh.Clear();
                return;
            }

            // Keep bounds + notify bypass for perf (we call RecalculateBounds explicitly,
            // and UI meshes are consumed via CanvasRenderer.SetMesh — no listeners).
            // Don't bypass index validation: FillMesh is public and bad indices crash the GPU.
            const MeshUpdateFlags k_BypassFlags =
                MeshUpdateFlags.DontRecalculateBounds
                | MeshUpdateFlags.DontNotifyMeshUsers;

            // Single-stream interleaved AoS layout matches UIVertex's field order. One
            // SetVertexBufferData call uploads all 9 channels in one bulk memcpy.
            mesh.SetVertexBufferParams(m_VertCount, s_VertexLayout);
            mesh.SetIndexBufferParams(m_IndexCount, IndexFormat.UInt16);
            mesh.SetVertexBufferData(m_Verts, 0, 0, m_VertCount, 0, k_BypassFlags);
            mesh.SetIndexBufferData(m_Indices, 0, 0, m_IndexCount, k_BypassFlags);

            mesh.subMeshCount = 1;
            // Must set vertexCount on the descriptor explicitly: SetSubMesh only
            // auto-derives it from the index data when DontRecalculateBounds is unset.
            // Leaving it at the default (0) makes the draw call submit zero verts.
            mesh.SetSubMesh(0, new SubMeshDescriptor(0, m_IndexCount, MeshTopology.Triangles)
            {
                firstVertex = 0,
                vertexCount = m_VertCount,
            }, k_BypassFlags);
            mesh.RecalculateBounds();
        }

        /// <summary>
        /// Add a single vertex to the stream.
        /// </summary>
        /// <param name="position">Position of the vert</param>
        /// <param name="color">Color of the vert</param>
        /// <param name="uv0">UV of the vert</param>
        /// <param name="uv1">UV1 of the vert</param>
        /// <param name="uv2">UV2 of the vert</param>
        /// <param name="uv3">UV3 of the vert</param>
        /// <param name="normal">Normal of the vert.</param>
        /// <param name="tangent">Tangent of the vert</param>
        /// <param name="prevPosition">Previous position of the vert (in the UV4 slot)</param>
        public void AddVert(Vector3 position, Color32 color, Vector4 uv0, Vector4 uv1, Vector4 uv2, Vector4 uv3, Vector3 normal, Vector4 tangent, Vector4 prevPosition)
        {
            InitializeIfRequired();
            EnsureVertCapacity(m_VertCount + 1);

            m_Verts[m_VertCount] = new UIVertex
            {
                position     = position,
                normal       = normal,
                tangent      = tangent,
                color        = color,
                uv0          = uv0,
                uv1          = uv1,
                uv2          = uv2,
                uv3          = uv3,
                prevPosition = prevPosition,
            };
            m_VertCount++;
        }

        /// <summary>
        /// Add a single vertex to the stream.
        /// </summary>
        /// <param name="position">Position of the vert</param>
        /// <param name="color">Color of the vert</param>
        /// <param name="uv0">UV of the vert</param>
        /// <param name="uv1">UV1 of the vert</param>
        /// <param name="uv2">UV2 of the vert</param>
        /// <param name="uv3">UV3 of the vert</param>
        /// <param name="normal">Normal of the vert.</param>
        /// <param name="tangent">Tangent of the vert</param>
        public void AddVert(Vector3 position, Color32 color, Vector4 uv0, Vector4 uv1, Vector4 uv2, Vector4 uv3, Vector3 normal, Vector4 tangent)
        {
            AddVert(position, color, uv0, uv1, uv2, uv3, normal, tangent, Vector4.zero);
        }

        /// <summary>
        /// Add a single vertex to the stream.
        /// </summary>
        /// <param name="position">Position of the vert</param>
        /// <param name="color">Color of the vert</param>
        /// <param name="uv0">UV of the vert</param>
        /// <param name="uv1">UV1 of the vert</param>
        /// <param name="normal">Normal of the vert.</param>
        /// <param name="tangent">Tangent of the vert</param>
        public void AddVert(Vector3 position, Color32 color, Vector4 uv0, Vector4 uv1, Vector3 normal, Vector4 tangent)
        {
            AddVert(position, color, uv0, uv1, Vector4.zero, Vector4.zero, normal, tangent, Vector4.zero);
        }

        /// <summary>
        /// Add a single vertex to the stream.
        /// </summary>
        /// <param name="position">Position of the vert</param>
        /// <param name="color">Color of the vert</param>
        /// <param name="uv0">UV of the vert</param>
        public void AddVert(Vector3 position, Color32 color, Vector4 uv0)
        {
            AddVert(position, color, uv0, Vector4.zero, s_DefaultNormal, s_DefaultTangent);
        }

        /// <summary>
        /// Add a single vertex to the stream.
        /// </summary>
        /// <param name="v">The vertex to add</param>
        public void AddVert(UIVertex v)
        {
            InitializeIfRequired();
            EnsureVertCapacity(m_VertCount + 1);
            m_Verts[m_VertCount++] = v;
        }

        /// <summary>
        /// Add a triangle to the buffer.
        /// </summary>
        /// <param name="idx0">index 0</param>
        /// <param name="idx1">index 1</param>
        /// <param name="idx2">index 2</param>
        public void AddTriangle(int idx0, int idx1, int idx2)
        {
            InitializeIfRequired();
            EnsureIndexCapacity(m_IndexCount + 3);

            m_Indices[m_IndexCount + 0] = (ushort)idx0;
            m_Indices[m_IndexCount + 1] = (ushort)idx1;
            m_Indices[m_IndexCount + 2] = (ushort)idx2;
            m_IndexCount += 3;
        }

        /// <summary>
        /// Add a quad to the stream.
        /// </summary>
        /// <param name="verts">4 Vertices representing the quad.</param>
        public void AddUIVertexQuad(UIVertex[] verts)
        {
            int startIndex = m_VertCount;

            InitializeIfRequired();
            EnsureVertCapacity(startIndex + 4);
            m_Verts[startIndex + 0] = verts[0];
            m_Verts[startIndex + 1] = verts[1];
            m_Verts[startIndex + 2] = verts[2];
            m_Verts[startIndex + 3] = verts[3];
            m_VertCount = startIndex + 4;

            AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

        /// <summary>
        /// Add a stream of custom UIVertex and corresponding indices.
        /// </summary>
        /// <param name="verts">The custom stream of verts to add to the helpers internal data.</param>
        /// <param name="indices">The custom stream of indices to add to the helpers internal data.</param>
        public void AddUIVertexStream(List<UIVertex> verts, List<int> indices)
        {
            InitializeIfRequired();

            if (verts != null && verts.Count > 0)
            {
                int addedVerts = verts.Count;
                EnsureVertCapacity(addedVerts);

                var backing = NoAllocHelpers.ExtractArrayFromList(verts);
                NativeArray<UIVertex>.Copy(backing, 0, m_Verts, 0, addedVerts);
                m_VertCount = addedVerts;
            }

            if (indices != null && indices.Count > 0)
            {
                int addedIdx = indices.Count;
                EnsureIndexCapacity(addedIdx);
                var backing = NoAllocHelpers.ExtractArrayFromList(indices);
                for (int i = 0; i < addedIdx; i++)
                    m_Indices[i] = (ushort)backing[i];
                m_IndexCount = addedIdx;
            }
        }

        /// <summary>
        /// Add a list of triangles to the stream. Each group of 3 verts becomes one triangle.
        /// </summary>
        /// <param name="verts">Vertices to add. Length should be divisible by 3.</param>
        public void AddUIVertexTriangleStream(List<UIVertex> verts)
        {
            if (verts == null || verts.Count == 0) return;

            InitializeIfRequired();

            int addedVerts = verts.Count;
            EnsureVertCapacity(addedVerts);

            var backing = NoAllocHelpers.ExtractArrayFromList(verts);
            NativeArray<UIVertex>.Copy(backing, 0, m_Verts, 0, addedVerts);
            m_VertCount = addedVerts;

            EnsureIndexCapacity(addedVerts);
            for (int i = 0; i < addedVerts; i++)
                m_Indices[i] = (ushort)i;
            m_IndexCount = addedVerts;
        }

        /// <summary>
        /// Create a stream of UI vertex (in triangles) from the stream.
        /// </summary>
        /// <param name="stream">The list to populate with the current vertex data in triangle-stream format.</param>
        public void GetUIVertexStream(List<UIVertex> stream)
        {
            if (stream == null) return;

            InitializeIfRequired();

            // Expand index list back into a flat per-triangle vert stream.
            NoAllocHelpers.EnsureListElemCount(stream, m_IndexCount);
            if (m_IndexCount == 0) return;
            var backing = NoAllocHelpers.ExtractArrayFromList(stream);
            for (int i = 0; i < m_IndexCount; i++)
                backing[i] = m_Verts[m_Indices[i]];
        }
    }
}
