using System;

using UnityEngine;

namespace UIFramework
{
    public struct WindowAccessPlayable
    {
        public ImplicitWindowAnimation animation { get; private set; }
        public float length { get; private set; }
        public float startTime { get; private set; }
        public PlayMode playMode { get; private set; }
        public EasingMode easingMode { get; private set; }
        public float playbackSpeed { get; private set; }
        public bool unscaledTime { get; private set; }

        public WindowAccessPlayable(ImplicitWindowAnimation animation, float length)
            : this(animation, length, 0.0F, PlayMode.Forward, EasingMode.Linear, false, 1.0F) { }

        public WindowAccessPlayable(ImplicitWindowAnimation animation, float length, float startTime)
            : this(animation, length, startTime, PlayMode.Forward, EasingMode.Linear, false, 1.0F) { }

        public WindowAccessPlayable(ImplicitWindowAnimation animation, float length, float startTime, PlayMode playMode)
            : this(animation, length, startTime, playMode, EasingMode.Linear, false, 1.0F) { }

        public WindowAccessPlayable(ImplicitWindowAnimation animation, float length, float startTime, PlayMode playMode, EasingMode easingMode)
            : this(animation, length, startTime, playMode, easingMode, false, 1.0F) { }

        public WindowAccessPlayable(ImplicitWindowAnimation animation, float length, float startTime, PlayMode playMode, EasingMode easingMode, bool unscaledTime)
            : this(animation, length, startTime, playMode, easingMode, unscaledTime, 1.0F) { }

        public WindowAccessPlayable(ImplicitWindowAnimation animation, float length, float startTime, PlayMode playMode, EasingMode easingMode, bool unscaledTime, float playbackSpeed)
        {
            if (animation == null)
            {
                throw new ArgumentNullException(nameof(animation));
            }

            this.length = length;
            this.animation = animation;
            this.easingMode = easingMode;
            this.startTime = Mathf.Clamp(startTime, 0.0F, this.length);
            this.playMode = playMode;
            this.unscaledTime = unscaledTime;
            this.playbackSpeed = playbackSpeed;
        }

        public AnimationPlayable CreatePlayable(IWindow window)
        {
            return animation.CreatePlayable(window, length, startTime, playMode, easingMode, unscaledTime);
        }
    }
}
