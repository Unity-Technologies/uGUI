using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.TextCore;

namespace TMPro
{
    // Base class inherited by the various TextMeshPro Assets.
    [Serializable]
    public abstract class TMP_Asset : ScriptableObject
    {
        /// <summary>
        /// The version of the text asset class.
        /// Version 1.1.0 introduces new data structure to be compatible with new font asset structure.
        /// </summary>
        public string version
        {
            get { return m_Version; }
            internal set { m_Version = value; }
        }

        /// <summary>
        /// Instance ID of the TMP Asset
        /// </summary>
        public int instanceID
        {
            get
            {
                if (m_InstanceID == 0)
                    m_InstanceID = GetInstanceID();

                return m_InstanceID;
            }
        }

        /// <summary>
        /// HashCode based on the name of the asset.
        /// </summary>
        public int hashCode
        {
            get
            {
                if (m_HashCode == 0)
                    m_HashCode = TMP_TextUtilities.GetHashCode(name);

                return m_HashCode;
            }
            set => m_HashCode = value;
        }

        /// <summary>
        /// Information about the face of the asset.
        /// </summary>
        public FaceInfo faceInfo
        {
            get { return m_FaceInfo; }
            set { m_FaceInfo = value; }
        }

        /// <summary>
        /// The material used by this asset.
        /// </summary>
        public Material material
        {
            get => m_Material;
            set => m_Material = value;
        }

        /// <summary>
        /// HashCode based on the name of the material assigned to this asset.
        /// </summary>
        public int materialHashCode
        {
            get
            {
                if (m_MaterialHashCode == 0)
                {
                    if (m_Material == null)
                        return 0;

                    m_MaterialHashCode = TMP_TextUtilities.GetSimpleHashCode(m_Material.name);
                }

                return m_MaterialHashCode;
            }
            set => m_MaterialHashCode = value;
        }

        // =============================================
        // Private backing fields for public properties.
        // =============================================

        [SerializeField]
        internal string m_Version;

        internal int m_InstanceID;

        internal int m_HashCode;

        [SerializeField]
        internal FaceInfo m_FaceInfo;

        [SerializeField][FormerlySerializedAs("material")]
        internal Material m_Material;

        internal int m_MaterialHashCode;
    }
}
