namespace UIFramework
{
    public struct WindowAccessPlayable
    {
        public ImplicitWindowAnimation Animation { get; private set; }
        public float Length { get; private set; }
        public EasingMode EasingMode { get; private set; }

        public WindowAccessPlayable(float length)
            : this(null, length, EasingMode.Linear) { }

        public WindowAccessPlayable(ImplicitWindowAnimation animation, float length)
            : this(animation, length, EasingMode.Linear) { }

        public WindowAccessPlayable(float length, EasingMode easingMode)
            : this(null, length, easingMode) { }

        public WindowAccessPlayable(ImplicitWindowAnimation animation, float length, EasingMode easingMode)
        {
            this.Length = length;
            this.Animation = animation;
            this.EasingMode = easingMode;            
        }

        public AnimationPlayable CreatePlayable(IWindow window, AccessOperation accessOperation, float startOffset = 0.0F, TimeMode timeMode = TimeMode.Scaled)
        {            
            if(Animation == null)
            {
                Animation = window.CreateDefaultAccessAnimation(Length);
            }
            return Animation.CreatePlayable(window, Length, accessOperation, startOffset, EasingMode, timeMode);
        }
    }
}
