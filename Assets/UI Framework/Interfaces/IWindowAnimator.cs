namespace UIFramework
{
    public delegate void WindowAnimatorEvent(IWindowAnimator animator);

    public interface IWindowAnimator
    {
        WindowAnimation animation { get; }
        PlayMode playMode { get; }
        float currentTime { get; }
        float currentNormalisedTime { get; }        
        float remainingTime { get; }

        bool isPlaying { get; }        

        WindowAnimatorEvent onAnimationComplete { get; set; }

        WindowAnimation.Type fallbackAnimationType { get; }

        void Play(in WindowAnimation animation);

        bool Rewind();

        bool Stop();

        bool SetCurrentTime(float time);

        bool IsSupportedAnimationType(WindowAnimation.Type animationType);

        void Update(float deltaTime);

        void ResetAnimatedComponents();
    }
}
