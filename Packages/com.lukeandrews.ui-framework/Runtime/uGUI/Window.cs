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
        public AccessState AccessState { get; private set; } = AccessState.Unitialized;
        public AnimationPlayer.PlaybackData AccessAnimationPlaybackData { get; private set; } = default;
        public AccessAnimationPlayable AccessAnimationPlayable { get; private set; } = default;

        // IWindow
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
            _rectTransform.anchoredPosition = _activeAnchoredPosition;
            _rectTransform.localScale = Vector3.one;
            _rectTransform.localRotation = Quaternion.identity;
            _canvasGroup.alpha = 1.0F;
        }

        public void Init()
        {
            if (AccessState != AccessState.Unitialized)
            {
                throw new InvalidOperationException("Window already initialized.");
            }

            _activeAnchoredPosition = RectTransform.anchoredPosition;
            AccessState = AccessState.Closed;
            gameObject.SetActive(false);
            OnInit();
        }

        public bool Open(in AccessAnimationPlayable playable, IAccessibleAction onComplete = null)
        {
            if (AccessState == AccessState.Closing || AccessState == AccessState.Closed)
            {
                gameObject.SetActive(true);
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
                OnOpen();
                return true;
            }
            return false;
        }

        public bool Open()
        {
            if (AccessState == AccessState.Closing || AccessState == AccessState.Closed)
            {
                gameObject.SetActive(true);
                if (AccessState == AccessState.Closing)
                {
                    Debug.Log("Open was called on a Window without an animation while already playing a state animation, " +
                        "this may cause unexpected behviour of the UI.");
                    IsInteractable = true;
                    _animationPlayer.SetCurrentTime(0.0F);
                    _animationPlayer.Stop();
                    ClearAnimationReferences();
                    _onAccessAnimationComplete = null;
                }
                AccessState = AccessState.Open;
                OnOpen();
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
                OnClose();
                return true;
            }
            return false;
        }

        public bool Close()
        {
            if (AccessState == AccessState.Opening || AccessState == AccessState.Open)
            {
                if (AccessState == AccessState.Opening)
                {
                    Debug.Log("Close was called on a Window without an animation while already playing a state animation, " +
                        "this may cause unexpected behviour of the UI.");
                    IsInteractable = true;
                    _animationPlayer.SetCurrentTime(0.0F);
                    _animationPlayer.Stop();
                    ClearAnimationReferences();
                    _onAccessAnimationComplete = null;
                }

                AccessState = AccessState.Closed;
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
        public virtual void SetData(object data) { }
        public virtual bool IsValidData(object data) { return false; }

        // UGUIWindow
        public virtual bool ShowWaitingIndicator(bool show)
        {
            return false;
        }

        protected virtual void OnInit() { }

        protected virtual void OnOpen() { }

        protected virtual void OnOpened() { }

        protected virtual void OnClose() { }

        protected virtual void OnClosed() { }

        private void OnAnimationComplete(IAnimation animation)
        {
            ResetAnimatedProperties();
            ClearAnimationReferences();
            IsInteractable = true;
            if (AccessState == AccessState.Opening)
            {
                AccessState = AccessState.Open;
                OnOpened();
                _onAccessAnimationComplete?.Invoke(this);
                _onAccessAnimationComplete = null;
            }
            else if (AccessState == AccessState.Closing)
            {
                AccessState = AccessState.Closed;
                gameObject.SetActive(false);
                OnClosed();
                _onAccessAnimationComplete?.Invoke(this);
                _onAccessAnimationComplete = null;
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
            _animationPlayer.OnComplete = null;
            _animationPlayer = null;
            AccessAnimationPlaybackData.ReleaseReferences();
        }
    }
}