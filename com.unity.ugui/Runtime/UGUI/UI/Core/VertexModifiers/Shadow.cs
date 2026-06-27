using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace UnityEngine.UI
{
    /// <summary>
    /// Adds a drop shadow to a graphic by duplicating its mesh vertices, offsetting them by
    /// <see cref="effectDistance"/>, and recoloring them with <see cref="effectColor"/>.
    /// </summary>
    [AddComponentMenu("UI (Canvas)/Effects/Shadow", 80)]
    [UGUIHelpURL("Shadow")]
    public class Shadow : BaseMeshEffect
    {
        [SerializeField]
        private Color m_EffectColor = new Color(0f, 0f, 0f, 0.5f);

        [SerializeField]
        private Vector2 m_EffectDistance = new Vector2(1f, -1f);

        [SerializeField]
        private bool m_UseGraphicAlpha = true;

        private const float kMaxEffectDistance = 600f;

        /// <summary>
        /// Protected default constructor. Use <see cref="GameObject.AddComponent{T}"/> to add a Shadow to a GameObject.
        /// </summary>
        protected Shadow()
        {}

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            effectDistance = m_EffectDistance;
            base.OnValidate();
        }

#endif
        /// <summary>
        /// Color for the effect
        /// </summary>
        public Color effectColor
        {
            get { return m_EffectColor; }
            set
            {
                m_EffectColor = value;
                if (graphic != null)
                    graphic.SetVerticesDirty();
            }
        }

        /// <summary>
        /// How far is the shadow from the graphic.
        /// </summary>
        public Vector2 effectDistance
        {
            get { return m_EffectDistance; }
            set
            {
                if (value.x > kMaxEffectDistance)
                    value.x = kMaxEffectDistance;
                if (value.x < -kMaxEffectDistance)
                    value.x = -kMaxEffectDistance;

                if (value.y > kMaxEffectDistance)
                    value.y = kMaxEffectDistance;
                if (value.y < -kMaxEffectDistance)
                    value.y = -kMaxEffectDistance;

                if (m_EffectDistance == value)
                    return;

                m_EffectDistance = value;

                if (graphic != null)
                    graphic.SetVerticesDirty();
            }
        }

        /// <summary>
        /// Should the shadow inherit the alpha from the graphic?
        /// </summary>
        public bool useGraphicAlpha
        {
            get { return m_UseGraphicAlpha; }
            set
            {
                m_UseGraphicAlpha = value;
                if (graphic != null)
                    graphic.SetVerticesDirty();
            }
        }

        /// <summary>
        /// Appends shadow copies of the vertices in <paramref name="verts"/> from index <paramref name="start"/> up to
        /// (but not including) <paramref name="end"/>, offset by (<paramref name="x"/>, <paramref name="y"/>) and
        /// recolored with <paramref name="color"/>. Pre-sizes the list capacity to avoid extra allocations.
        /// </summary>
        /// <param name="verts">The vertex list to append the shadow copies to.</param>
        /// <param name="color">The color to apply to the shadow vertices.</param>
        /// <param name="start">The index of the first vertex in <paramref name="verts"/> to copy.</param>
        /// <param name="end">The exclusive upper index of vertices in <paramref name="verts"/> to copy.</param>
        /// <param name="x">Horizontal offset of the shadow.</param>
        /// <param name="y">Vertical offset of the shadow.</param>
        protected void ApplyShadowZeroAlloc(List<UIVertex> verts, Color32 color, int start, int end, float x, float y)
        {
            UIVertex vt;

            var neededCapacity = verts.Count + end - start;
            if (verts.Capacity < neededCapacity)
                verts.Capacity = neededCapacity;

            for (int i = start; i < end; ++i)
            {
                vt = verts[i];
                verts.Add(vt);

                Vector3 v = vt.position;
                v.x += x;
                v.y += y;
                vt.position = v;
                var newColor = color;
                if (m_UseGraphicAlpha)
                    newColor.a = (byte)((newColor.a * verts[i].color.a) / 255);
                vt.color = newColor;
                verts[i] = vt;
            }
        }

        /// <summary>
        /// Duplicate vertices from start to end and turn them into shadows with the given offset.
        /// </summary>
        /// <param name="verts">Vert list to copy</param>
        /// <param name="color">Shadow color</param>
        /// <param name="start">The start index in the verts list</param>
        /// <param name="end">The end index in the vers list</param>
        /// <param name="x">The shadows x offset</param>
        /// <param name="y">The shadows y offset</param>
        protected void ApplyShadow(List<UIVertex> verts, Color32 color, int start, int end, float x, float y)
        {
            ApplyShadowZeroAlloc(verts, color, start, end, x, y);
        }

        /// <summary>
        /// Applies the shadow effect to the geometry held in <paramref name="vh"/> by appending
        /// offset, recolored copies of all current vertices behind the originals.
        /// </summary>
        /// <param name="vh">The <see cref="VertexHelper"/> containing the graphic's mesh data to modify.</param>
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
                return;

            var output = ListPool<UIVertex>.Get();
            vh.GetUIVertexStream(output);

            ApplyShadow(output, effectColor, 0, output.Count, effectDistance.x, effectDistance.y);
            vh.Clear();
            vh.AddUIVertexTriangleStream(output);
            ListPool<UIVertex>.Release(output);
        }
    }
}
