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
        public bool unscaledTime { get; private set; }

        public AnimationPlayable(Animation animation)
            : this(animation, 0.0F, PlayMode.Forward, EasingMode.Linear, false, 1.0F) { }

        public AnimationPlayable(Animation animation, float startTime)
            : this(animation, startTime, PlayMode.Forward, EasingMode.Linear, false, 1.0F) { }

        public AnimationPlayable(Animation animation, float startTime, PlayMode playMode)
            : this(animation, startTime, playMode, EasingMode.Linear, false, 1.0F) { }

        public AnimationPlayable(Animation animation, float startTime, PlayMode playMode, EasingMode easingMode)
            : this(animation, startTime, playMode, easingMode, false, 1.0F) { }

        public AnimationPlayable(Animation animation, float startTime, PlayMode playMode, EasingMode easingMode, bool unscaledTime)
            : this(animation, startTime, playMode, easingMode, unscaledTime, 1.0F) { }

        public AnimationPlayable(Animation animation, float startTime, PlayMode playMode, EasingMode easingMode, bool unscaledTime, float playbackSpeed)
        {
            if(animation == null)
            {
                throw new ArgumentNullException(nameof(animation));
            }

            this.animation = animation;
            this.easingMode = easingMode;
            this.startTime = Mathf.Clamp(startTime, 0.0F, animation.length);
            this.playMode = playMode;
            this.unscaledTime = unscaledTime;
            this.playbackSpeed = playbackSpeed;
        }

        public AnimationPlayable CreateInverse(float startTime)
        {
            return new AnimationPlayable(animation, startTime, playMode.InvertPlayMode(), 
                easingMode.GetInverseEasingMode(), unscaledTime, playbackSpeed);
        }

        public AnimationPlayable CreateInverse()
        {
            return new AnimationPlayable(animation, animation.length - startTime, playMode.InvertPlayMode(), 
                easingMode.GetInverseEasingMode(), unscaledTime, playbackSpeed);
        }
    }
}
