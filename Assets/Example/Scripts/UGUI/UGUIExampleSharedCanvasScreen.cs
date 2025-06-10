using UIFramework;

using UnityEngine;
using UnityEngine.Extension;
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

    protected override void OnInitialize()
    {
        base.OnInitialize();
        returnButton?.onClick.AddListener(delegate() { Controller.CloseScreen(); });
        travelToTransitionScreenButton?.onClick.AddListener(TravelToTransitionScreen);
        travelToAlternateCanvasScreenButton?.onClick.AddListener(TravelToAlternateCanvasScreen);
        travelToUITKTransitionScreenButton?.onClick.AddListener(TravelToUITKTransitionScreen);
    }

    private void TravelToTransitionScreen()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.Custom(0.5F, EasingMode.EaseInOut, GenericWindowAnimationType.Fade, GenericWindowAnimationType.SlideFromRight, TransitionAnimationParams.SortPriority.Target);
        Controller.OpenScreen<UGUIExampleTransitionScreen>(in transition);
    }

    private void TravelToAlternateCanvasScreen()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.Custom(0.5F, EasingMode.EaseInOut, GenericWindowAnimationType.Fade, GenericWindowAnimationType.Expand, TransitionAnimationParams.SortPriority.Target);
        Controller.OpenScreen<UGUIExampleAlternateCanvasScreen>(in transition);
    }

    private void TravelToUITKTransitionScreen()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.Custom(0.5F, EasingMode.EaseInOut, GenericWindowAnimationType.Fade, GenericWindowAnimationType.Fade);
        Controller.OpenScreen<UITKExampleTransitionScreen>(in transition);
    }
}