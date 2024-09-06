namespace UIFramework
{   
    public abstract class Animation : IAnimation
    {
        public virtual float Length { get; protected set; } = 0.0F;

        private Animation() { }

        protected Animation(float length)
        {
            Length = length;
        }

        public abstract void Evaluate(float normalisedTime);
        public virtual void Prepare() { }

        public AnimationPlayable CreatePlayable(EasingMode easingMode = EasingMode.Linear, TimeMode timeMode = TimeMode.Scaled)
        {
            return new AnimationPlayable(this, 0.0F, PlayMode.Forward, easingMode, timeMode, 1.0F);
        }

        public AnimationPlayable CreatePlayable(float length, EasingMode easingMode = EasingMode.Linear, TimeMode timeMode = TimeMode.Scaled)
        {
            float playbackSpeed = Length / length;
            return new AnimationPlayable(this, 0.0F, PlayMode.Forward, easingMode, timeMode, playbackSpeed);
        }

        public AnimationPlayable CreatePlayable(float length, PlayMode playMode, EasingMode easingMode = EasingMode.Linear, TimeMode timeMode = TimeMode.Scaled)
        {
            float playbackSpeed = Length / length;
            float startTime = playMode == PlayMode.Forward ? 0.0F : Length;
            return new AnimationPlayable(this, startTime, playMode, easingMode, timeMode, playbackSpeed);
        }

        public AnimationPlayable CreatePlayable(float length, float startOffset, PlayMode playMode, EasingMode easingMode = EasingMode.Linear, TimeMode timeMode = TimeMode.Scaled)
        {
            float playbackSpeed = Length / length;
            float startTime = playMode == PlayMode.Forward ? startOffset : length - startOffset;
            return new AnimationPlayable(this, startTime, playMode, easingMode, timeMode, playbackSpeed);
        }
    }
}