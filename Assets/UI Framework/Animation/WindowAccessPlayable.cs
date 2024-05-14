using System;

namespace UIFramework
{
    public struct WindowAccessPlayable
    {
        public ImplicitWindowAnimation animation { get; private set; }
        public float length { get; private set; }
        public EasingMode easingMode { get; private set; }

        public WindowAccessPlayable(float length)
            : this(null, length, EasingMode.Linear) { }

        public WindowAccessPlayable(ImplicitWindowAnimation animation, float length)
            : this(animation, length, EasingMode.Linear) { }

        public WindowAccessPlayable(float length, EasingMode easingMode)
            : this(null, length, EasingMode.Linear) { }

        public WindowAccessPlayable(ImplicitWindowAnimation animation, float length, EasingMode easingMode)
        {
            if (animation == null)
            {
                throw new ArgumentNullException(nameof(animation));
            }

            this.length = length;
            this.animation = animation;
            this.easingMode = easingMode;            
        }

        public AnimationPlayable CreatePlayable(IWindow window, AccessOperation accessOperation, float startOffset = 0.0F, TimeMode timeMode = TimeMode.Scaled)
        {            
            if(animation == null)
            {
                animation = window.CreateDefaultAccessAnimation(length);
            }
            return animation.CreatePlayable(window, length, accessOperation, startOffset, easingMode, timeMode);
        }
    }
}
