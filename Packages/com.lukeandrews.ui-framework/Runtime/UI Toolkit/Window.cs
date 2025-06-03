using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Extension;
using UnityEngine.UIElements;

namespace UIFramework.UIToolkit
{
    [RequireComponent(typeof(UIDocument))]
    public abstract class Window : UIBehaviour, IWindow
    {
        private const string _NonInteractiveClassName = ".non-interactive";

        // UI Toolkit Window
        public UIDocument UIDocument { get { return _uiDocument; } }
        private UIDocument _uiDocument = null;

        public VisualElement VisualElement { get { return _visualElement; } }
        private VisualElement _visualElement = null;

        // IAccessible
        public AccessState AccessState { get; private set; } = AccessState.Unitialized;

        public event IAccessibleAction Opening = default;
        public event IAccessibleAction Opened = default;
        public event IAccessibleAction Closed = default;
        public event IAccessibleAction Closing = default;

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
                    VisualElement.SetEnabled(true);
                }
                else
                {
                    IsInteractable = false;
                    VisualElement.SetEnabled(false);
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
                    SetInteractable(VisualElement, true);
                }
                else
                {
                    SetInteractable(VisualElement, false);
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

                VisualElement.Hierarchy parentHierarchy = VisualElement.parent.hierarchy;
                int newIndexInHierarchy = Mathf.Clamp(_sortOrder, 0, parentHierarchy.childCount - 1);
                int existingIndexInHierarchy = parentHierarchy.IndexOf(VisualElement);
                if (newIndexInHierarchy != existingIndexInHierarchy)
                {
                    VisualElement.PlaceInFront(parentHierarchy.ElementAt(newIndexInHierarchy));
                }

                UIDocument.sortingOrder = _sortOrder;
            }
        }
        private int _sortOrder = 0;

        // UI Toolkit Window
        private bool _isWaiting = false;
        private AnimationPlayer _animationPlayer = null;
        private IAccessibleAction _onAccessAnimationComplete = null;

        private Dictionary<GenericWindowAnimationType, GenericWindowAnimation> _genericAnimations = new Dictionary<GenericWindowAnimationType, GenericWindowAnimation>();

        protected Window() { }

        public Window(UIDocument uiDocument, VisualElement visualElement)
        {
            _uiDocument = uiDocument;
            _visualElement = visualElement;
        }

        // IWindow
        public GenericWindowAnimation GetAnimation(GenericWindowAnimationType type)
        {
            GenericWindowAnimation animation;
            if (!_genericAnimations.TryGetValue(type, out animation))
            {
                animation = new UITKGenericWindowAnimation(VisualElement, type);
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
            _visualElement.style.opacity = 1;
            _visualElement.style.translate = new Translate(Length.Percent(0.0F), Length.Percent(0.0F));
            _visualElement.style.scale = new Scale(Vector2.one);
            _visualElement.style.rotate = new Rotate(0.0F);
        }

        public void Init()
        {
            if (AccessState != AccessState.Unitialized)
            {
                throw new InvalidOperationException("Window already initialized.");
            }

            AccessState = AccessState.Closed;
            SetActive(false);
            OnInit();
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
                SetActive(true);
                Opening?.Invoke(this);
                OnOpen();
                return true;
            }
            return false;
        }

        public bool Open()
        {
            if (AccessState == AccessState.Closing || AccessState == AccessState.Closed)
            {
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
                SetActive(true);
                Opening?.Invoke(this);
                OnOpen();
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
                Closing?.Invoke(this);
                OnClose();
                SetActive(false);
                Closed?.Invoke(this);
                OnClosed();
                return true;
            }
            return false;
        }

        public bool SkipAccessAnimation()
        {
            if(_animationPlayer != null)
            {
                _animationPlayer.Complete();
                return true;
            }
            return false;
        }

        // IDataRecipient
        public virtual void SetData(object data) { }

        public virtual bool IsValidData(object data) { return false; }

        // UIToolkitWindow
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
                Opened?.Invoke(this);
                OnOpened();
                _onAccessAnimationComplete?.Invoke(this);
                _onAccessAnimationComplete = null;
            }
            else if (AccessState == AccessState.Closing)
            {
                AccessState = AccessState.Closed;
                SetActive(false);
                Closed?.Invoke(this);
                OnClosed();
                _onAccessAnimationComplete?.Invoke(this);
                _onAccessAnimationComplete = null;
            }
            else
            {
                throw new Exception(string.Format("Window animation complete while in unexpected state: {0}", AccessState.ToString()));
            }
        }

        protected void SetActive(bool active)
        {
            VisualElement.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
        }

        protected void SetInteractable(VisualElement visualElement, bool interactable)
        {
            PickingMode pickingMode = interactable ? PickingMode.Position : PickingMode.Ignore;
            SetInteractablePickingMode(visualElement, pickingMode);
            visualElement.IterateHierarchy(delegate (VisualElement child)
            {
                SetInteractablePickingMode(child, pickingMode);
            });
        }

        private void SetInteractablePickingMode(VisualElement visualElement, PickingMode pickingMode)
        {
            if(pickingMode == PickingMode.Position)
            {
                bool isNonInteractive = false;
                List<string> classList = visualElement.GetClasses() as List<string>;
                if (classList != null)
                {
                    isNonInteractive = classList.Contains(_NonInteractiveClassName);
                }
                else
                {
                    // Linq as fallback incase class list type changes in the future
                    isNonInteractive = visualElement.GetClasses().Contains(_NonInteractiveClassName);
                    Debug.LogWarning("VisualElement.GetClasses() no longer returns a List<string>.");
                }

                if(isNonInteractive)
                {
                    visualElement.pickingMode = PickingMode.Position;
                    visualElement.RemoveFromClassList(_NonInteractiveClassName);
                }
            }
            else if(visualElement.pickingMode == PickingMode.Position)
            {
                visualElement.pickingMode = PickingMode.Ignore;
                visualElement.AddToClassList(_NonInteractiveClassName);
            }            
        }

        private void ClearAnimationReferences()
        {
            _animationPlayer.OnComplete = null;
            _animationPlayer = null;
            AccessAnimationPlaybackData.ReleaseReferences();
        }
    }
}