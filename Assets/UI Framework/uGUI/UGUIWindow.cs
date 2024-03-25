using UnityEngine;
using UnityEngine.UI;

namespace UIFramework
{
    [RequireComponent(typeof(CanvasGroup))]
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
                    _canvasGroup.alpha = 0.0F;
                    _canvasGroup.interactable = true;
                    _canvasGroup.blocksRaycasts = false;
                }
                return _canvasGroup;
            }
        }
        private CanvasGroup _canvasGroup = null;

        private float _fadeTimer = 0.25F;
        private bool _fade = false;
        private float _fadeTime = 0.0F;
        private float _startAlpha = 0.0F;
        private float _targetAlpha = 0.0F;

        public WindowState state { get { return _state; } protected set { _state = value; } }
        private WindowState _state = WindowState.Closed;

        public bool isVisible { get { return state != WindowState.Closed; } }
        public bool isEnabled { get { return _windowEnabled; } set { _windowEnabled = value; } }
        public bool isInteractable { get; set; }

        private bool _windowEnabled = true;

        public float windowTransitionLength { get { return _fadeTimer; } }
        public float windowTransitionTime { get { return _fadeTime; } }
        public float windowTransitionAlpha { get { return _fadeTime / _fadeTimer; } }

        public abstract bool requiresData { get; }
        public object data { get; private set; } = null;

        public IWindowAnimator animator { get; private set; }

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();
            canvasGroup.alpha = 0.0F;
            canvasGroup.blocksRaycasts = false;
            animator = CreateAnimator();
        }

        // IWindowAnimatorFactory
        public virtual IWindowAnimator CreateAnimator()
        {
            return new UGUIWindowAnimator(_rectTransform, _canvasGroup);
        }

        // UGUIBehaviour
        public override void UpdateUI(float deltaTime)
        {
            base.UpdateUI(deltaTime);
            if (_fade)
            {
                CanvasGroup c = canvasGroup;
                _fadeTime += deltaTime;
                float normalTime = _fadeTime / _fadeTimer;
                if (normalTime < 1.0F)
                {
                    c.alpha = Mathf.Lerp(_startAlpha, _targetAlpha, normalTime);
                }
                else
                {
                    c.alpha = _targetAlpha;
                    _fade = false;
                    _fadeTime = 0.0F;
                    if (state == WindowState.Opening)
                    {
                        state = WindowState.Open;
                        //c.interactable = true;
                        c.blocksRaycasts = true;
                        OnOpened();
                    }
                    else if (state == WindowState.Closing)
                    {
                        state = WindowState.Closed;
                        OnClosed();
                    }
                    else
                    {
                        Debug.LogError("Window - UpdateUI - Invalid window state when finalizing animation!");
                    }
                }
            }
        }

        // IWindow
        public bool Enable()
        {
            if (!_windowEnabled)
            {
                _windowEnabled = true;
                if (isVisible)
                {
                    canvasGroup.interactable = true;
                }
                return true;
            }
            return false;
        }

        public bool Disable()
        {
            if (_windowEnabled)
            {
                _windowEnabled = false;
                if (isVisible)
                {
                    canvasGroup.interactable = false;
                }
                return true;
            }
            return false;
        }

        public bool SetWaiting(bool waiting)
        {
            if (waiting)
            {
                if (Disable())
                {
                    ShowWaitingIndicator(waiting);
                    return true;
                }
            }
            else
            {
                if (Enable())
                {
                    ShowWaitingIndicator(waiting);
                    return true;
                }
            }
            return false;
        }        

        public bool Open(in WindowAnimation animation)
        {
            return Open(false);
        }

        public bool Open()
        {
            return Open(true);
        }        

        public bool SetData(object data)
        {
            if (requiresData)
            {
                if (OnUpdateData(data))
                {
                    this.data = data;
                    return true;
                }
                Debug.LogWarning(GetType().ToString() + " - UpdateData - data can only bet set via Window.Open(object data)");
            }
            Debug.LogWarning(GetType().ToString() + " - UpdateData - Window does not require any data");
            return false;
        }

        public abstract bool IsValidData(object data);

        public bool Close(in WindowAnimation animation)
        {
            return Close(false);
        }

        public bool Close()
        {
            return Close(true);
        }

        // UGUIWindow
        public virtual bool ShowWaitingIndicator(bool show)
        {
            return false;
        }

        protected abstract bool OnUpdateData(object data);

        private bool Open(bool force)
        {
            if (state == WindowState.Closed || state == WindowState.Closing)
            {
                this.data = data;
                gameObject.SetActive(true);
                CanvasGroup c = canvasGroup;
                if (force)
                {
                    state = WindowState.Open;
                    c.alpha = 1.0F;
                    //c.interactable = true;
                    c.blocksRaycasts = true;
                }
                else
                {
                    state = WindowState.Opening;
                    _fade = true;
                    _startAlpha = canvasGroup.alpha;
                    _targetAlpha = 1.0F;
                    _fadeTime = 0.0F;
                }
                //c.blocksRaycasts = true;
                //OnOpen(this.data);
                //if (force)
                //{
                //    OnOpened();
                //}
                return true;
            }
            return false;
        }

        protected abstract void OnOpen(object data);

        protected virtual void OnOpened()
        {
            return;
        }

        private bool Close(bool force)
        {
            if (state == WindowState.Open || state == WindowState.Opening ||
                (force && (canvasGroup.alpha > 0.0F || gameObject.activeSelf)))
            {
                if (force)
                {
                    state = WindowState.Closed;
                    canvasGroup.alpha = 0.0F;
                }
                else
                {
                    state = WindowState.Closing;
                    _fade = true;
                    _startAlpha = canvasGroup.alpha;
                    _targetAlpha = 0.0F;
                    _fadeTime = 0.0F;
                }
                //canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                OnClose();
                if (force)
                {
                    OnClosed();
                }
                return true;
            }
            return false;
        }

        protected virtual void OnClose()
        {
            return;
        }

        protected virtual void OnClosed()
        {
            gameObject.SetActive(false);
        }

        protected void RebuildLayout(RectTransform target = null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(target != null ? target : rectTransform);
        }
    }
}