using UIFramework;

using UnityEngine;
using UnityEngine.Extension;
using UnityEngine.UI;

public class UGUIExampleTransitionScreen : UIFramework.UGUI.Screen
{
    [SerializeField] private float _transitionLength = 0.5F;

    [SerializeField] private Button _fadeTransitionButton = null;

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

    protected override void OnInitialize()
    {
        base.OnInitialize();
        _fadeTransitionButton?.onClick.AddListener(FadeTransition);
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

    protected override void OnTerminate()
    {
        base.OnTerminate();
        _fadeTransitionButton?.onClick.RemoveListener(FadeTransition);
        _slideFromLeftTransitionButton?.onClick.RemoveListener(SlideFromLeftTransition);
        _slideFromRightTransitionButton?.onClick?.RemoveListener(SlideFromRightTransition);
        _slideFromBottomTransitionButton?.onClick.RemoveListener(SlideFromBottomTransition);
        _slideFromTopTransitionButton?.onClick.RemoveListener(SlideFromTopTransition);
        _slideOverLeftTransitionButton?.onClick.RemoveListener(SlideOverLeftTransition);
        _slideOverRightTransitionButton?.onClick?.RemoveListener(SlideOverRightTransition);
        _slideOverBottomTransitionButton?.onClick.RemoveListener(SlideOverBottomTransition);
        _slideOverTopTransitionButton?.onClick.RemoveListener(SlideOverTopTransition);
        _flipTransitionButton?.onClick.RemoveListener(FlipTransition);
        _expandTransitionButton?.onClick.RemoveListener(ExpandTransition);
    }

    private object GetTargetScreenData()
    {
        return null;
    }

    // UGUIExampleTransitionScreenBase
    private void FadeTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.Fade(_transitionLength, EasingMode.EaseInOut);
        Controller.OpenScreen<UGUIExampleSharedCanvasScreen>(GetTargetScreenData(), in transition);
    }

    private void SlideFromLeftTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideFromLeft(_transitionLength, EasingMode.EaseOutBounce);
        Controller.OpenScreen<UGUIExampleSharedCanvasScreen>(GetTargetScreenData(), in transition);
    }

    private void SlideFromRightTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideFromRight(_transitionLength, EasingMode.EaseOutBounce);
        Controller.OpenScreen<UGUIExampleSharedCanvasScreen>(GetTargetScreenData(), in transition);
    }

    private void SlideFromBottomTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideFromBottom(_transitionLength, EasingMode.EaseOutBounce);
        Controller.OpenScreen<UGUIExampleSharedCanvasScreen>(GetTargetScreenData(), in transition);
    }

    private void SlideFromTopTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideFromTop(_transitionLength, EasingMode.EaseOutBounce);
        Controller.OpenScreen<UGUIExampleSharedCanvasScreen>(GetTargetScreenData(), in transition);
    }

    private void SlideOverLeftTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideOverFromLeft(_transitionLength, EasingMode.EaseOutBounce);
        Controller.OpenScreen<UGUIExampleSharedCanvasScreen>(GetTargetScreenData(), in transition);
    }

    private void SlideOverRightTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideOverFromRight(_transitionLength, EasingMode.EaseOutBounce);
        Controller.OpenScreen<UGUIExampleSharedCanvasScreen>(GetTargetScreenData(), in transition);
    }

    private void SlideOverBottomTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideOverFromBottom(_transitionLength, EasingMode.EaseOutBounce);
        Controller.OpenScreen<UGUIExampleSharedCanvasScreen>(GetTargetScreenData(), in transition);
    }

    private void SlideOverTopTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideOverFromTop(_transitionLength, EasingMode.EaseOutBounce);
        Controller.OpenScreen<UGUIExampleSharedCanvasScreen>(GetTargetScreenData(), in transition);
    }

    private void FlipTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.Flip(_transitionLength, EasingMode.EaseOutBounce);
        Controller.OpenScreen<UGUIExampleSharedCanvasScreen>(GetTargetScreenData(), in transition);
    }

    private void ExpandTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.Expand(_transitionLength, EasingMode.EaseInOutBack);
        Controller.OpenScreen<UGUIExampleSharedCanvasScreen>(GetTargetScreenData(),in transition);
    }  
}
