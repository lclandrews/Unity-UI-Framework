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

        public bool isPlaying { get; private set; }

        public WindowAnimatorEvent onAnimationComplete { get; private set; }

        public WindowAnimation.Type fallbackAnimationType { get { return WindowAnimation.Type.Fade; } }        

        WindowAnimatorEvent IWindowAnimator.onAnimationComplete { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

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

            switch (animation.type)
            {
                case WindowAnimation.Type.Fade:
                    break;
            }
        }

        public void SetCurrentTime(float time)
        {
            throw new System.NotImplementedException();
        }

        public bool Stop()
        {
            throw new System.NotImplementedException();
        }

        public bool IsSupportedAnimationType(WindowAnimation.Type animationType)
        {
            switch (animationType)
            {
                default:
                    return false;
                case WindowAnimation.Type.Fade:
                    return true;
            }
        }

        public void Update(float deltaTime)
        {
            float animationDelta = playMode == PlayMode.Forward ? deltaTime : -deltaTime;
            currentTime = Mathf.Clamp(currentTime + animationDelta, 0.0F, length);

            float endTime = playMode == PlayMode.Forward ? length : 0.0F;
            bool isAtEnd = Mathf.Approximately(currentTime, endTime);
            if(isAtEnd)
            {

            }
            else
            {

            }
        }

        public void Rewind()
        {
            playMode ^= (PlayMode)1;
        }
    }
}
