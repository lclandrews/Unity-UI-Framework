namespace UIFramework
{
    public delegate void WindowAnimatorEvent(IWindowAnimator animator);

    public interface IWindowAnimator
    {
        WindowAnimation.Type type { get; }
        PlayMode playMode { get; }
        EasingMode easingMode { get; }
        float currentTime { get; }
        float currentNormalisedTime { get; }        
        float remainingTime { get; }

        bool isPlaying { get; }        

        WindowAnimatorEvent onComplete { get; set; }

        WindowAnimation.Type fallbackType { get; }

        void Play(in WindowAnimation animation);

        bool Rewind();

        bool Stop();

        bool Complete();

        bool SetCurrentTime(float time);

        bool IsSupportedType(WindowAnimation.Type type);

        void Update(float deltaTime);

        void ResetAnimatedComponents();
    }
}
