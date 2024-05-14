using System;

using UnityEngine.UIElements;

namespace UIFramework.UIToolkit
{
    public abstract class Screen : Window, IScreen
    {
        public Controller controller { get; private set; } = null;    

        public virtual bool supportsHistory { get; } = true;

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
        public ControllerType GetController<ControllerType>() where ControllerType : Controller
        {
            return controller as ControllerType;
        }

        public virtual void Init(Controller controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            if (this.controller != null)
            {
                throw new InvalidOperationException("Screen already initialized.");
            }

            this.controller = controller;
            if (!string.IsNullOrWhiteSpace(backButtonName))
            {
                UQueryBuilder<Button> backButtonQueryBuilder = visualElement.Query<Button>(backButtonName);
                UQueryState<Button> backButtonQuery = backButtonQueryBuilder.Build();
                backButton = backButtonQuery.First();
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

        public bool Equals(INavigableWindow other)
        {
            return other as Screen == this;
        }
    }
}