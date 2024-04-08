using UnityEngine;
using UnityEngine.UIElements;

namespace UIFramework.UIToolkit
{    
    public abstract class Screen<ControllerType> : Window, IScreen<ControllerType> where ControllerType : Controller<ControllerType>
    {
        public ControllerType controller { get { return _controller; } }
        private ControllerType _controller = null;        

        public virtual bool supportsHistory { get; } = true;

        public virtual ScreenTransition defaultTransition { get; protected set; } = ScreenTransition.Fade(0.25F, EasingMode.EaseInOut);

        protected virtual string backButtonName { get; } = null;
        private Button backButton = null;

        public Screen(UIDocument uiDocument, VisualElement visualElement) : base(uiDocument, visualElement) { }

        public int sortOrder
        {
            get
            {
                return _sortOrder;
            }
            set
            {
                _sortOrder = value;

                VisualElement.Hierarchy parentHierarchy = visualElement.parent.hierarchy;
                int newIndexInHierarchy = Mathf.Clamp(_sortOrder, 0, parentHierarchy.childCount - 1);
                int existingIndexInHierarchy = parentHierarchy.IndexOf(visualElement);
                if(newIndexInHierarchy != existingIndexInHierarchy)
                {
                    visualElement.PlaceInFront(parentHierarchy.ElementAt(newIndexInHierarchy));
                }

                uiDocument.sortingOrder = _sortOrder;
            }
        }
        private int _sortOrder = 0;

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
    }
}