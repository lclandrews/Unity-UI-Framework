namespace UIFramework
{
    public class ImplicitWindowAnimation
    {
        private Animation _animation = null;
        private GenericWindowAnimationType? _genericAnimationType = null;

        public bool IsGeneric { get; private set; } = false;
        public bool IsAccessAnimation { get; private set; } = false;

        private ImplicitWindowAnimation() { }

        private ImplicitWindowAnimation(Animation animation)
        {
            _animation = animation;
            if(_animation is WindowAccessAnimation)
            {
                IsAccessAnimation = true;
            }
        }

        private ImplicitWindowAnimation(GenericWindowAnimationType genericAnimationType)
        {
            _genericAnimationType = genericAnimationType;
            IsGeneric = true;
        }        

        private ImplicitWindowAnimation(WindowAccessAnimation accessAnimation)
        {
            _animation = accessAnimation;
            IsAccessAnimation = true;
        }        

        public static implicit operator ImplicitWindowAnimation(Animation animation)
        {
            return new ImplicitWindowAnimation(animation);
        }

        public static implicit operator Animation(ImplicitWindowAnimation implicitAnimation)
        {
            return implicitAnimation._animation;
        }

        public static implicit operator ImplicitWindowAnimation(GenericWindowAnimationType animationType)
        {
            return new ImplicitWindowAnimation(animationType);
        }

        public static implicit operator GenericWindowAnimationType(ImplicitWindowAnimation implicitAnimation)
        {
            return implicitAnimation._genericAnimationType.GetValueOrDefault(GenericWindowAnimationType.Fade);
        }

        public static implicit operator ImplicitWindowAnimation(WindowAccessAnimation directionalAnimation)
        {
            return new ImplicitWindowAnimation(directionalAnimation);
        }

        public static implicit operator WindowAccessAnimation(ImplicitWindowAnimation implicitAnimation)
        {
            return implicitAnimation._animation as WindowAccessAnimation;
        }

        public AnimationPlayable CreatePlayable(IWindow window, float length, AccessOperation accessOperation,
            float startOffset = 0.0F, EasingMode easingMode = EasingMode.Linear, TimeMode timeMode = TimeMode.Scaled)
        {
            PlayMode playMode;
            if (IsGeneric)
            {
                playMode = accessOperation == AccessOperation.Open ? PlayMode.Forward : PlayMode.Reverse;
            }
            else if(IsAccessAnimation)
            {
                WindowAccessAnimation accessAnimation = _animation as WindowAccessAnimation;
                playMode = accessOperation == accessAnimation.AccessOperation ? PlayMode.Forward : PlayMode.Reverse;
            }
            else
            {
                playMode = PlayMode.Forward;
            }

            float startTime = playMode == PlayMode.Forward ? startOffset : length - startOffset;
            return CreatePlayable(window, length, startTime, playMode, easingMode, timeMode);
        }

        private AnimationPlayable CreatePlayable(IWindow window, float length,
            float startTime = 0.0F, PlayMode playMode = PlayMode.Forward, EasingMode easingMode = EasingMode.Linear, TimeMode timeMode = TimeMode.Scaled)
        {
            if (_animation != null)
            {
                float playbackSpeed = _animation.Length / length;
                return new AnimationPlayable(_animation, startTime, playMode, easingMode, timeMode, playbackSpeed);
            }
            else if (_genericAnimationType != null)
            {
                Animation animation = window.CreateAnimation(_genericAnimationType.Value, length);
                return new AnimationPlayable(animation, startTime, playMode, easingMode, timeMode);
            }
            return default;
        }
    }
}
