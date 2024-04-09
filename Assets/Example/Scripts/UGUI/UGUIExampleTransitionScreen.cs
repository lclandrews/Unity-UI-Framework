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
        ScreenTransition transition = ScreenTransition.Fade(_transitionLength, EasingMode.EaseInOut);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    private void DissolveTransition()
    {
        ScreenTransition transition = ScreenTransition.Dissolve(_transitionLength, EasingMode.EaseInOut);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    private void SlideFromLeftTransition()
    {
        ScreenTransition transition = ScreenTransition.SlideFromLeft(_transitionLength, EasingMode.EaseOutBounce);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    private void SlideFromRightTransition()
    {
        ScreenTransition transition = ScreenTransition.SlideFromRight(_transitionLength, EasingMode.EaseOutBounce);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    private void SlideFromBottomTransition()
    {
        ScreenTransition transition = ScreenTransition.SlideFromBottom(_transitionLength, EasingMode.EaseInOut);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    private void SlideFromTopTransition()
    {
        ScreenTransition transition = ScreenTransition.SlideFromTop(_transitionLength, EasingMode.EaseOutBounce);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    private void SlideOverLeftTransition()
    {
        ScreenTransition transition = ScreenTransition.SlideOverFromLeft(_transitionLength, EasingMode.EaseOutBounce);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    private void SlideOverRightTransition()
    {
        ScreenTransition transition = ScreenTransition.SlideOverFromRight(_transitionLength, EasingMode.EaseOutBounce);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    private void SlideOverBottomTransition()
    {
        ScreenTransition transition = ScreenTransition.SlideOverFromBottom(_transitionLength, EasingMode.EaseOutBounce);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    private void SlideOverTopTransition()
    {
        ScreenTransition transition = ScreenTransition.SlideOverFromTop(_transitionLength, EasingMode.EaseOutBounce);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    private void FlipTransition()
    {
        ScreenTransition transition = ScreenTransition.Flip(_transitionLength, EasingMode.EaseInBounce);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    private void ExpandTransition()
    {
        ScreenTransition transition = ScreenTransition.Expand(_transitionLength, EasingMode.EaseInOutBack);
        controller.navigation.Travel(_targetScreen, in transition, GetTargetScreenData());
    }

    protected object GetTargetScreenData()
    {
        return null;
    }
}
