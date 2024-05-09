using System;
using System.Collections.Generic;

using UnityEngine;

namespace UIFramework
{
    public abstract class Controller<ControllerType> : MonoBehaviour  where ControllerType : Controller<ControllerType>
    {
        public enum UpdateTimeMode
        {
            Scaled,
            Unscaled
        }

        [Flags]
        public enum StateAnimationMode
        {
            None = 0,
            Self = 1,
            Screen = 2
        }

        public AccessState state { get; private set; } = AccessState.Unitialized;
        public bool isVisible { get { return state != AccessState.Closed; } }

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

        public abstract UpdateTimeMode updateTimeMode { get; protected set; }

        protected abstract StateAnimationMode stateAnimationMode { get; }

        protected WindowAnimationBase animator
        {
            get
            {
                if (_animator == null)
                {
                    _animator = CreateAnimationSequences();
                }
                return _animator;
            }
        }
        private WindowAnimationBase _animator = null;

        public Navigation<IScreen<ControllerType>> navigation = null;

        public Action onOpenAction { get; set; } = null;
        public Action onCloseAction { get; set; } = null;

        [SerializeField] protected CollectorComponent[] screenCollectors = null;
        private ArrayDictionary<IScreen<ControllerType>> _screens = null;

        private bool stateAnimatesSelf { get { return stateAnimationMode.HasFlag(StateAnimationMode.Self); } }
        private bool stateAnimatesScreen { get { return stateAnimationMode.HasFlag(StateAnimationMode.Screen); } }

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
                switch (updateTimeMode)
                {
                    default:
                    case UpdateTimeMode.Scaled:
                        deltaTime = Time.deltaTime;
                        break;
                    case UpdateTimeMode.Unscaled:
                        deltaTime = Time.unscaledDeltaTime;
                        break;
                }

                UpdateUI(deltaTime);
            }
        }

        protected virtual void OnDestroy()
        {

        }

        protected virtual void OnApplicationFocus(bool hasFocus)
        {

        }

        // IWindowAnimatorFactor
        public abstract WindowAnimationBase CreateAnimationSequences();

        // ScreenController<ControllerType>
        public void Init()
        {
            if (state != AccessState.Unitialized)
            {
                throw new InvalidOperationException("Controller already initialized.");
            }

            if(screenCollectors == null)
            {
                throw new InvalidOperationException("screenCollectors are null.");
            }

            List<IScreen<ControllerType>> screenList = new List<IScreen<ControllerType>>();
            for (int i = 0; i < screenCollectors.Length; i++)
            {
                screenList.AddRange(screenCollectors[i].Collect<IScreen<ControllerType>>());
            }

            _screens = new ArrayDictionary<IScreen<ControllerType>>(screenList.ToArray());

            navigation = new Navigation<IScreen<ControllerType>>(_screens, updateTimeMode == UpdateTimeMode.Unscaled);
            navigation.onNavigationUpdate += OnNavigationUpdate;            

            if (animator != null)
            {
                animator.onComplete += OnAnimationComplete;
            }

            SetBackButtonActive(false);
            state = AccessState.Closed;
            for (int i = 0; i < _screens.array.Length; i++)
            {
                _screens.array[i].Init(this);
                _screens.array[i].Close();
            }
            OnInit();
        }

        protected virtual void OnInit() { }

        protected virtual void UpdateUI(float deltaTime)
        {
            if (animator != null)
            {
                animator.Update(deltaTime);
            }

            if(_screens.array != null)
            {
                for (int i = 0; i < _screens.array.Length; i++)
                {
                    if (_screens.array[i].isVisible)
                    {
                        _screens.array[i].UpdateUI(deltaTime);
                    }
                }
            }            
        }

        public ScreenType GetScreen<ScreenType>() where ScreenType : class, IScreen<ControllerType>
        {
            ScreenType screen = null;
            Type targetType = typeof(ScreenType);
            IScreen<ControllerType> screenInterface;
            if (_screens.dictionary.TryGetValue(targetType, out screenInterface))
            {
                screen = (ScreenType)screenInterface;
            }
            return screen;
        }

        private void OnNavigationUpdate(NavigationEvent navigationEvent)
        {
            if (navigationEvent.success)
            {
                bool backButtonActive = navigationEvent.historyCount > 0;
                SetBackButtonActive(backButtonActive);
            }
        }

        protected abstract void SetBackButtonActive(bool active);

        protected virtual void Enabled() { }
        protected virtual void Disabled() { }

        public abstract void SetWaiting(bool waiting);
      
        public bool Open<ScreenType>(object data) where ScreenType : IScreen<ControllerType>
        {
            if (state == AccessState.Closing || state == AccessState.Closed)
            {
                Type targetScreenType = typeof(ScreenType);
                IScreen<ControllerType> targetScreen;
                if (!_screens.dictionary.TryGetValue(targetScreenType, out targetScreen))
                {
                    throw new Exception("Attempted to open Controller to a screen that cannot be found.");
                }

                if(state == AccessState.Closing)
                {
                    Debug.Log("Open was called on a Controller without an animation while already playing a state animation, " +
                        "this may cause unexpected behaviour of the UI.");
                    if(stateAnimatesSelf)
                    {
                        animator.Stop();
                    }                    
                }

                state = AccessState.Open;
                navigation.Init<ScreenType>(data);
                OnOpen();
                onOpenAction?.Invoke();
                OnOpened();
                return true;
            }
            return false;
        }

        public bool Open<ScreenType>(in WindowAnimationBase animation, object data) where ScreenType : IScreen<ControllerType>
        {
            if (stateAnimationMode == StateAnimationMode.None)
            {
                throw new Exception("Attempted to open Controller with animation that does not support state animation.");
            }

            if (stateAnimatesSelf && animator == null)
            {
                throw new Exception("Attempted to open Controller with animation while animator is null.");
            }

            if (state == AccessState.Closing || state == AccessState.Closed)
            {
                Type targetScreenType = typeof(ScreenType);
                IScreen<ControllerType> targetScreen;
                if (!_screens.dictionary.TryGetValue(targetScreenType, out targetScreen))
                {
                    throw new Exception("Attempted to open Controller to a screen that cannot be found.");
                }

                if(stateAnimatesSelf)
                {
                    if (state == AccessState.Closing)
                    {
                        if (animator.type != animation.type)
                        {
                            Debug.Log(string.Format("Controller is already playing a close animation of type {0}, " +
                                "provided open animation is ignored and the current close animation is rewound.", animator.type.ToString()));
                        }
                        animator.Rewind();
                    }
                    else
                    {
                        animator.Play(in animation);
                    }
                }                

                state = AccessState.Opening;
                if (stateAnimatesScreen)
                {
                    navigation.Init<ScreenType>(in animation, data);
                }
                else
                {
                    navigation.Init<ScreenType>(data);
                }                
                OnOpen();
                onOpenAction?.Invoke();
                return true;
            }
            return false;
        }

        private void OpenInternal(Type targetScreenType, object data)
        {
            if (state == AccessState.Closing)
            {
                Debug.Log("Open was called on a Controller without an animation while already playing a state animation, " +
                    "this may cause unexpected behaviour of the UI.");
                if (stateAnimatesSelf)
                {
                    animator.Stop();
                }
            }

            state = AccessState.Open;
            navigation.Init(targetScreenType, data);
            OnOpen();
            onOpenAction?.Invoke();
            OnOpened();
        }

        public void OpenInternal(Type targetScreenType, in WindowAnimationBase animation, object data)
        {
            if (stateAnimationMode == StateAnimationMode.None)
            {
                throw new Exception("Attempted to open Controller with animation that does not support state animation.");
            }

            if (stateAnimatesSelf && animator == null)
            {
                throw new Exception("Attempted to open Controller with animation while animator is null.");
            }

            if (stateAnimatesSelf)
            {
                if (state == AccessState.Closing)
                {
                    if (animator.type != animation.type)
                    {
                        Debug.Log(string.Format("Controller is already playing a close animation of type {0}, " +
                            "provided open animation is ignored and the current close animation is rewound.", animator.type.ToString()));
                    }
                    animator.Rewind();
                }
                else
                {
                    animator.Play(in animation);
                }
            }

            state = AccessState.Opening;
            if (stateAnimatesScreen)
            {
                navigation.Init(targetScreenType, in animation, data);
            }
            else
            {
                navigation.Init(targetScreenType, data);
            }

            OnOpen();
            onOpenAction?.Invoke();
        }

        private bool ValidateOpen(Type screenType, IScreen<ControllerType> screen)
        {
            IScreen<ControllerType> cachedScreen = null;
            if (!ValidateOpen(screenType, out cachedScreen))
            {
                return false;
            }

            if (cachedScreen != screen)
            {
                throw new Exception(string.Format("Unable open controller to: {0}, the screen type exists but the instance of the screen provided does not.", screenType.ToString()));
            }
            return true;
        }

        private bool ValidateOpen(Type screenType, out IScreen<ControllerType> screen)
        {
            screen = null;
            if (state != AccessState.Closing && state != AccessState.Closed)
            {
                return false;
            }
            
            if (!_screens.dictionary.TryGetValue(screenType, out screen))
            {
                throw new Exception(string.Format("Unable to open controller to: {0}, screen not found.", screenType.ToString()));
            }
            return true;
        }

        protected virtual void OnOpen() { }
        protected virtual void OnOpened() { }

        public bool Close()
        {
            if (state == AccessState.Opening || state == AccessState.Open)
            {
                if(state == AccessState.Opening)
                {
                    Debug.Log("Close was called on a Controller without an animation while already playing a state animation, " +
                        "this may cause unexpected behaviour of the UI.");
                    if (stateAnimatesSelf)
                    {
                        animator.Stop();
                    }
                }

                state = AccessState.Closed;
                navigation.Terminate();
                OnClose();
                onCloseAction?.Invoke();
                OnClosed();
                return true;
            }
            return false;
        }

        public bool Close(in WindowAnimationBase animation)
        {
            if (stateAnimationMode == StateAnimationMode.None)
            {
                throw new Exception("Attempted to close Controller with animation that does not support state animation.");
            }

            if (stateAnimatesSelf && animator == null)
            {
                throw new Exception("Attempted to close Controller with animation while animator is null.");
            }

            if (navigation.isTransitioning)
            {
                Debug.Log("Controller is unable to close with an animation during a navigation transition.");
                return false;
            }

            if (state == AccessState.Opening || state == AccessState.Open)
            {
                if(stateAnimatesSelf)
                {
                    if(state == AccessState.Opening)
                    {
                        if (animator.type != animation.type)
                        {
                            Debug.Log(string.Format("Controller is already playing a open animation of type {0}, " +
                                "provided close animation is ignored and the current open animation is rewound.", animator.type.ToString()));
                        }
                        animator.Rewind();
                    }
                    else
                    {
                        animator.Play(in animation);
                    }
                }

                state = AccessState.Closing;
                if (stateAnimatesScreen)
                {
                    navigation.Terminate(in animation);
                }
                else
                {
                    navigation.Terminate();
                }
                OnClose();
                onCloseAction?.Invoke();
                return true;
            }
            return false;
        }

        protected virtual void OnClose() { }
        protected virtual void OnClosed() { }

        private void OnAnimationComplete(WindowAnimationBase animator)
        {
            if (state == AccessState.Opening)
            {
                state = AccessState.Open;
                OnOpened();
            }
            else if (state == AccessState.Closing)
            {
                state = AccessState.Closed;
                OnClosed();
            }
            else
            {
                throw new Exception(string.Format("Controller animation complete while in unexpected state: {0}", state.ToString()));
            }
        }
    }
}