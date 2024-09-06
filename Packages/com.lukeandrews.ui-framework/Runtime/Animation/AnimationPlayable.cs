using System;

using UnityEngine;

namespace UIFramework
{    
    public struct AnimationPlayable
    {       
        public IAnimation Animation { get; private set; }        
        public float StartTime { get; private set; }
        public PlayMode PlayMode { get; private set; }
        public EasingMode EasingMode { get; private set; }
        public float PlaybackSpeed { get; private set; }
        public TimeMode TimeMode { get; private set; }

        public AnimationPlayable(IAnimation animation, float startTime, PlayMode playMode, EasingMode easingMode, TimeMode timeMode, float playbackSpeed)
        {
            if(animation == null)
            {
                throw new ArgumentNullException(nameof(animation));
            }

            Animation = animation;
            EasingMode = easingMode;
            StartTime = Mathf.Clamp(startTime, 0.0F, animation.Length);
            PlayMode = playMode;
            TimeMode = timeMode;
            PlaybackSpeed = playbackSpeed;
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
