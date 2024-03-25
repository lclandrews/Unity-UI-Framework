using System;

using UnityEngine;

namespace UIFramework
{
    public struct ScreenTransition : IEquatable<ScreenTransition>
    {
        public enum Type
        {
            None,
            Fade,
            Dissolve,
            SlideFromLeft,
            SlideFromRight,
            SlideFromBottom,
            SlideFromTop,
            SlideOverFromLeft,
            SlideOverFromRight,
            SlideOverFromBottom,
            SlideOverFromTop,
            Flip
        }

        public enum AnimationTargets
        {
            None,
            Source,
            Target,
            Both
        }

        public Type type { get; private set; }
        public float length { get; private set; }
        public int lengthMs { get; private set; }
        public EasingMode easingMode { get; private set; }
        public bool isInterruptible { get; private set; }
        public AnimationTargets animationTargets
        {
            get
            {
                switch (type)
                {
                    default:
                    case Type.None:
                        return AnimationTargets.None;
                    case Type.Fade:                    
                    case Type.SlideOverFromLeft:
                    case Type.SlideOverFromRight:
                    case Type.SlideOverFromTop:
                    case Type.SlideOverFromBottom:
                        return AnimationTargets.Target;
                    case Type.SlideFromLeft:
                    case Type.SlideFromRight:
                    case Type.SlideFromBottom:
                    case Type.SlideFromTop:
                        return AnimationTargets.Both;
                    case Type.Dissolve:
                    case Type.Flip:
                        return AnimationTargets.Source;
                }
            }
        }        

        public ScreenTransition(Type type, float length)
        {
            this.type = type;
            this.length = length;
            lengthMs = SecondsToMilliseconds(length);
            easingMode = EasingMode.Linear;
            isInterruptible = true;            
        }

        public ScreenTransition(Type type, float length, bool isInterruptible)
        {
            this.type = type;
            this.length = length;
            lengthMs = SecondsToMilliseconds(length);
            easingMode = EasingMode.Linear;
            this.isInterruptible = isInterruptible;            
        }

        public ScreenTransition(Type type, float length, EasingMode easingMode)
        {
            this.type = type;
            this.length = length;
            lengthMs = SecondsToMilliseconds(length);
            this.easingMode = easingMode;
            isInterruptible = true;            
        }

        public ScreenTransition(Type type, float length, EasingMode easingMode, bool isInterruptible)
        {
            this.type = type;
            this.length = length;
            lengthMs = SecondsToMilliseconds(length);
            this.easingMode = easingMode;
            this.isInterruptible = isInterruptible;            
        }

        private static int SecondsToMilliseconds(float seconds)
        {
            return Mathf.RoundToInt(seconds * 1000);
        }

        public override bool Equals(object obj)
        {
            return obj is ScreenTransition other && Equals(other);
        }

        public bool Equals(ScreenTransition other)
        {
            return type == other.type && lengthMs == other.lengthMs && easingMode == other.easingMode && isInterruptible == other.isInterruptible;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(type, lengthMs, easingMode, isInterruptible);
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
