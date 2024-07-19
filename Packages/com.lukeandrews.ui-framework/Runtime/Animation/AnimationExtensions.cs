namespace UIFramework
{
    public static class AnimationExtensions
    {
        public static PlayMode InvertPlayMode(this PlayMode playMode)
        {
            return playMode ^ (PlayMode)1;
        }

        public static AccessOperation InvertAccessOperation(this AccessOperation accessOperation)
        {
            return accessOperation ^ (AccessOperation)1;
        }

        public static AnimationPlayable CreatePlayable(this GenericWindowAnimationType type, IWindow window, float length, AccessOperation accessOperation,
            float startOffset = 0.0F, EasingMode easingMode = EasingMode.Linear, TimeMode timeMode = TimeMode.Scaled)
        {
            PlayMode playMode = accessOperation == AccessOperation.Open ? PlayMode.Forward : PlayMode.Reverse;
            float startTime = playMode == PlayMode.Forward ? startOffset : length - startOffset;
            Animation animation = window.CreateAnimation(type, length);
            return new AnimationPlayable(animation, startTime, playMode, easingMode, timeMode);
        }

        public static AnimationPlayable CreatePlayable(this IWindow window, GenericWindowAnimationType type, float length, AccessOperation accessOperation,
            float startOffset = 0.0F, EasingMode easingMode = EasingMode.Linear, TimeMode timeMode = TimeMode.Scaled)
        {
            PlayMode playMode = accessOperation == AccessOperation.Open ? PlayMode.Forward : PlayMode.Reverse;
            float startTime = playMode == PlayMode.Forward ? startOffset : length - startOffset;
            Animation animation = window.CreateAnimation(type, length);
            return new AnimationPlayable(animation, startTime, playMode, easingMode, timeMode);
        }
    }
}