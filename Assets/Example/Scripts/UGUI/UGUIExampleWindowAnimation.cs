using UIFramework;
using UIFramework.UGUI;

using UnityEngine;

public class UGUIExampleWindowAnimation : UGUIGenericWindowAnimation
{
    public UGUIExampleWindowAnimation(RectTransform displayRectTransform, RectTransform rectTransform, Vector3 activeAnchoredPosition, CanvasGroup canvasGroup, GenericWindowAnimationType type, float length)
        : base(displayRectTransform, rectTransform, activeAnchoredPosition, canvasGroup, type) { }

    protected override void SlideFromLeft(float normalisedTime)
    {
        base.SlideFromLeft(normalisedTime);
        Fade(normalisedTime);
    }

    protected override void SlideFromRight(float normalisedTime)
    {
        base.SlideFromRight(normalisedTime);
        Fade(normalisedTime);
    }

    protected override void SlideFromBottom(float normalisedTime)
    {
        base.SlideFromBottom(normalisedTime);
        Fade(normalisedTime);
    }

    protected override void SlideFromTop(float normalisedTime)
    {
        base.SlideFromTop(normalisedTime);
        Fade(normalisedTime);
    }
}
