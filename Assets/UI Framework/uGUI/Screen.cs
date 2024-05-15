using System;

using UnityEngine;
using UnityEngine.UI;

namespace UIFramework.UGUI
{    
    public abstract class Screen : Window, IScreen
    {
        public Controller controller { get; private set; } = null;   

        public virtual bool supportsHistory { get; } = true;

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
        public ControllerType GetController<ControllerType>() where ControllerType : Controller
        {
            return controller as ControllerType;
        }

        public virtual void Init(Controller controller)
        {
            if(controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            if (this.controller != null)
            {
                throw new InvalidOperationException("Screen already initialized.");
            }

            this.controller = controller;
            _canvas = GetComponentInParent<Canvas>(true);
            if (backButton != null)
            {
                backButton.onClick.AddListener(delegate () { controller.CloseScreen(); });
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

        public bool Equals(INavigableWindow other)
        {
            return other as Screen == this;
        }
    }
}