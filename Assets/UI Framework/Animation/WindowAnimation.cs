using System;

using UnityEngine;

namespace UIFramework
{
    public enum PlayMode
    {
        Forward,
        Reverse
    }

    public struct WindowAnimation
    {
        /// <summary>
        /// Type of animation. Some assumptions are made.
        /// Fade, Dissolve, Flip, Expand: These are assumed to be from a inactive state to an active state.
        /// E.g, animating from an alpha of 0 to 1 for Fade.
        /// </summary>
        public enum Type
        {
            Fade,
            Dissolve,
            SlideFromLeft,
            SlideFromRight,
            SlideFromBottom,
            SlideFromTop,
            Flip,
            Expand,
            Custom
        }

        public ICustomWindowAnimation customAnimation { get; private set; }
        public Type type { get; private set; }
        public float length { get; private set; }
        public EasingMode easingMode { get; private set; }
        public float startTime { get; private set; }
        public PlayMode playMode { get; private set; }

        public WindowAnimation(Type type, float length)
            : this(type, length, EasingMode.Linear, 0.0F, PlayMode.Forward) { }

        public WindowAnimation(Type type, float length, float startTime)
            : this(type, length, EasingMode.Linear, startTime, PlayMode.Forward) { }

        public WindowAnimation(Type type, float length, float startTime, PlayMode playMode)
            : this(type, length, EasingMode.Linear, startTime, playMode) { }

        public WindowAnimation(Type type, float length, EasingMode easingMode)
            : this(type, length, easingMode, 0.0F, PlayMode.Forward) { }

        public WindowAnimation(Type type, float length, EasingMode easingMode, float startTime)
            : this(type, length, easingMode, startTime, PlayMode.Forward) { }

        public WindowAnimation(Type type, float length, EasingMode easingMode, float startTime, PlayMode playMode)
        {
            if(type == Type.Custom)
            {
                throw new InvalidOperationException("Unabled to create a WindowAnimation of type Custom without providing an instance of ICustomWindowAnimation");
            }

            customAnimation = null;
            this.type = type;
            this.length = Mathf.Max(length, 0.0F);
            this.easingMode = easingMode;
            this.startTime = Mathf.Clamp(startTime, 0.0F, length);
            this.playMode = playMode;
        }

        public WindowAnimation(ICustomWindowAnimation customAnimation, float length)
            : this(customAnimation, length, EasingMode.Linear, 0.0F, PlayMode.Forward) { }

        public WindowAnimation(ICustomWindowAnimation customAnimation, float length, float startTime)
            : this(customAnimation, length, EasingMode.Linear, startTime, PlayMode.Forward) { }

        public WindowAnimation(ICustomWindowAnimation customAnimation, float length, float startTime, PlayMode playMode)
            : this(customAnimation, length, EasingMode.Linear, startTime, playMode) { }

        public WindowAnimation(ICustomWindowAnimation customAnimation, float length, EasingMode easingMode)
            : this(customAnimation, length, easingMode, 0.0F, PlayMode.Forward) { }

        public WindowAnimation(ICustomWindowAnimation customAnimation, float length, EasingMode easingMode, float startTime)
            : this(customAnimation, length, easingMode, startTime, PlayMode.Forward) { }

        public WindowAnimation(ICustomWindowAnimation customAnimation, float length, EasingMode easingMode, float startTime, PlayMode playMode)
        {
            this.customAnimation = customAnimation;
            this.type = Type.Custom;
            this.length = Mathf.Max(length, 0.0F);
            this.easingMode = easingMode;
            this.startTime = Mathf.Clamp(startTime, 0.0F, length);
            this.playMode = playMode;
        }

        public WindowAnimation CreateInverseAnimation(float startTime)
        {
            return new WindowAnimation(type, length, Easing.GetInverseEasingMode(easingMode), startTime, InvertPlayMode(playMode));
        }

        public WindowAnimation CreateInverseAnimation()
        {
            return new WindowAnimation(type, length, Easing.GetInverseEasingMode(easingMode), length - startTime, InvertPlayMode(playMode));
        }

        public static PlayMode InvertPlayMode(PlayMode playMode)
        {
            return playMode ^ (PlayMode)1;
        }
    }
}
