using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace UIFramework.UGUI
{
    public class ButtonGraphicComponent : MonoBehaviour, IButtonComponent
    {
        [SerializeField] private Graphic _targetGraphic;

        [SerializeField] private Selectable.Transition _transition = Selectable.Transition.ColorTint;

        [SerializeField] private ColorBlock _colors = ColorBlock.defaultColorBlock;

        [SerializeField] private Sprite _normalSprite = null;
        [SerializeField] private SpriteState _spriteState = default;

        public AnimationTriggers AnimationTriggers { get { return _animationTriggers; } }
        [SerializeField] private AnimationTriggers _animationTriggers = new AnimationTriggers();

        [SerializeField] private ButtonState[] _states = null;

        public string State { get; private set; }

        private Dictionary<string, int> _stateMap = null;

        private SelectionState _selectionState = SelectionState.Normal;

        private ColorBlock _activeColors = ColorBlock.defaultColorBlock;
        private Sprite _activeNormalSprite = null;
        private SpriteState _activeSpriteState = default;
        private AnimationTriggers _activeAnimationTriggers = new AnimationTriggers();

        public void DoStateTransition(SelectionState state, bool instant)
        {
            _selectionState = state;

            if (!gameObject.activeInHierarchy)
                return;

            Color tintColor;
            Sprite transitionSprite;
            string triggerName;

            switch (state)
            {
                case SelectionState.Normal:
                    tintColor = _activeColors.normalColor;
                    transitionSprite = _activeNormalSprite;
                    triggerName = _activeAnimationTriggers.normalTrigger;
                    break;
                case SelectionState.Highlighted:
                    tintColor = _activeColors.highlightedColor;
                    transitionSprite = _activeSpriteState.highlightedSprite;
                    triggerName = _activeAnimationTriggers.highlightedTrigger;
                    break;
                case SelectionState.Pressed:
                    tintColor = _activeColors.pressedColor;
                    transitionSprite = _activeSpriteState.pressedSprite;
                    triggerName = _activeAnimationTriggers.pressedTrigger;
                    break;
                case SelectionState.Selected:
                    tintColor = _activeColors.selectedColor;
                    transitionSprite = _activeSpriteState.selectedSprite;
                    triggerName = _activeAnimationTriggers.selectedTrigger;
                    break;
                case SelectionState.Disabled:
                    tintColor = _activeColors.disabledColor;
                    transitionSprite = _activeSpriteState.disabledSprite;
                    triggerName = _activeAnimationTriggers.disabledTrigger;
                    break;
                default:
                    tintColor = Color.black;
                    transitionSprite = null;
                    triggerName = string.Empty;
                    break;
            }

            switch (_transition)
            {
                case Selectable.Transition.ColorTint:
                    StartColorTween(tintColor * _activeColors.colorMultiplier, instant);
                    break;
                case Selectable.Transition.SpriteSwap:
                    DoSpriteSwap(transitionSprite);
                    break;
                case Selectable.Transition.Animation:
                    TriggerAnimation(triggerName);
                    break;
            }
        }

        public void ResetButtonState()
        {
            SetButtonState("default", _colors, _normalSprite, _spriteState, _animationTriggers);
        }

        public void SetButtonState(string state)
        {
            Dictionary<string, int> stateMap = GetStateMap();
            int index;
            if (stateMap.TryGetValue(state, out index))
            {
                SetButtonState(state, _states[index].Colors, _states[index].NormalSprite, _states[index].SpriteState, _states[index].AnimationTriggers);
            }
        }

        private void StartColorTween(Color targetColor, bool instant)
        {
            if (_targetGraphic != null)
            {
                _targetGraphic.CrossFadeColor(targetColor, instant ? 0f : _activeColors.fadeDuration, true, true);
            }            
        }

        private void DoSpriteSwap(Sprite newSprite)
        {
            Image image = _targetGraphic as Image;
            if (image != null)
            {
                image.overrideSprite = newSprite;
            }            
        }

        private void TriggerAnimation(string triggername)
        {
            if(_transition == Selectable.Transition.Animation)
            {
                Animator animator = GetComponent<Animator>();
                if (animator != null && animator.isActiveAndEnabled && animator.hasBoundPlayables && !string.IsNullOrEmpty(triggername))
                {
                    animator.ResetTrigger(_activeAnimationTriggers.normalTrigger);
                    animator.ResetTrigger(_activeAnimationTriggers.highlightedTrigger);
                    animator.ResetTrigger(_activeAnimationTriggers.pressedTrigger);
                    animator.ResetTrigger(_activeAnimationTriggers.selectedTrigger);
                    animator.ResetTrigger(_activeAnimationTriggers.disabledTrigger);

                    animator.SetTrigger(triggername);
                }                
            }        
        }

        private void SetButtonState(string state, in ColorBlock colors, Sprite normalSprite, in SpriteState spriteState, AnimationTriggers animationTriggers)
        {
            State = state;
            _activeColors = colors;
            _activeNormalSprite = normalSprite;
            _activeSpriteState = spriteState;
            _activeAnimationTriggers = animationTriggers;
            DoStateTransition(_selectionState, false);
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
    }
}
