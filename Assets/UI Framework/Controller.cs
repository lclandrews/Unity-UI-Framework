using System;
using System.Collections.Generic;

using UnityEngine;

namespace UIFramework
{
    public enum TimeMode
    {
        Scaled,
        Unscaled
    }

    public abstract class Controller : MonoBehaviour
    {
        public AccessState accessState { get; private set; }

        public bool isOpen { get { return accessState == AccessState.Open || accessState == AccessState.Opening; } }
        public bool isVisible { get { return accessState != AccessState.Closed; } }

        public bool isEnabled
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

        public virtual TimeMode timeMode { get { return _timeMode; } protected set { _timeMode = value; } }
        [SerializeField] private TimeMode _timeMode = TimeMode.Scaled;

        [SerializeField] protected CollectorComponent[] screenCollectors = null;

        public Action onOpen { get; set; } = null;
        public Action onClose { get; set; } = null;

        private WindowNavigation<IScreen> _navigation = null;
        private WindowTransitionManager _transitionManager = null;
        private History<WindowTransitionPlayable> _transitionHistory = new History<WindowTransitionPlayable>(16);

        private IScreen[] _screens = null;
        private AnimationPlayer _animationPlayer = null;

        public IScreen activeScreen
        {
            get
            {
                return _navigation.activeWindow;
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

        }

        protected virtual void Start()
        {

        }

        private void Update()
        {
            if (isVisible)
            {
                float deltaTime;
                switch (timeMode)
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
                        if (_screens[i].isVisible)
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

        }

        protected virtual void OnApplicationFocus(bool hasFocus)
        {

        }

        // ScreenController
        protected virtual AnimationPlayable CreateAccessPlayable(AccessOperation accessOperation, float length)
        {
            return default;
        }

        public void Init()
        {
            if (accessState != AccessState.Unitialized)
            {
                throw new InvalidOperationException("Controller already initialized.");
            }

            if (screenCollectors == null)
            {
                throw new InvalidOperationException("screenCollectors are null.");
            }

            List<IScreen> screenList = new List<IScreen>();
            for (int i = 0; i < screenCollectors.Length; i++)
            {
                if (screenCollectors[i] == null)
                {
                    throw new NullReferenceException("Null screen collector on controller.");
                }
                screenList.AddRange(screenCollectors[i].Collect<IScreen>());
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
            _navigation.onNavigationUpdate += OnNavigationUpdate;

            _transitionManager = new WindowTransitionManager(timeMode);

            SetBackButtonActive(false);
            OnInit();

            accessState = AccessState.Closed;
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
            if (navigationEvent.success)
            {
                Animation sourceWindowAnimation = navigationEvent.previousActiveWindow != null ?
                navigationEvent.previousActiveWindow.CreateDefaultAccessAnimation(animationLength) : null;
                Animation targetWindowAnimation = navigationEvent.activeWindow.CreateDefaultAccessAnimation(animationLength);

                WindowTransitionPlayable transitionPlayable = WindowTransitionPlayable.Custom(animationLength, EasingMode.EaseInOut,
                            sourceWindowAnimation, targetWindowAnimation, WindowTransitionPlayable.SortPriority.Target);

                return OpenScreenInternal<ScreenType>(in transitionPlayable, navigationEvent.activeWindow, navigationEvent.previousActiveWindow, excludeCurrentFromHistory);
            }
            else
            {
                return (ScreenType)navigationEvent.activeWindow;
            }
        }

        public void OpenScreen(IScreen screen, float animationLength = 0.0F, bool excludeCurrentFromHistory = false)
        {
            WindowNavigationEvent navigationEvent = NavigateToScreen(screen, excludeCurrentFromHistory);
            if (navigationEvent.success)
            {
                Animation sourceWindowAnimation = navigationEvent.previousActiveWindow != null ?
                navigationEvent.previousActiveWindow.CreateDefaultAccessAnimation(animationLength) : null;
                Animation targetWindowAnimation = navigationEvent.activeWindow.CreateDefaultAccessAnimation(animationLength);

                WindowTransitionPlayable transitionPlayable = WindowTransitionPlayable.Custom(animationLength, EasingMode.EaseInOut,
                            sourceWindowAnimation, targetWindowAnimation, WindowTransitionPlayable.SortPriority.Target);

                OpenScreenInternal(in transitionPlayable, navigationEvent.activeWindow, navigationEvent.previousActiveWindow, excludeCurrentFromHistory);
            }
        }

        public ScreenType OpenScreen<ScreenType>(object data, float animationLength = 0.0F, bool excludeCurrentFromHistory = false) where ScreenType : IScreen
        {
            WindowNavigationEvent navigationEvent = NavigateToScreen<ScreenType>(excludeCurrentFromHistory);
            if (navigationEvent.success)
            {
                Animation sourceWindowAnimation = navigationEvent.previousActiveWindow != null ?
                navigationEvent.previousActiveWindow.CreateDefaultAccessAnimation(animationLength) : null;
                Animation targetWindowAnimation = navigationEvent.activeWindow.CreateDefaultAccessAnimation(animationLength);

                WindowTransitionPlayable transitionPlayable = WindowTransitionPlayable.Custom(animationLength, EasingMode.EaseInOut,
                            sourceWindowAnimation, targetWindowAnimation, WindowTransitionPlayable.SortPriority.Target);

                return OpenScreenInternal<ScreenType>(in transitionPlayable, navigationEvent.activeWindow, navigationEvent.previousActiveWindow, data, excludeCurrentFromHistory);
            }
            else
            {
                navigationEvent.activeWindow.SetData(data);
                return (ScreenType)navigationEvent.activeWindow;
            }
        }

        public void OpenScreen(IScreen screen, object data, float animationLength = 0.0F, bool excludeCurrentFromHistory = false)
        {
            WindowNavigationEvent navigationEvent = NavigateToScreen(screen, excludeCurrentFromHistory);
            if (navigationEvent.success)
            {
                Animation sourceWindowAnimation = navigationEvent.previousActiveWindow != null ?
                navigationEvent.previousActiveWindow.CreateDefaultAccessAnimation(animationLength) : null;
                Animation targetWindowAnimation = navigationEvent.activeWindow.CreateDefaultAccessAnimation(animationLength);

                WindowTransitionPlayable transitionPlayable = WindowTransitionPlayable.Custom(animationLength, EasingMode.EaseInOut,
                            sourceWindowAnimation, targetWindowAnimation, WindowTransitionPlayable.SortPriority.Target);

                OpenScreenInternal(in transitionPlayable, navigationEvent.activeWindow, navigationEvent.previousActiveWindow, data, excludeCurrentFromHistory);
            }
            else
            {
                navigationEvent.activeWindow.SetData(data);
            }
        }

        public ScreenType OpenScreen<ScreenType>(in WindowAccessPlayable accessPlayable, bool excludeCurrentFromHistory = false) where ScreenType : IScreen
        {
            WindowNavigationEvent navigationEvent = NavigateToScreen<ScreenType>(excludeCurrentFromHistory);
            if (navigationEvent.success)
            {
                Animation sourceWindowAnimation = navigationEvent.previousActiveWindow != null ?
                navigationEvent.previousActiveWindow.CreateDefaultAccessAnimation(accessPlayable.length) : null;

                WindowTransitionPlayable transitionPlayable = WindowTransitionPlayable.Custom(accessPlayable.length, accessPlayable.easingMode,
                            sourceWindowAnimation, accessPlayable.animation, WindowTransitionPlayable.SortPriority.Target);

                return OpenScreenInternal<ScreenType>(in transitionPlayable, navigationEvent.activeWindow, navigationEvent.previousActiveWindow, excludeCurrentFromHistory);
            }
            else
            {
                return (ScreenType)navigationEvent.activeWindow;
            }
        }

        public void OpenScreen(IScreen screen, in WindowAccessPlayable accessPlayable, bool excludeCurrentFromHistory = false)
        {
            WindowNavigationEvent navigationEvent = NavigateToScreen(screen, excludeCurrentFromHistory);
            if (navigationEvent.success)
            {
                Animation sourceWindowAnimation = navigationEvent.previousActiveWindow != null ?
                navigationEvent.previousActiveWindow.CreateDefaultAccessAnimation(accessPlayable.length) : null;

                WindowTransitionPlayable transitionPlayable = WindowTransitionPlayable.Custom(accessPlayable.length, accessPlayable.easingMode,
                            sourceWindowAnimation, accessPlayable.animation, WindowTransitionPlayable.SortPriority.Target);

                OpenScreenInternal(in transitionPlayable, navigationEvent.activeWindow, navigationEvent.previousActiveWindow, excludeCurrentFromHistory);
            }
        }

        public ScreenType OpenScreen<ScreenType>(object data, in WindowAccessPlayable accessPlayable, bool excludeCurrentFromHistory = false) where ScreenType : IScreen
        {
            WindowNavigationEvent navigationEvent = NavigateToScreen<ScreenType>(excludeCurrentFromHistory);
            if (navigationEvent.success)
            {
                Animation sourceWindowAnimation = navigationEvent.previousActiveWindow != null ?
                navigationEvent.previousActiveWindow.CreateDefaultAccessAnimation(accessPlayable.length) : null;

                WindowTransitionPlayable transitionPlayable = WindowTransitionPlayable.Custom(accessPlayable.length, accessPlayable.easingMode,
                            sourceWindowAnimation, accessPlayable.animation, WindowTransitionPlayable.SortPriority.Target);

                return OpenScreenInternal<ScreenType>(in transitionPlayable, navigationEvent.activeWindow, navigationEvent.previousActiveWindow, data, excludeCurrentFromHistory);
            }
            else
            {
                navigationEvent.activeWindow.SetData(data);
                return (ScreenType)navigationEvent.activeWindow;
            }
        }

        public void OpenScreen(IScreen screen, object data, in WindowAccessPlayable accessPlayable, bool excludeCurrentFromHistory = false)
        {
            WindowNavigationEvent navigationEvent = NavigateToScreen(screen, excludeCurrentFromHistory);
            if (navigationEvent.success)
            {
                Animation sourceWindowAnimation = navigationEvent.previousActiveWindow != null ?
                navigationEvent.previousActiveWindow.CreateDefaultAccessAnimation(accessPlayable.length) : null;

                WindowTransitionPlayable transitionPlayable = WindowTransitionPlayable.Custom(accessPlayable.length, accessPlayable.easingMode,
                            sourceWindowAnimation, accessPlayable.animation, WindowTransitionPlayable.SortPriority.Target);

                OpenScreenInternal(in transitionPlayable, navigationEvent.activeWindow, navigationEvent.previousActiveWindow, data, excludeCurrentFromHistory);
            }
            else
            {
                navigationEvent.activeWindow.SetData(data);
            }
        }

        public ScreenType OpenScreen<ScreenType>(in WindowTransitionPlayable transitionPlayable, bool excludeCurrentFromHistory = false) where ScreenType : IScreen
        {
            WindowNavigationEvent navigationEvent = NavigateToScreen<ScreenType>(excludeCurrentFromHistory);
            if (navigationEvent.success)
            {
                return OpenScreenInternal<ScreenType>(in transitionPlayable, navigationEvent.activeWindow, navigationEvent.previousActiveWindow, excludeCurrentFromHistory);
            }
            else
            {
                return (ScreenType)navigationEvent.activeWindow;
            }
        }

        public void OpenScreen(IScreen screen, in WindowTransitionPlayable transitionPlayable, bool excludeCurrentFromHistory = false)
        {
            WindowNavigationEvent navigationEvent = NavigateToScreen(screen, excludeCurrentFromHistory);
            if (navigationEvent.success)
            {
                OpenScreenInternal(in transitionPlayable, navigationEvent.activeWindow, navigationEvent.previousActiveWindow, excludeCurrentFromHistory);
            }
        }

        public ScreenType OpenScreen<ScreenType>(object data, in WindowTransitionPlayable transitionPlayable, bool excludeCurrentFromHistory = false) where ScreenType : IScreen
        {
            WindowNavigationEvent navigationEvent = NavigateToScreen<ScreenType>(excludeCurrentFromHistory);
            if (navigationEvent.success)
            {
                return OpenScreenInternal<ScreenType>(in transitionPlayable, navigationEvent.activeWindow, navigationEvent.previousActiveWindow, data, excludeCurrentFromHistory);
            }
            else
            {
                navigationEvent.activeWindow.SetData(data);
                return (ScreenType)navigationEvent.activeWindow;
            }
        }

        public void OpenScreen(IScreen screen, object data, in WindowTransitionPlayable transitionPlayable, bool excludeCurrentFromHistory = false)
        {
            WindowNavigationEvent navigationEvent = NavigateToScreen(screen, excludeCurrentFromHistory);
            if (navigationEvent.success)
            {
                OpenScreenInternal(in transitionPlayable, navigationEvent.activeWindow, navigationEvent.previousActiveWindow, data, excludeCurrentFromHistory);
            }
            else
            {
                navigationEvent.activeWindow.SetData(data);
            }
        }

        private ScreenType OpenScreenInternal<ScreenType>(in WindowTransitionPlayable transitionPlayable, IWindow targetScreen, IWindow sourceScreen, object data, bool excludeCurrentFromHistory) where ScreenType : IScreen
        {
            OpenScreenInternal(in transitionPlayable, targetScreen, sourceScreen, data, excludeCurrentFromHistory);
            return (ScreenType)targetScreen;
        }

        private ScreenType OpenScreenInternal<ScreenType>(in WindowTransitionPlayable transitionPlayable, IWindow targetScreen, IWindow sourceScreen, bool excludeCurrentFromHistory) where ScreenType : IScreen
        {
            OpenScreenInternal(in transitionPlayable, targetScreen, sourceScreen, excludeCurrentFromHistory);
            return (ScreenType)targetScreen;
        }

        private void OpenScreenInternal(in WindowTransitionPlayable transitionPlayable, IWindow targetScreen, IWindow sourceScreen, object data, bool excludeCurrentFromHistory)
        {
            targetScreen.SetData(data);
            OpenScreenInternal(in transitionPlayable, targetScreen, sourceScreen, excludeCurrentFromHistory);
        }

        private void OpenScreenInternal(in WindowTransitionPlayable transitionPlayable, IWindow targetScreen, IWindow sourceScreen, bool excludeCurrentFromHistory)
        {
            if (sourceScreen != null)
            {
                if (transitionPlayable.length > 0.0F && (transitionPlayable.entryAnimation != null || transitionPlayable.exitAnimation != null))
                {
                    _transitionManager.Transition(in transitionPlayable, sourceScreen, targetScreen);
                }
                else
                {
                    _transitionManager.Transition(WindowTransitionPlayable.None(), sourceScreen, targetScreen);
                }

                if (!excludeCurrentFromHistory)
                {
                    _transitionHistory.Push(transitionPlayable);
                }
            }
            else
            {
                if (transitionPlayable.length > 0.0F && transitionPlayable.entryAnimation != null)
                {
                    AnimationPlayable animationPlayable = transitionPlayable.entryAnimation.CreatePlayable(targetScreen, transitionPlayable.length, AccessOperation.Open, 0.0F,
                    transitionPlayable.easingMode, timeMode);
                    targetScreen.Open(in animationPlayable);
                }
                else
                {
                    targetScreen.Open();
                }
            }

            if (sourceScreen == null)
            {
                OpenInternal(transitionPlayable.length);
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
                    AnimationPlayable animationPlayable = CreateAccessPlayable(AccessOperation.Open, animationLength);
                    if (animationPlayable.animation != null)
                    {
                        _animationPlayer = AnimationPlayer.PlayAnimation(in animationPlayable);
                        _animationPlayer.onComplete += OnAnimationComplete;
                        animateOpen = true;
                    }
                }
            }

            if (animateOpen)
            {
                accessState = AccessState.Opening;
                OnOpen();
                onOpen?.Invoke();
            }
            else
            {
                if (_animationPlayer != null)
                {
                    Debug.Log("Open was called on a Controller without an animation while already playing a state animation, " +
                        "this may cause unexpected behviour of the UI.");
                    _animationPlayer.SetCurrentTime(0.0F);
                    _animationPlayer.Stop();
                    _animationPlayer.onComplete -= OnAnimationComplete;
                    _animationPlayer = null;
                }

                accessState = AccessState.Opening;
                OnOpen();
                onOpen?.Invoke();
                accessState = AccessState.Open;
                OnOpened();
            }
        }

        protected virtual void OnOpen() { }
        protected virtual void OnOpened() { }

        public bool CloseScreen()
        {
            WindowNavigationEvent navigationEvent = _navigation.Back();
            if (navigationEvent.success)
            {
                WindowTransitionPlayable transition = _transitionHistory.Pop();
                _transitionManager.ReverseTransition(in transition, navigationEvent.activeWindow, navigationEvent.previousActiveWindow);
                return true;
            }
            return false;
        }

        public bool CloseAll(float animationLength = 0.0F)
        {
            WindowNavigationEvent navigationEvent = _navigation.Clear();
            if (navigationEvent.success)
            {
                WindowAccessPlayable animationPlayable =
                    new WindowAccessPlayable(navigationEvent.previousActiveWindow.CreateDefaultAccessAnimation(animationLength), animationLength, EasingMode.EaseInOut);
                CloseAllInternal(in animationPlayable, navigationEvent.previousActiveWindow);
                return true;
            }
            return false;
        }

        public bool CloseAll(in WindowAccessPlayable animationPlayable)
        {
            WindowNavigationEvent navigationEvent = _navigation.Clear();
            if (navigationEvent.success)
            {
                CloseAllInternal(in animationPlayable, navigationEvent.previousActiveWindow);
                return true;
            }
            return false;
        }

        private void CloseAllInternal(in WindowAccessPlayable animationPlayable, IWindow targetScreen)
        {
            _transitionManager.Terminate(Mathf.Approximately(animationPlayable.length, 0.0F));

            if (animationPlayable.length > 0.0F)
            {
                targetScreen.Close(animationPlayable.CreatePlayable(targetScreen, AccessOperation.Close, 0.0F, timeMode));
            }
            else
            {
                targetScreen.Close();
            }

            CloseInternal(animationPlayable.length);
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
                    AnimationPlayable animationPlayable = CreateAccessPlayable(AccessOperation.Close, animationLength);
                    if (animationPlayable.animation != null)
                    {
                        _animationPlayer = AnimationPlayer.PlayAnimation(in animationPlayable);
                        _animationPlayer.onComplete += OnAnimationComplete;
                        animateClose = true;
                    }
                }
            }

            if (animateClose)
            {
                accessState = AccessState.Closing;
                OnClose();
                onClose?.Invoke();
            }
            else
            {
                if (_animationPlayer != null)
                {
                    Debug.Log("Open was called on a Controller without an animation while already playing a state animation, " +
                        "this may cause unexpected behviour of the UI.");
                    _animationPlayer.SetCurrentTime(0.0F);
                    _animationPlayer.Stop();
                    _animationPlayer.onComplete -= OnAnimationComplete;
                    _animationPlayer = null;
                }

                accessState = AccessState.Closing;
                OnClose();
                onClose?.Invoke();
                accessState = AccessState.Closed;
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
            if (navigationEvent.success)
            {
                _transitionHistory.ClearLatestGroup();
            }
        }

        public void InsertHistory<ScreenType>(in WindowTransitionPlayable transitionPlayable) where ScreenType : IScreen
        {
            WindowNavigationEvent navigationEvent = _navigation.InsertHistory<ScreenType>();
            if (navigationEvent.success)
            {
                _transitionHistory.Push(transitionPlayable);
            }
        }

        public void ClearHistory()
        {
            WindowNavigationEvent navigationEvent = _navigation.ClearHistory();
            if (navigationEvent.success)
            {
                _transitionHistory.Clear();
            }
        }

        private void OnAnimationComplete(Animation animation)
        {
            if (accessState == AccessState.Opening)
            {
                accessState = AccessState.Open;
                OnOpened();
            }
            else if (accessState == AccessState.Closing)
            {
                accessState = AccessState.Closed;
                OnClosed();
            }
            _animationPlayer.onComplete -= OnAnimationComplete;
            _animationPlayer = null;
        }

        private void OnNavigationUpdate(WindowNavigationEvent navigationEvent)
        {
            if (navigationEvent.success)
            {
                bool backButtonActive = navigationEvent.historyCount > 0;
                SetBackButtonActive(backButtonActive);
            }
        }
    }
}