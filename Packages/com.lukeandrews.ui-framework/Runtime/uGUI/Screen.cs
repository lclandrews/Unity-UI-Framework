using System;

using UnityEngine;
using UnityEngine.UI;

namespace UIFramework.UGUI
{    
    public abstract class Screen : Window, IScreen
    {
        public Controller Controller { get; private set; } = null;   

        public virtual bool SupportsHistory { get; } = true;

        protected virtual Button BackButton { get; } = null;
        private Canvas _canvas = null;

        public override int SortOrder
        {
            get
            {
                return base.SortOrder;
            }
            set
            {
                base.SortOrder = value;
                _canvas.sortingOrder = value;
            }
        }

        // IScreen
        public ControllerType GetController<ControllerType>() where ControllerType : Controller
        {
            return Controller as ControllerType;
        }

        public virtual void Init(Controller controller)
        {
            if(controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            if (this.Controller != null)
            {
                throw new InvalidOperationException("Screen already initialized.");
            }

            this.Controller = controller;
            _canvas = GetComponentInParent<Canvas>(true);
            if (BackButton != null)
            {
                BackButton.onClick.AddListener(delegate () { controller.CloseScreen(); });
            }
            Init();
        }

        public bool SetBackButtonActive(bool active)
        {
            if (BackButton != null)
            {
                BackButton.gameObject.SetActive(active);
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