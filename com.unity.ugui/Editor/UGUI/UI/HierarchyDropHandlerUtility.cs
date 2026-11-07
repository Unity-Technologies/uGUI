using System;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UnityEditor.UI
{
    // Shared by the handlers that assign assets dropped on a Canvas GameObject in the Hierarchy to its Graphic
    internal static class HierarchyDropHandlerUtility
    {
        internal static DragAndDropVisualMode GetDragVisualMode<TGraphic>(GameObject target, int droppedCount, HierarchyDropFlags dropMode)
            where TGraphic : Graphic
        {
            if (droppedCount == 0)
                return DragAndDropVisualMode.None;

            // Only a drop onto the target can assign its Graphic
            bool isDropUponTarget = (dropMode & HierarchyDropFlags.DropUpon) != 0;
            if (!isDropUponTarget)
                return DragAndDropVisualMode.Rejected;

            // A GameObject holds at most one Graphic, and only a TGraphic can show the dropped assets
            var graphic = target.GetComponent<Graphic>();
            if (graphic != null && !(graphic is TGraphic))
                return DragAndDropVisualMode.Rejected;

            // Multiple assets create a clip for an Animator on the target, which is added only if the target has no other animation player (e.g. Animation)
            if (droppedCount > 1 && !target.TryGetComponent(out Animator _) && HasNonAnimatorAnimationPlayer(target))
                return DragAndDropVisualMode.Rejected;

            return DragAndDropVisualMode.Copy;
        }

        internal static bool PerformDrop<TAsset, TGraphic>(
            GameObject target,
            TAsset[] assets,
            string undoName,
            string animationUndoName,
            Action<TGraphic, TAsset> assignToGraphic,
            Action<AnimationClip, TAsset[]> addAnimationCurve,
            SpriteUtility.ShowFileDialogDelegate saveFileDialog)
            where TAsset : Object
            where TGraphic : Graphic
        {
            bool isAnimation = assets.Length > 1;
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(isAnimation ? animationUndoName : undoName);
            int undoGroup = Undo.GetCurrentGroup();

            if (!isAnimation)
            {
                if (!AssignToGraphic(target, assets[0], undoName, assignToGraphic))
                {
                    Undo.RevertAllDownToGroup(undoGroup);
                    return false;
                }
                return true;
            }

            // Same order as the clip's frames, so the Graphic shows the clip's first frame
            Array.Sort(assets, (a, b) => EditorUtility.NaturalCompare(a.name, b.name));
            // If the Graphic can't be added or the clip prompt is cancelled, everything the drop did is reverted
            if (!AssignToGraphic(target, assets[0], undoName, assignToGraphic) || !CreateAnimation(target, assets, addAnimationCurve, saveFileDialog))
            {
                Undo.RevertAllDownToGroup(undoGroup);
                return false;
            }

            // The group can advance while the modal clip prompt is open (e.g. switching apps), which would split the drop into several undo steps
            Undo.CollapseUndoOperations(undoGroup);
            Undo.SetCurrentGroupName(animationUndoName);
            return true;
        }

        static bool AssignToGraphic<TAsset, TGraphic>(GameObject target, TAsset asset, string undoName, Action<TGraphic, TAsset> assignToGraphic)
            where TGraphic : Graphic
        {
            if (!target.TryGetComponent(out TGraphic graphic))
            {
                // Null if the add was blocked, e.g. by a conflicting component
                graphic = Undo.AddComponent<TGraphic>(target);
                if (graphic == null)
                    return false;
            }
            else
            {
                Undo.RecordObject(graphic, undoName);
            }

            assignToGraphic(graphic, asset);

            // Without this, a change to a Prefab instance is not saved with the scene
            PrefabUtility.RecordPrefabInstancePropertyModifications(graphic);
            return true;
        }

        static bool HasNonAnimatorAnimationPlayer(GameObject target)
        {
            return target.TryGetComponent(out Animation _) || target.TryGetComponent(out IAnimationClipSource _);
        }

        static bool CreateAnimation<TAsset>(GameObject target, TAsset[] assets, Action<AnimationClip, TAsset[]> addAnimationCurve, SpriteUtility.ShowFileDialogDelegate saveFileDialog)
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
            addAnimationCurve(clip, assets);
            if (!MecanimUtilities.AddClipToAnimatorComponent(animator, clip))
                return false;

            // A new controller assigned to an existing Animator on a Prefab instance must be saved as an override
            PrefabUtility.RecordPrefabInstancePropertyModifications(animator);
            return true;
        }
    }
}
