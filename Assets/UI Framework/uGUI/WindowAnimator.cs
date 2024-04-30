using System;

using UnityEngine;

namespace UIFramework.UGUI
{
    public class WindowAnimator : IWindowAnimator
    {
        public ICustomWindowAnimation customAnimation { get; private set; }
        public WindowAnimation.Type type { get; private set; }
        public PlayMode playMode { get; private set; }
        public EasingMode easingMode { get; private set; }
        public float length { get; private set; }
        public float currentTime { get; private set; }
        public float currentNormalisedTime { get { return currentTime / length; } }

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
        public virtual WindowAnimation.Type fallbackType { get { return WindowAnimation.Type.Fade; } }

        public WindowAnimatorEvent onComplete { get; set; } = default;

        private RectTransform _displayRectTransform = null;
        private RectTransform _rectTransform = null;
        private CanvasGroup _canvasGroup = null;

        private Vector2 _activeAnchoredPosition = Vector2.zero;

        private Vector3 _offDisplayLeft = Vector3.zero;
        private Vector3 _offDisplayRight = Vector3.zero;
        private Vector3 _offDisplayBottom = Vector3.zero;
        private Vector3 _offDisplayTop = Vector3.zero;

        protected WindowAnimator() { }

        public WindowAnimator(RectTransform displayRectTransform, RectTransform rectTransform, CanvasGroup canvasGroup)
        {
            if (displayRectTransform == null)
            {
                throw new ArgumentNullException("displayRectTransform");
            }

            if (rectTransform == null)
            {
                throw new ArgumentNullException("rectTransform");
            }

            if (canvasGroup == null)
            {
                throw new ArgumentNullException("canvasGroup");
            }

            _displayRectTransform = displayRectTransform;
            _rectTransform = rectTransform;
            _canvasGroup = canvasGroup;

            _activeAnchoredPosition = _rectTransform.anchoredPosition;            
        }

        public void Play(in WindowAnimation animation)
        {
            type = animation.type;
            if (!IsSupportedType(type))
            {
                type = fallbackType;
                Debug.LogWarning(string.Format("No UGUI animation implemented for: {0}, falling back to: {1}", animation.type, fallbackType.ToString()));
            }
            customAnimation = animation.customAnimation;

            isPlaying = true;
            easingMode = animation.easingMode;
            playMode = animation.playMode;
            length = animation.length;
            currentTime = Mathf.Clamp(animation.startTime, 0.0F, length);

            ResetAnimatedComponents();
            switch (type)
            {
                case WindowAnimation.Type.SlideFromLeft:
                    CalculateOffDisplayLeft();
                    break;
                case WindowAnimation.Type.SlideFromRight:
                    CalculateOffDisplayRight();
                    break;
                case WindowAnimation.Type.SlideFromBottom:
                    CalculateOffDisplayBottom();
                    break;
                case WindowAnimation.Type.SlideFromTop:
                    CalculateOffDisplayTop();
                    break;
            }
            InterpolateAnimation(type, currentNormalisedTime, easingMode);
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

        public bool Complete()
        {
            if (isPlaying)
            {
                isPlaying = false;
                float endTime = playMode == PlayMode.Forward ? length : 0.0F;
                currentTime = endTime;
                InterpolateAnimation(type, currentNormalisedTime, easingMode);
                onComplete.Invoke(this);
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

        public virtual bool IsSupportedType(WindowAnimation.Type type)
        {
            switch (type)
            {
                default:
                    return false;
                case WindowAnimation.Type.Fade:
                case WindowAnimation.Type.SlideFromLeft:
                case WindowAnimation.Type.SlideFromRight:
                case WindowAnimation.Type.SlideFromBottom:
                case WindowAnimation.Type.SlideFromTop:
                case WindowAnimation.Type.Flip:
                case WindowAnimation.Type.Expand:
                case WindowAnimation.Type.Custom:
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

                InterpolateAnimation(type, currentNormalisedTime, easingMode);

                if (isAtEnd)
                {
                    isPlaying = false;
                    onComplete.Invoke(this);
                }
            }            
        }

        public void InterpolateAnimation(WindowAnimation.Type type, float normalisedTime, EasingMode easingMode)
        {
            switch (type)
            {
                case WindowAnimation.Type.Fade:
                    Fade(Easing.PerformEase(normalisedTime, easingMode));
                    break;
                case WindowAnimation.Type.Dissolve:
                    Dissolve(Easing.PerformEase(normalisedTime, easingMode));
                    break;
                case WindowAnimation.Type.SlideFromLeft:
                    SlideFromLeft(Easing.PerformEase(normalisedTime, easingMode));
                    break;
                case WindowAnimation.Type.SlideFromRight:
                    SlideFromRight(Easing.PerformEase(normalisedTime, easingMode));
                    break;
                case WindowAnimation.Type.SlideFromBottom:
                    SlideFromBottom(Easing.PerformEase(normalisedTime, easingMode));
                    break;
                case WindowAnimation.Type.SlideFromTop:
                    SlideFromTop(Easing.PerformEase(normalisedTime, easingMode));
                    break;
                case WindowAnimation.Type.Flip:
                    Flip(Easing.PerformEase(normalisedTime, easingMode));
                    break;
                case WindowAnimation.Type.Expand:
                    Expand(Easing.PerformEase(normalisedTime, easingMode));
                    break;
                case WindowAnimation.Type.Custom:
                    customAnimation.Interpolate(Easing.PerformEase(normalisedTime, easingMode));
                    break;

            }
        }

        public virtual void ResetAnimatedComponents()
        {
            _rectTransform.anchoredPosition = _activeAnchoredPosition;
            _rectTransform.localScale = Vector3.one;
            _rectTransform.localRotation = Quaternion.identity;
            _canvasGroup.alpha = 1.0F;
            if(customAnimation != null)
            {
                customAnimation.Reset();
            }
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
            _rectTransform.anchoredPosition = Vector2.LerpUnclamped(_offDisplayLeft, _activeAnchoredPosition, normalisedTime);
        }

        public virtual void SlideFromRight(float normalisedTime)
        {
            _rectTransform.anchoredPosition = Vector2.LerpUnclamped(_offDisplayRight, _activeAnchoredPosition, normalisedTime);
        }

        public virtual void SlideFromBottom(float normalisedTime)
        {
            _rectTransform.anchoredPosition = Vector2.LerpUnclamped(_offDisplayBottom, _activeAnchoredPosition, normalisedTime);
        }

        public virtual void SlideFromTop(float normalisedTime)
        {
            _rectTransform.anchoredPosition = Vector2.LerpUnclamped(_offDisplayTop, _activeAnchoredPosition, normalisedTime);
        }

        // Flip is dependant on the pivot point of the screens rect transform
        public virtual void Flip(float normalisedTime)
        {
            Vector3 euler = _rectTransform.eulerAngles;
            euler.y = (1.0F - normalisedTime) * 90.0F;
            _rectTransform.localRotation = Quaternion.Euler(euler);
        } 
        
        public virtual void Expand(float normalisedTime)
        {
            Vector3 scale = Vector3.one * normalisedTime;
            _rectTransform.localScale = scale;
        }

        private void CalculateOffDisplayLeft()
        {
            Vector3 displayWorldBottomLeftCorner = _displayRectTransform.GetWorldBottomLeftCorner();
            Vector3 displayLocalBottomLeftCorner = _rectTransform.InverseTransformPoint(displayWorldBottomLeftCorner);

            Vector3 localBottomRightCorner = _rectTransform.GetLocalBottomRightCorner();
            float distance = displayLocalBottomLeftCorner.x - localBottomRightCorner.x;
            _offDisplayLeft = new Vector2(_activeAnchoredPosition.x + distance, _activeAnchoredPosition.y);
        }

        private void CalculateOffDisplayRight()
        {
            Vector3 displayWorldBottomRightCorner = _displayRectTransform.GetWorldBottomRightCorner();
            Vector3 displayLocalBottomRightCorner = _rectTransform.InverseTransformPoint(displayWorldBottomRightCorner);

            Vector3 localBottomLeftCorner = _rectTransform.GetLocalBottomLeftCorner();
            float distance = displayLocalBottomRightCorner.x - localBottomLeftCorner.x;
            _offDisplayRight = new Vector2(_activeAnchoredPosition.x + distance, _activeAnchoredPosition.y);
        }

        private void CalculateOffDisplayBottom()
        {
            Vector3 displayWorldBottomLeftCorner = _displayRectTransform.GetWorldBottomLeftCorner();
            Vector3 displayLocalBottomLeftCorner = _rectTransform.InverseTransformPoint(displayWorldBottomLeftCorner);

            Vector3 localTopLeftCorner = _rectTransform.GetLocalTopLeftCorner();
            float distance = displayLocalBottomLeftCorner.y - localTopLeftCorner.y;
            _offDisplayBottom = new Vector2(_activeAnchoredPosition.x, _activeAnchoredPosition.y + distance);
        }

        private void CalculateOffDisplayTop()
        {
            Vector3 displayWorldTopLeftCorner = _displayRectTransform.GetWorldTopLeftCorner();
            Vector3 displayLocalTopLeftCorner = _rectTransform.InverseTransformPoint(displayWorldTopLeftCorner);

            Vector3 localBottomLeftCorner = _rectTransform.GetLocalBottomLeftCorner();
            float distance = displayLocalTopLeftCorner.y - localBottomLeftCorner.y;
            _offDisplayTop = new Vector2(_activeAnchoredPosition.x, _activeAnchoredPosition.y + distance);
        }
    }
}
