using System;
using System.Collections.Generic;
using System.Resources;

using UnityEngine;
using UnityEngine.UI;

namespace UIFramework
{
    public abstract class Controller<ControllerType> : MonoBehaviour where ControllerType : Controller<ControllerType>
    {
        public enum UpdateTimeMode
        {
            Scaled,
            Unscaled,
            Disabled
        }

        public WindowState state { get; private set; } = WindowState.Closed;
        public bool isEnabled { get; private set; } = true;
        public bool isVisible { get { return state != WindowState.Closed; } }

        protected abstract UpdateTimeMode updateTimeMode { get; set; }

        protected abstract IWindowAnimator animator { get; set; }

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
            _screens.array = GetComponentsInChildren<IScreen<ControllerType>>(true);
            _screens.dictionary = new Dictionary<Type, IScreen<ControllerType>>(_screens.array.Length);            

            for (int i = 0; i < _screens.array.Length; i++)
            {
                Type screenType = _screens.array[i].GetType();
                if (!_screens.dictionary.ContainsKey(screenType))
                {
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

        // ScreenController<ControllerType>
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

        public bool Enable()
        {
            if (!isEnabled)
            {
                isEnabled = true;
                Enabled();
                return true;
            }
            return false;
        }

        protected virtual void Enabled() { }

        public bool Disable()
        {
            if (isEnabled)
            {
                isEnabled = false;
                Disabled();
                return true;
            }
            return false;
        }

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
                        targetScreen.UpdateData(data);
                    }
                    else
                    {
                        WindowAnimation windowAnimation = new WindowAnimation(WindowAnimation.Type.Fade, animator.remainingTime);
                        navigation.Travel<ScreenType>(in windowAnimation, data, true);
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
                        targetScreen.UpdateData(data);
                    }
                    else
                    {
                        WindowAnimation windowAnimation = new WindowAnimation(WindowAnimation.Type.Fade, animator.remainingTime);
                        navigation.Travel<ScreenType>(in windowAnimation, data, true);
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
                Debug.LogWarning(string.Format("Controller animation complete while in unexpected state: {0}", state.ToString()));
            }
        }
    }
}