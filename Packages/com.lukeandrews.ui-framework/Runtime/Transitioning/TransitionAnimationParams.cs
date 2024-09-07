using System;

using UnityEngine;
using UnityEngine.Extension;

namespace UIFramework
{
    public struct TransitionAnimationParams : IEquatable<TransitionAnimationParams>
    {
        public enum AnimationTarget    
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

        public float Length { get; private set; }
        public int LengthMs { get; private set; }
        public EasingMode EasingMode { get; private set; }
        public ImplicitWindowAnimation ExitAnimation { get; private set; }
        public ImplicitWindowAnimation EntryAnimation { get; private set; }
        public SortPriority WindowSortPriority { get; private set; }

        public AnimationTarget Target
        {
            get
            {
                if(ExitAnimation != null && EntryAnimation != null)
                {
                    return AnimationTarget.Both;
                }
                else if(ExitAnimation != null)
                {
                    return AnimationTarget.Source;
                }
                else if (EntryAnimation != null)
                {
                    return AnimationTarget.Target;
                }
                else
                {
                    return AnimationTarget.None;
                }
            }
        }

        private TransitionAnimationParams(float length, EasingMode easingMode, ImplicitWindowAnimation exitAnimation, ImplicitWindowAnimation entryAnimation, SortPriority sortPriority)
        {
            Length = length;
            LengthMs = SecondsToMilliseconds(length);
            EasingMode = easingMode;
            ExitAnimation = exitAnimation;
            EntryAnimation = entryAnimation;
            WindowSortPriority = sortPriority;
        }

        private static int SecondsToMilliseconds(float seconds)
        {
            return Mathf.RoundToInt(seconds * 1000);
        }

        public static TransitionAnimationParams None()
        {
            return new TransitionAnimationParams(0.0F, EasingMode.Linear, null, null, SortPriority.Auto);
        }

        public static TransitionAnimationParams Fade(float length, EasingMode easingMode)
        {
            return new TransitionAnimationParams(length, easingMode, null, GenericWindowAnimationType.Fade, SortPriority.Auto);
        }

        public static TransitionAnimationParams Dissolve(float length, EasingMode easingMode)
        {
            return new TransitionAnimationParams(length, easingMode, GenericWindowAnimationType.Dissolve, null, SortPriority.Auto);
        }

        public static TransitionAnimationParams SlideFromLeft(float length, EasingMode easingMode)
        {
            return new TransitionAnimationParams(length, easingMode, GenericWindowAnimationType.SlideFromRight, GenericWindowAnimationType.SlideFromLeft, SortPriority.Auto);
        }

        public static TransitionAnimationParams SlideFromRight(float length, EasingMode easingMode)
        {
            return new TransitionAnimationParams(length, easingMode, GenericWindowAnimationType.SlideFromLeft, GenericWindowAnimationType.SlideFromRight, SortPriority.Auto);
        }

        public static TransitionAnimationParams SlideFromBottom(float length, EasingMode easingMode)
        {
            return new TransitionAnimationParams(length, easingMode, GenericWindowAnimationType.SlideFromTop, GenericWindowAnimationType.SlideFromBottom, SortPriority.Auto);
        }

        public static TransitionAnimationParams SlideFromTop(float length, EasingMode easingMode)
        {
            return new TransitionAnimationParams(length, easingMode, GenericWindowAnimationType.SlideFromBottom, GenericWindowAnimationType.SlideFromTop, SortPriority.Auto);
        }

        public static TransitionAnimationParams SlideOverFromLeft(float length, EasingMode easingMode)
        {
            return new TransitionAnimationParams(length, easingMode, null, GenericWindowAnimationType.SlideFromLeft, SortPriority.Auto);
        }

        public static TransitionAnimationParams SlideOverFromRight(float length, EasingMode easingMode)
        {
            return new TransitionAnimationParams(length, easingMode, null, GenericWindowAnimationType.SlideFromRight, SortPriority.Auto);
        }

        public static TransitionAnimationParams SlideOverFromBottom(float length, EasingMode easingMode)
        {
            return new TransitionAnimationParams(length, easingMode, null, GenericWindowAnimationType.SlideFromBottom, SortPriority.Auto);
        }

        public static TransitionAnimationParams SlideOverFromTop(float length, EasingMode easingMode)
        {
            return new TransitionAnimationParams(length, easingMode, null, GenericWindowAnimationType.SlideFromTop, SortPriority.Auto);
        }

        public static TransitionAnimationParams Flip(float length, EasingMode easingMode)
        {
            return new TransitionAnimationParams(length, easingMode, GenericWindowAnimationType.Flip, null, SortPriority.Auto);
        }

        public static TransitionAnimationParams Expand(float length, EasingMode easingMode)
        {
            return new TransitionAnimationParams(length, easingMode, null, GenericWindowAnimationType.Expand, SortPriority.Auto);
        }

        public static TransitionAnimationParams Custom(float length, EasingMode easingMode, ImplicitWindowAnimation exitAnimation, ImplicitWindowAnimation entryAnimation, SortPriority sortPriority = SortPriority.Auto)
        {
            return new TransitionAnimationParams(length, easingMode, exitAnimation, entryAnimation, sortPriority);
        }

        public override bool Equals(object obj)
        {
            return obj is TransitionAnimationParams other && Equals(other);
        }

        public bool Equals(TransitionAnimationParams other)
        {
            return LengthMs == other.LengthMs && EasingMode == other.EasingMode && ExitAnimation == other.ExitAnimation && EntryAnimation == other.EntryAnimation;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LengthMs, EasingMode, ExitAnimation, EntryAnimation);
        }

        public static bool operator ==(TransitionAnimationParams lhs, TransitionAnimationParams rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(TransitionAnimationParams lhs, TransitionAnimationParams rhs)
        {
            return !(lhs == rhs);
        }
    }
}
