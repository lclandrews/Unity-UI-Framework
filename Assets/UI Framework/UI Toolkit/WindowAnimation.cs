using UnityEngine;
using UnityEngine.UIElements;

namespace UIFramework.UIToolkit
{
    public class WindowAnimation : WindowAnimationBase
    {
        private VisualElement _visualElement = null;

        public WindowAnimation(VisualElement visualElement, WindowAnimationType type, float length)
            : base(type, length)
        {
            if(visualElement == null)
            {
                throw new System.ArgumentNullException(nameof(visualElement));
            }
            _visualElement = visualElement;
        }

        public override void ResetAnimatedComponents()
        {
            _visualElement.style.opacity = 1;
            _visualElement.style.translate = new Translate(Length.Percent(0.0F), Length.Percent(0.0F));
            _visualElement.style.scale = new Scale(Vector2.one);
            _visualElement.style.rotate = new Rotate(0.0F);
        }

        protected override void Fade(float normalisedTime)
        {
            _visualElement.style.opacity = normalisedTime;
        }

        protected override void Dissolve(float normalisedTime)
        {
            throw new System.NotImplementedException();
        }

        protected override void SlideFromLeft(float normalisedTime)
        {
            float percent = (-1.0F + normalisedTime) * 100.0F;
            _visualElement.style.translate = new Translate(Length.Percent(percent), Length.Percent(0.0F));
        }

        protected override void SlideFromRight(float normalisedTime)
        {
            float percent = (1.0F - normalisedTime) * 100.0F;
            _visualElement.style.translate = new Translate(Length.Percent(percent), Length.Percent(0.0F));
        }

        protected override void SlideFromBottom(float normalisedTime)
        {
            float percent = (1.0F - normalisedTime) * 100.0F;
            _visualElement.style.translate = new Translate(Length.Percent(0.0F), Length.Percent(percent));
        }

        protected override void SlideFromTop(float normalisedTime)
        {
            float percent = (-1.0F + normalisedTime) * 100.0F;
            _visualElement.style.translate = new Translate(Length.Percent(0.0F), Length.Percent(percent));
        }

        protected override void Flip(float normalisedTime)
        {
            Fade(normalisedTime);
            float degrees = (1.0F - normalisedTime) * 180.0F;
            _visualElement.style.rotate = new Rotate(degrees);
        }

        protected override void Expand(float normalisedTime)
        {
            _visualElement.style.scale = new Scale(new Vector2(normalisedTime, normalisedTime));
        }

        protected override bool IsSupportedType(WindowAnimationType type)
        {
            switch (type)
            {
                default: return false;
                case WindowAnimationType.Fade:
                case WindowAnimationType.SlideFromLeft:
                case WindowAnimationType.SlideFromRight:
                case WindowAnimationType.SlideFromTop:
                case WindowAnimationType.SlideFromBottom:
                case WindowAnimationType.Flip:
                case WindowAnimationType.Expand:
                    return true;
            }
        }
    }
}
