using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Extension;

namespace UIFramework
{
    public abstract class Controller : MonoBehaviour, IUpdatable
    {
        public AccessState AccessState { get; private set; }

        public bool IsOpen { get { return AccessState == AccessState.Open || AccessState == AccessState.Opening; } }
        public bool IsVisible { get { return AccessState != AccessState.Closed; } }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    if (_isEnabled)
                    {
                        Enabled();
                    }
                    else
                    {
                        Disabled();
                    }
                }
            }
        }
        private bool _isEnabled = false;

        public virtual TimeMode TimeMode { get { return _timeMode; } protected set { _timeMode = value; } }
        [SerializeField] private TimeMode _timeMode = TimeMode.Scaled;

        [SerializeField] protected CollectorComponent[] ScreenCollectors = null;

        public Action OnOpenAction { get; set; } = null;
        public Action OnCloseAction { get; set; } = null;

        private WindowNavigation<IScreen> _navigation = null;
        private WindowTransitionManager _transitionManager = null;
        private History<TransitionAnimationParams> _transitionHistory = new History<TransitionAnimationParams>(16);

        private IScreen[] _screens = null;
        private AnimationPlayer _animationPlayer = null;

        public IScreen ActiveScreen
        {
            get
            {
                return _navigation.ActiveWindow;
            }
        }

        // Unity Messages
#if UNITY_EDITOR
        protected virtual void OnValidate()
        {

        }
#endif

        protected virtual void Awake()
        {
            UpdateManager.AddUpdatable(this);
        }

        protected virtual void Start()
        {
            
        }

        public void ManagedUpdate()
        {
            if (IsVisible)
            {
                float deltaTime;
                switch (TimeMode)
                {
                    default:
                    case TimeMode.Scaled:
                        deltaTime = Time.deltaTime;
                        break;
                    case TimeMode.Unscaled:
                        deltaTime = Time.unscaledDeltaTime;
                        break;
                }

                if (_screens != null)
                {
                    for (int i = 0; i < _screens.Length; i++)
                    {
                        if (_screens[i].IsVisible)
                        {
                            _screens[i].UpdateUI(deltaTime);
                        }
                    }
                }

                OnUpdate(deltaTime);
            }
        }

        protected virtual void OnDestroy()
        {
            UpdateManager.RemoveUpdatable(this);
        }

        protected virtual void OnApplicationFocus(bool hasFocus)
        {

        }

        // Controller
        protected virtual AccessAnimationPlayable CreateAccessPlayable(AccessOperation accessOperation, float length)
        {
            return default;
        }

        public void Init()
        {
            if (AccessState != AccessState.Unitialized)
            {
                throw new InvalidOperationException("Controller already initialized.");
            }

            if (ScreenCollectors == null)
            {
                throw new InvalidOperationException("screenCollectors are null.");
            }

            List<IScreen> screenList = new List<IScreen>();
            for (int i = 0; i < ScreenCollectors.Length; i++)
            {
                if (ScreenCollectors[i] == null)
                {
                    throw new NullReferenceException("Null screen collector on controller.");
                }
                screenList.AddRange(ScreenCollectors[i].Collect<IScreen>());
            }

            _screens = screenList.ToArray();
            Dictionary<Type, IScreen> screenDictionary = new Dictionary<Type, IScreen>();
            for (int i = 0; i < _screens.Length; i++)
            {
                Type type = _screens[i].GetType();
                if (!screenDictionary.ContainsKey(type))
                {
                    screenDictionary.Add(type, _screens[i]);
                    _screens[i].Init(this);
                    _screens[i].Close();
                }
                else
                {
                    throw new InvalidOperationException("Multiple instances of the same screen type have been found, " +
                        "please ensure all instances are of a unique type.");
                }
            }

            _navigation = new WindowNavigation<IScreen>(screenDictionary);
            _navigation.OnNavigationUpdate += OnNavigationUpdate;

            _transitionManager = new WindowTransitionManager(TimeMode);

            SetBackButtonActive(false);
            OnInit();

            AccessState = AccessState.Closed;
        }

        protected virtual void OnInit() { }
        protected virtual void OnUpdate(float deltaTime) { }

        protected abstract void SetBackButtonActive(bool active);

        protected virtual void Enabled() { }
        protected virtual void Disabled() { }

        public abstract void SetWaiting(bool waiting);

        public ScreenType OpenScreen<ScreenType>(float animationLength = 0.0F, bool excludeCurrentFromHistory = false) where ScreenType : IScreen
        {
            WindowNavigationEvent navigationEvent = NavigateToScreen<ScreenType>(excludeCurrentFromHistory);
            if (navigationEvent.Success)
            {
                AccessAnimation sourceWindowAnimation = navigationEvent.PreviousActiveWindow != null ? navigationEvent.PreviousActiveWindow.GetDefaultAccessAnimation() : null;
                AccessAnimation targetWindowAnimation = navigationEvent.ActiveWindow.GetDefaultAccessAnimation();

                TransitionAnimationParams transitionPlayable = TransitionAnimationParams.Custom(animationLength, EasingMode.EaseInOut,
                            sourceWindowAnimation, targetWindowAnimation, TransitionAnimationParams.SortPriority.Target);

                return OpenScreenInternal<ScreenType>(in transitionPlayable, navigationEvent.ActiveWindow, navigationEvent.PreviousActiveWindow, excludeCurrentFromHistory);
            }
            else
            {
                return (ScreenType)navigationEvent.ActiveWindow;
            }
        }

        public void OpenScreen(IScreen screen, float animationLength = 0.0F, bool excludeCurrentFromHistory = false)
        {
            WindowNavigationEvent navigationEvent = NavigateToScreen(screen, excludeCurrentFromHistory);
            if (navigationEvent.Success)
            {
                AccessAnimation sourceWindowAnimation = navigationEvent.PreviousActiveWindow != null ? navigationEvent.PreviousActiveWindow.GetDefaultAccessAnimation() : null;
                AccessAnimation targetWindowAnimation = navigationEvent.ActiveWindow.GetDefaultAccessAnimation();

                TransitionAnimationParams transitionPlayable = TransitionAnimationParams.Custom(animationLength, EasingMode.EaseInOut,
                            sourceWindowAnimation, targetWindowAnimation, TransitionAnimationParams.SortPriority.Target);

                OpenScreenInternal(in transitionPlayable, navigationEvent.ActiveWindow, navigationEvent.PreviousActiveWindow, excludeCurrentFromHistory);
            }
        }

        public ScreenType OpenScreen<ScreenType>(object data, float animationLength = 0.0F, bool excludeCurrentFromHistory = false) where ScreenType : IScreen
        {
            WindowNavigationEvent navigationEvent = NavigateToScreen<ScreenType>(excludeCurrentFromHistory);
            if (navigationEvent.Success)
            {
                AccessAnimation sourceWindowAnimation = navigationEvent.PreviousActiveWindow != null ? navigationEvent.PreviousActiveWindow.GetDefaultAccessAnimation() : null;
                AccessAnimation targetWindowAnimation = navigationEvent.ActiveWindow.GetDefaultAccessAnimation();

                TransitionAnimationParams transitionPlayable = TransitionAnimationParams.Custom(animationLength, EasingMode.EaseInOut,
                            sourceWindowAnimation, targetWindowAnimation, TransitionAnimationParams.SortPriority.Target);

                return OpenScreenInternal<ScreenType>(in transitionPlayable, navigationEvent.ActiveWindow, navigationEvent.PreviousActiveWindow, data, excludeCurrentFromHistory);
            }
            else
            {
                navigationEvent.ActiveWindow.SetData(data);
                return (ScreenType)navigationEvent.ActiveWindow;
            }
        }

        public void OpenScreen(IScreen screen, object data, float animationLength = 0.0F, bool excludeCurrentFromHistory = false)
        {
            WindowNavigationEvent navigationEvent = NavigateToScreen(screen, excludeCurrentFromHistory);
            if (navigationEvent.Success)
            {
                AccessAnimation sourceWindowAnimation = navigationEvent.PreviousActiveWindow != null ? navigationEvent.PreviousActiveWindow.GetDefaultAccessAnimation() : null;
                AccessAnimation targetWindowAnimation = navigationEvent.ActiveWindow.GetDefaultAccessAnimation();

                TransitionAnimationParams transitionPlayable = TransitionAnimationParams.Custom(animationLength, EasingMode.EaseInOut,
                            sourceWindowAnimation, targetWindowAnimation, TransitionAnimationParams.SortPriority.Target);

                OpenScreenInternal(in transitionPlayable, navigationEvent.ActiveWindow, navigationEvent.PreviousActiveWindow, data, excludeCurrentFromHistory);
            }
            else
            {
                navigationEvent.ActiveWindow.SetData(data);
            }
        }

        public ScreenType OpenScreen<ScreenType>(in AccessAnimationParams accessPlayable, bool excludeCurrentFromHistory = false) where ScreenType : IScreen
        {
            WindowNavigationEvent navigationEvent = NavigateToScreen<ScreenType>(excludeCurrentFromHistory);
            if (navigationEvent.Success)
            {
                AccessAnimation sourceWindowAnimation = navigationEvent.PreviousActiveWindow != null ? navigationEvent.PreviousActiveWindow.GetDefaultAccessAnimation() : null;

                TransitionAnimationParams transitionPlayable = TransitionAnimationParams.Custom(accessPlayable.Length, accessPlayable.EasingMode,
                            sourceWindowAnimation, accessPlayable.ImplicitAnimation, TransitionAnimationParams.SortPriority.Target);

                return OpenScreenInternal<ScreenType>(in transitionPlayable, navigationEvent.ActiveWindow, navigationEvent.PreviousActiveWindow, excludeCurrentFromHistory);
            }
            else
            {
                return (ScreenType)navigationEvent.ActiveWindow;
            }
        }

        public void OpenScreen(IScreen screen, in AccessAnimationParams accessPlayable, bool excludeCurrentFromHistory = false)
        {
            WindowNavigationEvent navigationEvent = NavigateToScreen(screen, excludeCurrentFromHistory);
            if (navigationEvent.Success)
            {
                AccessAnimation sourceWindowAnimation = navigationEvent.PreviousActiveWindow != null ? navigationEvent.PreviousActiveWindow.GetDefaultAccessAnimation() : null;

                TransitionAnimationParams transitionPlayable = TransitionAnimationParams.Custom(accessPlayable.Length, accessPlayable.EasingMode,
                            sourceWindowAnimation, accessPlayable.ImplicitAnimation, TransitionAnimationParams.SortPriority.Target);

                OpenScreenInternal(in transitionPlayable, navigationEvent.ActiveWindow, navigationEvent.PreviousActiveWindow, excludeCurrentFromHistory);
            }
        }

        public ScreenType OpenScreen<ScreenType>(object data, in AccessAnimationParams accessPlayable, bool excludeCurrentFromHistory = false) where ScreenType : IScreen
        {
            WindowNavigationEvent navigationEvent = NavigateToScreen<ScreenType>(excludeCurrentFromHistory);
            if (navigationEvent.Success)
            {
                AccessAnimation sourceWindowAnimation = navigationEvent.PreviousActiveWindow != null ? navigationEvent.PreviousActiveWindow.GetDefaultAccessAnimation() : null;

                TransitionAnimationParams transitionPlayable = TransitionAnimationParams.Custom(accessPlayable.Length, accessPlayable.EasingMode,
                            sourceWindowAnimation, accessPlayable.ImplicitAnimation, TransitionAnimationParams.SortPriority.Target);

                return OpenScreenInternal<ScreenType>(in transitionPlayable, navigationEvent.ActiveWindow, navigationEvent.PreviousActiveWindow, data, excludeCurrentFromHistory);
            }
            else
            {
                navigationEvent.ActiveWindow.SetData(data);
                return (ScreenType)navigationEvent.ActiveWindow;
            }
        }

        public void OpenScreen(IScreen screen, object data, in AccessAnimationParams accessPlayable, bool excludeCurrentFromHistory = false)
        {
            WindowNavigationEvent navigationEvent = NavigateToScreen(screen, excludeCurrentFromHistory);
            if (navigationEvent.Success)
            {
                AccessAnimation sourceWindowAnimation = navigationEvent.PreviousActiveWindow != null ? navigationEvent.PreviousActiveWindow.GetDefaultAccessAnimation() : null;

                TransitionAnimationParams transitionPlayable = TransitionAnimationParams.Custom(accessPlayable.Length, accessPlayable.EasingMode,
                            sourceWindowAnimation, accessPlayable.ImplicitAnimation, TransitionAnimationParams.SortPriority.Target);

                OpenScreenInternal(in transitionPlayable, navigationEvent.ActiveWindow, navigationEvent.PreviousActiveWindow, data, excludeCurrentFromHistory);
            }
            else
            {
                navigationEvent.ActiveWindow.SetData(data);
            }
        }

        public ScreenType OpenScreen<ScreenType>(in TransitionAnimationParams transitionPlayable, bool excludeCurrentFromHistory = false) where ScreenType : IScreen
        {
            WindowNavigationEvent navigationEvent = NavigateToScreen<ScreenType>(excludeCurrentFromHistory);
            if (navigationEvent.Success)
            {
                return OpenScreenInternal<ScreenType>(in transitionPlayable, navigationEvent.ActiveWindow, navigationEvent.PreviousActiveWindow, excludeCurrentFromHistory);
            }
            else
            {
                return (ScreenType)navigationEvent.ActiveWindow;
            }
        }

        public void OpenScreen(IScreen screen, in TransitionAnimationParams transitionPlayable, bool excludeCurrentFromHistory = false)
        {
            WindowNavigationEvent navigationEvent = NavigateToScreen(screen, excludeCurrentFromHistory);
            if (navigationEvent.Success)
            {
                OpenScreenInternal(in transitionPlayable, navigationEvent.ActiveWindow, navigationEvent.PreviousActiveWindow, excludeCurrentFromHistory);
            }
        }

        public ScreenType OpenScreen<ScreenType>(object data, in TransitionAnimationParams transitionPlayable, bool excludeCurrentFromHistory = false) where ScreenType : IScreen
        {
            WindowNavigationEvent navigationEvent = NavigateToScreen<ScreenType>(excludeCurrentFromHistory);
            if (navigationEvent.Success)
            {
                return OpenScreenInternal<ScreenType>(in transitionPlayable, navigationEvent.ActiveWindow, navigationEvent.PreviousActiveWindow, data, excludeCurrentFromHistory);
            }
            else
            {
                navigationEvent.ActiveWindow.SetData(data);
                return (ScreenType)navigationEvent.ActiveWindow;
            }
        }

        public void OpenScreen(IScreen screen, object data, in TransitionAnimationParams transitionPlayable, bool excludeCurrentFromHistory = false)
        {
            WindowNavigationEvent navigationEvent = NavigateToScreen(screen, excludeCurrentFromHistory);
            if (navigationEvent.Success)
            {
                OpenScreenInternal(in transitionPlayable, navigationEvent.ActiveWindow, navigationEvent.PreviousActiveWindow, data, excludeCurrentFromHistory);
            }
            else
            {
                navigationEvent.ActiveWindow.SetData(data);
            }
        }

        private ScreenType OpenScreenInternal<ScreenType>(in TransitionAnimationParams transitionPlayable, IWindow targetScreen, IWindow sourceScreen, object data, bool excludeCurrentFromHistory) where ScreenType : IScreen
        {
            OpenScreenInternal(in transitionPlayable, targetScreen, sourceScreen, data, excludeCurrentFromHistory);
            return (ScreenType)targetScreen;
        }

        private ScreenType OpenScreenInternal<ScreenType>(in TransitionAnimationParams transitionPlayable, IWindow targetScreen, IWindow sourceScreen, bool excludeCurrentFromHistory) where ScreenType : IScreen
        {
            OpenScreenInternal(in transitionPlayable, targetScreen, sourceScreen, excludeCurrentFromHistory);
            return (ScreenType)targetScreen;
        }

        private void OpenScreenInternal(in TransitionAnimationParams transitionPlayable, IWindow targetScreen, IWindow sourceScreen, object data, bool excludeCurrentFromHistory)
        {
            targetScreen.SetData(data);
            OpenScreenInternal(in transitionPlayable, targetScreen, sourceScreen, excludeCurrentFromHistory);
        }

        private void OpenScreenInternal(in TransitionAnimationParams transitionPlayable, IWindow targetScreen, IWindow sourceScreen, bool excludeCurrentFromHistory)
        {
            if (sourceScreen != null)
            {
                if (transitionPlayable.Length > 0.0F && (transitionPlayable.EntryAnimation != null || transitionPlayable.ExitAnimation != null))
                {
                    _transitionManager.Transition(in transitionPlayable, sourceScreen, targetScreen);
                }
                else
                {
                    _transitionManager.Transition(TransitionAnimationParams.None(), sourceScreen, targetScreen);
                }

                if (!excludeCurrentFromHistory)
                {
                    _transitionHistory.Push(transitionPlayable);
                }
            }
            else
            {
                if (transitionPlayable.Length > 0.0F && transitionPlayable.EntryAnimation != null)
                {
                    AccessAnimationPlayable animationPlayable = transitionPlayable.EntryAnimation.GetWindowAnimation(targetScreen).CreatePlayable(AccessOperation.Open, transitionPlayable.Length,
                        transitionPlayable.EasingMode, TimeMode);
                    targetScreen.Open(in animationPlayable);
                }
                else
                {
                    targetScreen.Open();
                }
            }

            if (sourceScreen == null)
            {
                OpenInternal(transitionPlayable.Length);
            }
        }

        private WindowNavigationEvent NavigateToScreen<ScreenType>(bool excludeCurrentFromHistory) where ScreenType : IScreen
        {
            return _navigation.Travel<ScreenType>(excludeCurrentFromHistory);
        }

        private WindowNavigationEvent NavigateToScreen(IScreen screen, bool excludeCurrentFromHistory)
        {
            return _navigation.Travel(screen, excludeCurrentFromHistory);
        }

        private void OpenInternal(float animationLength)
        {
            bool animateOpen = false;
            if (animationLength > 0.0F)
            {
                if (_animationPlayer != null)
                {
                    Debug.Log("Controller is already playing a close animation, the current animation is rewound.");
                    _animationPlayer.Rewind();
                    animateOpen = true;
                }
                else
                {
                    AccessAnimationPlayable playable = CreateAccessPlayable(AccessOperation.Open, animationLength);
                    if (playable.Animation != null)
                    {
                        _animationPlayer = AnimationPlayer.PlayAnimation(playable.Animation, playable.StartTime, playable.PlaybackMode, playable.EasingMode, playable.TimeMode, playable.PlaybackSpeed);
                        _animationPlayer.OnComplete += OnAnimationComplete;
                        animateOpen = true;
                    }
                }
            }

            if (animateOpen)
            {
                AccessState = AccessState.Opening;
                OnOpen();
                OnOpenAction?.Invoke();
            }
            else
            {
                if (_animationPlayer != null)
                {
                    Debug.Log("Open was called on a Controller without an animation while already playing a state animation, " +
                        "this may cause unexpected behviour of the UI.");
                    _animationPlayer.SetCurrentTime(0.0F);
                    _animationPlayer.Stop();
                    _animationPlayer.OnComplete -= OnAnimationComplete;
                    _animationPlayer = null;
                }

                AccessState = AccessState.Opening;
                OnOpen();
                OnOpenAction?.Invoke();
                AccessState = AccessState.Open;
                OnOpened();
            }
        }

        protected virtual void OnOpen() { }
        protected virtual void OnOpened() { }

        public bool CloseScreen()
        {
            WindowNavigationEvent navigationEvent = _navigation.Back();
            if (navigationEvent.Success)
            {
                TransitionAnimationParams transition = _transitionHistory.Pop();
                _transitionManager.ReverseTransition(in transition, navigationEvent.ActiveWindow, navigationEvent.PreviousActiveWindow);
                return true;
            }
            return false;
        }

        public bool CloseAll(float animationLength = 0.0F)
        {
            WindowNavigationEvent navigationEvent = _navigation.Clear();
            if (navigationEvent.Success)
            {
                CloseAllInternal(new AccessAnimationParams(animationLength, EasingMode.EaseInOut), navigationEvent.PreviousActiveWindow);
                return true;
            }
            return false;
        }

        public bool CloseAll(in AccessAnimationParams accessPlayable)
        {
            WindowNavigationEvent navigationEvent = _navigation.Clear();
            if (navigationEvent.Success)
            {
                CloseAllInternal(in accessPlayable, navigationEvent.PreviousActiveWindow);
                return true;
            }
            return false;
        }

        private void CloseAllInternal(in AccessAnimationParams accessPlayable, IWindow targetScreen)
        {
            _transitionManager.Terminate(Mathf.Approximately(accessPlayable.Length, 0.0F));

            if (accessPlayable.Length > 0.0F)
            {
                targetScreen.Close(accessPlayable.CreatePlayable(targetScreen, AccessOperation.Close, 0.0F, TimeMode));
            }
            else
            {
                targetScreen.Close();
            }

            CloseInternal(accessPlayable.Length);
        }

        private void CloseInternal(float animationLength)
        {
            bool animateClose = false;
            if (animationLength > 0.0F)
            {
                if (_animationPlayer != null)
                {
                    Debug.Log("Controller is already playing an open animation, the current animation is rewound.");
                    _animationPlayer.Rewind();
                    animateClose = true;
                }
                else
                {
                    AccessAnimationPlayable playable = CreateAccessPlayable(AccessOperation.Close, animationLength);
                    if (playable.Animation != null)
                    {
                        _animationPlayer = AnimationPlayer.PlayAnimation(playable.Animation, playable.StartTime, playable.PlaybackMode, playable.EasingMode, playable.TimeMode, playable.PlaybackSpeed);
                        _animationPlayer.OnComplete += OnAnimationComplete;
                        animateClose = true;
                    }
                }
            }

            if (animateClose)
            {
                AccessState = AccessState.Closing;
                OnClose();
                OnCloseAction?.Invoke();
            }
            else
            {
                if (_animationPlayer != null)
                {
                    Debug.Log("Open was called on a Controller without an animation while already playing a state animation, " +
                        "this may cause unexpected behviour of the UI.");
                    _animationPlayer.SetCurrentTime(0.0F);
                    _animationPlayer.Stop();
                    _animationPlayer.OnComplete -= OnAnimationComplete;
                    _animationPlayer = null;
                }

                AccessState = AccessState.Closing;
                OnClose();
                OnCloseAction?.Invoke();
                AccessState = AccessState.Closed;
                OnClosed();
            }
        }

        protected virtual void OnClose() { }
        protected virtual void OnClosed() { }

        public void StartNewHistoryGroup()
        {
            _navigation.StartNewHistoryGroup();
            _transitionHistory.StartNewGroup();
        }

        public void ClearLatestHistoryGroup()
        {
            WindowNavigationEvent navigationEvent = _navigation.ClearLatestHistoryGroup();
            if (navigationEvent.Success)
            {
                _transitionHistory.ClearLatestGroup();
            }
        }

        public void InsertHistory<ScreenType>(in TransitionAnimationParams transitionPlayable) where ScreenType : IScreen
        {
            WindowNavigationEvent navigationEvent = _navigation.InsertHistory<ScreenType>();
            if (navigationEvent.Success)
            {
                _transitionHistory.Push(transitionPlayable);
            }
        }

        public void ClearHistory()
        {
            WindowNavigationEvent navigationEvent = _navigation.ClearHistory();
            if (navigationEvent.Success)
            {
                _transitionHistory.Clear();
            }
        }

        private void OnAnimationComplete(IAnimation animation)
        {
            if (AccessState == AccessState.Opening)
            {
                AccessState = AccessState.Open;
                OnOpened();
            }
            else if (AccessState == AccessState.Closing)
            {
                AccessState = AccessState.Closed;
                OnClosed();
            }
            _animationPlayer.OnComplete -= OnAnimationComplete;
            _animationPlayer = null;
        }

        private void OnNavigationUpdate(WindowNavigationEvent navigationEvent)
        {
            if (navigationEvent.Success)
            {
                bool backButtonActive = navigationEvent.HistoryCount > 0;
                SetBackButtonActive(backButtonActive);
            }
        }
    }
}