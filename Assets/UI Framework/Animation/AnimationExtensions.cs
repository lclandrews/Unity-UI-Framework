namespace UIFramework
{
    public static class AnimationExtensions
    {
        public static PlayMode InvertPlayMode(this PlayMode playMode)
        {
            return playMode ^ (PlayMode)1;
        }

        public static AnimationPlayer PlayAnimation(this IUIBehaviour behaviour, in AnimationPlayable animationPlayable)
        {
            return PlayAnimation(behaviour, animationPlayable.animation, animationPlayable.startTime,
                animationPlayable.playMode, animationPlayable.easingMode, animationPlayable.unscaledTime, 
                animationPlayable.playbackSpeed);
        }

        public static AnimationPlayer PlayAnimation(this IUIBehaviour behaviour, Animation animation, 
            float startTime = 0.0F, PlayMode playMode = PlayMode.Forward, EasingMode easingMode = EasingMode.Linear, bool unscaledTime = false, 
            float playbackSpeed = 1.0F)
        {
            AnimationPlayer player = new AnimationPlayer(animation);
            player.Play(startTime, playMode, easingMode, unscaledTime);
            return player;
        }

        public static AnimationPlayable CreatePlayable(this IWindow window, WindowAnimationType type, float length,
            float startTime = 0.0F, PlayMode playMode = PlayMode.Forward, EasingMode easingMode = EasingMode.Linear, bool unscaledTime = false)
        {
            Animation animation = window.CreateAnimation(type, length);
            return new AnimationPlayable(animation, startTime, playMode, easingMode, unscaledTime);
        }
    }
}