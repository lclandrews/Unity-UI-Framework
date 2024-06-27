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
        WindowTransitionPlayable transition = WindowTransitionPlayable.Fade(_transitionLength, UIFramework.EasingMode.EaseInOut);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void DissolveTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.Dissolve(_transitionLength, UIFramework.EasingMode.EaseInOut);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideFromLeftTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideFromLeft(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideFromRightTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideFromRight(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideFromBottomTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideFromBottom(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideFromTopTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideFromTop(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideOverLeftTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideOverFromLeft(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideOverRightTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideOverFromRight(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideOverBottomTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideOverFromBottom(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void SlideOverTopTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.SlideOverFromTop(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void FlipTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.Flip(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }

    private void ExpandTransition()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.Expand(_transitionLength, UIFramework.EasingMode.EaseInOutBack);
        Controller.OpenScreen<UITKExampleScreen>(in transition);
    }
}
