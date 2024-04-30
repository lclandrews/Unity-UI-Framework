using UIFramework;
using UIFramework.UGUI;

using UnityEngine;
using UnityEngine.UI;

public class UGUIExampleTransitionScreen : Screen<ExampleController>
{
    [SerializeField] private Screen<ExampleController> _targetScreen = null;

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
        WindowTransition transition = WindowTransition.Fade(_transitionLength, EasingMode.EaseInOut);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    private void DissolveTransition()
    {
        WindowTransition transition = WindowTransition.Dissolve(_transitionLength, EasingMode.EaseInOut);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    private void SlideFromLeftTransition()
    {
        WindowTransition transition = WindowTransition.SlideFromLeft(_transitionLength, EasingMode.EaseOutBounce);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    private void SlideFromRightTransition()
    {
        WindowTransition transition = WindowTransition.SlideFromRight(_transitionLength, EasingMode.EaseOutBounce);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    private void SlideFromBottomTransition()
    {
        WindowTransition transition = WindowTransition.SlideFromBottom(_transitionLength, EasingMode.EaseInOut);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    private void SlideFromTopTransition()
    {
        WindowTransition transition = WindowTransition.SlideFromTop(_transitionLength, EasingMode.EaseOutBounce);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    private void SlideOverLeftTransition()
    {
        WindowTransition transition = WindowTransition.SlideOverFromLeft(_transitionLength, EasingMode.EaseOutBounce);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    private void SlideOverRightTransition()
    {
        WindowTransition transition = WindowTransition.SlideOverFromRight(_transitionLength, EasingMode.EaseOutBounce);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    private void SlideOverBottomTransition()
    {
        WindowTransition transition = WindowTransition.SlideOverFromBottom(_transitionLength, EasingMode.EaseOutBounce);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    private void SlideOverTopTransition()
    {
        WindowTransition transition = WindowTransition.SlideOverFromTop(_transitionLength, EasingMode.EaseOutBounce);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    private void FlipTransition()
    {
        WindowTransition transition = WindowTransition.Flip(_transitionLength, EasingMode.EaseInBounce);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    private void ExpandTransition()
    {
        WindowTransition transition = WindowTransition.Expand(_transitionLength, EasingMode.EaseInOutBack);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    protected object GetTargetScreenData()
    {
        return null;
    }
}
