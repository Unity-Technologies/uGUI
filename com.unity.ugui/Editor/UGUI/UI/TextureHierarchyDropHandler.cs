using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [InitializeOnLoad]
    internal static class TextureHierarchyDropHandler
    {
        const string k_UndoAssignTexture = "Assign Texture";
        const string k_UndoAssignTextureAnimation = "Assign Texture Animation";
        const string k_TexturePropertyName = "m_Texture";

        static TextureHierarchyDropHandler()
        {
            TextureDragAndDropUtility.AddTextureToGameObjectDragAndDropHandler(GetDragVisualMode, PerformDrop);
        }

        static DragAndDropVisualMode GetDragVisualMode(GameObject target, Texture[] textures, HierarchyDropFlags dropMode)
        {
            // A RawImage only shows 2D textures, so e.g. a Cubemap is left to the other texture handlers
            if (Array.Exists(textures, texture => texture.dimension != TextureDimension.Tex2D))
                return DragAndDropVisualMode.None;

            // A Renderer means the target is not a UI element, e.g. a 3D object under the Canvas, so its existing texture handler takes the drop
            if (target.TryGetComponent(out Renderer _))
                return DragAndDropVisualMode.None;

            return HierarchyDropHandlerUtility.GetDragVisualMode<RawImage>(target, textures.Length, dropMode);
        }

        internal static bool PerformDrop(GameObject target, Texture[] textures, HierarchyDropFlags _, SpriteUtility.ShowFileDialogDelegate saveFileDialog)
        {
            return HierarchyDropHandlerUtility.PerformDrop<Texture, RawImage>(
                target,
                textures,
                k_UndoAssignTexture,
                k_UndoAssignTextureAnimation,
                (rawImage, texture) => rawImage.texture = texture,
                AddTextureAnimationCurveToClip,
                saveFileDialog);
        }

        static void AddTextureAnimationCurveToClip(AnimationClip clip, Texture[] textures)
        {
            clip.frameRate = SpriteUtility.k_DefaultAnimationFrameRate;

            var keyframes = new ObjectReferenceKeyframe[textures.Length];
            for (int i = 0; i < keyframes.Length; ++i)
            {
                keyframes[i].value = textures[i];
                keyframes[i].time = i / clip.frameRate;
            }

            var curveBinding = EditorCurveBinding.PPtrCurve("", typeof(RawImage), k_TexturePropertyName);
            AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyframes);
        }
    }
}
