using UnityEngine;
using UnityEngine.UIElements;

namespace UIFramework.UIToolkit
{
    public class GenericWindowAnimation : GenericWindowAnimationBase
    {
        private VisualElement _visualElement = null;

        public GenericWindowAnimation(VisualElement visualElement, GenericWindowAnimationType type, float length)
            : base(type, length)
        {
            if(visualElement == null)
            {
                throw new System.ArgumentNullException(nameof(visualElement));
            }
            _visualElement = visualElement;
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
            _visualElement.style.translate = new Translate(UnityEngine.UIElements.Length.Percent(percent), UnityEngine.UIElements.Length.Percent(0.0F));
        }

        protected override void SlideFromRight(float normalisedTime)
        {
            float percent = (1.0F - normalisedTime) * 100.0F;
            _visualElement.style.translate = new Translate(UnityEngine.UIElements.Length.Percent(percent), UnityEngine.UIElements.Length.Percent(0.0F));
        }

        protected override void SlideFromBottom(float normalisedTime)
        {
            float percent = (1.0F - normalisedTime) * 100.0F;
            _visualElement.style.translate = new Translate(UnityEngine.UIElements.Length.Percent(0.0F), UnityEngine.UIElements.Length.Percent(percent));
        }

        protected override void SlideFromTop(float normalisedTime)
        {
            float percent = (-1.0F + normalisedTime) * 100.0F;
            _visualElement.style.translate = new Translate(UnityEngine.UIElements.Length.Percent(0.0F), UnityEngine.UIElements.Length.Percent(percent));
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

        protected override bool IsSupportedType(GenericWindowAnimationType type)
        {
            switch (type)
            {
                default: return false;
                case GenericWindowAnimationType.Fade:
                case GenericWindowAnimationType.SlideFromLeft:
                case GenericWindowAnimationType.SlideFromRight:
                case GenericWindowAnimationType.SlideFromTop:
                case GenericWindowAnimationType.SlideFromBottom:
                case GenericWindowAnimationType.Flip:
                case GenericWindowAnimationType.Expand:
                    return true;
            }
        }
    }
}
