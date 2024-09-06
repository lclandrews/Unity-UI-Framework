namespace UIFramework
{
    public class ImplicitWindowAnimation
    {
        private WindowAccessAnimation _animation = null;
        private GenericWindowAnimationType? _genericAnimationType = null;

        public bool IsGeneric { get; private set; } = false;

        private ImplicitWindowAnimation() { }

        private ImplicitWindowAnimation(GenericWindowAnimationType genericAnimationType)
        {
            _genericAnimationType = genericAnimationType;
            IsGeneric = true;
        }        

        private ImplicitWindowAnimation(WindowAccessAnimation accessAnimation)
        {
            _animation = accessAnimation;
        }        

        public static implicit operator ImplicitWindowAnimation(GenericWindowAnimationType animationType)
        {
            return new ImplicitWindowAnimation(animationType);
        }

        public static implicit operator GenericWindowAnimationType(ImplicitWindowAnimation implicitAnimation)
        {
            return implicitAnimation._genericAnimationType.GetValueOrDefault(GenericWindowAnimationType.Fade);
        }

        public static implicit operator ImplicitWindowAnimation(WindowAccessAnimation accessAnimation)
        {
            return new ImplicitWindowAnimation(accessAnimation);
        }

        public static implicit operator WindowAccessAnimation(ImplicitWindowAnimation implicitAnimation)
        {
            return implicitAnimation._animation;
        }

        public WindowAccessAnimation GetWindowAnimation(IWindow window)
        {
            if (IsGeneric)
            {
                return window.GetAnimation(_genericAnimationType.Value);
            }
            else 
            {
                return _animation;
            }
        }
    }
}
