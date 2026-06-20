using System;
using System.Linq;
using UnityEngine;


namespace TMPro
{
    public static class ShaderUtilities
    {
        // Shader Property IDs
        public static readonly int ID_MainTex = Shader.PropertyToID("_MainTex");

        public static readonly int ID_FaceTex = Shader.PropertyToID("_FaceTex");
        public static readonly int ID_FaceColor = Shader.PropertyToID("_FaceColor");
        public static readonly int ID_FaceDilate = Shader.PropertyToID("_FaceDilate");
        public static readonly int ID_Shininess = Shader.PropertyToID("_FaceShininess");

        /// <summary>
        /// Property ID for the _OutlineOffset1 shader property used by URP and HDRP shaders
        /// </summary>
        public static readonly int ID_OutlineOffset1 = Shader.PropertyToID("_OutlineOffset1");

        /// <summary>
        /// Property ID for the _OutlineOffset2 shader property used by URP and HDRP shaders
        /// </summary>
        public static readonly int ID_OutlineOffset2 = Shader.PropertyToID("_OutlineOffset2");

        /// <summary>
        /// Property ID for the _OutlineOffset3 shader property used by URP and HDRP shaders
        /// </summary>
        public static readonly int ID_OutlineOffset3 = Shader.PropertyToID("_OutlineOffset3");

        /// <summary>
        /// Property ID for the ID_AdditiveOutlineMode shader property used by URP and HDRP shaders
        /// </summary>
        public static readonly int ID_OutlineMode = Shader.PropertyToID("_OutlineMode");

        /// <summary>
        /// Property ID for the _IsoPerimeter shader property used by URP and HDRP shaders
        /// </summary>
        public static readonly int ID_IsoPerimeter = Shader.PropertyToID("_IsoPerimeter");

        /// <summary>
        /// Property ID for the _Softness shader property used by URP and HDRP shaders
        /// </summary>
        public static readonly int ID_Softness = Shader.PropertyToID("_Softness");

        public static readonly int ID_UnderlayColor = Shader.PropertyToID("_UnderlayColor");
        public static readonly int ID_UnderlayOffsetX = Shader.PropertyToID("_UnderlayOffsetX");
        public static readonly int ID_UnderlayOffsetY = Shader.PropertyToID("_UnderlayOffsetY");
        public static readonly int ID_UnderlayDilate = Shader.PropertyToID("_UnderlayDilate");
        public static readonly int ID_UnderlaySoftness = Shader.PropertyToID("_UnderlaySoftness");

        /// <summary>
        /// Property ID for the _UnderlayOffset shader property used by URP and HDRP shaders
        /// </summary>
        public static readonly int ID_UnderlayOffset = Shader.PropertyToID("_UnderlayOffset");

        /// <summary>
        /// Property ID for the _UnderlayIsoPerimeter shader property used by URP and HDRP shaders
        /// </summary>
        public static readonly int ID_UnderlayIsoPerimeter = Shader.PropertyToID("_UnderlayIsoPerimeter");

        public static readonly int ID_WeightNormal = Shader.PropertyToID("_WeightNormal");
        public static readonly int ID_WeightBold = Shader.PropertyToID("_WeightBold");

        public static readonly int ID_OutlineTex = Shader.PropertyToID("_OutlineTex");
        public static readonly int ID_OutlineWidth = Shader.PropertyToID("_OutlineWidth");
        public static readonly int ID_OutlineSoftness = Shader.PropertyToID("_OutlineSoftness");
        public static readonly int ID_OutlineColor = Shader.PropertyToID("_OutlineColor");

        public static readonly int ID_Outline2Color = Shader.PropertyToID("_Outline2Color");
        public static readonly int ID_Outline2Width = Shader.PropertyToID("_Outline2Width");

        public static readonly int ID_Padding = Shader.PropertyToID("_Padding");
        public static readonly int ID_GradientScale = Shader.PropertyToID("_GradientScale");
        public static readonly int ID_ScaleX = Shader.PropertyToID("_ScaleX");
        public static readonly int ID_ScaleY = Shader.PropertyToID("_ScaleY");
        public static readonly int ID_PerspectiveFilter = Shader.PropertyToID("_PerspectiveFilter");
        public static readonly int ID_Sharpness = Shader.PropertyToID("_Sharpness");

        public static readonly int ID_TextureWidth = Shader.PropertyToID("_TextureWidth");
        public static readonly int ID_TextureHeight = Shader.PropertyToID("_TextureHeight");

        public static readonly int ID_BevelAmount = Shader.PropertyToID("_Bevel");

        public static readonly int ID_GlowColor = Shader.PropertyToID("_GlowColor");
        public static readonly int ID_GlowOffset = Shader.PropertyToID("_GlowOffset");
        public static readonly int ID_GlowPower = Shader.PropertyToID("_GlowPower");
        public static readonly int ID_GlowOuter = Shader.PropertyToID("_GlowOuter");
        public static readonly int ID_GlowInner = Shader.PropertyToID("_GlowInner");

        public static readonly int ID_LightAngle = Shader.PropertyToID("_LightAngle");

        public static readonly int ID_EnvMap = Shader.PropertyToID("_Cube");
        public static readonly int ID_EnvMatrix = Shader.PropertyToID("_EnvMatrix");
        public static readonly int ID_EnvMatrixRotation = Shader.PropertyToID("_EnvMatrixRotation");

        //public static int ID_MaskID;
        public static readonly int ID_MaskCoord = Shader.PropertyToID("_MaskCoord");
        public static readonly int ID_ClipRect = Shader.PropertyToID("_ClipRect");
        public static readonly int ID_MaskSoftnessX = Shader.PropertyToID("_MaskSoftnessX");
        public static readonly int ID_MaskSoftnessY = Shader.PropertyToID("_MaskSoftnessY");
        public static readonly int ID_VertexOffsetX = Shader.PropertyToID("_VertexOffsetX");
        public static readonly int ID_VertexOffsetY = Shader.PropertyToID("_VertexOffsetY");
        public static readonly int ID_UseClipRect = Shader.PropertyToID("_UseClipRect");

        public static readonly int ID_StencilID = Shader.PropertyToID("_Stencil");
        public static readonly int ID_StencilOp = Shader.PropertyToID("_StencilOp");
        public static readonly int ID_StencilComp = Shader.PropertyToID("_StencilComp");
        public static readonly int ID_StencilReadMask = Shader.PropertyToID("_StencilReadMask");
        public static readonly int ID_StencilWriteMask = Shader.PropertyToID("_StencilWriteMask");

        public static readonly int ID_ShaderFlags = Shader.PropertyToID("_ShaderFlags");
        public static readonly int ID_ScaleRatio_A = Shader.PropertyToID("_ScaleRatioA");
        public static readonly int ID_ScaleRatio_B = Shader.PropertyToID("_ScaleRatioB");
        public static readonly int ID_ScaleRatio_C = Shader.PropertyToID("_ScaleRatioC");

        public static readonly string Keyword_Bevel = "BEVEL_ON";
        public static readonly string Keyword_Glow = "GLOW_ON";
        public static readonly string Keyword_Underlay = "UNDERLAY_ON";
        public static readonly string Keyword_Ratios = "RATIOS_OFF";
        //public static readonly string Keyword_MASK_OFF = "MASK_OFF";
        public static readonly string Keyword_MASK_SOFT = "MASK_SOFT";
        public static readonly string Keyword_MASK_HARD = "MASK_HARD";
        public static readonly string Keyword_MASK_TEX = "MASK_TEX";
        public static readonly string Keyword_Outline = "OUTLINE_ON";

        public static readonly string ShaderTag_ZTestMode = "unity_GUIZTestMode";
        public static readonly string ShaderTag_CullMode = "_CullMode";

        /// <summary>
        /// The shader tag used to designate the XR motion vectors pass.
        /// </summary>
        public static readonly string ShaderTag_Spacewarp = "_XRMotionVectorsPass";

        private static readonly float m_clamp = 1.0f;

        [Obsolete("You no longer need to check if isInitialized is true. This class is now automatically initialized.", true)]
        public static bool isInitialized = false;


        /// <summary>
        /// Returns a reference to the mobile distance field shader.
        /// </summary>
        internal static Shader ShaderRef_MobileSDF
        {
            get
            {
                if (k_ShaderRef_MobileSDF == null)
                    k_ShaderRef_MobileSDF = Shader.Find("TextMeshPro/Mobile/Distance Field");

                return k_ShaderRef_MobileSDF;
            }
        }
        static Shader k_ShaderRef_MobileSDF;

        /// <summary>
        /// Returns a reference to the mobile bitmap shader.
        /// </summary>
        internal static Shader ShaderRef_MobileBitmap
        {
            get
            {
                if (k_ShaderRef_MobileBitmap == null)
                    k_ShaderRef_MobileBitmap = Shader.Find("TextMeshPro/Mobile/Bitmap");

                return k_ShaderRef_MobileBitmap;
            }
        }
        static Shader k_ShaderRef_MobileBitmap;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void ResetStaticsOnLoad()
        {
            k_ShaderRef_MobileSDF = default;
            k_ShaderRef_MobileBitmap = default;
        }
#endif

        /// <summary>
        /// Obsolete. Calling this method is no longer required.
        /// </summary>
        [Obsolete("Calling this method is no longer required.", true)]
        public static void GetShaderPropertyIDs()
        {
        }

        // Scale Ratios to ensure property ranges are optimum in Material Editor
        public static void UpdateShaderRatios(Material mat)
        {
            //Debug.Log("UpdateShaderRatios() called.");

            float ratio_A = 1;
            float ratio_B = 1;
            float ratio_C = 1;

            bool isRatioEnabled = !mat.shaderKeywords.Contains(Keyword_Ratios);

            if (!mat.HasProperty(ID_GradientScale) || !mat.HasProperty(ID_FaceDilate))
                return;

            // Compute Ratio A
            float scale = mat.GetFloat(ID_GradientScale);
            float faceDilate = mat.GetFloat(ID_FaceDilate);
            float outlineThickness = mat.GetFloat(ID_OutlineWidth);
            float outlineSoftness = mat.GetFloat(ID_OutlineSoftness);

            float weight = Mathf.Max(mat.GetFloat(ID_WeightNormal), mat.GetFloat(ID_WeightBold)) / 4.0f;

            float t = Mathf.Max(1, weight + faceDilate + outlineThickness + outlineSoftness);

            ratio_A = isRatioEnabled ? (scale - m_clamp) / (scale * t) : 1;

            //float ratio_A_old = mat.GetFloat(ID_ScaleRatio_A);

            // Only set the ratio if it has changed.
            //if (ratio_A != ratio_A_old)
                mat.SetFloat(ID_ScaleRatio_A, ratio_A);

            // Compute Ratio B
            if (mat.HasProperty(ID_GlowOffset))
            {
                float glowOffset = mat.GetFloat(ID_GlowOffset);
                float glowOuter = mat.GetFloat(ID_GlowOuter);

                float range = (weight + faceDilate) * (scale - m_clamp);

                t = Mathf.Max(1, glowOffset + glowOuter);

                ratio_B = isRatioEnabled ? Mathf.Max(0, scale - m_clamp - range) / (scale * t) : 1;
                //float ratio_B_old = mat.GetFloat(ID_ScaleRatio_B);

                // Only set the ratio if it has changed.
                //if (ratio_B != ratio_B_old)
                    mat.SetFloat(ID_ScaleRatio_B, ratio_B);
            }

            // Compute Ratio C
            if (mat.HasProperty(ID_UnderlayOffsetX))
            {
                float underlayOffsetX = mat.GetFloat(ID_UnderlayOffsetX);
                float underlayOffsetY = mat.GetFloat(ID_UnderlayOffsetY);
                float underlayDilate = mat.GetFloat(ID_UnderlayDilate);
                float underlaySoftness = mat.GetFloat(ID_UnderlaySoftness);

                float range = (weight + faceDilate) * (scale - m_clamp);

                t = Mathf.Max(1, Mathf.Max(Mathf.Abs(underlayOffsetX), Mathf.Abs(underlayOffsetY)) + underlayDilate + underlaySoftness);

                ratio_C = isRatioEnabled ? Mathf.Max(0, scale - m_clamp - range) / (scale * t) : 1;
                //float ratio_C_old = mat.GetFloat(ID_ScaleRatio_C);

                // Only set the ratio if it has changed.
                //if (ratio_C != ratio_C_old)
                    mat.SetFloat(ID_ScaleRatio_C, ratio_C);
            }
        }



        // Function to calculate padding required for Outline Width & Dilation for proper text alignment
        public static Vector4 GetFontExtent(Material material)
        {
            // Revised implementation where style no longer affects alignment
            return Vector4.zero;

            /*
            if (material == null || !material.HasProperty(ShaderUtilities.ID_GradientScale))
                return Vector4.zero;   // We are using an non SDF Shader.

            float scaleRatioA = material.GetFloat(ID_ScaleRatio_A);
            float faceDilate = material.GetFloat(ID_FaceDilate) * scaleRatioA;
            float outlineThickness = material.GetFloat(ID_OutlineWidth) * scaleRatioA;

            float extent = Mathf.Min(1, faceDilate + outlineThickness);
            extent *= material.GetFloat(ID_GradientScale);

            return new Vector4(extent, extent, extent, extent);
            */
        }


        // Function to check if Masking is enabled
        public static bool IsMaskingEnabled(Material material)
        {
            if (material == null || !material.HasProperty(ShaderUtilities.ID_ClipRect))
                return false;

            if (material.shaderKeywords.Contains(ShaderUtilities.Keyword_MASK_SOFT) || material.shaderKeywords.Contains(ShaderUtilities.Keyword_MASK_HARD) || material.shaderKeywords.Contains(ShaderUtilities.Keyword_MASK_TEX))
                return true;

            return false;
        }


        // Function to determine how much extra padding is required as a result of material properties like dilate, outline thickness, softness, glow, etc...
        public static float GetPadding(Material material, bool enableExtraPadding, bool isBold)
        {
            //Debug.Log("GetPadding() called.");

            // Return if Material is null
            if (material == null) return 0;

            int extraPadding = enableExtraPadding ? 4 : 0;

            // Check if we are using a non Distance Field Shader
            if (material.HasProperty(ID_GradientScale) == false)
            {
                if (material.HasProperty(ID_Padding))
                    extraPadding += (int)material.GetFloat(ID_Padding);

                return extraPadding + 1.0f;
            }

            // Special handling for new SRP Shaders
            if (material.HasProperty(ID_IsoPerimeter))
            {
                return ComputePaddingForProperties(material) + 0.25f + extraPadding;
            }

            Vector4 padding = Vector4.zero;
            Vector4 maxPadding = Vector4.zero;

            //float weight = 0;
            float faceDilate = 0;
            float faceSoftness = 0;
            float outlineThickness = 0;
            float scaleRatio_A = 0;
            float scaleRatio_B = 0;
            float scaleRatio_C = 0;

            float glowOffset = 0;
            float glowOuter = 0;

            float gradientScale = 0;
            float uniformPadding = 0;
            // Iterate through each of the assigned materials to find the max values to set the padding.

            // Update Shader Ratios prior to computing padding
            UpdateShaderRatios(material);

            string[] shaderKeywords = material.shaderKeywords;

            if (material.HasProperty(ID_ScaleRatio_A))
                scaleRatio_A = material.GetFloat(ID_ScaleRatio_A);

            //weight = 0; // Mathf.Max(material.GetFloat(ID_WeightNormal), material.GetFloat(ID_WeightBold)) / 2.0f * scaleRatio_A;

            if (material.HasProperty(ID_FaceDilate))
                faceDilate = material.GetFloat(ID_FaceDilate) * scaleRatio_A;

            if (material.HasProperty(ID_OutlineSoftness))
                faceSoftness = material.GetFloat(ID_OutlineSoftness) * scaleRatio_A;

            if (material.HasProperty(ID_OutlineWidth))
                outlineThickness = material.GetFloat(ID_OutlineWidth) * scaleRatio_A;

            uniformPadding = outlineThickness + faceSoftness + faceDilate;

            // Glow padding contribution
            if (material.HasProperty(ID_GlowOffset) && shaderKeywords.Contains(Keyword_Glow)) // Generates GC
            {
                if (material.HasProperty(ID_ScaleRatio_B))
                    scaleRatio_B = material.GetFloat(ID_ScaleRatio_B);

                glowOffset = material.GetFloat(ID_GlowOffset) * scaleRatio_B;
                glowOuter = material.GetFloat(ID_GlowOuter) * scaleRatio_B;
            }

            uniformPadding = Mathf.Max(uniformPadding, faceDilate + glowOffset + glowOuter);

            // Underlay padding contribution
            if (material.HasProperty(ID_UnderlaySoftness) && shaderKeywords.Contains(Keyword_Underlay)) // Generates GC
            {
                if (material.HasProperty(ID_ScaleRatio_C))
                    scaleRatio_C = material.GetFloat(ID_ScaleRatio_C);

                float offsetX = 0;
                float offsetY = 0;
                float dilate = 0;
                float softness = 0;

                if (material.HasProperty(ID_UnderlayOffset))
                {
                    Vector2 underlayOffset = material.GetVector(ID_UnderlayOffset);
                    offsetX = underlayOffset.x;
                    offsetY = underlayOffset.y;

                    dilate = material.GetFloat(ID_UnderlayDilate);
                    softness = material.GetFloat(ID_UnderlaySoftness);
                }
                else if (material.HasProperty(ID_UnderlayOffsetX))
                {

                    offsetX = material.GetFloat(ID_UnderlayOffsetX) * scaleRatio_C;
                    offsetY = material.GetFloat(ID_UnderlayOffsetY) * scaleRatio_C;
                    dilate = material.GetFloat(ID_UnderlayDilate) * scaleRatio_C;
                    softness = material.GetFloat(ID_UnderlaySoftness) * scaleRatio_C;
                }

                padding.x = Mathf.Max(padding.x, faceDilate + dilate + softness - offsetX);
                padding.y = Mathf.Max(padding.y, faceDilate + dilate + softness - offsetY);
                padding.z = Mathf.Max(padding.z, faceDilate + dilate + softness + offsetX);
                padding.w = Mathf.Max(padding.w, faceDilate + dilate + softness + offsetY);
            }

            padding.x = Mathf.Max(padding.x, uniformPadding);
            padding.y = Mathf.Max(padding.y, uniformPadding);
            padding.z = Mathf.Max(padding.z, uniformPadding);
            padding.w = Mathf.Max(padding.w, uniformPadding);

            padding.x += extraPadding;
            padding.y += extraPadding;
            padding.z += extraPadding;
            padding.w += extraPadding;

            padding.x = Mathf.Min(padding.x, 1);
            padding.y = Mathf.Min(padding.y, 1);
            padding.z = Mathf.Min(padding.z, 1);
            padding.w = Mathf.Min(padding.w, 1);

            maxPadding.x = maxPadding.x < padding.x ? padding.x : maxPadding.x;
            maxPadding.y = maxPadding.y < padding.y ? padding.y : maxPadding.y;
            maxPadding.z = maxPadding.z < padding.z ? padding.z : maxPadding.z;
            maxPadding.w = maxPadding.w < padding.w ? padding.w : maxPadding.w;

            gradientScale = material.GetFloat(ID_GradientScale);
            padding *= gradientScale;

            // Set UniformPadding to the maximum value of any of its components.
            uniformPadding = Mathf.Max(padding.x, padding.y);
            uniformPadding = Mathf.Max(padding.z, uniformPadding);
            uniformPadding = Mathf.Max(padding.w, uniformPadding);

            return uniformPadding + 1.25f;
        }


        static float ComputePaddingForProperties(Material mat)
        {
            Vector4 dilation = mat.GetVector(ID_IsoPerimeter);
            Vector2 outlineOffset1 = mat.GetVector(ID_OutlineOffset1);
            Vector2 outlineOffset2 = mat.GetVector(ID_OutlineOffset2);
            Vector2 outlineOffset3 = mat.GetVector(ID_OutlineOffset3);
            bool isOutlineModeEnabled = mat.GetFloat(ID_OutlineMode) != 0;

            Vector4 softness = mat.GetVector(ID_Softness);
            float gradientScale = mat.GetFloat(ID_GradientScale);

            // Face
            float padding = Mathf.Max(0, dilation.x + softness.x * 0.5f);

            // Outlines
            if (!isOutlineModeEnabled)
            {
                padding = Mathf.Max(padding, dilation.y + softness.y * 0.5f + Mathf.Max(Mathf.Abs(outlineOffset1.x), Mathf.Abs(outlineOffset1.y)));
                padding = Mathf.Max(padding, dilation.z + softness.z * 0.5f + Mathf.Max(Mathf.Abs(outlineOffset2.x), Mathf.Abs(outlineOffset2.y)));
                padding = Mathf.Max(padding, dilation.w + softness.w * 0.5f + Mathf.Max(Mathf.Abs(outlineOffset3.x), Mathf.Abs(outlineOffset3.y)));
            }
            else
            {
                float offsetOutline1 = Mathf.Max(Mathf.Abs(outlineOffset1.x), Mathf.Abs(outlineOffset1.y));
                float offsetOutline2 = Mathf.Max(Mathf.Abs(outlineOffset2.x), Mathf.Abs(outlineOffset2.y));

                padding = Mathf.Max(padding, dilation.y + softness.y * 0.5f + offsetOutline1);
                padding = Mathf.Max(padding, dilation.z + softness.z * 0.5f + offsetOutline2);

                float maxOffset = Mathf.Max(offsetOutline1, offsetOutline2);
                padding += Mathf.Max(0 ,(dilation.w + softness.w * 0.5f) - Mathf.Max(0, padding - maxOffset));
            }

            // Underlay
            Vector2 underlayOffset = mat.GetVector(ID_UnderlayOffset);
            float underlayDilation = mat.GetFloat(ID_UnderlayDilate);
            float underlaySoftness = mat.GetFloat(ID_UnderlaySoftness);
            padding = Mathf.Max(padding, underlayDilation + underlaySoftness * 0.5f + Mathf.Max(Mathf.Abs(underlayOffset.x), Mathf.Abs(underlayOffset.y)));

            return padding * gradientScale;
        }

        // Function to determine how much extra padding is required as a result of material properties like dilate, outline thickness, softness, glow, etc...
        public static float GetPadding(Material[] materials, bool enableExtraPadding, bool isBold)
        {
            //Debug.Log("GetPadding() called.");

            // Return if Material is null
            if (materials == null) return 0;

            int extraPadding = enableExtraPadding ? 4 : 0;

            // Check if we are using a Bitmap Shader
            if (materials[0].HasProperty(ID_Padding))
                return extraPadding + materials[0].GetFloat(ID_Padding);

            Vector4 padding = Vector4.zero;
            Vector4 maxPadding = Vector4.zero;

            float faceDilate = 0;
            float faceSoftness = 0;
            float outlineThickness = 0;
            float scaleRatio_A = 0;
            float scaleRatio_B = 0;
            float scaleRatio_C = 0;

            float glowOffset = 0;
            float glowOuter = 0;

            float uniformPadding = 0;
            // Iterate through each of the assigned materials to find the max values to set the padding.
            for (int i = 0; i < materials.Length; i++)
            {
                // Update Shader Ratios prior to computing padding
                ShaderUtilities.UpdateShaderRatios(materials[i]);

                string[] shaderKeywords = materials[i].shaderKeywords;

                if (materials[i].HasProperty(ShaderUtilities.ID_ScaleRatio_A))
                    scaleRatio_A = materials[i].GetFloat(ShaderUtilities.ID_ScaleRatio_A);

                if (materials[i].HasProperty(ShaderUtilities.ID_FaceDilate))
                    faceDilate = materials[i].GetFloat(ShaderUtilities.ID_FaceDilate) * scaleRatio_A;

                if (materials[i].HasProperty(ShaderUtilities.ID_OutlineSoftness))
                    faceSoftness = materials[i].GetFloat(ShaderUtilities.ID_OutlineSoftness) * scaleRatio_A;

                if (materials[i].HasProperty(ShaderUtilities.ID_OutlineWidth))
                    outlineThickness = materials[i].GetFloat(ShaderUtilities.ID_OutlineWidth) * scaleRatio_A;

                uniformPadding = outlineThickness + faceSoftness + faceDilate;

                // Glow padding contribution
                if (materials[i].HasProperty(ShaderUtilities.ID_GlowOffset) && shaderKeywords.Contains(ShaderUtilities.Keyword_Glow))
                {
                    if (materials[i].HasProperty(ShaderUtilities.ID_ScaleRatio_B))
                        scaleRatio_B = materials[i].GetFloat(ShaderUtilities.ID_ScaleRatio_B);

                    glowOffset = materials[i].GetFloat(ShaderUtilities.ID_GlowOffset) * scaleRatio_B;
                    glowOuter = materials[i].GetFloat(ShaderUtilities.ID_GlowOuter) * scaleRatio_B;
                }

                uniformPadding = Mathf.Max(uniformPadding, faceDilate + glowOffset + glowOuter);

                // Underlay padding contribution
                if (materials[i].HasProperty(ShaderUtilities.ID_UnderlaySoftness) && shaderKeywords.Contains(ShaderUtilities.Keyword_Underlay))
                {
                    if (materials[i].HasProperty(ShaderUtilities.ID_ScaleRatio_C))
                        scaleRatio_C = materials[i].GetFloat(ShaderUtilities.ID_ScaleRatio_C);

                    float offsetX = materials[i].GetFloat(ShaderUtilities.ID_UnderlayOffsetX) * scaleRatio_C;
                    float offsetY = materials[i].GetFloat(ShaderUtilities.ID_UnderlayOffsetY) * scaleRatio_C;
                    float dilate = materials[i].GetFloat(ShaderUtilities.ID_UnderlayDilate) * scaleRatio_C;
                    float softness = materials[i].GetFloat(ShaderUtilities.ID_UnderlaySoftness) * scaleRatio_C;

                    padding.x = Mathf.Max(padding.x, faceDilate + dilate + softness - offsetX);
                    padding.y = Mathf.Max(padding.y, faceDilate + dilate + softness - offsetY);
                    padding.z = Mathf.Max(padding.z, faceDilate + dilate + softness + offsetX);
                    padding.w = Mathf.Max(padding.w, faceDilate + dilate + softness + offsetY);
                }

                padding.x = Mathf.Max(padding.x, uniformPadding);
                padding.y = Mathf.Max(padding.y, uniformPadding);
                padding.z = Mathf.Max(padding.z, uniformPadding);
                padding.w = Mathf.Max(padding.w, uniformPadding);

                padding.x += extraPadding;
                padding.y += extraPadding;
                padding.z += extraPadding;
                padding.w += extraPadding;

                padding.x = Mathf.Min(padding.x, 1);
                padding.y = Mathf.Min(padding.y, 1);
                padding.z = Mathf.Min(padding.z, 1);
                padding.w = Mathf.Min(padding.w, 1);

                maxPadding.x = maxPadding.x < padding.x ? padding.x : maxPadding.x;
                maxPadding.y = maxPadding.y < padding.y ? padding.y : maxPadding.y;
                maxPadding.z = maxPadding.z < padding.z ? padding.z : maxPadding.z;
                maxPadding.w = maxPadding.w < padding.w ? padding.w : maxPadding.w;

            }

            float gradientScale = materials[0].GetFloat(ShaderUtilities.ID_GradientScale);
            padding *= gradientScale;

            // Set UniformPadding to the maximum value of any of its components.
            uniformPadding = Mathf.Max(padding.x, padding.y);
            uniformPadding = Mathf.Max(padding.z, uniformPadding);
            uniformPadding = Mathf.Max(padding.w, uniformPadding);

            return uniformPadding + 0.25f;
        }


    }

}
