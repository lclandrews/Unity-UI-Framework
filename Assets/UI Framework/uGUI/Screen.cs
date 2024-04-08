using UnityEngine;
using UnityEngine.UI;

namespace UIFramework.UGUI
{    
    public abstract class Screen<ControllerType> : Window, IScreen<ControllerType> where ControllerType : Controller<ControllerType>
    {
        public ControllerType controller { get { return _controller; } }
        private ControllerType _controller = null;        

        public virtual bool supportsHistory { get; } = true;

        public virtual ScreenTransition defaultTransition { get; protected set; } = ScreenTransition.Fade(0.25F, EasingMode.EaseInOut);

        protected virtual Button backButton { get; } = null;
        private Canvas _canvas = null;

        public int sortOrder
        {
            get
            {
                return _sortOrder;
            }
            set
            {
                _sortOrder = value;
                // [TODO] Determine a better way of handling implemented sort order.
                // This current approach is used to support transitioning between screens within the same Canvas and
                // screens that utilise different Canvases, or a hybrid approach.
                // This means that if screens share a Canvas when the sort order is set to transition between children, 
                // both will set the sort order of the Canvas which is undesirable :(
                rectTransform.SetSiblingIndex(_sortOrder);              
                _canvas.sortingOrder = _sortOrder;
            }
        }
        private int _sortOrder = 0;

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
    }
}