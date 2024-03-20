using UnityEngine.UI;

namespace UIFramework
{
    public abstract class UGUIScreen<ControllerType> : UGUIWindow, IScreen<ControllerType> where ControllerType : Controller<ControllerType>
    {
        public ControllerType controller { get { return _controller; } }
        private ControllerType _controller = null;

        public virtual Button backButton { get; } = null;

        public virtual bool supportsHistory { get; } = true;

        // IScreen
        public virtual void Init(Controller<ControllerType> controller)
        {
            if (controller != null)
            {
                _controller = controller as ControllerType;
                if (backButton != null)
                {
                    backButton.onClick.AddListener(controller.navigation.Back);
                }
            }
        }

        public bool SetBackButtonActive(bool active)
        {
            if (backButton != null)
            {
                backButton.gameObject.SetActive(active);
            }
            return false;
        }
    }
}