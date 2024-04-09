using UIFramework;
using UIFramework.UGUI;

using UnityEngine;
using UnityEngine.UI;

public class UGUIExampleAlternateCanvasScreen : Screen<ExampleController>
{
    [SerializeField] private Button returnButton = null;
    [SerializeField] private Button travelToTransitionScreenButton = null;
    [SerializeField] private Button travelToSharedCanvasScreenButton = null;
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
        travelToSharedCanvasScreenButton?.onClick.AddListener(TravelToSharedCanvasScreen);
        travelToUITKTransitionScreenButton?.onClick.AddListener(TravelToUITKTransitionScreen);
    }

    private void TravelToTransitionScreen()
    {
        ScreenTransition transition = ScreenTransition.Custom(0.5F, EasingMode.EaseInOut, WindowAnimation.Type.Flip, WindowAnimation.Type.Fade, ScreenTransition.SortPriority.Source);
        controller.navigation.Travel<UGUIExampleTransitionScreen>(in transition, null);
    }

    private void TravelToSharedCanvasScreen()
    {
        ScreenTransition transition = ScreenTransition.Custom(0.5F, EasingMode.EaseInOut, WindowAnimation.Type.Fade, WindowAnimation.Type.Fade, ScreenTransition.SortPriority.Target);
        controller.navigation.Travel<UGUIExampleSharedCanvasScreen>(in transition, null);
    }

    private void TravelToUITKTransitionScreen()
    {
        ScreenTransition transition = ScreenTransition.Custom(0.5F, EasingMode.EaseInOut, WindowAnimation.Type.Fade, WindowAnimation.Type.Fade);
        controller.navigation.Travel<UITKExampleTransitionScreen>(in transition, null);
    }
}
