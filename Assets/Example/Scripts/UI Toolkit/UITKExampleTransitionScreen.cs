using UIFramework;

using UnityEngine.UIElements;

public class UITKExampleTransitionScreen : UIFramework.UIToolkit.Screen
{
    private float _transitionLength = 0.5F;

    private Button _fadeTransitionButton = null;
    private Button _dissolveTransitionButton = null;

    private Button _slideFromLeftTransitionButton = null;
    private Button _slideFromRightTransitionButton = null;
    private Button _slideFromBottomTransitionButton = null;
    private Button _slideFromTopTransitionButton = null;

    private Button _slideOverLeftTransitionButton = null;
    private Button _slideOverRightTransitionButton = null;
    private Button _slideOverBottomTransitionButton = null;
    private Button _slideOverTopTransitionButton = null;

    private Button _flipTransitionButton = null;
    private Button _expandTransitionButton = null;

    public UITKExampleTransitionScreen(UIDocument uiDocument, VisualElement visualElement) : base(uiDocument, visualElement) { }

    public override bool IsValidData(object data)
    {
        return false;
    }

    protected override void OnInit()
    {
        base.OnInit();

        _fadeTransitionButton = VisualElement.Q<Button>("fadeButton");
        _fadeTransitionButton.clicked += FadeTransition;

        _dissolveTransitionButton = VisualElement.Q<Button>("dissolveButton");
        _dissolveTransitionButton.clicked += DissolveTransition;

        _slideFromLeftTransitionButton = VisualElement.Q<Button>("slideFromLeftButton");
        _slideFromLeftTransitionButton.clicked += SlideFromLeftTransition;
        _slideFromRightTransitionButton = VisualElement.Q<Button>("slideFromRightButton");
        _slideFromRightTransitionButton.clicked += SlideFromRightTransition;
        _slideFromBottomTransitionButton = VisualElement.Q<Button>("slideFromBottomButton");
        _slideFromBottomTransitionButton.clicked += SlideFromBottomTransition;
        _slideFromTopTransitionButton = VisualElement.Q<Button>("slideFromTopButton");
        _slideFromTopTransitionButton.clicked += SlideFromTopTransition;

        _slideOverLeftTransitionButton = VisualElement.Q<Button>("slideOverLeftButton");
        _slideOverLeftTransitionButton.clicked += SlideOverLeftTransition;
        _slideOverRightTransitionButton = VisualElement.Q<Button>("slideOverRightButton");
        _slideOverRightTransitionButton.clicked += SlideOverRightTransition;
        _slideOverBottomTransitionButton = VisualElement.Q<Button>("slideOverBottomButton");
        _slideOverBottomTransitionButton.clicked += SlideOverBottomTransition;
        _slideOverTopTransitionButton = VisualElement.Q<Button>("slideOverTopButton");
        _slideOverTopTransitionButton.clicked += SlideOverTopTransition;

        _flipTransitionButton = VisualElement.Q<Button>("flipButton");
        _flipTransitionButton.clicked += FlipTransition;
        _expandTransitionButton = VisualElement.Q<Button>("expandButton");
        _expandTransitionButton.clicked += ExpandTransition;
    }

    private void FadeTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.Fade(_transitionLength, UnityEngine.Extension.EasingMode.EaseInOut);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void DissolveTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.Dissolve(_transitionLength, UnityEngine.Extension.EasingMode.EaseInOut);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideFromLeftTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideFromLeft(_transitionLength, UnityEngine.Extension.EasingMode.EaseOutBounce);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideFromRightTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideFromRight(_transitionLength, UnityEngine.Extension.EasingMode.EaseOutBounce);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideFromBottomTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideFromBottom(_transitionLength, UnityEngine.Extension.EasingMode.EaseOutBounce);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideFromTopTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideFromTop(_transitionLength, UnityEngine.Extension.EasingMode.EaseOutBounce);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideOverLeftTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideOverFromLeft(_transitionLength, UnityEngine.Extension.EasingMode.EaseOutBounce);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideOverRightTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideOverFromRight(_transitionLength, UnityEngine.Extension.EasingMode.EaseOutBounce);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideOverBottomTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideOverFromBottom(_transitionLength, UnityEngine.Extension.EasingMode.EaseOutBounce);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideOverTopTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.SlideOverFromTop(_transitionLength, UnityEngine.Extension.EasingMode.EaseOutBounce);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void FlipTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.Flip(_transitionLength, UnityEngine.Extension.EasingMode.EaseOutBounce);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void ExpandTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.Expand(_transitionLength, UnityEngine.Extension.EasingMode.EaseInOutBack);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }
}
