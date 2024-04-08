using System;

using UnityEngine;

namespace UIFramework
{
    public struct NavigationEvent
    {
        public enum Type
        {
            Travel,
            Back,
            History,
            Lock
        }

        public Type type { get; private set; }
        public bool success { get; private set; }
        public System.Type target { get; private set; }
        public int historyCount { get; private set; }
        public bool isLocked { get; private set; }

        public bool exit { get { return type == Type.Back && target == null && success; } }

        public NavigationEvent(Type type, bool success, System.Type target, int historyCount, bool isLocked)
        {
            this.type = type;
            this.success = success;
            this.target = target;
            this.historyCount = historyCount;
            this.isLocked = isLocked;
        }
    }

    public delegate void NavigationUpdate(NavigationEvent navigationEvent);

    // [TODO] Should consider if this aligns with single responsibility (probably not).
    // ScreenTransitionManager could be factored out from the ScreenNavigation class.
    // Instead of the navigation component being responsible for transitions, the controller
    // reads the responses of returned navigation events to invoke a given transition.
    // The side effect of this would be creating wrapper functions for all travel functions on the 
    // controller class, also there is a issue where in this current implementation the history stack of navigation is
    // coupled with a transition. This moves toward a model where the controller is the single point of responsibility for
    // controlling all dependant components instead of chaining dependency in this way.
    public class ScreenNavigation<ControllerType> where ControllerType : Controller<ControllerType>
    {
        private ScreenCollection<ControllerType> _screens = null;

        public IScreen<ControllerType> activeScreen
        {
            get
            {
                return activeScreenType != null ? _screens.dictionary[activeScreenType] : null;
            }
        }

        public Type activeScreenType { get; private set; } = null;

        public bool isTransitioning { get { return _transitionManager.isTransitionActive; } }

        public int historyCount { get { return _history.count; } }

        public NavigationUpdate onNavigationUpdate = null;

        public bool isLocked
        {
            get { return _isLockedCounter > 0; }
            private set
            {
                if (value)
                {
                    _isLockedCounter++;
                }
                else
                {
                    _isLockedCounter = Mathf.Max(_isLockedCounter - 1, 0);
                }
            }
        }
        private int _isLockedCounter = 0;

        private ScreenHistory<ControllerType> _history = null;
        private ScreenTransitionManager<ControllerType> _transitionManager = null;

        private ScreenNavigation() { }

        public ScreenNavigation(ScreenCollection<ControllerType> screens)
        {
            if (screens == null)
            {
                throw new ArgumentNullException("screens");
            }
            _screens = screens;
            _history = new ScreenHistory<ControllerType>(_screens.array.Length);
            _transitionManager = new ScreenTransitionManager<ControllerType>();
        }

        public void Init<ScreenType>(object targetScreenData) where ScreenType : IScreen<ControllerType>
        {
            Type targetScreenType = typeof(ScreenType);
            IScreen<ControllerType> targetScreen;
            if (!ValidateInit(targetScreenType, out targetScreen))
            {
                throw new Exception(string.Format("Attempted to initialize ScreenNavigation with a screen type of {0} that has not been found.", targetScreenType.ToString()));
            }

            InitInternal(targetScreenType, targetScreen, targetScreenData);
            targetScreen.Open();
        }

        public void Init<ScreenType>(in WindowAnimation animation, object targetScreenData) where ScreenType : IScreen<ControllerType>
        {
            Type targetScreenType = typeof(ScreenType);
            IScreen<ControllerType> targetScreen;
            if (!ValidateInit(targetScreenType, out targetScreen))
            {
                throw new Exception(string.Format("Attempted to initialize ScreenNavigation with a screen type of {0} that has not been found.", targetScreenType.ToString()));
            }

            InitInternal(targetScreenType, targetScreen, targetScreenData);
            targetScreen.Open(in animation);
        }

        public void Init(IScreen<ControllerType> targetScreen, object targetScreenData)
        {
            Type targetScreenType = targetScreen.GetType();
            if (!ValidateInit(targetScreenType, targetScreen))
            {
                throw new Exception(string.Format("Attempted to initialize ScreenNavigation with a screen type of {0} that has not been found.", targetScreenType.ToString()));
            }

            InitInternal(targetScreenType, targetScreen, targetScreenData);
            targetScreen.Open();
        }

        public void Init(IScreen<ControllerType> targetScreen, in WindowAnimation animation, object targetScreenData)
        {
            Type targetScreenType = targetScreen.GetType();
            if (!ValidateInit(targetScreenType, targetScreen))
            {
                throw new Exception(string.Format("Attempted to initialize ScreenNavigation with a screen type of {0} that has not been found.", targetScreenType.ToString()));
            }

            InitInternal(targetScreenType, targetScreen, targetScreenData);
            targetScreen.Open(in animation);
        }

        public void Init(Type targetScreenType, object targetScreenData)
        {
            if (!targetScreenType.IsSubclassOf(typeof(IScreen<ControllerType>)))
            {
                throw new Exception("Attempted to initialize ScreenNavigation with unrecognised type.");
            }

            IScreen<ControllerType> targetScreen;
            if (!ValidateInit(targetScreenType, out targetScreen))
            {
                throw new Exception(string.Format("Attempted to initialize ScreenNavigation with a screen type of {0} that has not been found.", targetScreenType.ToString()));
            }

            InitInternal(targetScreenType, targetScreen, targetScreenData);
            targetScreen.Open();
        }

        public void Init(Type targetScreenType, in WindowAnimation animation, object targetScreenData)
        {
            if (!targetScreenType.IsSubclassOf(typeof(IScreen<ControllerType>)))
            {
                throw new Exception("Attempted to initialize ScreenNavigation with unrecognised type.");
            }

            IScreen<ControllerType> targetScreen;
            if (!ValidateInit(targetScreenType, out targetScreen))
            {
                throw new Exception(string.Format("Attempted to initialize ScreenNavigation with a screen type of {0} that has not been found.", targetScreenType.ToString()));
            }

            InitInternal(targetScreenType, targetScreen, targetScreenData);
            targetScreen.Open(in animation);
        }

        private void InitInternal(Type screenType, IScreen<ControllerType> screen, object data)
        {
            screen.SetData(data);
            activeScreenType = screenType;
        }

        private bool ValidateInit(Type screenType, IScreen<ControllerType> screen)
        {
            IScreen<ControllerType> cachedScreen;
            if(!ValidateInit(screenType, out cachedScreen))
            {
                return false;
            }

            if(cachedScreen != screen)
            {
                Debug.LogWarning(string.Format("Unable to init with: {0}, the screen type exists in navigation but the instance of the screen provided does not.", screenType.ToString()));
                return false;
            }
            return true;
        }

        private bool ValidateInit(Type screenType, out IScreen<ControllerType> screen)
        {
            if (_screens == null)
            {
                throw new Exception("Attempted to initialize ScreenNavigation with no screens set.");
            }

            if (activeScreenType != null)
            {
                throw new Exception("Attempted to initialize ScreenNavigation that has already been initialized.");
            }

            screen = null;

            if (!_screens.dictionary.TryGetValue(screenType, out screen))
            {
                return false;
            }
            return true;
        }

        public NavigationEvent Travel<ScreenType>(in ScreenTransition transition, object data, bool excludeCurrentFromHistory = false)
            where ScreenType : IScreen<ControllerType>
        {
            Type targetType = typeof(ScreenType);
            IScreen<ControllerType> targetScreen;
            if(ValidateTravelTarget(targetType, out targetScreen))
            {
                return InvokeNavigationUpdate(TravelInternal(targetType, targetScreen, in transition, data, excludeCurrentFromHistory));
            }
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Travel, false, targetType, historyCount, isLocked));
        }

        public NavigationEvent Travel<ScreenType>(object data, bool excludeCurrentFromHistory = false)
            where ScreenType : IScreen<ControllerType>
        {
            Type targetType = typeof(ScreenType);
            IScreen<ControllerType> targetScreen;
            if (ValidateTravelTarget(targetType, out targetScreen))
            {
                return InvokeNavigationUpdate(TravelInternal(targetType, targetScreen, data, excludeCurrentFromHistory));
            }
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Travel, false, targetType, historyCount, isLocked));
        }

        public NavigationEvent Travel(IScreen<ControllerType> targetScreen, in ScreenTransition transition, object data, bool excludeCurrentFromHistory = false)
        {
            Type targetScreenType = targetScreen.GetType();
            if (ValidateTravelTarget(targetScreenType, targetScreen))
            {
                return TravelInternal(targetScreenType, targetScreen, in transition, data, excludeCurrentFromHistory);
            }
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Travel, false, targetScreenType, historyCount, isLocked));
        }

        public NavigationEvent Travel(IScreen<ControllerType> targetScreen, object data, bool excludeCurrentFromHistory = false)
        {
            Type targetScreenType = targetScreen.GetType();
            if (ValidateTravelTarget(targetScreenType, targetScreen))
            {
                return TravelInternal(targetScreenType, targetScreen, data, excludeCurrentFromHistory);
            }
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Travel, false, targetScreenType, historyCount, isLocked));
        }

        public NavigationEvent Travel(Type screenType, in ScreenTransition transition, object data, bool excludeCurrentFromHistory = false)
        {
            if (screenType.IsSubclassOf(typeof(IScreen<ControllerType>)))
            {
                IScreen<ControllerType> targetScreen;
                if (ValidateTravelTarget(screenType, out targetScreen))
                {
                    return TravelInternal(screenType, targetScreen, in transition, data, excludeCurrentFromHistory);
                }
            }
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Travel, false, screenType, historyCount, isLocked));
        }

        public NavigationEvent Travel(Type screenType, object data, bool excludeCurrentFromHistory = false)
        {
            if (screenType.IsSubclassOf(typeof(IScreen<ControllerType>)))
            {
                IScreen<ControllerType> targetScreen;
                if (ValidateTravelTarget(screenType, out targetScreen))
                {
                    return TravelInternal(screenType, targetScreen, data, excludeCurrentFromHistory);
                }
            }
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Travel, false, screenType, historyCount, isLocked));
        }

        private NavigationEvent TravelInternal(Type screenType, IScreen<ControllerType> screen, object data, bool excludeCurrentFromHistory)
        {
            return TravelInternal(screenType, screen, screen.defaultTransition, data, excludeCurrentFromHistory);
        }

        private NavigationEvent TravelInternal(Type screenType, IScreen<ControllerType> screen, in ScreenTransition transition, object data, bool excludeCurrentFromHistory)
        {
            if(isLocked)
            {
                return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Travel, false, screenType, historyCount, isLocked));
            }

            if(activeScreenType == null)
            {
                throw new InvalidOperationException("Unable to travel while there is no active screen");
            }

            IScreen<ControllerType> activeScreen = _screens.dictionary[activeScreenType];
            if (activeScreen.supportsHistory && !excludeCurrentFromHistory)
            {
                _history.Push(activeScreenType, in transition);
            }

            screen.SetData(data);
            _transitionManager.Transition(in transition, activeScreen, screen);

            activeScreenType = screenType;
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Travel, true, screenType, historyCount, isLocked));
        }

        private bool ValidateTravelTarget(Type screenType, IScreen<ControllerType> screen)
        {            
            IScreen<ControllerType> cachedScreen = null;
            if(!ValidateTravelTarget(screenType, out cachedScreen))
            {
                return false;
            }

            if(cachedScreen != screen)
            {
                Debug.LogWarning(string.Format("Unable to travel to: {0}, the screen type exists in navigation but the instance of the screen provided does not.", screenType.ToString()));
                return false;
            }
            return true;
        }

        private bool ValidateTravelTarget(Type screenType, out IScreen<ControllerType> screen)
        {
            screen = null;
            if(activeScreenType == screenType)
            {
                Debug.LogWarning(string.Format("Unable to travel to: {0} as it is already the active screen.", screenType.ToString()));
                return false;
            }

            if(!_screens.dictionary.TryGetValue(screenType, out screen))
            {
                Debug.LogWarning(string.Format("Unable to travel to: {0}, not found in navigation.", screenType.ToString()));
                return false;
            }
            return true;
        }

        public void Back()
        {
            Back(false);
        }

        public NavigationEvent Back(bool allowExit)
        {
            if (isLocked)
            {
                return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Back, false, null, historyCount, isLocked));
            }

            IScreen<ControllerType> activeScreen;
            if (_screens.dictionary.TryGetValue(activeScreenType, out activeScreen))
            {
                if (historyCount > 0)
                {                    
                    Type previousScreenType;
                    ScreenTransition transition; 
                    _history.Pop(out previousScreenType, out transition);
                    IScreen<ControllerType> previousScreen = _screens.dictionary[previousScreenType];

                    previousScreen.SetData(previousScreen.data);
                    _transitionManager.ReverseTransition(in transition, previousScreen, activeScreen);

                    activeScreenType = previousScreenType;

                    return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Back, true, activeScreenType, historyCount, isLocked));
                }
                else if (allowExit)
                {
                    return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Back, true, null, historyCount, isLocked));
                }
            }
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Back, false, null, historyCount, isLocked));
        }

        public NavigationEvent Lock()
        {
            isLocked = true;
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Lock, isLocked == true, null, historyCount, isLocked));
        }

        public NavigationEvent Unlock()
        {
            isLocked = false;
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Lock, isLocked == false, null, historyCount, isLocked));
        }

        public void Terminate()
        {
            IScreen<ControllerType> activeScreen = this.activeScreen;
            TerminateInternal();
            _transitionManager.Terminate(true);
            activeScreen.Close();
        }

        public void Terminate(in WindowAnimation animation)
        {
            IScreen<ControllerType> activeScreen = this.activeScreen;
            TerminateInternal();
            _transitionManager.Terminate(false);
            activeScreen.Close(in animation);
        }

        private void TerminateInternal()
        {
            if (_screens == null)
            {
                throw new Exception("Attempted to terminate ScreenNavigation with no screens set.");
            }

            if (activeScreenType == null)
            {
                throw new Exception("Attempted to terminate ScreenNavigation that has not been intialized.");
            }

            isLocked = false;

            ClearHistory();            
            activeScreenType = null;
        }

        public void StartNewHistoryGroup()
        {
            _history.StartNewGroup();
        }

        public NavigationEvent ClearLatestHistoryGroup()
        {
            if(_history.ClearLatestGroup())
            {
                return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.History, true, null, historyCount, isLocked));
            }
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.History, false, null, historyCount, isLocked));
        }

        public NavigationEvent InsertHistory<T>(in ScreenTransition transition) where T : IScreen<ControllerType>
        {
            _history.Push(typeof(T), in transition);
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.History, true, null, historyCount, isLocked));
        }

        public NavigationEvent ClearHistory()
        {
            if(_history.Clear())
            {
                return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.History, true, null, historyCount, isLocked));
            }
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.History, false, null, historyCount, isLocked));
        }

        private NavigationEvent InvokeNavigationUpdate(NavigationEvent navigationEvent)
        {
            onNavigationUpdate.Invoke(navigationEvent);
            return navigationEvent;
        }
    }
}
