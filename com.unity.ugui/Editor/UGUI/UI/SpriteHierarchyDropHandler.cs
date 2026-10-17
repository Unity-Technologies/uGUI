using System;
using UnityEditor.Animations;
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
            if (sprites.Length == 0)
                return DragAndDropVisualMode.None;

            // Only a drop onto the target can assign its Image
            bool isDropUponTarget = (dropMode & HierarchyDropFlags.DropUpon) != 0;
            if (!isDropUponTarget)
                return DragAndDropVisualMode.Rejected;

            // A GameObject holds at most one Graphic, and only an Image can show a sprite
            var graphic = target.GetComponent<Graphic>();
            if (graphic != null && !(graphic is Image))
                return DragAndDropVisualMode.Rejected;

            // Multiple sprites create a clip for an Animator on the target, which is added only if the target has no other animation player (e.g. Animation)
            if (sprites.Length > 1 && !target.TryGetComponent(out Animator _) && HasNonAnimatorAnimationPlayer(target))
                return DragAndDropVisualMode.Rejected;

            return DragAndDropVisualMode.Copy;
        }

        internal static bool PerformDrop(GameObject target, Sprite[] sprites, HierarchyDropFlags _, SpriteUtility.ShowFileDialogDelegate saveFileDialog)
        {
            bool isAnimation = sprites.Length > 1;
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(isAnimation ? k_UndoAssignSpriteAnimation : k_UndoAssignSprite);
            int undoGroup = Undo.GetCurrentGroup();

            if (!isAnimation)
            {
                if (!AssignSpriteToImage(target, sprites[0]))
                {
                    Undo.RevertAllDownToGroup(undoGroup);
                    return false;
                }
                return true;
            }

            // Same order as the clip's frames, so the Image shows the clip's first frame
            Array.Sort(sprites, (a, b) => EditorUtility.NaturalCompare(a.name, b.name));
            // If the Image can't be added or the clip prompt is cancelled, everything the drop did is reverted
            if (!AssignSpriteToImage(target, sprites[0]) || !CreateImageAnimation(target, sprites, saveFileDialog))
            {
                Undo.RevertAllDownToGroup(undoGroup);
                return false;
            }

            // The group can advance while the modal clip prompt is open (e.g. switching apps), which would split the drop into several undo steps
            Undo.CollapseUndoOperations(undoGroup);
            Undo.SetCurrentGroupName(k_UndoAssignSpriteAnimation);
            return true;
        }

        static bool AssignSpriteToImage(GameObject target, Sprite sprite)
        {
            if (!target.TryGetComponent(out Image image))
            {
                // Null if the add was blocked, e.g. by a conflicting component
                image = Undo.AddComponent<Image>(target);
                if (image == null)
                    return false;
            }
            else
            {
                Undo.RecordObject(image, k_UndoAssignSprite);
            }

            image.sprite = sprite;
            return true;
        }

        static bool HasNonAnimatorAnimationPlayer(GameObject target)
        {
            return target.TryGetComponent(out Animation _) || target.TryGetComponent(out IAnimationClipSource _);
        }

        static bool CreateImageAnimation(GameObject target, Sprite[] sprites, SpriteUtility.ShowFileDialogDelegate saveFileDialog)
        {
            string clipPath = SpriteUtility.ShowSaveSpriteAnimationDialog(target, saveFileDialog);
            if (string.IsNullOrEmpty(clipPath))
                return false;

            if (!target.TryGetComponent(out Animator animator))
            {
                animator = Undo.AddComponent<Animator>(target);
                if (animator == null)
                    return false;
            }

            var clip = MecanimUtilities.CreateNewClipAtPath(clipPath);
            SpriteUtility.AddSpriteAnimationCurveToClip(clip, sprites, typeof(Image));
            return MecanimUtilities.AddClipToAnimatorComponent(animator, clip);
        }
    }
}
