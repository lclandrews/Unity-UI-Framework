using System;

using UnityEngine;

namespace UIFramework
{    
    public struct AnimationPlayable
    {       
        public Animation animation { get; private set; }        
        public float startTime { get; private set; }
        public PlayMode playMode { get; private set; }
        public EasingMode easingMode { get; private set; }
        public float playbackSpeed { get; private set; }
        public TimeMode timeMode { get; private set; }

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

            this.animation = animation;
            this.easingMode = easingMode;
            this.startTime = Mathf.Clamp(startTime, 0.0F, animation.length);
            this.playMode = playMode;
            this.timeMode = timeMode;
            this.playbackSpeed = playbackSpeed;
        }

        public void Invert()
        {
            startTime = animation.length - startTime;
            playMode = playMode.InvertPlayMode();
        }

        public AnimationPlayable CreateInverse(float startTime)
        {
            return new AnimationPlayable(animation, startTime, playMode.InvertPlayMode(), 
                easingMode.GetInverseEasingMode(), timeMode, playbackSpeed);
        }

        public AnimationPlayable CreateInverse()
        {
            return new AnimationPlayable(animation, animation.length - startTime, playMode.InvertPlayMode(), 
                easingMode.GetInverseEasingMode(), timeMode, playbackSpeed);
        }
    }
}
