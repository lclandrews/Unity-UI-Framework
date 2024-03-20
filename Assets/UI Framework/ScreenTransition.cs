namespace UIFramework
{
    public struct ScreenTransition
    {
        public enum Type
        {
            Fade,
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

        public Type type { get; private set; }
        public float length { get; private set; }
        public EasingMode easingMode { get; private set; }

        public ScreenTransition(Type type, float length)
        {
            this.type = type;
            this.length = length;
            easingMode = EasingMode.Linear;
        }

        public ScreenTransition(Type type, float length, EasingMode easingMode)
        {
            this.type = type;
            this.length = length;
            this.easingMode = easingMode;
        }
    }
}
