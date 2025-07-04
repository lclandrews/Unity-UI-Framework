using UIFramework;

using UnityEngine;
using UnityEngine.Extension;
using UnityEngine.UI;

public class UGUIExampleAlternateCanvasScreen : UIFramework.UGUI.Screen
{
    [SerializeField] private Button returnButton = null;
    [SerializeField] private Button travelToTransitionScreenButton = null;
    [SerializeField] private Button travelToSharedCanvasScreenButton = null;
    [SerializeField] private Button travelToUITKTransitionScreenButton = null;

    public override bool IsValidData(object data)
    {
        return false;
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        returnButton?.onClick.AddListener(Controller.CloseScreen);
        travelToTransitionScreenButton?.onClick.AddListener(TravelToTransitionScreen);
        travelToSharedCanvasScreenButton?.onClick.AddListener(TravelToSharedCanvasScreen);
        travelToUITKTransitionScreenButton?.onClick.AddListener(TravelToUITKTransitionScreen);
    }

    protected override void OnTerminate()
    {
        base.OnTerminate();
        returnButton?.onClick.RemoveListener(Controller.CloseScreen);
        travelToTransitionScreenButton?.onClick.RemoveListener(TravelToTransitionScreen);
        travelToSharedCanvasScreenButton?.onClick.RemoveListener(TravelToSharedCanvasScreen);
        travelToUITKTransitionScreenButton?.onClick.RemoveListener(TravelToUITKTransitionScreen);
    }
    
    private void TravelToTransitionScreen()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.Custom(0.5F, EasingMode.EaseInOut, GenericWindowAnimationType.Flip, GenericWindowAnimationType.Fade, TransitionAnimationParams.SortPriority.Source);
        Controller.OpenScreen<UGUIExampleTransitionScreen>(in transition);
    }

    private void TravelToSharedCanvasScreen()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.Custom(0.5F, EasingMode.EaseInOut, GenericWindowAnimationType.Fade, GenericWindowAnimationType.Fade, TransitionAnimationParams.SortPriority.Target);
        Controller.OpenScreen<UGUIExampleSharedCanvasScreen>(in transition);
    }

    private void TravelToUITKTransitionScreen()
    {
        TransitionAnimationParams transition = TransitionAnimationParams.Custom(0.5F, EasingMode.EaseInOut, GenericWindowAnimationType.Fade, GenericWindowAnimationType.Fade);
        Controller.OpenScreen<UITKExampleTransitionScreen>(in transition);
    }
}
