using UnityEngine;
using UnityEngine.UI;

namespace UIFramework.UGUI
{    
    public abstract class Screen<ControllerType> : Window, IScreen<ControllerType> where ControllerType : Controller<ControllerType>
    {
        public ControllerType controller { get { return _controller; } }
        private ControllerType _controller = null;        

        public virtual bool supportsHistory { get; } = true;

        public virtual WindowTransitionPlayable defaultTransition { get; protected set; } = WindowTransitionPlayable.Fade(0.25F, EasingMode.EaseInOut);

        protected virtual Button backButton { get; } = null;
        private Canvas _canvas = null;

        public override int sortOrder
        {
            get
            {
                return base.sortOrder;
            }
            set
            {
                base.sortOrder = value;
                _canvas.sortingOrder = value;
            }
        }

        // IScreen
        public virtual void Init(Controller<ControllerType> controller)
        {
            if (controller != null)
            {
                _controller = controller as ControllerType;
                _canvas = GetComponentInParent<Canvas>(true);
                if (backButton != null)
                {
                    backButton.onClick.AddListener(controller.navigation.Back);
                }
            }
            Init();
        }

        public bool SetBackButtonActive(bool active)
        {
            if (backButton != null)
            {
                backButton.gameObject.SetActive(active);
                return true;
            }
            return false;
        }

        public bool Equals(INavigable other)
        {
            return other as Screen<ControllerType> == this;
        }
    }
}