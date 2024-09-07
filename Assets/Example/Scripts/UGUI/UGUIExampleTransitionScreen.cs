using UIFramework;

using UnityEngine;
using UnityEngine.Extension;
using UnityEngine.UI;

public class UGUIExampleTransitionScreen : UIFramework.UGUI.Screen
{
    [SerializeField] private UIFramework.UGUI.Screen _targetScreen = null;

    [SerializeField] private float _transitionLength = 0.5F;

    [SerializeField] private Button _fadeTransitionButton = null;
    [SerializeField] private Button _dissolveTransitionButton = null;

    [SerializeField] private Button _slideFromLeftTransitionButton = null;
    [SerializeField] private Button _slideFromRightTransitionButton = null;
    [SerializeField] private Button _slideFromBottomTransitionButton = null;
    [SerializeField] private Button _slideFromTopTransitionButton = null;

    [SerializeField] private Button _slideOverLeftTransitionButton = null;
    [SerializeField] private Button _slideOverRightTransitionButton = null;
    [SerializeField] private Button _slideOverBottomTransitionButton = null;
    [SerializeField] private Button _slideOverTopTransitionButton = null;

    [SerializeField] private Button _flipTransitionButton = null;
    [SerializeField] private Button _expandTransitionButton = null;

    public override bool IsValidData(object data)
    {
        return false;
    }

    protected override void OnInit()
    {
        base.OnInit();
        _fadeTransitionButton?.onClick.AddListener(FadeTransition);
        _dissolveTransitionButton?.onClick.AddListener(DissolveTransition);
        _slideFromLeftTransitionButton?.onClick.AddListener(SlideFromLeftTransition);
        _slideFromRightTransitionButton?.onClick?.AddListener(SlideFromRightTransition);
        _slideFromBottomTransitionButton?.onClick.AddListener(SlideFromBottomTransition);
        _slideFromTopTransitionButton?.onClick.AddListener(SlideFromTopTransition);
        _slideOverLeftTransitionButton?.onClick.AddListener(SlideOverLeftTransition);
        _slideOverRightTransitionButton?.onClick?.AddListener(SlideOverRightTransition);
        _slideOverBottomTransitionButton?.onClick.AddListener(SlideOverBottomTransition);
        _slideOverTopTransitionButton?.onClick.AddListener(SlideOverTopTransition);
        _flipTransitionButton?.onClick.AddListener(FlipTransition);
        _expandTransitionButton?.onClick.AddListener(ExpandTransition);
    }

    // UGUIExampleTransitionScreenBase
    private void FadeTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.Fade(_transitionLength, EasingMode.EaseInOut);
        Controller.OpenScreen(_targetScreen, GetTargetScreenData(), in transition);
    }

    private void DissolveTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.Dissolve(_transitionLength, EasingMode.EaseInOut);
        Controller.OpenScreen(_targetScreen, GetTargetScreenData(), in transition);
    }

    private void SlideFromLeftTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideFromLeft(_transitionLength, EasingMode.EaseOutBounce);
        Controller.OpenScreen(_targetScreen, GetTargetScreenData(), in transition);
    }

    private void SlideFromRightTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideFromRight(_transitionLength, EasingMode.EaseOutBounce);
        Controller.OpenScreen(_targetScreen, GetTargetScreenData(), in transition);
    }

    private void SlideFromBottomTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideFromBottom(_transitionLength, EasingMode.EaseOutBounce);
        Controller.OpenScreen(_targetScreen, GetTargetScreenData(), in transition);
    }

    private void SlideFromTopTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideFromTop(_transitionLength, EasingMode.EaseOutBounce);
        Controller.OpenScreen(_targetScreen, GetTargetScreenData(), in transition);
    }

    private void SlideOverLeftTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideOverFromLeft(_transitionLength, EasingMode.EaseOutBounce);
        Controller.OpenScreen(_targetScreen, GetTargetScreenData(), in transition);
    }

    private void SlideOverRightTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideOverFromRight(_transitionLength, EasingMode.EaseOutBounce);
        Controller.OpenScreen(_targetScreen, GetTargetScreenData(), in transition);
    }

    private void SlideOverBottomTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideOverFromBottom(_transitionLength, EasingMode.EaseOutBounce);
        Controller.OpenScreen(_targetScreen, GetTargetScreenData(), in transition);
    }

    private void SlideOverTopTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideOverFromTop(_transitionLength, EasingMode.EaseOutBounce);
        Controller.OpenScreen(_targetScreen, GetTargetScreenData(), in transition);
    }

    private void FlipTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.Flip(_transitionLength, EasingMode.EaseOutBounce);
        Controller.OpenScreen(_targetScreen, GetTargetScreenData(), in transition);
    }

    private void ExpandTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.Expand(_transitionLength, EasingMode.EaseInOutBack);
        Controller.OpenScreen(_targetScreen, GetTargetScreenData(),in transition);
    }

    protected object GetTargetScreenData()
    {
        return null;
    }
}
