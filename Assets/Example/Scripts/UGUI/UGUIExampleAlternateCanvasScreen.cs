using UIFramework;

using UnityEngine;
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

    protected override void OnInit()
    {
        base.OnInit();
        returnButton?.onClick.AddListener(delegate() { Controller.CloseScreen(); });
        travelToTransitionScreenButton?.onClick.AddListener(TravelToTransitionScreen);
        travelToSharedCanvasScreenButton?.onClick.AddListener(TravelToSharedCanvasScreen);
        travelToUITKTransitionScreenButton?.onClick.AddListener(TravelToUITKTransitionScreen);
    }

    private void TravelToTransitionScreen()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.Custom(0.5F, EasingMode.EaseInOut, GenericWindowAnimationType.Flip, GenericWindowAnimationType.Fade, WindowTransitionPlayable.SortPriority.Source);
        Controller.OpenScreen<UGUIExampleTransitionScreen>(in transition);
    }

    private void TravelToSharedCanvasScreen()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.Custom(0.5F, EasingMode.EaseInOut, GenericWindowAnimationType.Fade, GenericWindowAnimationType.Fade, WindowTransitionPlayable.SortPriority.Target);
        Controller.OpenScreen<UGUIExampleSharedCanvasScreen>(in transition);
    }

    private void TravelToUITKTransitionScreen()
    {
        WindowTransitionPlayable transition = WindowTransitionPlayable.Custom(0.5F, EasingMode.EaseInOut, GenericWindowAnimationType.Fade, GenericWindowAnimationType.Fade);
        Controller.OpenScreen<UITKExampleTransitionScreen>(in transition);
    }
}
