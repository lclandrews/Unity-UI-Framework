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
        WindowTransitionPlayable transition = WindowTransitionPlayable.Custom(0.5F, UIFramework.EasingMode.EaseInOut, GenericWindowAnimationType.Flip, GenericWindowAnimationType.Fade, WindowTransitionPlayable.SortPriority.Source);
        Controller.OpenScreen<UGUIExampleTransitionScreen>(in transition);
    }    

    private void TravelToUITKTransitionScreen()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.Custom(0.5F, UIFramework.EasingMode.EaseInOut, GenericWindowAnimationType.Fade, GenericWindowAnimationType.Fade);
        Controller.OpenScreen<UITKExampleTransitionScreen>(in transition);
    }

    private void TravelToUGUISharedCanvasScreen()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.Custom(0.5F, UIFramework.EasingMode.EaseInOut, GenericWindowAnimationType.Fade, GenericWindowAnimationType.Fade, WindowTransitionPlayable.SortPriority.Target);
        Controller.OpenScreen<UGUIExampleSharedCanvasScreen>(in transition);
    }

    private void TravelToUGUIAltCanvasScreen()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.Custom(0.5F, UIFramework.EasingMode.EaseInOut, GenericWindowAnimationType.Fade, GenericWindowAnimationType.Expand, WindowTransitionPlayable.SortPriority.Target);
        Controller.OpenScreen<UGUIExampleAlternateCanvasScreen>(in transition);
    }    
}
