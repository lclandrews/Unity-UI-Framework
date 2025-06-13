using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Extension;
using UnityEngine.UI;

namespace UIFramework.UGUI
{
    [RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
    public abstract class Window : UIBehaviour, IWindow
    {
        // UGUI Window
        public RectTransform RectTransform
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

        public CanvasGroup CanvasGroup
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
        public AccessState AccessState { get; private set; } = AccessState.None;

        public event IAccessibleAction Opening = default;
        public event IAccessibleAction Opened = default;
        public event IAccessibleAction Closed = default;
        public event IAccessibleAction Closing = default;

        public AnimationPlayer.PlaybackData AccessAnimationPlaybackData { get; private set; } = default;
        public AccessAnimationPlayable AccessAnimationPlayable { get; private set; } = default;

        // IWindow
        public string Identifier { get { return _identifier; } }
        [SerializeField] private string _identifier;

        public bool IsVisible { get { return AccessState != AccessState.Closed; } }

        public virtual bool IsEnabled
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
                    IsInteractable = true;
                    CanvasGroup.interactable = true;
                }
                else
                {
                    IsInteractable = false;
                    CanvasGroup.interactable = false;
                }
            }
        }
        private int _isEnabledCounter = 1;

        public bool IsInteractable
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
                    CanvasGroup.blocksRaycasts = true;
                }
                else
                {
                    CanvasGroup.blocksRaycasts = false;
                }
            }
        }
        private int _isInteractableCounter = 1;

        public virtual int SortOrder
        {
            get
            {
                return _sortOrder;
            }
            set
            {
                _sortOrder = value;
                RectTransform.SetSiblingIndex(_sortOrder);
            }
        }
        private int _sortOrder = 0;

        // UGUI Window
        private bool _isWaiting = false;
        private AnimationPlayer _animationPlayer = null;
        private IAccessibleAction _onAccessAnimationComplete = null;
        protected Vector3 _activeAnchoredPosition { get; private set; } = Vector3.zero;

        private Dictionary<GenericWindowAnimationType, GenericWindowAnimation> _genericAnimations = new Dictionary<GenericWindowAnimationType, GenericWindowAnimation>();

        public override void Initialize()
        {
            if (State == BehaviourState.Initialized)
            {
                throw new InvalidOperationException("Window already initialized.");
            }

            base.Initialize();
            AccessState = AccessState.Closed;

            _activeAnchoredPosition = RectTransform.anchoredPosition;
            AccessState = AccessState.Closed;
            gameObject.SetActive(false);
            OnInitialize();
        }

        public override void Terminate()
        {
            if (State != BehaviourState.Initialized)
            {
                throw new InvalidOperationException("Window cannot be terminated.");
            }
            base.Terminate();

            Close();
            AccessState = AccessState.None;
            ResetAnimatedProperties();
            ClearAnimationReferences();
            _isEnabledCounter = 1;
            _isInteractableCounter = 1;
            _sortOrder = 0;
            _genericAnimations.Clear();
            OnTerminate();
        }

        // IWindow
        public virtual GenericWindowAnimation GetAnimation(GenericWindowAnimationType type)
        {
            GenericWindowAnimation animation;
            if(!_genericAnimations.TryGetValue(type, out animation))
            {
                Canvas canvas = GetComponentInParent<Canvas>(true);
                animation = new UGUIGenericWindowAnimation(canvas.transform as RectTransform, RectTransform, _activeAnchoredPosition, CanvasGroup, type);
                _genericAnimations.Add(type, animation);
            }
            return animation;
        }        

        public bool SetWaiting(bool waiting)
        {
            if (ShowWaitingIndicator(waiting))
            {
                if (waiting != _isWaiting)
                {
                    _isWaiting = waiting;
                    IsEnabled = !waiting;
                }
                return true;
            }
            return false;
        }

        // IAccessible
        public virtual AccessAnimation GetDefaultAccessAnimation()
        {
            return GetAnimation(GenericWindowAnimationType.Fade);
        }

        public virtual void ResetAnimatedProperties()
        {
            if(_rectTransform != null)
            {
                _rectTransform.anchoredPosition = _activeAnchoredPosition;
                _rectTransform.localScale = Vector3.one;
                _rectTransform.localRotation = Quaternion.identity;
            }            
            if(_canvasGroup != null)
            {
                _canvasGroup.alpha = 1.0F;
            }            
        }        

        public bool Open(in AccessAnimationPlayable playable, IAccessibleAction onComplete = null)
        {
            if (AccessState == AccessState.Closing || AccessState == AccessState.Closed)
            {
                if (_animationPlayer != null)
                {
                    Debug.Log("Window is already playing a close animation, the provided open animation is ignored and the current close animation is rewound.");
                    _animationPlayer.Rewind();
                }
                else
                {
                    IsInteractable = false;
                    AccessAnimationPlayable = playable;
                    _animationPlayer = AnimationPlayer.PlayAnimation(playable.Animation, playable.StartTime, playable.PlaybackMode, playable.EasingMode, playable.TimeMode, playable.PlaybackSpeed);
                    _animationPlayer.OnComplete += OnAnimationComplete;
                    AccessAnimationPlaybackData = _animationPlayer.Data;
                }
                _onAccessAnimationComplete = onComplete;
                AccessState = AccessState.Opening;
                gameObject.SetActive(true);
                Opening?.Invoke(this);
                OnOpen();
                return true;
            }
            return false;
        }

        public bool Open()
        {
            if (AccessState == AccessState.Closing || AccessState == AccessState.Closed || AccessState == AccessState.Opening)
            {
                if (AccessState == AccessState.Closing || AccessState == AccessState.Opening)
                {
                    Debug.Log("Open was called on a Window without an animation while already playing a state animation, " +
                        "this may cause unexpected behviour of the UI.");
                    IsInteractable = true;

                    float time = 0.0F;
                    if (AccessState == AccessState.Closing) time = 0.0F;
                    else if (AccessState == AccessState.Opening) time = 1.0F;

                    _animationPlayer.SetCurrentTime(time);
                    _animationPlayer.Stop();
                    ClearAnimationReferences();
                    _onAccessAnimationComplete = null;
                }
                AccessState previousState = AccessState;
                AccessState = AccessState.Open;
                if (previousState != AccessState.Opening)
                {
                    gameObject.SetActive(true);
                    Opening?.Invoke(this);
                    OnOpen();
                }
                Opened?.Invoke(this);
                OnOpened();
                return true;
            }
            return false;
        }

        public bool Close(in AccessAnimationPlayable playable, IAccessibleAction onComplete = null)
        {
            if (AccessState == AccessState.Opening || AccessState == AccessState.Open)
            {
                if (_animationPlayer != null)
                {
                    Debug.Log("Window is already playing a open animation, the provided open animation is ignored and the current close animation is rewound.");
                    _animationPlayer.Rewind();
                }
                else
                {
                    IsInteractable = false;
                    AccessAnimationPlayable = playable;
                    _animationPlayer = AnimationPlayer.PlayAnimation(playable.Animation, playable.StartTime, playable.PlaybackMode, playable.EasingMode, playable.TimeMode, playable.PlaybackSpeed);
                    _animationPlayer.OnComplete += OnAnimationComplete;
                    AccessAnimationPlaybackData = _animationPlayer.Data;
                }
                _onAccessAnimationComplete = onComplete;
                AccessState = AccessState.Closing;
                Closing?.Invoke(this);
                OnClose();
                return true;
            }
            return false;
        }

        public bool Close()
        {
            if (AccessState == AccessState.Opening || AccessState == AccessState.Open || AccessState == AccessState.Closing)
            {
                if (AccessState == AccessState.Opening || AccessState == AccessState.Closing)
                {
                    Debug.Log("Close was called on a Window without an animation while already playing a state animation, " +
                        "this may cause unexpected behviour of the UI.");
                    IsInteractable = true;

                    float time = 0.0F;
                    if (AccessState == AccessState.Opening) time = 0.0F;
                    else if (AccessState == AccessState.Closing) time = 1.0F;

                    _animationPlayer.SetCurrentTime(time);
                    _animationPlayer.Stop();
                    ClearAnimationReferences();
                    _onAccessAnimationComplete = null;
                }
                AccessState previousState = AccessState;
                AccessState = AccessState.Closed;
                if (previousState != AccessState.Closing)
                {
                    Closing?.Invoke(this);
                    OnClose();
                }
                gameObject.SetActive(false);
                Closed?.Invoke(this);
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
        public virtual void SetData(object data) { }
        public virtual bool IsValidData(object data) { return false; }

        // UGUIWindow
        public virtual bool ShowWaitingIndicator(bool show)
        {
            return false;
        }

        protected virtual void OnInitialize() { }

        protected virtual void OnOpen() { }
        protected virtual void OnOpened() { }
        protected virtual void OnClose() { }
        protected virtual void OnClosed() { }

        protected virtual void OnTerminate() { }

        private void OnAnimationComplete(IAnimation animation)
        {
            ResetAnimatedProperties();
            ClearAnimationReferences();
            IsInteractable = true;
            if (AccessState == AccessState.Opening)
            {
                AccessState = AccessState.Open;
                Opened?.Invoke(this);
                OnOpened();
                IAccessibleAction action = _onAccessAnimationComplete;
                _onAccessAnimationComplete = null;
                action?.Invoke(this);
            }
            else if (AccessState == AccessState.Closing)
            {
                AccessState = AccessState.Closed;
                gameObject.SetActive(false);
                Closed?.Invoke(this);
                OnClosed();
                IAccessibleAction action = _onAccessAnimationComplete;
                _onAccessAnimationComplete = null;
                action?.Invoke(this);
            }
            else
            {
                throw new Exception(string.Format("Window animation complete while in unexpected state: {0}", AccessState.ToString()));
            }
        }

        protected void RebuildLayout(RectTransform target = null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(target != null ? target : RectTransform);
        }

        private void ClearAnimationReferences()
        {
            if(_animationPlayer != null)
            {
                _animationPlayer.OnComplete = null;
                _animationPlayer = null;                
            }
            AccessAnimationPlaybackData.ReleaseReferences();
        }
    }
}