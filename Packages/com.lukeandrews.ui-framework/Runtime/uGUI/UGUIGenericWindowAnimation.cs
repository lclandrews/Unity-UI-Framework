using System;

using UnityEngine;

namespace UIFramework.UGUI
{
    public class UGUIGenericWindowAnimation : GenericWindowAnimation
    {
        private RectTransform _displayRectTransform = null;
        private RectTransform _rectTransform = null;
        private CanvasGroup _canvasGroup = null;

        private Vector2 _activeAnchoredPosition = Vector2.zero;

        private Vector3 _offDisplayLeft = Vector3.zero;
        private Vector3 _offDisplayRight = Vector3.zero;
        private Vector3 _offDisplayBottom = Vector3.zero;
        private Vector3 _offDisplayTop = Vector3.zero;

        public UGUIGenericWindowAnimation(RectTransform displayRectTransform, RectTransform rectTransform, Vector3 activeAnchoredPosition, 
            CanvasGroup canvasGroup, GenericWindowAnimationType type) : base(type)
        {
            if (displayRectTransform == null)
            {
                throw new ArgumentNullException(nameof(displayRectTransform));
            }

            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }

            if (canvasGroup == null)
            {
                throw new ArgumentNullException(nameof(canvasGroup));
            }

            _displayRectTransform = displayRectTransform;
            _rectTransform = rectTransform;
            _canvasGroup = canvasGroup;

            _activeAnchoredPosition = activeAnchoredPosition;            
        }

        public override void Prepare()
        {
            base.Prepare();

            switch (Type)
            {
                case GenericWindowAnimationType.SlideFromLeft:
                    CalculateOffDisplayLeft();
                    break;
                case GenericWindowAnimationType.SlideFromRight:
                    CalculateOffDisplayRight();
                    break;
                case GenericWindowAnimationType.SlideFromBottom:
                    CalculateOffDisplayBottom();
                    break;
                case GenericWindowAnimationType.SlideFromTop:
                    CalculateOffDisplayTop();
                    break;
            }            
        }

        protected override void Fade(float normalisedTime)
        {
            _canvasGroup.alpha = normalisedTime;
        }

        protected override void SlideFromLeft(float normalisedTime)
        {
            _rectTransform.anchoredPosition = Vector2.LerpUnclamped(_offDisplayLeft, _activeAnchoredPosition, normalisedTime);
        }

        protected override void SlideFromRight(float normalisedTime)
        {
            _rectTransform.anchoredPosition = Vector2.LerpUnclamped(_offDisplayRight, _activeAnchoredPosition, normalisedTime);
        }

        protected override void SlideFromBottom(float normalisedTime)
        {
            _rectTransform.anchoredPosition = Vector2.LerpUnclamped(_offDisplayBottom, _activeAnchoredPosition, normalisedTime);
        }

        protected override void SlideFromTop(float normalisedTime)
        {
            _rectTransform.anchoredPosition = Vector2.LerpUnclamped(_offDisplayTop, _activeAnchoredPosition, normalisedTime);
        }

        // Flip is dependant on the pivot point of the screens rect transform
        protected override void Flip(float normalisedTime)
        {
            Vector3 euler = _rectTransform.eulerAngles;
            euler.y = (1.0F - normalisedTime) * 90.0F;
            _rectTransform.localRotation = Quaternion.Euler(euler);
        }

        protected override void Expand(float normalisedTime)
        {
            Vector3 scale = Vector3.one * normalisedTime;
            _rectTransform.localScale = scale;
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
