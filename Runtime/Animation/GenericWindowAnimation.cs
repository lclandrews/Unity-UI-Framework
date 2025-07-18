using System;

using UnityEngine;

namespace UIFramework
{
    /// <summary>
    /// Type of animation. Some assumptions are made.
    /// Fade, Flip, Expand: These are assumed to be from a inactive state to an active state.
    /// E.g, animating from an alpha of 0 to 1 for Fade.
    /// </summary>
    public enum GenericWindowAnimationType
    {
        Fade,
        SlideFromLeft,
        SlideFromRight,
        SlideFromBottom,
        SlideFromTop,
        Flip,
        Expand
    }

    public abstract class GenericWindowAnimation : AccessAnimation
    {
        public GenericWindowAnimationType Type { get; private set; } = GenericWindowAnimationType.Fade;

        public GenericWindowAnimationType FallbackType { get; private set; } = GenericWindowAnimationType.Fade;

        protected GenericWindowAnimation(GenericWindowAnimationType type) : base(AccessOperation.Open)
        {
            Type = type;
            if(!IsSupportedType(Type))
            {
                Debug.LogWarning(string.Format("WindowAnimation is unable to support type {0}, falling back to {1}.", Type, FallbackType));
            }
        }

        protected GenericWindowAnimation(GenericWindowAnimationType type, GenericWindowAnimationType fallbackType) : base(AccessOperation.Open)
        {
            Type = type;
            FallbackType = fallbackType;
            if (!IsSupportedType(Type))
            {
                Debug.LogWarning(string.Format("WindowAnimation is unable to support type {0}, falling back to {1}.", Type, FallbackType));
            }
        }

        public override void Evaluate(float normalisedTime)
        {
            GenericWindowAnimationType evaluationType = IsSupportedType(Type) ? Type : FallbackType;
            switch (Type)
            {
                case GenericWindowAnimationType.Fade:
                    Fade(normalisedTime);
                    break;
                case GenericWindowAnimationType.SlideFromLeft:
                    SlideFromLeft(normalisedTime);
                    break;
                case GenericWindowAnimationType.SlideFromRight:
                    SlideFromRight(normalisedTime);
                    break;
                case GenericWindowAnimationType.SlideFromBottom:
                    SlideFromBottom(normalisedTime);
                    break;
                case GenericWindowAnimationType.SlideFromTop:
                    SlideFromTop(normalisedTime);
                    break;
                case GenericWindowAnimationType.Flip:
                    Flip(normalisedTime);
                    break;
                case GenericWindowAnimationType.Expand:
                    Expand(normalisedTime);
                    break;
                default:
                    throw new NotImplementedException();
            }            
        }

        protected abstract void Fade(float normalisedTime);
        protected abstract void SlideFromLeft(float normalisedTime);
        protected abstract void SlideFromRight(float normalisedTime);
        protected abstract void SlideFromBottom(float normalisedTime);
        protected abstract void SlideFromTop(float normalisedTime);
        protected abstract void Flip(float normalisedTime);
        protected abstract void Expand(float normalisedTime);
        protected abstract bool IsSupportedType(GenericWindowAnimationType type);
    }
}
