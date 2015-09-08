using System;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    // Simple selectable object - derived from to create a control.
    [AddComponentMenu("UI/Selectable", 70)]
    [ExecuteInEditMode]
    [SelectionBase]
    [DisallowMultipleComponent]
    public class Selectable
        :
        UIBehaviour,
        IMoveHandler,
        IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler,
        ISelectHandler, IDeselectHandler
    {
        // Selection state

        // List of all the selectable objects currently active in the scene
        private static List<Selectable> s_List = new List<Selectable>();
        public static List<Selectable> allSelectables { get { return s_List; } }

        // Navigation information.
        [FormerlySerializedAs("navigation")]
        [SerializeField]
        private Navigation m_Navigation = Navigation.defaultNavigation;

        // Highlighting state
        public enum Transition
        {
            None,
            ColorTint,
            SpriteSwap,
            Animation
        }

        // Type of the transition that occurs when the button state changes.
        [FormerlySerializedAs("transition")]
        [SerializeField]
        private Transition m_Transition = Transition.ColorTint;

        // Colors used for a color tint-based transition.
        [FormerlySerializedAs("colors")]
        [SerializeField]
        private ColorBlock m_Colors = ColorBlock.defaultColorBlock;

        // Sprites used for a Image swap-based transition.
        [FormerlySerializedAs("spriteState")]
        [SerializeField]
        private SpriteState m_SpriteState;

        [FormerlySerializedAs("animationTriggers")]
        [SerializeField]
        private AnimationTriggers m_AnimationTriggers = new AnimationTriggers();

        [Tooltip("Can the Selectable be interacted with?")]
        [SerializeField]
        private bool m_Interactable = true;

        // Graphic that will be colored.
        [FormerlySerializedAs("highlightGraphic")]
        [FormerlySerializedAs("m_HighlightGraphic")]
        [SerializeField]
        private Graphic m_TargetGraphic;


        private bool m_GroupsAllowInteraction = true;

        private SelectionState m_CurrentSelectionState;

        public Navigation        navigation        { get { return m_Navigation; } set { if (SetPropertyUtility.SetStruct(ref m_Navigation, value))        OnSetProperty(); } }
        public Transition        transition        { get { return m_Transition; } set { if (SetPropertyUtility.SetStruct(ref m_Transition, value))        OnSetProperty(); } }
        public ColorBlock        colors            { get { return m_Colors; } set { if (SetPropertyUtility.SetStruct(ref m_Colors, value))            OnSetProperty(); } }
        public SpriteState       spriteState       { get { return m_SpriteState; } set { if (SetPropertyUtility.SetStruct(ref m_SpriteState, value))       OnSetProperty(); } }
        public AnimationTriggers animationTriggers { get { return m_AnimationTriggers; } set { if (SetPropertyUtility.SetClass(ref m_AnimationTriggers, value)) OnSetProperty(); } }
        public Graphic           targetGraphic     { get { return m_TargetGraphic; } set { if (SetPropertyUtility.SetClass(ref m_TargetGraphic, value))     OnSetProperty(); } }
        public bool              interactable      { get { return m_Interactable; } set { if (SetPropertyUtility.SetStruct(ref m_Interactable, value))      OnSetProperty(); } }

        private bool             isPointerInside   { get; set; }
        private bool             isPointerDown     { get; set; }
        private bool             hasSelection      { get; set; }

        protected Selectable()
        {}

        // Convenience function that converts the Graphic to a Image, if possible
        public Image image
        {
            get { return m_TargetGraphic as Image; }
            set { m_TargetGraphic = value; }
        }

        // Get the animator
        public Animator animator
        {
            get { return GetComponent<Animator>(); }
        }

        protected override void Awake()
        {
            if (m_TargetGraphic == null)
                m_TargetGraphic = GetComponent<Graphic>();
        }

        private readonly List<CanvasGroup> m_CanvasGroupCache = new List<CanvasGroup>();
        protected override void OnCanvasGroupChanged()
        {
            // Figure out if parent groups allow interaction
            // If no interaction is alowed... then we need
            // to not do that :)
            var groupAllowInteraction = true;
            Transform t = transform;
            while (t != null)
            {
                t.GetComponents(m_CanvasGroupCache);
                bool shouldBreak = false;
                for (var i = 0; i < m_CanvasGroupCache.Count; i++)
                {
                    // if the parent group does not allow interaction
                    // we need to break
                    if (!m_CanvasGroupCache[i].interactable)
                    {
                        groupAllowInteraction = false;
                        shouldBreak = true;
                    }
                    // if this is a 'fresh' group, then break
                    // as we should not consider parents
                    if (m_CanvasGroupCache[i].ignoreParentGroups)
                        shouldBreak = true;
                }
                if (shouldBreak)
                    break;

                t = t.parent;
            }

            if (groupAllowInteraction != m_GroupsAllowInteraction)
            {
                m_GroupsAllowInteraction = groupAllowInteraction;
                OnSetProperty();
            }
        }

        public virtual bool IsInteractable()
        {
            return m_GroupsAllowInteraction && m_Interactable;
        }

        // Call from unity if animation properties have changed
        protected override void OnDidApplyAnimationProperties()
        {
            OnSetProperty();
        }

        // Select on enable and add to the list.
        protected override void OnEnable()
        {
            base.OnEnable();

            s_List.Add(this);
            var state = SelectionState.Normal;

            // The button will be highlighted even in some cases where it shouldn't.
            // For example: We only want to set the State as Highlighted if the StandaloneInputModule.m_CurrentInputMode == InputMode.Buttons
            // But we dont have access to this, and it might not apply to other InputModules.
            // TODO: figure out how to solve this. Case 617348.
            if (hasSelection)
                state = SelectionState.Highlighted;

            m_CurrentSelectionState = state;
            InternalEvaluateAndTransitionToSelectionState(true);
        }

        private void OnSetProperty()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                InternalEvaluateAndTransitionToSelectionState(true);
            else
#endif
            InternalEvaluateAndTransitionToSelectionState(false);
        }

        // Remove from the list.
        protected override void OnDisable()
        {
            s_List.Remove(this);
            InstantClearState();
            base.OnDisable();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            m_Colors.fadeDuration = Mathf.Max(m_Colors.fadeDuration, 0.0f);

            // OnValidate can be called before OnEnable, this makes it unsafe to access other components
            // since they might not have been initialized yet.
            // OnSetProperty potentially access Animator or Graphics. (case 618186)
            if (isActiveAndEnabled)
            {
                // Need to clear out the override image on the target...
                DoSpriteSwap(null);

                // If the transition mode got changed, we need to clear all the transitions, since we don't know what the old transition mode was.
                StartColorTween(Color.white, true);
                TriggerAnimation(m_AnimationTriggers.normalTrigger);

                // And now go to the right state.
                InternalEvaluateAndTransitionToSelectionState(true);
            }
        }

        protected override void Reset()
        {
            m_TargetGraphic = GetComponent<Graphic>();
        }

#endif // if UNITY_EDITOR

        protected SelectionState currentSelectionState
        {
            get { return m_CurrentSelectionState; }
        }

        protected virtual void InstantClearState()
        {
            string triggerName = m_AnimationTriggers.normalTrigger;

            isPointerInside = false;
            isPointerDown = false;
            hasSelection = false;

            switch (m_Transition)
            {
                case Transition.ColorTint:
                    StartColorTween(Color.white, true);
                    break;
                case Transition.SpriteSwap:
                    DoSpriteSwap(null);
                    break;
                case Transition.Animation:
                    TriggerAnimation(triggerName);
                    break;
            }
        }

        protected virtual void DoStateTransition(SelectionState state, bool instant)
        {
            Color tintColor;
            Sprite transitionSprite;
            string triggerName;

            switch (state)
            {
                case SelectionState.Normal:
                    tintColor = m_Colors.normalColor;
                    transitionSprite = null;
                    triggerName = m_AnimationTriggers.normalTrigger;
                    break;
                case SelectionState.Highlighted:
                    tintColor = m_Colors.highlightedColor;
                    transitionSprite = m_SpriteState.highlightedSprite;
                    triggerName = m_AnimationTriggers.highlightedTrigger;
                    break;
                case SelectionState.Pressed:
                    tintColor = m_Colors.pressedColor;
                    transitionSprite = m_SpriteState.pressedSprite;
                    triggerName = m_AnimationTriggers.pressedTrigger;
                    break;
                case SelectionState.Disabled:
                    tintColor = m_Colors.disabledColor;
                    transitionSprite = m_SpriteState.disabledSprite;
                    triggerName = m_AnimationTriggers.disabledTrigger;
                    break;
                default:
                    tintColor = Color.black;
                    transitionSprite = null;
                    triggerName = string.Empty;
                    break;
            }

            if (gameObject.activeInHierarchy)
            {
                switch (m_Transition)
                {
                    case Transition.ColorTint:
                        StartColorTween(tintColor * m_Colors.colorMultiplier, instant);
                        break;
                    case Transition.SpriteSwap:
                        DoSpriteSwap(transitionSprite);
                        break;
                    case Transition.Animation:
                        TriggerAnimation(triggerName);
                        break;
                }
            }
        }

        protected enum SelectionState
        {
            Normal,
            Highlighted,
            Pressed,
            Disabled
        }

        // Selection logic

        // Find the next selectable object in the specified world-space direction.
        public Selectable FindSelectable(Vector3 dir)
        {
            dir = dir.normalized;
            Vector3 localDir = Quaternion.Inverse(transform.rotation) * dir;
            Vector3 pos = transform.TransformPoint(GetPointOnRectEdge(transform as RectTransform, localDir));
            float maxScore = Mathf.NegativeInfinity;
            Selectable bestPick = null;
            for (int i = 0; i < s_List.Count; ++i)
            {
                Selectable sel = s_List[i];

                if (sel == this || sel == null)
                    continue;

                if (!sel.IsInteractable() || sel.navigation.mode == Navigation.Mode.None)
                    continue;

                var selRect = sel.transform as RectTransform;
                Vector3 selCenter = selRect != null ? (Vector3)selRect.rect.center : Vector3.zero;
                Vector3 myVector = sel.transform.TransformPoint(selCenter) - pos;

                // Value that is the distance out along the direction.
                float dot = Vector3.Dot(dir, myVector);

                // Skip elements that are in the wrong direction or which have zero distance.
                // This also ensures that the scoring formula below will not have a division by zero error.
                if (dot <= 0)
                    continue;

                // This scoring function has two priorities:
                // - Score higher for positions that are closer.
                // - Score higher for positions that are located in the right direction.
                // This scoring function combines both of these criteria.
                // It can be seen as this:
                //   Dot (dir, myVector.normalized) / myVector.magnitude
                // The first part equals 1 if the direction of myVector is the same as dir, and 0 if it's orthogonal.
                // The second part scores lower the greater the distance is by dividing by the distance.
                // The formula below is equivalent but more optimized.
                //
                // If a given score is chosen, the positions that evaluate to that score will form a circle
                // that touches pos and whose center is located along dir. A way to visualize the resulting functionality is this:
                // From the position pos, blow up a circular balloon so it grows in the direction of dir.
                // The first Selectable whose center the circular balloon touches is the one that's chosen.
                float score = dot / myVector.sqrMagnitude;

                if (score > maxScore)
                {
                    maxScore = score;
                    bestPick = sel;
                }
            }
            return bestPick;
        }

        private static Vector3 GetPointOnRectEdge(RectTransform rect, Vector2 dir)
        {
            if (rect == null)
                return Vector3.zero;
            if (dir != Vector2.zero)
                dir /= Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
            dir = rect.rect.center + Vector2.Scale(rect.rect.size, dir * 0.5f);
            return dir;
        }

        // Convenience function -- change the selection to the specified object if it's not null and happens to be active.
        void Navigate(AxisEventData eventData, Selectable sel)
        {
            if (sel != null && sel.IsActive())
                eventData.selectedObject = sel.gameObject;
        }

        // Find the selectable object to the left of this one.
        public virtual Selectable FindSelectableOnLeft()
        {
            if (m_Navigation.mode == Navigation.Mode.Explicit)
            {
                return m_Navigation.selectOnLeft;
            }
            if ((m_Navigation.mode & Navigation.Mode.Horizontal) != 0)
            {
                return FindSelectable(transform.rotation * Vector3.left);
            }
            return null;
        }

        // Find the selectable object to the right of this one.
        public virtual Selectable FindSelectableOnRight()
        {
            if (m_Navigation.mode == Navigation.Mode.Explicit)
            {
                return m_Navigation.selectOnRight;
            }
            if ((m_Navigation.mode & Navigation.Mode.Horizontal) != 0)
            {
                return FindSelectable(transform.rotation * Vector3.right);
            }
            return null;
        }

        // Find the selectable object above this one
        public virtual Selectable FindSelectableOnUp()
        {
            if (m_Navigation.mode == Navigation.Mode.Explicit)
            {
                return m_Navigation.selectOnUp;
            }
            if ((m_Navigation.mode & Navigation.Mode.Vertical) != 0)
            {
                return FindSelectable(transform.rotation * Vector3.up);
            }
            return null;
        }

        // Find the selectable object below this one.
        public virtual Selectable FindSelectableOnDown()
        {
            if (m_Navigation.mode == Navigation.Mode.Explicit)
            {
                return m_Navigation.selectOnDown;
            }
            if ((m_Navigation.mode & Navigation.Mode.Vertical) != 0)
            {
                return FindSelectable(transform.rotation * Vector3.down);
            }
            return null;
        }

        public virtual void OnMove(AxisEventData eventData)
        {
            switch (eventData.moveDir)
            {
                case MoveDirection.Right:
                    Navigate(eventData, FindSelectableOnRight());
                    break;

                case MoveDirection.Up:
                    Navigate(eventData, FindSelectableOnUp());
                    break;

                case MoveDirection.Left:
                    Navigate(eventData, FindSelectableOnLeft());
                    break;

                case MoveDirection.Down:
                    Navigate(eventData, FindSelectableOnDown());
                    break;
            }
        }

        void StartColorTween(Color targetColor, bool instant)
        {
            if (m_TargetGraphic == null)
                return;

            m_TargetGraphic.CrossFadeColor(targetColor, instant ? 0f : m_Colors.fadeDuration, true, true);
        }

        void DoSpriteSwap(Sprite newSprite)
        {
            if (image == null)
                return;

            image.overrideSprite = newSprite;
        }

        void TriggerAnimation(string triggername)
        {
            if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null || string.IsNullOrEmpty(triggername))
                return;

            animator.ResetTrigger(m_AnimationTriggers.normalTrigger);
            animator.ResetTrigger(m_AnimationTriggers.pressedTrigger);
            animator.ResetTrigger(m_AnimationTriggers.highlightedTrigger);
            animator.ResetTrigger(m_AnimationTriggers.disabledTrigger);
            animator.SetTrigger(triggername);
        }

        // Whether the control should be 'selected'.
        protected bool IsHighlighted(BaseEventData eventData)
        {
            if (!IsActive())
                return false;

            if (IsPressed())
                return false;

            bool selected = hasSelection;
            if (eventData is PointerEventData)
            {
                var pointerData = eventData as PointerEventData;
                selected |=
                    (isPointerDown && !isPointerInside && pointerData.pointerPress == gameObject) // This object pressed, but pointer moved off
                    || (!isPointerDown && isPointerInside && pointerData.pointerPress == gameObject) // This object pressed, but pointer released over (PointerUp event)
                    || (!isPointerDown && isPointerInside && pointerData.pointerPress == null); // Nothing pressed, but pointer is over
            }
            else
            {
                selected |= isPointerInside;
            }
            return selected;
        }

        [Obsolete("Is Pressed no longer requires eventData", false)]
        protected bool IsPressed(BaseEventData eventData)
        {
            return IsPressed();
        }

        // Whether the control should be pressed.
        protected bool IsPressed()
        {
            if (!IsActive())
                return false;

            return isPointerInside && isPointerDown;
        }

        // The current visual state of the control.
        protected void UpdateSelectionState(BaseEventData eventData)
        {
            if (IsPressed())
            {
                m_CurrentSelectionState = SelectionState.Pressed;
                return;
            }

            if (IsHighlighted(eventData))
            {
                m_CurrentSelectionState = SelectionState.Highlighted;
                return;
            }

            m_CurrentSelectionState = SelectionState.Normal;
        }

        // Change the button to the correct state
        private void EvaluateAndTransitionToSelectionState(BaseEventData eventData)
        {
            if (!IsActive())
                return;

            UpdateSelectionState(eventData);
            InternalEvaluateAndTransitionToSelectionState(false);
        }

        private void InternalEvaluateAndTransitionToSelectionState(bool instant)
        {
            var transitionState = m_CurrentSelectionState;
            if (IsActive() && !IsInteractable())
                transitionState = SelectionState.Disabled;
            DoStateTransition(transitionState, instant);
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            // Selection tracking
            if (IsInteractable() && navigation.mode != Navigation.Mode.None)
                EventSystem.current.SetSelectedGameObject(gameObject, eventData);

            isPointerDown = true;
            EvaluateAndTransitionToSelectionState(eventData);
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            isPointerDown = false;
            EvaluateAndTransitionToSelectionState(eventData);
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            isPointerInside = true;
            EvaluateAndTransitionToSelectionState(eventData);
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            isPointerInside = false;
            EvaluateAndTransitionToSelectionState(eventData);
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            hasSelection = true;
            EvaluateAndTransitionToSelectionState(eventData);
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            hasSelection = false;
            EvaluateAndTransitionToSelectionState(eventData);
        }

        public virtual void Select()
        {
            if (EventSystem.current.alreadySelecting)
                return;

            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}
