using System;

using UnityEngine;

namespace UIFramework
{    
    public struct AnimationPlayable
    {       
        public Animation Animation { get; private set; }        
        public float StartTime { get; private set; }
        public PlayMode PlayMode { get; private set; }
        public EasingMode EasingMode { get; private set; }
        public float PlaybackSpeed { get; private set; }
        public TimeMode TimeMode { get; private set; }

        public AnimationPlayable(Animation animation)
            : this(animation, 0.0F, PlayMode.Forward, EasingMode.Linear, TimeMode.Scaled, 1.0F) { }

        public AnimationPlayable(Animation animation, float startTime)
            : this(animation, startTime, PlayMode.Forward, EasingMode.Linear, TimeMode.Scaled, 1.0F) { }

        public AnimationPlayable(Animation animation, float startTime, PlayMode playMode)
            : this(animation, startTime, playMode, EasingMode.Linear, TimeMode.Scaled, 1.0F) { }

        public AnimationPlayable(Animation animation, float startTime, PlayMode playMode, EasingMode easingMode)
            : this(animation, startTime, playMode, easingMode, TimeMode.Scaled, 1.0F) { }

        public AnimationPlayable(Animation animation, float startTime, PlayMode playMode, EasingMode easingMode, TimeMode timeMode)
            : this(animation, startTime, playMode, easingMode, timeMode, 1.0F) { }

        public AnimationPlayable(Animation animation, float startTime, PlayMode playMode, EasingMode easingMode, TimeMode timeMode, float playbackSpeed)
        {
            if(animation == null)
            {
                throw new ArgumentNullException(nameof(animation));
            }

            this.Animation = animation;
            this.EasingMode = easingMode;
            this.StartTime = Mathf.Clamp(startTime, 0.0F, animation.Length);
            this.PlayMode = playMode;
            this.TimeMode = timeMode;
            this.PlaybackSpeed = playbackSpeed;
        }

        public void Invert()
        {
            StartTime = Animation.Length - StartTime;
            PlayMode = PlayMode.InvertPlayMode();
        }

        public AnimationPlayable CreateInverse(float startTime)
        {
            return new AnimationPlayable(Animation, startTime, PlayMode.InvertPlayMode(), 
                EasingMode.GetInverseEasingMode(), TimeMode, PlaybackSpeed);
        }

        public AnimationPlayable CreateInverse()
        {
            return new AnimationPlayable(Animation, Animation.Length - StartTime, PlayMode.InvertPlayMode(), 
                EasingMode.GetInverseEasingMode(), TimeMode, PlaybackSpeed);
        }
    }
}
