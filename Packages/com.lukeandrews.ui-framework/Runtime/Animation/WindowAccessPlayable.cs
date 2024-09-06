namespace UIFramework
{
    public struct WindowAccessPlayable
    {        
        public float Length { get; private set; }
        public EasingMode EasingMode { get; private set; }
        public ImplicitWindowAnimation ImplicitAnimation { get; private set; }

        public WindowAccessPlayable(float length)
            : this(null, length, EasingMode.Linear) { }

        public WindowAccessPlayable(float length, EasingMode easingMode)
            : this(null, length, easingMode) { }

        public WindowAccessPlayable(ImplicitWindowAnimation animation, float length)
            : this(animation, length, EasingMode.Linear) { }        

        public WindowAccessPlayable(ImplicitWindowAnimation animation, float length, EasingMode easingMode)
        {
            Length = length;
            ImplicitAnimation = animation;
            EasingMode = easingMode;            
        }

        private WindowAccessAnimation GetWindowAnimation(IWindow window)
        {
            if(ImplicitAnimation == null)
            {
                return window.GetDefaultAccessAnimation();
            }
            else
            {
                return ImplicitAnimation.GetWindowAnimation(window);
            }
        }

        public AnimationPlayable CreatePlayable(IWindow window, AccessOperation accessOperation, float startOffset = 0.0F, TimeMode timeMode = TimeMode.Scaled)
        {
            return GetWindowAnimation(window).CreatePlayable(accessOperation, Length, startOffset, EasingMode, timeMode);
        }
    }
}
