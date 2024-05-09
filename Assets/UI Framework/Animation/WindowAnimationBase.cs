using System;

using UnityEngine;

namespace UIFramework
{
    /// <summary>
    /// Type of animation. Some assumptions are made.
    /// Fade, Dissolve, Flip, Expand: These are assumed to be from a inactive state to an active state.
    /// E.g, animating from an alpha of 0 to 1 for Fade.
    /// </summary>
    public enum WindowAnimationType
    {
        Fade,
        Dissolve,
        SlideFromLeft,
        SlideFromRight,
        SlideFromBottom,
        SlideFromTop,
        Flip,
        Expand
    }

    public abstract class WindowAnimationBase : Animation
    {
        public WindowAnimationType type { get; private set; } = WindowAnimationType.Fade;

        public WindowAnimationType fallbackType { get; private set; } = WindowAnimationType.Fade;

        protected WindowAnimationBase(WindowAnimationType type, float length)
            : base(length)
        {
            this.type = type;
            if(!IsSupportedType(this.type))
            {
                Debug.LogWarning(string.Format("WindowAnimation is unable to support type {0}, falling back to {1}.", this.type, fallbackType));
            }
        }

        protected WindowAnimationBase(WindowAnimationType type, WindowAnimationType fallbackType, float length)
            : base(length)
        {
            this.type = type;
            this.fallbackType = fallbackType;
            if (!IsSupportedType(this.type))
            {
                Debug.LogWarning(string.Format("WindowAnimation is unable to support type {0}, falling back to {1}.", this.type, this.fallbackType));
            }
        }

        public override void Evaluate(float normalisedTime)
        {
            WindowAnimationType evaluationType = IsSupportedType(type) ? type : fallbackType;
            switch (type)
            {
                case WindowAnimationType.Fade:
                    Fade(normalisedTime);
                    break;
                case WindowAnimationType.Dissolve:
                    Dissolve(normalisedTime);
                    break;
                case WindowAnimationType.SlideFromLeft:
                    SlideFromLeft(normalisedTime);
                    break;
                case WindowAnimationType.SlideFromRight:
                    SlideFromRight(normalisedTime);
                    break;
                case WindowAnimationType.SlideFromBottom:
                    SlideFromBottom(normalisedTime);
                    break;
                case WindowAnimationType.SlideFromTop:
                    SlideFromTop(normalisedTime);
                    break;
                case WindowAnimationType.Flip:
                    Flip(normalisedTime);
                    break;
                case WindowAnimationType.Expand:
                    Expand(normalisedTime);
                    break;
            }
            throw new NotImplementedException();
        }        

        protected abstract void Fade(float normalisedTime);
        protected abstract void Dissolve(float normalisedTime);
        protected abstract void SlideFromLeft(float normalisedTime);
        protected abstract void SlideFromRight(float normalisedTime);
        protected abstract void SlideFromBottom(float normalisedTime);
        protected abstract void SlideFromTop(float normalisedTime);
        protected abstract void Flip(float normalisedTime);
        protected abstract void Expand(float normalisedTime);
        protected abstract bool IsSupportedType(WindowAnimationType type);

        public virtual void ResetAnimatedComponents() { }
    }
}
