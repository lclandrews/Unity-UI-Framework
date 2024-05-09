using System;

using UnityEngine;

namespace UIFramework
{
    public struct WindowTransitionPlayable : IEquatable<WindowTransitionPlayable>
    {
        public enum AnimationTargets
        {
            None,
            Source,
            Target,
            Both
        }

        public enum SortPriority
        {
            Auto,
            Source,
            Target
        }

        public float length { get; private set; }
        public int lengthMs { get; private set; }
        public EasingMode easingMode { get; private set; }
        public ImplicitWindowAnimation exitAnimation { get; private set; }
        public ImplicitWindowAnimation entryAnimation { get; private set; }
        public SortPriority sortPriority { get; private set; }

        public AnimationTargets animationTargets
        {
            get
            {
                if(exitAnimation != null && entryAnimation != null)
                {
                    return AnimationTargets.Both;
                }
                else if(exitAnimation != null)
                {
                    return AnimationTargets.Source;
                }
                else if (entryAnimation != null)
                {
                    return AnimationTargets.Target;
                }
                else
                {
                    return AnimationTargets.None;
                }
            }
        }

        private WindowTransitionPlayable(float length, EasingMode easingMode, ImplicitWindowAnimation exitAnimation, ImplicitWindowAnimation entryAnimation, SortPriority sortPriority)
        {
            this.length = length;
            lengthMs = SecondsToMilliseconds(length);
            this.easingMode = easingMode;
            this.exitAnimation = exitAnimation;
            this.entryAnimation = entryAnimation;
            this.sortPriority = sortPriority;
        }

        private static int SecondsToMilliseconds(float seconds)
        {
            return Mathf.RoundToInt(seconds * 1000);
        }

        public static WindowTransitionPlayable None()
        {
            return new WindowTransitionPlayable(0.0F, EasingMode.Linear, null, null, SortPriority.Auto);
        }

        public static WindowTransitionPlayable Fade(float length, EasingMode easingMode)
        {
            return new WindowTransitionPlayable(length, easingMode, null, WindowAnimationType.Fade, SortPriority.Auto);
        }

        public static WindowTransitionPlayable Dissolve(float length, EasingMode easingMode)
        {
            return new WindowTransitionPlayable(length, easingMode, WindowAnimationType.Dissolve, null, SortPriority.Auto);
        }

        public static WindowTransitionPlayable SlideFromLeft(float length, EasingMode easingMode)
        {
            return new WindowTransitionPlayable(length, easingMode, WindowAnimationType.SlideFromRight, WindowAnimationType.SlideFromLeft, SortPriority.Auto);
        }

        public static WindowTransitionPlayable SlideFromRight(float length, EasingMode easingMode)
        {
            return new WindowTransitionPlayable(length, easingMode, WindowAnimationType.SlideFromLeft, WindowAnimationType.SlideFromRight, SortPriority.Auto);
        }

        public static WindowTransitionPlayable SlideFromBottom(float length, EasingMode easingMode)
        {
            return new WindowTransitionPlayable(length, easingMode, WindowAnimationType.SlideFromTop, WindowAnimationType.SlideFromBottom, SortPriority.Auto);
        }

        public static WindowTransitionPlayable SlideFromTop(float length, EasingMode easingMode)
        {
            return new WindowTransitionPlayable(length, easingMode, WindowAnimationType.SlideFromBottom, WindowAnimationType.SlideFromTop, SortPriority.Auto);
        }

        public static WindowTransitionPlayable SlideOverFromLeft(float length, EasingMode easingMode)
        {
            return new WindowTransitionPlayable(length, easingMode, null, WindowAnimationType.SlideFromLeft, SortPriority.Auto);
        }

        public static WindowTransitionPlayable SlideOverFromRight(float length, EasingMode easingMode)
        {
            return new WindowTransitionPlayable(length, easingMode, null, WindowAnimationType.SlideFromRight, SortPriority.Auto);
        }

        public static WindowTransitionPlayable SlideOverFromBottom(float length, EasingMode easingMode)
        {
            return new WindowTransitionPlayable(length, easingMode, null, WindowAnimationType.SlideFromBottom, SortPriority.Auto);
        }

        public static WindowTransitionPlayable SlideOverFromTop(float length, EasingMode easingMode)
        {
            return new WindowTransitionPlayable(length, easingMode, null, WindowAnimationType.SlideFromTop, SortPriority.Auto);
        }

        public static WindowTransitionPlayable Flip(float length, EasingMode easingMode)
        {
            return new WindowTransitionPlayable(length, easingMode, WindowAnimationType.Flip, null, SortPriority.Auto);
        }

        public static WindowTransitionPlayable Expand(float length, EasingMode easingMode)
        {
            return new WindowTransitionPlayable(length, easingMode, null, WindowAnimationType.Expand, SortPriority.Auto);
        }

        public static WindowTransitionPlayable Custom(float length, EasingMode easingMode, ImplicitWindowAnimation exitAnimation, ImplicitWindowAnimation entryAnimation, SortPriority sortPriority = SortPriority.Auto)
        {
            return new WindowTransitionPlayable(length, easingMode, exitAnimation, entryAnimation, sortPriority);
        }

        public override bool Equals(object obj)
        {
            return obj is WindowTransitionPlayable other && Equals(other);
        }

        public bool Equals(WindowTransitionPlayable other)
        {
            return lengthMs == other.lengthMs && easingMode == other.easingMode && exitAnimation == other.exitAnimation && entryAnimation == other.entryAnimation;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(lengthMs, easingMode, exitAnimation, entryAnimation);
        }

        public static bool operator ==(WindowTransitionPlayable lhs, WindowTransitionPlayable rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(WindowTransitionPlayable lhs, WindowTransitionPlayable rhs)
        {
            return !(lhs == rhs);
        }
    }
}
