using UIFramework;
using UIFramework.UGUI;

using UnityEngine;
using UnityEngine.UI;

public class UGUIExampleSharedCanvasScreen : Screen<ExampleController>
{
    [SerializeField] private Button returnButton = null;
    [SerializeField] private Button travelToTransitionScreenButton = null;
    [SerializeField] private Button travelToAlternateCanvasScreenButton = null;
    [SerializeField] private Button travelToUITKTransitionScreenButton = null;

    public override bool requiresData { get { return false; } }

    public override bool IsValidData(object data)
    {
        return false;
    }

    protected override void OnInit()
    {
        base.OnInit();
        returnButton?.onClick.AddListener(controller.navigation.Back);
        travelToTransitionScreenButton?.onClick.AddListener(TravelToTransitionScreen);
        travelToAlternateCanvasScreenButton?.onClick.AddListener(TravelToAlternateCanvasScreen);
        travelToUITKTransitionScreenButton?.onClick.AddListener(TravelToUITKTransitionScreen);
    }

    private void TravelToTransitionScreen()
    {
        WindowTransition transition = WindowTransition.Custom(0.5F, EasingMode.EaseInOut, WindowAnimation.Type.Fade, WindowAnimation.Type.SlideFromRight, WindowTransition.SortPriority.Target);
        controller.navigation.Travel<UGUIExampleTransitionScreen>(in transition, null);
    }

    private void TravelToAlternateCanvasScreen()
    {
        WindowTransition transition = WindowTransition.Custom(0.5F, EasingMode.EaseInOut, WindowAnimation.Type.Fade, WindowAnimation.Type.Expand, WindowTransition.SortPriority.Target);
        controller.navigation.Travel<UGUIExampleAlternateCanvasScreen>(in transition, null);
    }

    private void TravelToUITKTransitionScreen()
    {
        WindowTransition transition = WindowTransition.Custom(0.5F, EasingMode.EaseInOut, WindowAnimation.Type.Fade, WindowAnimation.Type.Fade);
        controller.navigation.Travel<UITKExampleTransitionScreen>(in transition, null);
    }
}