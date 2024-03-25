using UnityEngine;

namespace UIFramework
{
    public class UGUIWindowAnimator : IWindowAnimator
    {
        public WindowAnimation animation { get; private set; }

        public PlayMode playMode { get; private set; }

        public float length { get; private set; }

        public float currentTime { get; private set; }

        public float currentNormalisedTime { get { return currentTime / animation.length; } }

        public float remainingTime
        {
            get
            {
                switch (playMode)
                {
                    case PlayMode.Forward:
                        return length - currentTime;
                    case PlayMode.Reverse:
                        return currentTime;
                }
                return 0.0F;
            }
        }

        public bool isPlaying { get; private set; } = false;

        public WindowAnimation.Type fallbackAnimationType { get { return WindowAnimation.Type.Fade; } }

        public WindowAnimatorEvent onAnimationComplete { get; set; } = default;

        private RectTransform _rectTransform = null;
        private CanvasGroup _canvasGroup = null;

        private UGUIWindowAnimator() { }

        public UGUIWindowAnimator(RectTransform rectTransform, CanvasGroup canvasGroup)
        {
            _rectTransform = rectTransform;
            _canvasGroup = canvasGroup;
        }

        public void Play(in WindowAnimation animation)
        {
            WindowAnimation.Type animationType = animation.type;
            if (!IsSupportedAnimationType(animationType))
            {
                animationType = fallbackAnimationType;
                Debug.LogWarning(string.Format("No UGUI animation implemented for: {0}, falling back to: {1}", animationType, fallbackAnimationType.ToString()));
            }

            isPlaying = true;
            this.animation = animation;
            playMode = animation.playMode;
            length = animation.length;
            currentTime = Mathf.Clamp(animation.startTime, 0.0F, length);

            switch (animation.type)
            {
                case WindowAnimation.Type.Fade:
                    Fade(Easing.PerformEase(currentNormalisedTime, animation.easingMode));
                    break;
                case WindowAnimation.Type.Dissolve:
                    Dissolve(Easing.PerformEase(currentNormalisedTime, animation.easingMode));
                    break;
                case WindowAnimation.Type.SlideFromLeft:
                    SlideFromLeft(Easing.PerformEase(currentNormalisedTime, animation.easingMode));
                    break;
                case WindowAnimation.Type.SlideFromRight:
                    SlideFromRight(Easing.PerformEase(currentNormalisedTime, animation.easingMode));
                    break;
                case WindowAnimation.Type.SlideFromBottom:
                    SlideFromBottom(Easing.PerformEase(currentNormalisedTime, animation.easingMode));
                    break;
                case WindowAnimation.Type.SlideFromTop:
                    SlideFromTop(Easing.PerformEase(currentNormalisedTime, animation.easingMode));
                    break;
                case WindowAnimation.Type.Flip:
                    Flip(Easing.PerformEase(currentNormalisedTime, animation.easingMode));
                    break;
            }
        }

        public bool Rewind()
        {
            if (isPlaying)
            {
                playMode ^= (PlayMode)1;
                return true;
            }
            return false;
        }

        public bool Stop()
        {
            if (isPlaying)
            {
                isPlaying = false;
                return true;
            }
            return false;
        }

        public bool SetCurrentTime(float time)
        {
            if(isPlaying)
            {
                currentTime = Mathf.Clamp(time, 0.0F, length);
                return true;
            }
            return false;
        }        

        public virtual bool IsSupportedAnimationType(WindowAnimation.Type animationType)
        {
            switch (animationType)
            {
                default:
                    return false;
                case WindowAnimation.Type.Fade:
                case WindowAnimation.Type.SlideFromLeft:
                case WindowAnimation.Type.SlideFromRight:
                case WindowAnimation.Type.SlideFromBottom:
                case WindowAnimation.Type.SlideFromTop:
                case WindowAnimation.Type.Flip:
                    return true;
            }
        }

        public void Update(float deltaTime)
        {
            if(isPlaying)
            {
                float animationDelta = playMode == PlayMode.Forward ? deltaTime : -deltaTime;
                currentTime = Mathf.Clamp(currentTime + animationDelta, 0.0F, length);

                float endTime = playMode == PlayMode.Forward ? length : 0.0F;
                bool isAtEnd = Mathf.Approximately(currentTime, endTime);
                currentTime = isAtEnd ? endTime : currentTime;

                switch (animation.type)
                {
                    case WindowAnimation.Type.Fade:
                        Fade(Easing.PerformEase(currentNormalisedTime, animation.easingMode));
                        break;
                    case WindowAnimation.Type.Dissolve:
                        Dissolve(Easing.PerformEase(currentNormalisedTime, animation.easingMode));
                        break;
                    case WindowAnimation.Type.SlideFromLeft:
                        SlideFromLeft(Easing.PerformEase(currentNormalisedTime, animation.easingMode));
                        break;
                    case WindowAnimation.Type.SlideFromRight:
                        SlideFromRight(Easing.PerformEase(currentNormalisedTime, animation.easingMode));
                        break;
                    case WindowAnimation.Type.SlideFromBottom:
                        SlideFromBottom(Easing.PerformEase(currentNormalisedTime, animation.easingMode));
                        break;
                    case WindowAnimation.Type.SlideFromTop:
                        SlideFromTop(Easing.PerformEase(currentNormalisedTime, animation.easingMode));
                        break;
                    case WindowAnimation.Type.Flip:
                        Flip(Easing.PerformEase(currentNormalisedTime, animation.easingMode));
                        break;
                }

                if (isAtEnd)
                {
                    isPlaying = false;
                    onAnimationComplete.Invoke(this);
                }
            }            
        }

        public virtual void ResetAnimatedComponents()
        {
            _rectTransform.anchoredPosition = Vector2.zero;
            _canvasGroup.alpha = 1.0F;
        }

        public virtual void Fade(float normalisedTime)
        {
            _canvasGroup.alpha = normalisedTime;
        }

        public virtual void Dissolve(float normalisedTime)
        {
            throw new System.NotImplementedException();
        }

        public virtual void SlideFromLeft(float normalisedTime)
        {
            throw new System.NotImplementedException();
        }

        public virtual void SlideFromRight(float normalisedTime)
        {
            throw new System.NotImplementedException();
        }

        public virtual void SlideFromBottom(float normalisedTime)
        {
            throw new System.NotImplementedException();
        }

        public virtual void SlideFromTop(float normalisedTime)
        {
            throw new System.NotImplementedException();
        }

        public virtual void Flip(float normalisedTime)
        {
            throw new System.NotImplementedException();
        }        
    }
}
