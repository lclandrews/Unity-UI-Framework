using System;

using UnityEngine;
using UnityEngine.UIElements;

namespace UIFramework.UIToolkit
{
    public class WindowAnimator : IWindowAnimator
    {        
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

        private VisualElement _visualElement = null;

        private Vector3 _activeLeftPercent = Vector3.zero;
        private Vector3 _activeRightPercent = Vector3.zero;
        private Vector3 _activeBottomPercent = Vector3.zero;
        private Vector3 _activeTopPercent = Vector3.zero;

        protected WindowAnimator() { }

        public WindowAnimator(VisualElement visualElement)
        {
            if(visualElement == null)
            {
                throw new ArgumentNullException("visualElement");
            }
            _visualElement = visualElement;
        }

        public void Play(in WindowAnimation animation)
        {
            type = animation.type;
            if (!IsSupportedType(type))
            {
                type = fallbackType;
                Debug.LogWarning(string.Format("No UGUI animation implemented for: {0}, falling back to: {1}", animation.type, fallbackType.ToString()));
            }

            isPlaying = true;
            easingMode = animation.easingMode;
            playMode = animation.playMode;
            length = animation.length;
            currentTime = Mathf.Clamp(animation.startTime, 0.0F, length);

            ResetAnimatedComponents();
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
            }
        }

        public virtual void ResetAnimatedComponents()
        {
            _visualElement.style.opacity = 1;
            _visualElement.style.translate = new Translate(Length.Percent(0.0F), Length.Percent(0.0F));
            _visualElement.style.scale = new Scale(Vector2.one);
            _visualElement.style.rotate = new Rotate(0.0F);
        }

        public virtual void Fade(float normalisedTime)
        {
            _visualElement.style.opacity = normalisedTime;
        }

        public virtual void Dissolve(float normalisedTime)
        {
            throw new System.NotImplementedException();
        }

        public virtual void SlideFromLeft(float normalisedTime)
        {
            float percent = (-1.0F + normalisedTime) * 100.0F;
            _visualElement.style.translate = new Translate(Length.Percent(percent), Length.Percent(0.0F));
        }

        public virtual void SlideFromRight(float normalisedTime)
        {
            float percent = (1.0F - normalisedTime) * 100.0F;
            _visualElement.style.translate = new Translate(Length.Percent(percent), Length.Percent(0.0F));
        }

        public virtual void SlideFromBottom(float normalisedTime)
        {
            float percent = (1.0F - normalisedTime) * 100.0F;
            _visualElement.style.translate = new Translate(Length.Percent(0.0F), Length.Percent(percent));
        }

        public virtual void SlideFromTop(float normalisedTime)
        {
            float percent = (-1.0F + normalisedTime) * 100.0F;
            _visualElement.style.translate = new Translate(Length.Percent(0.0F), Length.Percent(percent));
        }

        public virtual void Flip(float normalisedTime)
        {
            Fade(normalisedTime);
            float degrees = (1.0F - normalisedTime) * 180.0F;
            _visualElement.style.rotate = new Rotate(degrees);
        } 
        
        public virtual void Expand(float normalisedTime)
        {
            _visualElement.style.scale = new Scale(new Vector2(normalisedTime, normalisedTime));
        }
    }
}
