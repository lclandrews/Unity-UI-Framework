using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIFramework
{
    [RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
    public abstract class UGUIWindow : UGUIBehaviour, IWindow, IWindowAnimatorFactory
    {
        public RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }
                return _rectTransform;
            }
        }
        private RectTransform _rectTransform = null;

        public CanvasGroup canvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                {
                    _canvasGroup = GetComponent<CanvasGroup>();
                }
                return _canvasGroup;
            }
        }
        private CanvasGroup _canvasGroup = null;

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

                if(_isEnabledCounter > 0)
                {                    
                    isInteractable = true;
                    canvasGroup.interactable = true;
                }
                else
                {                    
                    isInteractable = false;
                    canvasGroup.interactable = false;
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
                    canvasGroup.blocksRaycasts = true;
                }
                else
                {
                    canvasGroup.blocksRaycasts = false;
                }
            }
        }
        private int _isInteractableCounter = 1;

        public abstract bool requiresData { get; }
        public object data { get; private set; } = null;

        public IWindowAnimator animator 
        {
            get 
            {
                if(_animator == null)
                {
                    _animator = CreateAnimator();
                    animator.onAnimationComplete += OnAnimationComplete;
                }
                return _animator;
            } 
        }
        private IWindowAnimator _animator = null;

        private bool _isWaiting = false;

        // IWindowAnimatorFactory
        public virtual IWindowAnimator CreateAnimator()
        {
            return new UGUIWindowAnimator(rectTransform, canvasGroup);
        }

        // UGUIBehaviour
        public override void UpdateUI(float deltaTime)
        {
            base.UpdateUI(deltaTime);
            if(animator != null)
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
            gameObject.SetActive(false);
            OnInit();
        }

        public bool SetWaiting(bool waiting)
        {
            if(ShowWaitingIndicator(waiting))
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
                    if (animator.animation.type != animation.type)
                    {
                        Debug.Log(string.Format("Window is already playing a close animation of type {0}, " +
                            "provided open animation is ignored and the current close animation is rewound.", animator.animation.type.ToString()));
                    }
                    animator.Rewind();
                }
                else
                {
                    animator.Play(in animation);
                }
                state = WindowState.Opening;
                gameObject.SetActive(true);
                OnOpen();
                return true;
            }
            return false;
        }

        public bool Open()
        {
            if (state == WindowState.Closing || state == WindowState.Closed)
            {
                if (state == WindowState.Closed)
                {
                    state = WindowState.Open;
                    gameObject.SetActive(true);
                    animator.ResetAnimatedComponents();
                    OnOpen();
                    OnOpened();
                }
                else
                {
                    Debug.Log("Open was called on Window without an animation while already playing a close animation, " +
                        "the current animation will be rewound.");
                    animator.Rewind();

                    state = WindowState.Opening;
                    OnOpen();
                }
                return true;
            }
            return false;
        }

        public void SetData(object data)
        {
            if(requiresData)
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
            else if(data != null)
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
                    if (animator.animation.type != animation.type)
                    {
                        Debug.Log(string.Format("Controller is already playing a open animation of type {0}, " +
                            "provided close animation is ignored and the current open animation is rewound.", animator.animation.type.ToString()));
                    }
                    animator.Rewind();
                }
                else
                {
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
                if (state == WindowState.Open)
                {
                    state = WindowState.Closed;                    
                    OnClose();
                    gameObject.SetActive(false);
                    OnClosed();                    
                }
                else
                {
                    Debug.Log("Close was called on Window without an animation while already playing a open animation, " +
                        "the current animation will be rewound.");
                    animator.Rewind();

                    state = WindowState.Closing;
                    OnClose();
                }
                return true;
            }
            return false;
        }

        // UGUIWindow
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
            if(state == WindowState.Opening)
            {
                state = WindowState.Open;
                OnOpened();
            }
            else if(state == WindowState.Closing)
            {
                state = WindowState.Closed;
                gameObject.SetActive(false);
                OnClosed();
            }
            else
            {
                throw new Exception(string.Format("Window animation complete while in unexpected state: {0}", state.ToString()));
            }
        }

        protected void RebuildLayout(RectTransform target = null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(target != null ? target : rectTransform);
        }
    }
}