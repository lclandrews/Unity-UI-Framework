using UIFramework;

using UnityEngine;
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

    // UGUIScreen
    public override bool requiresData { get { return false; } }

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
        WindowTransitionPlayable transition = WindowTransitionPlayable.Fade(_transitionLength, EasingMode.EaseInOut);
        controller.OpenScreen(_targetScreen, in transition);
        _targetScreen.SetData(GetTargetScreenData());
    }

    private void DissolveTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.Dissolve(_transitionLength, EasingMode.EaseInOut);
        controller.OpenScreen(_targetScreen, in transition);
        _targetScreen.SetData(GetTargetScreenData());
    }

    private void SlideFromLeftTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideFromLeft(_transitionLength, EasingMode.EaseOutBounce);
        controller.OpenScreen(_targetScreen, in transition);
        _targetScreen.SetData(GetTargetScreenData());
    }

    private void SlideFromRightTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideFromRight(_transitionLength, EasingMode.EaseOutBounce);
        controller.OpenScreen(_targetScreen, in transition);
        _targetScreen.SetData(GetTargetScreenData());
    }

    private void SlideFromBottomTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideFromBottom(_transitionLength, EasingMode.EaseOutBounce);
        controller.OpenScreen(_targetScreen, in transition);
        _targetScreen.SetData(GetTargetScreenData());
    }

    private void SlideFromTopTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideFromTop(_transitionLength, EasingMode.EaseOutBounce);
        controller.OpenScreen(_targetScreen, in transition);
        _targetScreen.SetData(GetTargetScreenData());
    }

    private void SlideOverLeftTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideOverFromLeft(_transitionLength, EasingMode.EaseOutBounce);
        controller.OpenScreen(_targetScreen, in transition);
        _targetScreen.SetData(GetTargetScreenData());
    }

    private void SlideOverRightTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideOverFromRight(_transitionLength, EasingMode.EaseOutBounce);
        controller.OpenScreen(_targetScreen, in transition);
        _targetScreen.SetData(GetTargetScreenData());
    }

    private void SlideOverBottomTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideOverFromBottom(_transitionLength, EasingMode.EaseOutBounce);
        controller.OpenScreen(_targetScreen, in transition);
        _targetScreen.SetData(GetTargetScreenData());
    }

    private void SlideOverTopTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideOverFromTop(_transitionLength, EasingMode.EaseOutBounce);
        controller.OpenScreen(_targetScreen, in transition);
        _targetScreen.SetData(GetTargetScreenData());
    }

    private void FlipTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.Flip(_transitionLength, EasingMode.EaseOutBounce);
        controller.OpenScreen(_targetScreen, in transition);
        _targetScreen.SetData(GetTargetScreenData());
    }

    private void ExpandTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.Expand(_transitionLength, EasingMode.EaseInOutBack);
        controller.OpenScreen(_targetScreen, in transition);
        _targetScreen.SetData(GetTargetScreenData());
    }

    protected object GetTargetScreenData()
    {
        return null;
    }
}
