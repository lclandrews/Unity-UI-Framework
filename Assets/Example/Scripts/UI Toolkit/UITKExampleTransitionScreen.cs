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

        _fadeTransitionButton = visualElement.Q<Button>("fadeButton");
        _fadeTransitionButton.clicked += FadeTransition;

        _dissolveTransitionButton = visualElement.Q<Button>("dissolveButton");
        _dissolveTransitionButton.clicked += DissolveTransition;

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
        WindowTransitionPlayable transition = WindowTransitionPlayable.Fade(_transitionLength, UIFramework.EasingMode.EaseInOut);
        controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void DissolveTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.Dissolve(_transitionLength, UIFramework.EasingMode.EaseInOut);
        controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideFromLeftTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideFromLeft(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideFromRightTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideFromRight(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideFromBottomTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideFromBottom(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideFromTopTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideFromTop(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideOverLeftTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideOverFromLeft(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideOverRightTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideOverFromRight(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideOverBottomTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideOverFromBottom(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideOverTopTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideOverFromTop(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void FlipTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.Flip(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void ExpandTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.Expand(_transitionLength, UIFramework.EasingMode.EaseInOutBack);
        controller.OpenScreen<UITKExampleScreen>(in transition);
    }
}
