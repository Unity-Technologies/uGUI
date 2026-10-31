using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [InitializeOnLoad]
    internal static class SpriteHierarchyDropHandler
    {
        const string k_UndoAssignSprite = "Assign Sprite";
        const string k_UndoAssignSpriteAnimation = "Assign Sprite Animation";

        static SpriteHierarchyDropHandler()
        {
            SpriteUtility.AddSpriteToGameObjectDragAndDropHandler(GetDragVisualMode, PerformDrop);
        }

        static DragAndDropVisualMode GetDragVisualMode(GameObject target, Sprite[] sprites, HierarchyDropFlags dropMode)
        {
            return HierarchyDropHandlerUtility.GetDragVisualMode<Image>(target, sprites.Length, dropMode);
        }

        internal static bool PerformDrop(GameObject target, Sprite[] sprites, HierarchyDropFlags _, SpriteUtility.ShowFileDialogDelegate saveFileDialog)
        {
            return HierarchyDropHandlerUtility.PerformDrop<Sprite, Image>(
                target,
                sprites,
                k_UndoAssignSprite,
                k_UndoAssignSpriteAnimation,
                (image, sprite) => image.sprite = sprite,
                (clip, frames) => SpriteUtility.AddSpriteAnimationCurveToClip(clip, frames, typeof(Image)),
                saveFileDialog);
        }
    }
}
