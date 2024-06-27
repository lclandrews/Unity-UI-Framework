using UIFramework;

using UnityEngine;
using UnityEngine.UI;

public class UGUIExampleSharedCanvasScreen : UIFramework.UGUI.Screen
{   
    [SerializeField] private Button returnButton = null;
    [SerializeField] private Button travelToTransitionScreenButton = null;
    [SerializeField] private Button travelToAlternateCanvasScreenButton = null;
    [SerializeField] private Button travelToUITKTransitionScreenButton = null;

    public override bool IsValidData(object data)
    {
        return false;
    }

    protected override void OnInit()
    {
        base.OnInit();
        returnButton?.onClick.AddListener(delegate() { Controller.CloseScreen(); });
        travelToTransitionScreenButton?.onClick.AddListener(TravelToTransitionScreen);
        travelToAlternateCanvasScreenButton?.onClick.AddListener(TravelToAlternateCanvasScreen);
        travelToUITKTransitionScreenButton?.onClick.AddListener(TravelToUITKTransitionScreen);
    }

    private void TravelToTransitionScreen()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.Custom(0.5F, EasingMode.EaseInOut, GenericWindowAnimationType.Fade, GenericWindowAnimationType.SlideFromRight, WindowTransitionPlayable.SortPriority.Target);
        Controller.OpenScreen<UGUIExampleTransitionScreen>(in transition);
    }

    private void TravelToAlternateCanvasScreen()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.Custom(0.5F, EasingMode.EaseInOut, GenericWindowAnimationType.Fade, GenericWindowAnimationType.Expand, WindowTransitionPlayable.SortPriority.Target);
        Controller.OpenScreen<UGUIExampleAlternateCanvasScreen>(in transition);
    }

    private void TravelToUITKTransitionScreen()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.Custom(0.5F, EasingMode.EaseInOut, GenericWindowAnimationType.Fade, GenericWindowAnimationType.Fade);
        Controller.OpenScreen<UITKExampleTransitionScreen>(in transition);
    }
}