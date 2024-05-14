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

    public override bool requiresData { get { return false; } }

    public override bool IsValidData(object data)
    {
        return false;
    }

    protected override void OnInit()
    {
        base.OnInit();
        _returnButton = visualElement.Q<Button>("returnButton");
        _returnButton.clicked += delegate() { controller.Back(); };

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
        WindowTransitionPlayable transition = WindowTransitionPlayable.Custom(0.5F, UIFramework.EasingMode.EaseInOut, GenericWindowAnimationType.Flip, GenericWindowAnimationType.Fade, WindowTransitionPlayable.SortPriority.Source);
        controller.OpenScreen<UGUIExampleTransitionScreen>(in transition);
    }    

    private void TravelToUITKTransitionScreen()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.Custom(0.5F, UIFramework.EasingMode.EaseInOut, GenericWindowAnimationType.Fade, GenericWindowAnimationType.Fade);
        controller.OpenScreen<UITKExampleTransitionScreen>(in transition);
    }

    private void TravelToUGUISharedCanvasScreen()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.Custom(0.5F, UIFramework.EasingMode.EaseInOut, GenericWindowAnimationType.Fade, GenericWindowAnimationType.Fade, WindowTransitionPlayable.SortPriority.Target);
        controller.OpenScreen<UGUIExampleSharedCanvasScreen>(in transition);
    }

    private void TravelToUGUIAltCanvasScreen()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.Custom(0.5F, UIFramework.EasingMode.EaseInOut, GenericWindowAnimationType.Fade, GenericWindowAnimationType.Expand, WindowTransitionPlayable.SortPriority.Target);
        controller.OpenScreen<UGUIExampleAlternateCanvasScreen>(in transition);
    }    
}
