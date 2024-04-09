using System;

using UIFramework;
using UIFramework.UIToolkit;

using UnityEngine.UIElements;

public class UITKExampleScreen : Screen<ExampleController>
{
    private Button _returnButton = null;
    private Button _travelToUGUITransitionScreenButton = null;
    private Button _tavelToUITKTransitionScreenButton = null;
    private Button _travelToUGUISharedCanvasScreenButton = null;
    private Button _travelToUGUIAltCanvasScreenButton = null;

    public UITKExampleScreen(UIDocument uiDocument, VisualElement visualElement) : base(uiDocument, visualElement) { }

    public override bool requiresData { get { return false; } }

    public override bool IsValidData(object data)
    {
        return false;
    }

    protected override void OnInit()
    {
        base.OnInit();
        _returnButton = visualElement.Q<Button>("returnButton");
        _returnButton.clicked += controller.navigation.Back;

        _travelToUGUITransitionScreenButton = visualElement.Q<Button>("uguiTransitionButton");
        _travelToUGUITransitionScreenButton.clicked += TravelToUGUITransitionScreen;

        _tavelToUITKTransitionScreenButton = visualElement.Q<Button>("uitkTransitionButton");
        _tavelToUITKTransitionScreenButton.clicked += TravelToUITKTransitionScreen;

        _travelToUGUISharedCanvasScreenButton = visualElement.Q<Button>("uguiSharedCanvasButton");
        _travelToUGUISharedCanvasScreenButton.clicked += TravelToUGUISharedCanvasScreen;

        _travelToUGUIAltCanvasScreenButton = visualElement.Q<Button>("uguiAltCanvasButton");
        _travelToUGUIAltCanvasScreenButton.clicked += TravelToUGUIAltCanvasScreen;
    }

    private void TravelToUGUITransitionScreen()
    {
        ScreenTransition transition = ScreenTransition.Custom(0.5F, UIFramework.EasingMode.EaseInOut, WindowAnimation.Type.Flip, WindowAnimation.Type.Fade, ScreenTransition.SortPriority.Source);
        controller.navigation.Travel<UGUIExampleTransitionScreen>(in transition, null);
    }    

    private void TravelToUITKTransitionScreen()
    {
        ScreenTransition transition = ScreenTransition.Custom(0.5F, UIFramework.EasingMode.EaseInOut, WindowAnimation.Type.Fade, WindowAnimation.Type.Fade);
        controller.navigation.Travel<UITKExampleTransitionScreen>(in transition, null);
    }

    private void TravelToUGUISharedCanvasScreen()
    {
        ScreenTransition transition = ScreenTransition.Custom(0.5F, UIFramework.EasingMode.EaseInOut, WindowAnimation.Type.Fade, WindowAnimation.Type.Fade, ScreenTransition.SortPriority.Target);
        controller.navigation.Travel<UGUIExampleSharedCanvasScreen>(in transition, null);
    }

    private void TravelToUGUIAltCanvasScreen()
    {
        ScreenTransition transition = ScreenTransition.Custom(0.5F, UIFramework.EasingMode.EaseInOut, WindowAnimation.Type.Fade, WindowAnimation.Type.Expand, ScreenTransition.SortPriority.Target);
        controller.navigation.Travel<UGUIExampleAlternateCanvasScreen>(in transition, null);
    }    
}
