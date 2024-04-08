using System;

using UnityEngine;

namespace UIFramework
{
    public struct ScreenTransition : IEquatable<ScreenTransition>
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

        private ScreenTransition(float length, EasingMode easingMode, WindowAnimation.Type? exitAnimation, WindowAnimation.Type? entryAnimation, SortPriority sortPriority)
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

        public static ScreenTransition None()
        {
            return new ScreenTransition(0.0F, EasingMode.Linear, null, null, SortPriority.Auto);
        }

        public static ScreenTransition Fade(float length, EasingMode easingMode)
        {
            return new ScreenTransition(length, easingMode, null, WindowAnimation.Type.Fade, SortPriority.Auto);
        }

        public static ScreenTransition Dissolve(float length, EasingMode easingMode)
        {
            return new ScreenTransition(length, easingMode, WindowAnimation.Type.Dissolve, null, SortPriority.Auto);
        }

        public static ScreenTransition SlideFromLeft(float length, EasingMode easingMode)
        {
            return new ScreenTransition(length, easingMode, WindowAnimation.Type.SlideFromRight, WindowAnimation.Type.SlideFromLeft, SortPriority.Auto);
        }

        public static ScreenTransition SlideFromRight(float length, EasingMode easingMode)
        {
            return new ScreenTransition(length, easingMode, WindowAnimation.Type.SlideFromLeft, WindowAnimation.Type.SlideFromRight, SortPriority.Auto);
        }

        public static ScreenTransition SlideFromBottom(float length, EasingMode easingMode)
        {
            return new ScreenTransition(length, easingMode, WindowAnimation.Type.SlideFromTop, WindowAnimation.Type.SlideFromBottom, SortPriority.Auto);
        }

        public static ScreenTransition SlideFromTop(float length, EasingMode easingMode)
        {
            return new ScreenTransition(length, easingMode, WindowAnimation.Type.SlideFromBottom, WindowAnimation.Type.SlideFromTop, SortPriority.Auto);
        }

        public static ScreenTransition SlideOverFromLeft(float length, EasingMode easingMode)
        {
            return new ScreenTransition(length, easingMode, null, WindowAnimation.Type.SlideFromLeft, SortPriority.Auto);
        }

        public static ScreenTransition SlideOverFromRight(float length, EasingMode easingMode)
        {
            return new ScreenTransition(length, easingMode, null, WindowAnimation.Type.SlideFromRight, SortPriority.Auto);
        }

        public static ScreenTransition SlideOverFromBottom(float length, EasingMode easingMode)
        {
            return new ScreenTransition(length, easingMode, null, WindowAnimation.Type.SlideFromBottom, SortPriority.Auto);
        }

        public static ScreenTransition SlideOverFromTop(float length, EasingMode easingMode)
        {
            return new ScreenTransition(length, easingMode, null, WindowAnimation.Type.SlideFromTop, SortPriority.Auto);
        }

        public static ScreenTransition Flip(float length, EasingMode easingMode)
        {
            return new ScreenTransition(length, easingMode, WindowAnimation.Type.Flip, null, SortPriority.Auto);
        }

        public static ScreenTransition Expand(float length, EasingMode easingMode)
        {
            return new ScreenTransition(length, easingMode, null, WindowAnimation.Type.Expand, SortPriority.Auto);
        }

        public static ScreenTransition Custom(float length, EasingMode easingMode, WindowAnimation.Type? exitAnimation, WindowAnimation.Type? entryAnimation, SortPriority sortPriority = SortPriority.Auto)
        {
            return new ScreenTransition(length, easingMode, exitAnimation, entryAnimation, sortPriority);
        }

        public override bool Equals(object obj)
        {
            return obj is ScreenTransition other && Equals(other);
        }

        public bool Equals(ScreenTransition other)
        {
            return lengthMs == other.lengthMs && easingMode == other.easingMode && exitAnimation == other.exitAnimation && entryAnimation == other.entryAnimation;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(lengthMs, easingMode, exitAnimation, entryAnimation);
        }

        public static bool operator ==(ScreenTransition lhs, ScreenTransition rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ScreenTransition lhs, ScreenTransition rhs)
        {
            return !(lhs == rhs);
        }
    }
}
