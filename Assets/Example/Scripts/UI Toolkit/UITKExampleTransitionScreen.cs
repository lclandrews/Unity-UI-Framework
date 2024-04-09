using UIFramework;
using UIFramework.UIToolkit;

using UnityEngine.UIElements;

public class UITKExampleTransitionScreen : Screen<ExampleController>
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

    public override bool requiresData { get { return false; } }

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
        ScreenTransition transition = ScreenTransition.Fade(_transitionLength, UIFramework.EasingMode.EaseInOut);
        controller.navigation.Travel<UITKExampleScreen>(in transition, null);
    }

    private void DissolveTransition()
    {
        ScreenTransition transition = ScreenTransition.Dissolve(_transitionLength, UIFramework.EasingMode.EaseInOut);
        controller.navigation.Travel<UITKExampleScreen>(in transition, null);
    }

    private void SlideFromLeftTransition()
    {
        ScreenTransition transition = ScreenTransition.SlideFromLeft(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        controller.navigation.Travel<UITKExampleScreen>(in transition, null);
    }

    private void SlideFromRightTransition()
    {
        ScreenTransition transition = ScreenTransition.SlideFromRight(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        controller.navigation.Travel<UITKExampleScreen>(in transition, null);
    }

    private void SlideFromBottomTransition()
    {
        ScreenTransition transition = ScreenTransition.SlideFromBottom(_transitionLength, UIFramework.EasingMode.EaseInOut);
        controller.navigation.Travel<UITKExampleScreen>(in transition, null);
    }

    private void SlideFromTopTransition()
    {
        ScreenTransition transition = ScreenTransition.SlideFromTop(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        controller.navigation.Travel<UITKExampleScreen>(in transition, null);
    }

    private void SlideOverLeftTransition()
    {
        ScreenTransition transition = ScreenTransition.SlideOverFromLeft(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        controller.navigation.Travel<UITKExampleScreen>(in transition, null);
    }

    private void SlideOverRightTransition()
    {
        ScreenTransition transition = ScreenTransition.SlideOverFromRight(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        controller.navigation.Travel<UITKExampleScreen>(in transition, null);
    }

    private void SlideOverBottomTransition()
    {
        ScreenTransition transition = ScreenTransition.SlideOverFromBottom(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        controller.navigation.Travel<UITKExampleScreen>(in transition, null);
    }

    private void SlideOverTopTransition()
    {
        ScreenTransition transition = ScreenTransition.SlideOverFromTop(_transitionLength, UIFramework.EasingMode.EaseOutBounce);
        controller.navigation.Travel<UITKExampleScreen>(in transition, null);
    }

    private void FlipTransition()
    {
        ScreenTransition transition = ScreenTransition.Flip(_transitionLength, UIFramework.EasingMode.EaseInBounce);
        controller.navigation.Travel<UITKExampleScreen>(in transition, null);
    }

    private void ExpandTransition()
    {
        ScreenTransition transition = ScreenTransition.Expand(_transitionLength, UIFramework.EasingMode.EaseInOutBack);
        controller.navigation.Travel<UITKExampleScreen>(in transition, null);
    }
}
