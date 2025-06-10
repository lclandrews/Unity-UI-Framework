using UIFramework;
using UIFramework.UIToolkit;

using UnityEngine.UIElements;

public class UITKExampleTransitionScreen : UIFramework.UIToolkit.Screen
{
    private float _transitionLength = 0.5F;

    private Button _fadeTransitionButton = null;

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

    public UITKExampleTransitionScreen(UIBehaviourDocument uIBehaviourDocument, string indentifier) : base(uIBehaviourDocument, indentifier) { }

    public override bool IsValidData(object data)
    {
        return false;
    }

    protected override void OnInitialize(VisualElement visualElement)
    {
        base.OnInitialize(visualElement);

        _fadeTransitionButton = visualElement.Q<Button>("fadeButton");
        _fadeTransitionButton.clicked += FadeTransition;

        _slideFromLeftTransitionButton = visualElement.Q<Button>("slideFromLeftButton");
        _slideFromLeftTransitionButton.clicked += SlideFromLeftTransition;
        _slideFromRightTransitionButton = visualElement.Q<Button>("slideFromRightButton");
        _slideFromRightTransitionButton.clicked += SlideFromRightTransition;
        _slideFromBottomTransitionButton = visualElement.Q<Button>("slideFromBottomButton");
        _slideFromBottomTransitionButton.clicked += SlideFromBottomTransition;
        _slideFromTopTransitionButton = visualElement.Q<Button>("slideFromTopButton");
        _slideFromTopTransitionButton.clicked += SlideFromTopTransition;

        _slideOverLeftTransitionButton = visualElement.Q<Button>("slideOverLeftButton");
        _slideOverLeftTransitionButton.clicked += SlideOverLeftTransition;
        _slideOverRightTransitionButton = visualElement.Q<Button>("slideOverRightButton");
        _slideOverRightTransitionButton.clicked += SlideOverRightTransition;
        _slideOverBottomTransitionButton = visualElement.Q<Button>("slideOverBottomButton");
        _slideOverBottomTransitionButton.clicked += SlideOverBottomTransition;
        _slideOverTopTransitionButton = visualElement.Q<Button>("slideOverTopButton");
        _slideOverTopTransitionButton.clicked += SlideOverTopTransition;

        _flipTransitionButton = visualElement.Q<Button>("flipButton");
        _flipTransitionButton.clicked += FlipTransition;
        _expandTransitionButton = visualElement.Q<Button>("expandButton");
        _expandTransitionButton.clicked += ExpandTransition;
    }

    private void FadeTransition()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.Fade(_transitionLength, UnityEngine.Extension.EasingMode.EaseInOut);
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
