using UIFramework.UGUI;

public class UGUIExampleWindowAnimator : WindowAnimator
{
    public override void SlideFromLeft(float normalisedTime)
    {
        base.SlideFromLeft(normalisedTime);
        Fade(normalisedTime);
    }

    public override void SlideFromRight(float normalisedTime)
    {
        base.SlideFromRight(normalisedTime);
        Fade(normalisedTime);
    }

    public override void SlideFromBottom(float normalisedTime)
    {
        base.SlideFromBottom(normalisedTime);
        Fade(normalisedTime);
    }

    public override void SlideFromTop(float normalisedTime)
    {
        base.SlideFromTop(normalisedTime);
        Fade(normalisedTime);
    }
}
