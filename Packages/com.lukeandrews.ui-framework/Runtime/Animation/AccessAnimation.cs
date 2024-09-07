using UnityEngine.Extension;

namespace UIFramework
{
    public abstract class AccessAnimation : IAnimation
    {
        public virtual float Length { get; } = 1.0F;
        public AccessOperation AccessOperation { get; private set; } = AccessOperation.Open;

        protected AccessAnimation(AccessOperation accessOperation)
        {
            AccessOperation = accessOperation;
        }

        public abstract void Evaluate(float normalisedTime);
        public virtual void Prepare() { }

        public AccessAnimationPlayable CreatePlayable(AccessOperation accessOperation, EasingMode easingMode = EasingMode.Linear, TimeMode timeMode = TimeMode.Scaled)
        {
            PlaybackMode playbackMode = accessOperation == AccessOperation ? PlaybackMode.Forward : PlaybackMode.Reverse;
            float startTime = playbackMode == PlaybackMode.Forward ? 0.0F : Length;
            return new AccessAnimationPlayable(this, startTime, playbackMode, easingMode, timeMode, 1.0F);
        }

        public AccessAnimationPlayable CreatePlayable(AccessOperation accessOperation, float length, EasingMode easingMode = EasingMode.Linear, TimeMode timeMode = TimeMode.Scaled)
        {
            float playbackSpeed = Length / length;
            PlaybackMode playbackMode = accessOperation == AccessOperation ? PlaybackMode.Forward : PlaybackMode.Reverse;
            float startTime = playbackMode == PlaybackMode.Forward ? 0.0F : Length;
            return new AccessAnimationPlayable(this, startTime, playbackMode, easingMode, timeMode, playbackSpeed);
        }

        public AccessAnimationPlayable CreatePlayable(AccessOperation accessOperation, float length, float startOffset, EasingMode easingMode = EasingMode.Linear, TimeMode timeMode = TimeMode.Scaled)
        {
            float playbackSpeed = Length / length;
            PlaybackMode playbackMode = accessOperation == AccessOperation ? PlaybackMode.Forward : PlaybackMode.Reverse;
            float startTime = playbackMode == PlaybackMode.Forward ? startOffset : length - startOffset;
            return new AccessAnimationPlayable(this, startTime, playbackMode, easingMode, timeMode, playbackSpeed);
        }
    }
}