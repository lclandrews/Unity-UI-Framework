using System;

using UnityEngine.UIElements;

namespace UIFramework.UIToolkit
{
    public abstract class Screen : Window, IScreen
    {
        public Controller Controller { get; private set; } = null;    

        public virtual bool SupportsHistory { get; } = true;

        protected virtual string BackButtonName { get; } = null;
        private Button BackButton = null;        

        public override int SortOrder
        {
            get
            {
                return base.SortOrder;
            }
            set
            {
                base.SortOrder = value;
            }
        }

        public Screen(UIDocument uiDocument, VisualElement visualElement) : base(uiDocument, visualElement) { }

        // IScreen
        public ControllerType GetController<ControllerType>() where ControllerType : Controller
        {
            return Controller as ControllerType;
        }

        public virtual void Init(Controller controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            if (this.Controller != null)
            {
                throw new InvalidOperationException("Screen already initialized.");
            }

            this.Controller = controller;
            if (!string.IsNullOrWhiteSpace(BackButtonName))
            {
                UQueryBuilder<Button> backButtonQueryBuilder = VisualElement.Query<Button>(BackButtonName);
                UQueryState<Button> backButtonQuery = backButtonQueryBuilder.Build();
                BackButton = backButtonQuery.First();
            }
            Init();
        }

        public bool SetBackButtonActive(bool active)
        {
            if(BackButton != null)
            {
                BackButton.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
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