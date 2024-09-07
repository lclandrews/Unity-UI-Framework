using UIFramework;

using UnityEngine.UIElements;

public class UITKExampleScreen : UIFramework.UIToolkit.Screen
{
    private Button _returnButton = null;
    private Button _travelToUGUITransitionScreenButton = null;
    private Button _tavelToUITKTransitionScreenButton = null;
    private Button _travelToUGUISharedCanvasScreenButton = null;
    private Button _travelToUGUIAltCanvasScreenButton = null;

    public UITKExampleScreen(UIDocument uiDocument, VisualElement visualElement) : base(uiDocument, visualElement) { }

    public override bool IsValidData(object data)
    {
        return false;
    }

    protected override void OnInit()
    {
        base.OnInit();
        _returnButton = VisualElement.Q<Button>("returnButton");
        _returnButton.clicked += delegate() { Controller.CloseScreen(); };

        _travelToUGUITransitionScreenButton = VisualElement.Q<Button>("uguiTransitionButton");
        _travelToUGUITransitionScreenButton.clicked += TravelToUGUITransitionScreen;

        _tavelToUITKTransitionScreenButton = VisualElement.Q<Button>("uitkTransitionButton");
        _tavelToUITKTransitionScreenButton.clicked += TravelToUITKTransitionScreen;

        _travelToUGUISharedCanvasScreenButton = VisualElement.Q<Button>("uguiSharedCanvasButton");
        _travelToUGUISharedCanvasScreenButton.clicked += TravelToUGUISharedCanvasScreen;

        _travelToUGUIAltCanvasScreenButton = VisualElement.Q<Button>("uguiAltCanvasButton");
        _travelToUGUIAltCanvasScreenButton.clicked += TravelToUGUIAltCanvasScreen;
    }

    private void TravelToUGUITransitionScreen()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.Custom(0.5F, UnityEngine.Extension.EasingMode.EaseInOut, GenericWindowAnimationType.Flip, GenericWindowAnimationType.Fade, TransitionAnimationParams.SortPriority.Source);
        Controller.OpenScreen<UGUIExampleTransitionScreen>(in transition);
    }    

    private void TravelToUITKTransitionScreen()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.Custom(0.5F, UnityEngine.Extension.EasingMode.EaseInOut, GenericWindowAnimationType.Fade, GenericWindowAnimationType.Fade);
        Controller.OpenScreen<UITKExampleTransitionScreen>(in transition);
    }

    private void TravelToUGUISharedCanvasScreen()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.Custom(0.5F, UnityEngine.Extension.EasingMode.EaseInOut, GenericWindowAnimationType.Fade, GenericWindowAnimationType.Fade, TransitionAnimationParams.SortPriority.Target);
        Controller.OpenScreen<UGUIExampleSharedCanvasScreen>(in transition);
    }

    private void TravelToUGUIAltCanvasScreen()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.Custom(0.5F, UnityEngine.Extension.EasingMode.EaseInOut, GenericWindowAnimationType.Fade, GenericWindowAnimationType.Expand, TransitionAnimationParams.SortPriority.Target);
        Controller.OpenScreen<UGUIExampleAlternateCanvasScreen>(in transition);
    }    
}
