using System;

using UnityEngine;
using UnityEngine.UIElements;

namespace UIFramework.UIToolkit
{
    [RequireComponent(typeof(UIDocument))]
    public abstract class Window : UIBehaviour, IWindow, IWindowAnimatorFactory
    {
        public UIDocument uiDocument { get { return _uiDocument; } }
        private UIDocument _uiDocument = null;

        public VisualElement visualElement { get { return _visualElement; } }
        private VisualElement _visualElement = null;

        public WindowState state { get { return _state; } protected set { _state = value; } }
        private WindowState _state = WindowState.Unitialized;

        public bool isVisible { get { return state != WindowState.Closed; } }

        public virtual bool isEnabled
        {
            get { return _isEnabledCounter > 0; }
            set
            {
                if (value)
                {
                    _isEnabledCounter = Mathf.Min(_isInteractableCounter + 1, 1);
                }
                else
                {
                    _isEnabledCounter--;
                }

                if (_isEnabledCounter > 0)
                {
                    isInteractable = true;
                    visualElement.SetEnabled(true);
                }
                else
                {
                    isInteractable = false;
                    visualElement.SetEnabled(false);
                }
            }
        }
        private int _isEnabledCounter = 1;

        public bool isInteractable
        {
            get { return _isInteractableCounter > 0; }
            set
            {
                if (value)
                {
                    _isInteractableCounter = Mathf.Min(_isInteractableCounter + 1, 1);
                }
                else
                {
                    _isInteractableCounter--;
                }

                if (_isInteractableCounter > 0)
                {
                    SetInteractable(visualElement, true);
                }
                else
                {
                    SetInteractable(visualElement, false);
                }
            }
        }
        private int _isInteractableCounter = 1;

        public virtual int sortOrder
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
                if (newIndexInHierarchy != existingIndexInHierarchy)
                {
                    visualElement.PlaceInFront(parentHierarchy.ElementAt(newIndexInHierarchy));
                }

                uiDocument.sortingOrder = _sortOrder;
            }
        }
        private int _sortOrder = 0;

        public abstract bool requiresData { get; }
        public object data { get; private set; } = null;

        public IWindowAnimator animator
        {
            get
            {
                if (_animator == null)
                {
                    _animator = CreateAnimator();
                    animator.onComplete += OnAnimationComplete;
                }
                return _animator;
            }
        }
        private IWindowAnimator _animator = null;

        private bool _isWaiting = false;

        protected Window() { }

        public Window(UIDocument uiDocument, VisualElement visualElement)
        {
            _uiDocument = uiDocument;
            _visualElement = visualElement;
        }

        // IWindowAnimatorFactory
        public virtual IWindowAnimator CreateAnimator()
        {
            return new WindowAnimator(visualElement);
        }

        // UIToolkitBehaviour
        public override void UpdateUI(float deltaTime)
        {
            base.UpdateUI(deltaTime);
            if (animator != null)
            {
                animator.Update(deltaTime);
            }
        }

        // IWindow
        public void Init()
        {
            if (state != WindowState.Unitialized)
            {
                throw new InvalidOperationException("Window already initialized.");
            }

            state = WindowState.Closed;
            SetActive(false);
            OnInit();
        }

        public bool SetWaiting(bool waiting)
        {
            if (ShowWaitingIndicator(waiting))
            {
                if (waiting != _isWaiting)
                {
                    _isWaiting = waiting;
                    isEnabled = !waiting;
                }
                return true;
            }
            return false;
        }

        public bool Open(in WindowAnimation animation)
        {
            if (animator == null)
            {
                throw new Exception("Attempted to open Window with animation while animator is null.");
            }

            if (state == WindowState.Closing || state == WindowState.Closed)
            {
                if (animator.isPlaying)
                {
                    if (animator.type != animation.type)
                    {
                        Debug.Log(string.Format("Window is already playing a close animation of type {0}, " +
                            "provided open animation of type {1} is ignored and the current close animation is rewound.", animator.type, animation.type));
                    }
                    animator.Rewind();
                }
                else
                {
                    isInteractable = false;
                    animator.Play(in animation);
                }
                state = WindowState.Opening;
                SetActive(true);
                OnOpen();
                return true;
            }
            return false;
        }

        public bool Open()
        {
            if (state == WindowState.Closing || state == WindowState.Closed)
            {
                if (state == WindowState.Closing)
                {
                    Debug.Log("Open was called on a Window without an animation while already playing a state animation, " +
                        "this may cause unexpected behviour of the UI.");
                    isInteractable = true;
                    animator.Stop();
                }

                state = WindowState.Open;
                SetActive(true);
                if (animator != null)
                {
                    animator.ResetAnimatedComponents();
                }
                OnOpen();
                OnOpened();
                return true;
            }
            return false;
        }

        public void SetData(object data)
        {
            if (requiresData)
            {
                if (IsValidData(data))
                {
                    this.data = data;
                    OnDataSet();
                }
                else
                {
                    throw new InvalidOperationException("Attempted to set data on Window of the wrong type.");
                }
            }
            else if (data != null)
            {
                throw new InvalidOperationException("Cannot set data on Window as it requires none.");
            }
        }

        public abstract bool IsValidData(object data);

        public bool Close(in WindowAnimation animation)
        {
            if (animator == null)
            {
                throw new Exception("Attempted to close Controller with animation while animator is null.");
            }

            if (state == WindowState.Opening || state == WindowState.Open)
            {
                if (animator.isPlaying)
                {
                    if (animator.type != animation.type)
                    {
                        Debug.Log(string.Format("Controller is already playing a open animation of type {0}, " +
                            "provided close animation of type {1} is ignored and the current open animation is rewound.", animator.type, animation.type));
                    }
                    animator.Rewind();
                }
                else
                {
                    isInteractable = false;
                    animator.Play(in animation);
                }

                state = WindowState.Closing;
                OnClose();
                return true;
            }
            return false;
        }

        public bool Close()
        {
            if (state == WindowState.Opening || state == WindowState.Open)
            {
                if (state == WindowState.Opening)
                {
                    Debug.Log("Close was called on a Window without an animation while already playing a state animation, " +
                        "this may cause unexpected behviour of the UI.");
                    isInteractable = true;
                    animator.Stop();
                }

                state = WindowState.Closed;
                OnClose();
                SetActive(false);
                OnClosed();
                return true;
            }
            return false;
        }

        // UIToolkitWindow
        public virtual bool ShowWaitingIndicator(bool show)
        {
            return false;
        }

        protected virtual void OnDataSet() { }

        protected virtual void OnInit() { }

        protected virtual void OnOpen() { }

        protected virtual void OnOpened() { }

        protected virtual void OnClose() { }

        protected virtual void OnClosed() { }

        private void OnAnimationComplete(IWindowAnimator animator)
        {
            isInteractable = true;
            if (state == WindowState.Opening)
            {
                state = WindowState.Open;
                OnOpened();
            }
            else if (state == WindowState.Closing)
            {
                state = WindowState.Closed;
                SetActive(false);
                OnClosed();
            }
            else
            {
                throw new Exception(string.Format("Window animation complete while in unexpected state: {0}", state.ToString()));
            }
        }

        protected void SetActive(bool active)
        {
            visualElement.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
        }

        protected void SetInteractable(VisualElement visualElement, bool interactable)
        {
            visualElement.pickingMode = interactable ? PickingMode.Position : PickingMode.Ignore;
            visualElement.IterateHierarchy(delegate (VisualElement v)
            {
                v.pickingMode = interactable ? PickingMode.Position : PickingMode.Ignore;
            });
        }
    }
}