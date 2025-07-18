using UIFramework;
using UIFramework.UIToolkit;

using UnityEngine;
using UnityEngine.UIElements;

public class UITKExampleScreen : UIFramework.UIToolkit.Screen
{
    private Button _returnButton = null;
    private Button _travelToUGUITransitionScreenButton = null;
    private Button _tavelToUITKTransitionScreenButton = null;
    private Button _travelToUGUISharedCanvasScreenButton = null;
    private Button _travelToUGUIAltCanvasScreenButton = null;

    public UITKExampleScreen(UIBehaviourDocument uIBehaviourDocument, string identifier) : base(uIBehaviourDocument, identifier) { }

    public override bool IsValidData(object data)
    {
        return false;
    }

    protected override void OnInitialize(VisualElement visualElement)
    {
        base.OnInitialize(visualElement);
        _returnButton = visualElement.Q<Button>("returnButton");
        _returnButton.clicked += Controller.CloseScreen;

        _travelToUGUITransitionScreenButton = visualElement.Q<Button>("uguiTransitionButton");
        _travelToUGUITransitionScreenButton.clicked += TravelToUGUITransitionScreen;

        _tavelToUITKTransitionScreenButton = visualElement.Q<Button>("uitkTransitionButton");
        _tavelToUITKTransitionScreenButton.clicked += TravelToUITKTransitionScreen;

        _travelToUGUISharedCanvasScreenButton = visualElement.Q<Button>("uguiSharedCanvasButton");
        _travelToUGUISharedCanvasScreenButton.clicked += TravelToUGUISharedCanvasScreen;

        _travelToUGUIAltCanvasScreenButton = visualElement.Q<Button>("uguiAltCanvasButton");
        _travelToUGUIAltCanvasScreenButton.clicked += TravelToUGUIAltCanvasScreen;
    }

    protected override void OnTerminate()
    {
        base.OnTerminate();
        _returnButton.clicked -= Controller.CloseScreen;
        _travelToUGUITransitionScreenButton.clicked -= TravelToUGUITransitionScreen;
        _tavelToUITKTransitionScreenButton.clicked -= TravelToUITKTransitionScreen;
        _travelToUGUISharedCanvasScreenButton.clicked -= TravelToUGUISharedCanvasScreen;
        _travelToUGUIAltCanvasScreenButton.clicked -= TravelToUGUIAltCanvasScreen;
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
    
    public override void UpdateUI(float deltaTime)
    {
        base.UpdateUI(deltaTime);
        Debug.Log($"IsValid: {IsValid()}");
    }
}
