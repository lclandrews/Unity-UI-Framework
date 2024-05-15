namespace UIFramework
{
    public abstract class WindowAccessAnimation : Animation
    {
        public AccessOperation accessOperation { get; protected set; } = AccessOperation.Open;

        protected WindowAccessAnimation(AccessOperation accessOperation, float length) : base(length)
        {
            this.accessOperation = accessOperation;
        }

        public AnimationPlayable CreatePlayable(float length, AccessOperation accessOperation,
            float startOffset = 0.0F, EasingMode easingMode = EasingMode.Linear, TimeMode timeMode = TimeMode.Scaled)
        {
            PlayMode playMode = accessOperation == this.accessOperation ? PlayMode.Forward : PlayMode.Reverse;

            float startTime = playMode == PlayMode.Forward ? startOffset : length - startOffset;
            float playbackSpeed = this.length / length;
            return new AnimationPlayable(this, startTime, playMode, easingMode, timeMode, playbackSpeed);
        }
    }
}