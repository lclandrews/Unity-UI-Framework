using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace UIFramework.UGUI
{
    public class ButtonPlusPlus : Button, IButtonState
    {
        [SerializeField] private Sprite _normalSprite = null;
        [SerializeField] private ButtonState[] _states = null;

        public string State { get; private set; } = "default";

        private IButtonComponent[] _buttonComponents = null;
        private Dictionary<string, int> _stateMap = null;

        public void ResetButtonState()
        {
            SetButtonState("default", colors, spriteState, animationTriggers);
        }

        public void SetButtonState(string state)
        {
            Dictionary<string, int> stateMap = GetStateMap();
            int index;
            if (stateMap.TryGetValue(state, out index))
            {
                SetButtonState(state, _states[index].Colors, _states[index].SpriteState, _states[index].AnimationTriggers);
            }
        }

        private Dictionary<string, int> GetStateMap()
        {
            if (_stateMap == null)
            {
                _stateMap = new Dictionary<string, int>();
                for (int i = 0; i < _states.Length; i++)
                {
                    _stateMap.Add(_states[i].Name, i);
                }
            }
            return _stateMap;
        }

        private IButtonComponent[] GetButtonComponents()
        {
            if(_buttonComponents == null)
            {
                _buttonComponents = GetComponentsInChildren<IButtonComponent>();
            }
            return _buttonComponents;
        }

        private void SetButtonState(string state, in ColorBlock colors, in SpriteState spriteState, AnimationTriggers animationTriggers)
        {
            State = state;
            this.colors = colors;
            this.spriteState = spriteState;
            this.animationTriggers = animationTriggers;

            IButtonComponent[] buttonComponents = GetButtonComponents();
            for (int i = 0; i < buttonComponents.Length; i++)
            {
                buttonComponents[i].SetButtonState(state);
            }
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (!gameObject.activeInHierarchy)
                return;

            Color tintColor;
            Sprite transitionSprite;
            string triggerName;

            switch (state)
            {
                case SelectionState.Normal:
                    tintColor = colors.normalColor;
                    transitionSprite = _normalSprite;
                    triggerName = animationTriggers.normalTrigger;
                    break;
                case SelectionState.Highlighted:
                    tintColor = colors.highlightedColor;
                    transitionSprite = spriteState.highlightedSprite;
                    triggerName = animationTriggers.highlightedTrigger;
                    break;
                case SelectionState.Pressed:
                    tintColor = colors.pressedColor;
                    transitionSprite = spriteState.pressedSprite;
                    triggerName = animationTriggers.pressedTrigger;
                    break;
                case SelectionState.Selected:
                    tintColor = colors.selectedColor;
                    transitionSprite = spriteState.selectedSprite;
                    triggerName = animationTriggers.selectedTrigger;
                    break;
                case SelectionState.Disabled:
                    tintColor = colors.disabledColor;
                    transitionSprite = spriteState.disabledSprite;
                    triggerName = animationTriggers.disabledTrigger;
                    break;
                default:
                    tintColor = Color.black;
                    transitionSprite = null;
                    triggerName = string.Empty;
                    break;
            }

            switch (transition)
            {
                case Transition.ColorTint:
                    StartColorTween(tintColor * colors.colorMultiplier, instant);
                    break;
                case Transition.SpriteSwap:
                    DoSpriteSwap(transitionSprite);
                    break;
                case Transition.Animation:
                    TriggerAnimation(triggerName);
                    break;
            }

            IButtonComponent[] buttonComponents = GetButtonComponents();
            for(int i = 0; i < buttonComponents.Length; i++)
            {
                buttonComponents[i].DoStateTransition((UGUI.SelectionState)state, instant);
            }
        }

        void StartColorTween(Color targetColor, bool instant)
        {
            if (targetGraphic == null)
                return;

            targetGraphic.CrossFadeColor(targetColor, instant ? 0f : colors.fadeDuration, true, true);
        }

        void DoSpriteSwap(Sprite newSprite)
        {
            if (image == null)
                return;

            image.overrideSprite = newSprite;
        }

        void TriggerAnimation(string triggername)
        {
            if (transition != Transition.Animation || animator == null || !animator.isActiveAndEnabled || !animator.hasBoundPlayables || string.IsNullOrEmpty(triggername))
                return;

            animator.ResetTrigger(animationTriggers.normalTrigger);
            animator.ResetTrigger(animationTriggers.highlightedTrigger);
            animator.ResetTrigger(animationTriggers.pressedTrigger);
            animator.ResetTrigger(animationTriggers.selectedTrigger);
            animator.ResetTrigger(animationTriggers.disabledTrigger);

            animator.SetTrigger(triggername);
        }
    }
}
