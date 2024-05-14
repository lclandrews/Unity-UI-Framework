using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIFramework.UGUI
{
    [RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
    public abstract class Window : UIBehaviour, IWindow
    {
        // UGUI Window
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

        // IAccessible
        public AccessState accessState { get; private set; } = AccessState.Unitialized;
        public AnimationPlayer.PlaybackData accessAnimationPlaybackData { get; private set; } = default;
        public AnimationPlayable accessAnimationPlayable { get; private set; } = default;

        // IWindow
        public bool isVisible { get { return accessState != AccessState.Closed; } }

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

        public virtual int sortOrder
        {
            get
            {
                return _sortOrder;
            }
            set
            {
                _sortOrder = value;
                rectTransform.SetSiblingIndex(_sortOrder);
            }
        }
        private int _sortOrder = 0;

        // IDataRecipient
        public abstract bool requiresData { get; }
        public object data { get; private set; } = null;
        
        // UGUI Window
        private bool _isWaiting = false;
        private AnimationPlayer _animationPlayer = null;
        private IAccessibleAction _onAccessAnimationComplete = null;
        protected Vector3 _activeAnchoredPosition { get; private set; } = Vector3.zero;

        // IWindow
        public virtual GenericWindowAnimationBase CreateAnimation(GenericWindowAnimationType type, float length)
        {
            Canvas canvas = GetComponentInParent<Canvas>(true);
            return new GenericWindowAnimation(canvas.transform as RectTransform, rectTransform, _activeAnchoredPosition, canvasGroup, type, length);
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

        // IAccessible
        public virtual WindowAccessAnimation CreateDefaultAccessAnimation(float length)
        {
            return CreateAnimation(GenericWindowAnimationType.Fade, length);
        }

        public virtual void ResetAnimatedProperties()
        {
            _rectTransform.anchoredPosition = _activeAnchoredPosition;
            _rectTransform.localScale = Vector3.one;
            _rectTransform.localRotation = Quaternion.identity;
            _canvasGroup.alpha = 1.0F;
        }

        public void Init()
        {
            if (accessState != AccessState.Unitialized)
            {
                throw new InvalidOperationException("Window already initialized.");
            }

            _activeAnchoredPosition = rectTransform.anchoredPosition;
            accessState = AccessState.Closed;
            gameObject.SetActive(false);
            OnInit();
        }        

        public bool Open(in AnimationPlayable animationPlayable, IAccessibleAction onComplete = null)
        {
            if (accessState == AccessState.Closing || accessState == AccessState.Closed)
            {
                if (_animationPlayer != null)
                {
                    Debug.Log("Window is already playing a close animation, the provided open animation is ignored and the current close animation is rewound.");
                    _animationPlayer.Rewind();
                }
                else
                {
                    isInteractable = false;
                    accessAnimationPlayable = animationPlayable;                   
                    _animationPlayer = AnimationPlayer.PlayAnimation(in animationPlayable);
                    _animationPlayer.onComplete += OnAnimationComplete;
                    accessAnimationPlaybackData = _animationPlayer.playbackData;
                }
                _onAccessAnimationComplete = onComplete;
                accessState = AccessState.Opening;
                gameObject.SetActive(true);
                OnOpen();
                return true;
            }
            return false;
        }

        public bool Open()
        {
            if (accessState == AccessState.Closing || accessState == AccessState.Closed)
            {
                if(accessState == AccessState.Closing)
                {
                    Debug.Log("Open was called on a Window without an animation while already playing a state animation, " +
                        "this may cause unexpected behviour of the UI.");
                    isInteractable = true;
                    _animationPlayer.SetCurrentTime(0.0F);
                    _animationPlayer.Stop();
                    ClearAnimationReferences();
                    _onAccessAnimationComplete = null;
                }

                accessState = AccessState.Open;
                gameObject.SetActive(true);                
                OnOpen();
                OnOpened();
                return true;
            }
            return false;
        }        

        public bool Close(in AnimationPlayable animationPlayable, IAccessibleAction onComplete = null)
        {
            if (accessState == AccessState.Opening || accessState == AccessState.Open)
            {
                if (_animationPlayer != null)
                {
                    Debug.Log("Window is already playing a open animation, the provided open animation is ignored and the current close animation is rewound.");
                    _animationPlayer.Rewind();
                }
                else
                {                    
                    isInteractable = false;
                    accessAnimationPlayable = animationPlayable;
                    _animationPlayer = AnimationPlayer.PlayAnimation(in animationPlayable);
                    _animationPlayer.onComplete += OnAnimationComplete;
                    accessAnimationPlaybackData = _animationPlayer.playbackData;
                }
                _onAccessAnimationComplete = onComplete;
                accessState = AccessState.Closing;
                OnClose();
                return true;
            }
            return false;
        }

        public bool Close()
        {
            if (accessState == AccessState.Opening || accessState == AccessState.Open)
            {
                if(accessState == AccessState.Opening)
                {
                    Debug.Log("Close was called on a Window without an animation while already playing a state animation, " +
                        "this may cause unexpected behviour of the UI.");
                    isInteractable = true;
                    _animationPlayer.SetCurrentTime(0.0F);
                    _animationPlayer.Stop();
                    ClearAnimationReferences();
                    _onAccessAnimationComplete = null;
                }

                accessState = AccessState.Closed;
                OnClose();
                gameObject.SetActive(false);
                OnClosed();
                return true;
            }
            return false;
        }

        public bool SkipAccessAnimation()
        {
            if (_animationPlayer != null)
            {
                _animationPlayer.Complete();
                return true;
            }
            return false;
        }

        // IDataRecipient
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

        private void OnAnimationComplete(Animation animation)
        {
            ResetAnimatedProperties();
            ClearAnimationReferences();
            isInteractable = true;
            if(accessState == AccessState.Opening)
            {
                accessState = AccessState.Open;
                OnOpened();
                _onAccessAnimationComplete?.Invoke(this);
                _onAccessAnimationComplete = null;
            }
            else if(accessState == AccessState.Closing)
            {
                accessState = AccessState.Closed;
                gameObject.SetActive(false);
                OnClosed();
                _onAccessAnimationComplete?.Invoke(this);
                _onAccessAnimationComplete = null;
            }
            else
            {
                throw new Exception(string.Format("Window animation complete while in unexpected state: {0}", accessState.ToString()));
            }
        }

        protected void RebuildLayout(RectTransform target = null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(target != null ? target : rectTransform);
        }

        private void ClearAnimationReferences()
        {
            _animationPlayer.onComplete = null;
            _animationPlayer = null;
            accessAnimationPlaybackData.ReleaseReferences();            
        }
    }
}