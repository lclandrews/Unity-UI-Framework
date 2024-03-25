namespace UIFramework
{
    public enum PlayMode
    {
        Forward,
        Reverse
    }

    public struct WindowAnimation
    {
        /// <summary>
        /// Type of animation. Some assumptions are made.
        /// Fade, Dissolve, Flip: These are assumed to be from a inactive state to an active state.
        /// E.g, animating from an alpha of 0 to 1 for Fade.
        /// </summary>
        public enum Type
        {
            Fade,
            Dissolve,
            SlideFromLeft,
            SlideFromRight,
            SlideFromBottom,
            SlideFromTop,
            Flip
        }

        public Type type { get; private set; }
        public float length { get; private set; }
        public EasingMode easingMode { get; private set; }
        public float startTime { get; private set; }
        public PlayMode playMode { get; private set; }

        public WindowAnimation(Type type, float length)
        {
            this.type = type;
            this.length = length;
            easingMode = EasingMode.Linear;
            startTime = 0.0F;
            playMode = PlayMode.Forward;            
        }

        public WindowAnimation(Type type, float length, float startTime)
        {
            this.type = type;
            this.length = length;
            easingMode = EasingMode.Linear;
            this.startTime = startTime;
            playMode = PlayMode.Forward;            
        }

        public WindowAnimation(Type type, float length, float startTime, PlayMode playMode)
        {
            this.type = type;
            this.length = length;
            easingMode = EasingMode.Linear;
            this.startTime = startTime;
            this.playMode = playMode;
        }

        public WindowAnimation(Type type, float length, EasingMode easingMode)
        {
            this.type = type;
            this.length = length;
            this.easingMode = easingMode;
            startTime = 0.0F;
            playMode = PlayMode.Forward;            
        }

        public WindowAnimation(Type type, float length, EasingMode easingMode, float startTime)
        {
            this.type = type;
            this.length = length;
            this.easingMode = easingMode;
            this.startTime = startTime;
            playMode = PlayMode.Forward;
        }

        public WindowAnimation(Type type, float length, EasingMode easingMode, float startTime, PlayMode playMode)
        {
            this.type = type;
            this.length = length;
            this.easingMode = easingMode;
            this.startTime = startTime;
            this.playMode = playMode;
        }

        public WindowAnimation CreateInverseAnimation(float startTime)
        {
            return new WindowAnimation(type, length, easingMode, startTime, InvertPlayMode(playMode));
        }

        public static PlayMode InvertPlayMode(PlayMode playMode)
        {
            return playMode ^ (PlayMode)1;
        }
    }
}
