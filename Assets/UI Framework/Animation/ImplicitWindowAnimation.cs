namespace UIFramework
{
    public class ImplicitWindowAnimation
    {
        private Animation _animation = null;
        private WindowAnimationType? _animationType = null;

        private ImplicitWindowAnimation() { }

        public ImplicitWindowAnimation(WindowAnimationType animationType)
        {
            _animationType = animationType;
        }

        public ImplicitWindowAnimation(Animation animation)
        {
            _animation = animation;
        }

        public static implicit operator ImplicitWindowAnimation(WindowAnimationType animationType)
        {
            return new ImplicitWindowAnimation(animationType);
        }

        public static implicit operator ImplicitWindowAnimation(Animation animation)
        {
            return new ImplicitWindowAnimation(animation);
        }

        public AnimationPlayable CreatePlayable(IWindow window, float length,
            float startTime = 0.0F, PlayMode playMode = PlayMode.Forward, EasingMode easingMode = EasingMode.Linear, bool unscaledTime = false)
        {
            if (_animation != null)
            {
                float playbackSpeed = _animation.length / length;
                return new AnimationPlayable(_animation, startTime, playMode, easingMode, unscaledTime, playbackSpeed);
            }
            else if (_animationType != null)
            {
                return window.CreatePlayable(_animationType.Value, length, startTime, playMode, easingMode, unscaledTime);
            }
            return default;
        }
    }
}
