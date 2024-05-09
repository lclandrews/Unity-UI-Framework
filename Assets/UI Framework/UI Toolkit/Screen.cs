using UnityEngine.UIElements;

namespace UIFramework.UIToolkit
{
    public abstract class Screen<ControllerType> : Window, IScreen<ControllerType> where ControllerType : Controller<ControllerType>
    {
        public ControllerType controller { get { return _controller; } }
        private ControllerType _controller = null;        

        public virtual bool supportsHistory { get; } = true;

        public virtual WindowTransitionPlayable defaultTransition { get; protected set; } = WindowTransitionPlayable.Fade(0.25F, EasingMode.EaseInOut);

        protected virtual string backButtonName { get; } = null;
        private Button backButton = null;

        public Screen(UIDocument uiDocument, VisualElement visualElement) : base(uiDocument, visualElement) { }

        public override int sortOrder
        {
            get
            {
                return base.sortOrder;
            }
            set
            {
                base.sortOrder = value;
            }
        }

        // IScreen
        public virtual void Init(Controller<ControllerType> controller)
        {
            if (controller != null)
            {
                _controller = controller as ControllerType;
                if(!string.IsNullOrWhiteSpace(backButtonName))
                {
                    UQueryBuilder<Button> backButtonQueryBuilder = visualElement.Query<Button>(backButtonName);
                    UQueryState<Button> backButtonQuery = backButtonQueryBuilder.Build();
                    backButton = backButtonQuery.First();
                }                
            }
            Init();
        }

        public bool SetBackButtonActive(bool active)
        {
            if(backButton != null)
            {
                backButton.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
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