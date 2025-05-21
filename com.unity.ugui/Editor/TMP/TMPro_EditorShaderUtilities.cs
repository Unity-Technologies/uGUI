using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;


namespace TMPro.EditorUtilities
{

    public static class EditorShaderUtilities 
    {

        /// <summary>
        /// Copy Shader properties from source to destination material.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static void CopyMaterialProperties(Material source, Material destination)
        {
            MaterialProperty[] source_prop = MaterialEditor.GetMaterialProperties(new Material[] { source });

            for (int i = 0; i < source_prop.Length; i++)
            {
                int property_ID = Shader.PropertyToID(source_prop[i].name);
                if (destination.HasProperty(property_ID))
                {
                    //Debug.Log(source_prop[i].name + "  Type:" + source.shader.GetPropertyType(i));
                    switch (source.shader.GetPropertyType(i))
                    {
                        case UnityEngine.Rendering.ShaderPropertyType.Color:
                            destination.SetColor(property_ID, source.GetColor(property_ID));                          
                            break;
                        case UnityEngine.Rendering.ShaderPropertyType.Float:
                            destination.SetFloat(property_ID, source.GetFloat(property_ID));
                            break;
                        case UnityEngine.Rendering.ShaderPropertyType.Range:
                            destination.SetFloat(property_ID, source.GetFloat(property_ID));
                            break;
                        case UnityEngine.Rendering.ShaderPropertyType.Texture:
                            destination.SetTexture(property_ID, source.GetTexture(property_ID));
                            break;
                        case UnityEngine.Rendering.ShaderPropertyType.Vector:
                            destination.SetVector(property_ID, source.GetVector(property_ID));
                            break;
                    }
                }
            }

        }
      
    }

}