using UnityEngine;
using UnityEngine.UIElements;

namespace UIFramework.UIToolkit
{
    public class UITKGenericWindowAnimation : GenericWindowAnimation
    {
        private VisualElement _visualElement = null;

        public UITKGenericWindowAnimation(VisualElement visualElement, GenericWindowAnimationType type) : base(type)
        {
            if(visualElement == null)
            {
                throw new System.ArgumentNullException(nameof(visualElement));
            }
            _visualElement = visualElement;
        }

        protected override void Fade(float normalisedTime)
        {
            if(_visualElement != null)
                _visualElement.style.opacity = normalisedTime;
        }

        protected override void SlideFromLeft(float normalisedTime)
        {
            if (_visualElement != null)
            {
                float percent = (-1.0F + normalisedTime) * 100.0F;
                _visualElement.style.translate = new Translate(UnityEngine.UIElements.Length.Percent(percent), UnityEngine.UIElements.Length.Percent(0.0F));
            }
        }

        protected override void SlideFromRight(float normalisedTime)
        {
            if (_visualElement != null)
            {
                float percent = (1.0F - normalisedTime) * 100.0F;
                _visualElement.style.translate = new Translate(UnityEngine.UIElements.Length.Percent(percent), UnityEngine.UIElements.Length.Percent(0.0F));
            }
        }

        protected override void SlideFromBottom(float normalisedTime)
        {
            if (_visualElement != null)
            {
                float percent = (1.0F - normalisedTime) * 100.0F;
                _visualElement.style.translate = new Translate(UnityEngine.UIElements.Length.Percent(0.0F), UnityEngine.UIElements.Length.Percent(percent));
            }
        }

        protected override void SlideFromTop(float normalisedTime)
        {
            if (_visualElement != null)
            {
                float percent = (-1.0F + normalisedTime) * 100.0F;
                _visualElement.style.translate = new Translate(UnityEngine.UIElements.Length.Percent(0.0F), UnityEngine.UIElements.Length.Percent(percent));
            }
        }

        protected override void Flip(float normalisedTime)
        {
            if (_visualElement != null)
            {
                Fade(normalisedTime);
                float degrees = (1.0F - normalisedTime) * 180.0F;
                _visualElement.style.rotate = new Rotate(degrees);
            }
        }

        protected override void Expand(float normalisedTime)
        {
            if (_visualElement != null)
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
