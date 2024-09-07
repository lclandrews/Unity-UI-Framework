using System;

using UnityEngine;
using UnityEngine.Extension;

namespace UIFramework
{    
    public struct AccessAnimationPlayable
    {       
        public AccessAnimation Animation { get; private set; }        
        public float StartTime { get; private set; }
        public PlaybackMode PlaybackMode { get; private set; }
        public EasingMode EasingMode { get; private set; }
        public float PlaybackSpeed { get; private set; }
        public TimeMode TimeMode { get; private set; }

        public AccessAnimationPlayable(AccessAnimation animation, float startTime, PlaybackMode playbackMode, EasingMode easingMode, TimeMode timeMode, float playbackSpeed)
        {
            if(animation == null)
            {
                throw new ArgumentNullException(nameof(animation));
            }

            Animation = animation;
            EasingMode = easingMode;
            StartTime = Mathf.Clamp(startTime, 0.0F, animation.Length);
            PlaybackMode = playbackMode;
            TimeMode = timeMode;
            PlaybackSpeed = playbackSpeed;
        }

        public void Invert()
        {
            StartTime = Animation.Length - StartTime;
            PlaybackMode = PlaybackMode.Invert();
        }

        public AccessAnimationPlayable CreateInverse(float startTime)
        {
            return new AccessAnimationPlayable(Animation, startTime, PlaybackMode.Invert(), EasingMode.GetInverseEasingMode(), TimeMode, PlaybackSpeed);
        }

        public AccessAnimationPlayable CreateInverse()
        {
            return new AccessAnimationPlayable(Animation, Animation.Length - StartTime, PlaybackMode.Invert(), EasingMode.GetInverseEasingMode(), TimeMode, PlaybackSpeed);
        }
    }
}
