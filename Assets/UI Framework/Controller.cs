using System;
using System.Collections.Generic;

using Unity.VisualScripting;

using UnityEngine;

namespace UIFramework
{
    public abstract class Controller<ControllerType> : MonoBehaviour, IWindowAnimatorFactory where ControllerType : Controller<ControllerType>
    {
        public enum UpdateTimeMode
        {
            Scaled,
            Unscaled,
            Disabled
        }

        [SerializeField] private Transform[] rootScreenTransforms = new Transform[0];

        public WindowState state { get; private set; } = WindowState.Unitialized;        
        public bool isVisible { get { return state != WindowState.Closed; } }

        public bool isEnabled 
        {
            get { return _isEnabled; } 
            private set
            {
                if (_isEnabled != value) 
                {
                    _isEnabled = value;
                    if(_isEnabled)
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

        protected abstract UpdateTimeMode updateTimeMode { get; set; }

        protected IWindowAnimator animator 
        { 
            get
            {
                if(_animator == null)
                {
                    _animator = CreateAnimator();
                }
                return _animator;
            }
        }
        private IWindowAnimator _animator = null;

        public ScreenNavigation<ControllerType> navigation = null;

        private ScreenCollection<ControllerType> _screens = new ScreenCollection<ControllerType>(null, null);        

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
            if(isVisible)
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

                if (updateTimeMode != UpdateTimeMode.Disabled)
                {
                    UpdateUI(deltaTime);
                }
            }             
        }

        protected virtual void OnDestroy()
        {
            
        }

        protected virtual void OnApplicationFocus(bool hasFocus)
        {

        }

        // IWindowAnimatorFactor
        public abstract IWindowAnimator CreateAnimator();

        // ScreenController<ControllerType>
        public void Init()
        {
            if (state != WindowState.Unitialized)
            {
                throw new InvalidOperationException("Controller already initialized.");
            }

            if (rootScreenTransforms == null || rootScreenTransforms.Length == 0)
            {
                throw new InvalidOperationException("Unable to init Controller with no rootScreenTransforms set.");
            }

            List<IScreen<ControllerType>> screenList = new List<IScreen<ControllerType>>();
            for (int i = 0; i < rootScreenTransforms.Length; i++)
            {
                if (rootScreenTransforms[i] == null)
                {
                    throw new InvalidOperationException(string.Format("Attempting to init while rootScreenTransform[{0}] is null.", i));
                }
                screenList.AddRange(rootScreenTransforms[i].GetComponentsInChildren<IScreen<ControllerType>>(true));
            }

            _screens.array = screenList.ToArray();
            _screens.dictionary = new Dictionary<Type, IScreen<ControllerType>>(_screens.array.Length);

            for (int i = 0; i < _screens.array.Length; i++)
            {
                Type screenType = _screens.array[i].GetType();
                if (!_screens.dictionary.ContainsKey(screenType))
                {
                    _screens.array[i].Init(this);
                    _screens.array[i].Close();
                    _screens.dictionary.Add(screenType, _screens.array[i]);
                }
                else
                {
                    Debug.LogWarning("Multiple instances of the same IScreen<ControllerType> type have been found. " +
                        "Please ensure all IScreen<ControllerType> instances are of a unqiue type.");
                }
            }

            navigation = new ScreenNavigation<ControllerType>(_screens);
            navigation.onNavigationUpdate += OnNavigationUpdate;

            if (animator != null)
            {
                animator.onAnimationComplete += OnAnimationComplete;
            }

            SetBackButtonActive(false);
            state = WindowState.Closed;
            OnInit();
        }

        protected virtual void OnInit() { }

        protected virtual void UpdateUI(float deltaTime)
        {
            if(animator != null)
            {
                animator.Update(deltaTime);
            }

            for(int i = 0; i < _screens.array.Length; i++)
            {
                if (_screens.array[i].isVisible)
                {
                    _screens.array[i].UpdateUI(deltaTime);
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
            if(navigationEvent.success)
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
            if(state == WindowState.Closing || state == WindowState.Closed)
            {
                Type targetScreenType = typeof(ScreenType);
                IScreen<ControllerType> targetScreen;
                if (!_screens.dictionary.TryGetValue(targetScreenType, out targetScreen))
                {
                    throw new Exception("Attempted to open Controller to a screen that cannot be found.");
                }                

                if (state == WindowState.Closed)
                {
                    state = WindowState.Open;
                    navigation.Init<ScreenType>(data);
                    OnOpen();
                    OnOpened();
                }
                else
                {
                    Debug.Log("Open was called on Controller without an animation while already playing a close animation, " +
                        "the current animation will be rewound.");
                    animator.Rewind();

                    state = WindowState.Opening;
                    if (navigation.activeScreenType == targetScreenType)
                    {
                        targetScreen.SetData(data);
                    }
                    else
                    {
                        ScreenTransition windowTransition = new ScreenTransition(ScreenTransition.Type.Fade, animator.remainingTime);
                        navigation.Travel<ScreenType>(in windowTransition, data, true);
                    }
                    OnOpen();
                }                                
                return true;
            }            
            return false;
        }

        public bool Open<ScreenType>(in WindowAnimation animation, object data) where ScreenType : IScreen<ControllerType>
        {
            if(animator == null)
            {
                throw new Exception("Attempted to open Controller with animation while animator is null.");
            }            

            if (state == WindowState.Closing || state == WindowState.Closed)
            {
                Type targetScreenType = typeof(ScreenType);
                IScreen<ControllerType> targetScreen;
                if (!_screens.dictionary.TryGetValue(targetScreenType, out targetScreen))
                {
                    throw new Exception("Attempted to open Controller to a screen that cannot be found.");
                }

                if (animator.isPlaying)
                {
                    if(animator.animation.type != animation.type)
                    {
                        Debug.Log(string.Format("Controller is already playing a close animation of type {0}, " +
                            "provided open animation is ignored and the current close animation is rewound.", animator.animation.type.ToString()));
                    }                    
                    animator.Rewind();
                }
                else
                {
                    animator.Play(in animation);
                }

                WindowState previousState = state;
                state = WindowState.Opening;

                if (previousState == WindowState.Closed)
                {
                    navigation.Init<ScreenType>(data);
                }
                else
                {
                    if (navigation.activeScreenType == targetScreenType)
                    {
                        targetScreen.SetData(data);
                    }
                    else
                    {
                        ScreenTransition windowTransition = new ScreenTransition(ScreenTransition.Type.Fade, animator.remainingTime);
                        navigation.Travel<ScreenType>(in windowTransition, data, true);
                    }                    
                }                
                
                OnOpen();
                return true;
            }
            return false;
        }

        protected virtual void OnOpen() { }
        protected virtual void OnOpened() { }

        public bool Close()
        {
            if (state == WindowState.Opening || state == WindowState.Open)
            {
                if (state == WindowState.Open)
                {
                    state = WindowState.Closed;
                    navigation.ClearHistory();
                    OnClose();
                    OnClosed();
                }
                else
                {
                    Debug.Log("Close was called on Controller without an animation while already playing a open animation, " +
                        "the current animation will be rewound.");
                    animator.Rewind();

                    state = WindowState.Closing;
                    navigation.ClearHistory();
                    OnClose();
                }                
                return true;
            }
            return false;
        }        

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
                navigation.ClearHistory();
                OnClose();
                return true;
            }
            return false;
        }

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
                navigation.Terminate();
                OnClosed();
            }
            else
            {
                throw new Exception(string.Format("Controller animation complete while in unexpected state: {0}", state.ToString()));
            }
        }        
    }
}