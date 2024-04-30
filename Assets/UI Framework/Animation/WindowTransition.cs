using System;

using UnityEngine;

namespace UIFramework
{
    public struct WindowTransition : IEquatable<WindowTransition>
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
        public WindowAnimation.Type? exitAnimation { get; private set; }
        public WindowAnimation.Type? entryAnimation { get; private set; }
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

        private WindowTransition(float length, EasingMode easingMode, WindowAnimation.Type? exitAnimation, WindowAnimation.Type? entryAnimation, SortPriority sortPriority)
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

        public static WindowTransition None()
        {
            return new WindowTransition(0.0F, EasingMode.Linear, null, null, SortPriority.Auto);
        }

        public static WindowTransition Fade(float length, EasingMode easingMode)
        {
            return new WindowTransition(length, easingMode, null, WindowAnimation.Type.Fade, SortPriority.Auto);
        }

        public static WindowTransition Dissolve(float length, EasingMode easingMode)
        {
            return new WindowTransition(length, easingMode, WindowAnimation.Type.Dissolve, null, SortPriority.Auto);
        }

        public static WindowTransition SlideFromLeft(float length, EasingMode easingMode)
        {
            return new WindowTransition(length, easingMode, WindowAnimation.Type.SlideFromRight, WindowAnimation.Type.SlideFromLeft, SortPriority.Auto);
        }

        public static WindowTransition SlideFromRight(float length, EasingMode easingMode)
        {
            return new WindowTransition(length, easingMode, WindowAnimation.Type.SlideFromLeft, WindowAnimation.Type.SlideFromRight, SortPriority.Auto);
        }

        public static WindowTransition SlideFromBottom(float length, EasingMode easingMode)
        {
            return new WindowTransition(length, easingMode, WindowAnimation.Type.SlideFromTop, WindowAnimation.Type.SlideFromBottom, SortPriority.Auto);
        }

        public static WindowTransition SlideFromTop(float length, EasingMode easingMode)
        {
            return new WindowTransition(length, easingMode, WindowAnimation.Type.SlideFromBottom, WindowAnimation.Type.SlideFromTop, SortPriority.Auto);
        }

        public static WindowTransition SlideOverFromLeft(float length, EasingMode easingMode)
        {
            return new WindowTransition(length, easingMode, null, WindowAnimation.Type.SlideFromLeft, SortPriority.Auto);
        }

        public static WindowTransition SlideOverFromRight(float length, EasingMode easingMode)
        {
            return new WindowTransition(length, easingMode, null, WindowAnimation.Type.SlideFromRight, SortPriority.Auto);
        }

        public static WindowTransition SlideOverFromBottom(float length, EasingMode easingMode)
        {
            return new WindowTransition(length, easingMode, null, WindowAnimation.Type.SlideFromBottom, SortPriority.Auto);
        }

        public static WindowTransition SlideOverFromTop(float length, EasingMode easingMode)
        {
            return new WindowTransition(length, easingMode, null, WindowAnimation.Type.SlideFromTop, SortPriority.Auto);
        }

        public static WindowTransition Flip(float length, EasingMode easingMode)
        {
            return new WindowTransition(length, easingMode, WindowAnimation.Type.Flip, null, SortPriority.Auto);
        }

        public static WindowTransition Expand(float length, EasingMode easingMode)
        {
            return new WindowTransition(length, easingMode, null, WindowAnimation.Type.Expand, SortPriority.Auto);
        }

        public static WindowTransition Custom(float length, EasingMode easingMode, WindowAnimation.Type? exitAnimation, WindowAnimation.Type? entryAnimation, SortPriority sortPriority = SortPriority.Auto)
        {
            return new WindowTransition(length, easingMode, exitAnimation, entryAnimation, sortPriority);
        }

        public override bool Equals(object obj)
        {
            return obj is WindowTransition other && Equals(other);
        }

        public bool Equals(WindowTransition other)
        {
            return lengthMs == other.lengthMs && easingMode == other.easingMode && exitAnimation == other.exitAnimation && entryAnimation == other.entryAnimation;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(lengthMs, easingMode, exitAnimation, entryAnimation);
        }

        public static bool operator ==(WindowTransition lhs, WindowTransition rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(WindowTransition lhs, WindowTransition rhs)
        {
            return !(lhs == rhs);
        }
    }
}
