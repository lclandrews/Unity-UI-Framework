namespace UIFramework
{
    public abstract class WindowAccessAnimation : Animation
    {
        public AccessOperation AccessOperation { get; private set; } = AccessOperation.Open;

        protected WindowAccessAnimation(AccessOperation accessOperation, float length) : base(length)
        {
            this.AccessOperation = accessOperation;
        }

        public AnimationPlayable CreatePlayable(EasingMode easingMode = EasingMode.Linear, TimeMode timeMode = TimeMode.Scaled)
        {
            return new AnimationPlayable(this, 0.0F, PlayMode.Forward, easingMode, timeMode, 1.0F);
        }

        public AnimationPlayable CreatePlayable(AccessOperation accessOperation, EasingMode easingMode = EasingMode.Linear, TimeMode timeMode = TimeMode.Scaled)
        {
            PlayMode playMode = accessOperation == this.AccessOperation ? PlayMode.Forward : PlayMode.Reverse;
            float startTime = playMode == PlayMode.Forward ? 0.0F : Length;
            return new AnimationPlayable(this, startTime, playMode, easingMode, timeMode, 1.0F);
        }
    }
}