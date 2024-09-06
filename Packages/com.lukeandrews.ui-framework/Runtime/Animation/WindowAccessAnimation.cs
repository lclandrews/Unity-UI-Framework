using System;

namespace UIFramework
{
    public abstract class WindowAccessAnimation : IAnimation
    {
        public virtual float Length { get; } = 1.0F;
        public AccessOperation AccessOperation { get; private set; } = AccessOperation.Open;

        protected WindowAccessAnimation(AccessOperation accessOperation)
        {
            AccessOperation = accessOperation;
        }

        public abstract void Evaluate(float normalisedTime);
        public virtual void Prepare() { }

        public AnimationPlayable CreatePlayable(AccessOperation accessOperation, EasingMode easingMode = EasingMode.Linear, TimeMode timeMode = TimeMode.Scaled)
        {
            PlayMode playMode = accessOperation == AccessOperation ? PlayMode.Forward : PlayMode.Reverse;
            float startTime = playMode == PlayMode.Forward ? 0.0F : Length;
            return new AnimationPlayable(this, startTime, playMode, easingMode, timeMode, 1.0F);
        }

        public AnimationPlayable CreatePlayable(AccessOperation accessOperation, float length, EasingMode easingMode = EasingMode.Linear, TimeMode timeMode = TimeMode.Scaled)
        {
            float playbackSpeed = Length / length;
            PlayMode playMode = accessOperation == AccessOperation ? PlayMode.Forward : PlayMode.Reverse;
            float startTime = playMode == PlayMode.Forward ? 0.0F : Length;
            return new AnimationPlayable(this, startTime, playMode, easingMode, timeMode, playbackSpeed);
        }

        public AnimationPlayable CreatePlayable(AccessOperation accessOperation, float length, float startOffset, EasingMode easingMode = EasingMode.Linear, TimeMode timeMode = TimeMode.Scaled)
        {
            float playbackSpeed = Length / length;
            PlayMode playMode = accessOperation == AccessOperation ? PlayMode.Forward : PlayMode.Reverse;
            float startTime = playMode == PlayMode.Forward ? startOffset : length - startOffset;
            return new AnimationPlayable(this, startTime, playMode, easingMode, timeMode, playbackSpeed);
        }
    }
}