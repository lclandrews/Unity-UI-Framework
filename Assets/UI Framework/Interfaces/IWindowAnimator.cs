namespace UIFramework
{
    public delegate void AnimatorEvent(IWindowAnimator animator);

    public interface IWindowAnimator
    {
        public enum PlayMode
        {
            Forward,
            Reverse
        }

        WindowAnimation animation { get; }
        PlayMode playMode { get; }
        float currentTime { get; }
        float currentNormalisedTime { get; }        
        float remainingTime { get; }

        bool isPlaying { get; }        

        AnimatorEvent onAnimationComplete { get; set; }

        WindowAnimation.Type fallbackAnimationType { get; }

        void Play(in WindowAnimation animation, float startTime = 0.0F, PlayMode playMode = PlayMode.Forward);

        void Rewind();

        bool Stop();

        void SetCurrentTime(float time);

        bool IsSupportedAnimationType(WindowAnimation.Type animationType);

        void Update(float deltaTime);        
    }
}
