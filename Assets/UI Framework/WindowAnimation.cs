namespace UIFramework
{
    public struct WindowAnimation
    {
        public enum Type
        {
            Fade,
            SlideVertical,
            SlideHorizontal,
            Flip
        }

        public enum Direction
        {
            In,
            Out
        }

        public Type type { get; private set; }
        public Direction direction { get; private set; }
        public float length { get; private set; }
        public EasingMode easingMode { get; private set; }

        public WindowAnimation(Type type, float length)
        {
            this.type = type;
            direction = Direction.In;
            this.length = length;
            easingMode = EasingMode.Linear;
        }

        public WindowAnimation(Type type, float length, EasingMode easingMode)
        {
            this.type = type;
            direction = Direction.In;
            this.length = length;
            this.easingMode = easingMode;
        }

        public WindowAnimation(Type type, Direction direction, float length)
        {
            this.type = type;
            this.direction = direction;
            this.length = length;
            easingMode = EasingMode.Linear;
        }

        public WindowAnimation(Type type, Direction direction, float length, EasingMode easingMode)
        {
            this.type = type;
            this.direction = direction;
            this.length = length;
            this.easingMode = easingMode;
        }

        public WindowAnimation CreateInverseAnimation()
        {
            return new WindowAnimation(type, direction ^ (Direction)1, length, easingMode);
        }
    }
}
