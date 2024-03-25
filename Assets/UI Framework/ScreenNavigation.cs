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
            History
        }

        public Type type { get; private set; }
        public bool success { get; private set; }
        public System.Type target { get; private set; }
        public int historyCount { get; private set; }

        public bool exit { get { return type == Type.Back && target == null && success; } }

        public NavigationEvent(Type type, bool success, System.Type target, int historyCount)
        {
            this.type = type;
            this.success = success;
            this.target = target;
            this.historyCount = historyCount;
        }
    }

    public delegate void NavigationUpdate(NavigationEvent navigationEvent);

    public class ScreenNavigation<ControllerType> where ControllerType : Controller<ControllerType>
    {
        private ScreenCollection<ControllerType> _screens = null;

        public Type activeScreenType { get; private set; } = null;

        public int historyCount { get; private set; }

        public NavigationUpdate onNavigationUpdate = null;

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
            if (_screens == null)
            {
                throw new Exception("Attempted to initialize ScreenNavigation with no screens set.");
            }

            Type targetScreenType = typeof(ScreenType);
            IScreen<ControllerType> targetScreen;
            if(!ValidateTravelTarget(targetScreenType, out targetScreen))
            {
                throw new Exception (string.Format("Attempted to initialize ScreenNavigation with a screen type of {0} that has not been found.", targetScreenType.ToString()));
            }

            targetScreen.SetData(targetScreenData);
            targetScreen.Open();
            activeScreenType = targetScreenType;

            for (int i = 0; i < _screens.array.Length; i++)
            {
                Type screenType = _screens.array[i].GetType();
                if (screenType != targetScreenType)
                {
                    _screens.array[i].Close();
                }
            }
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
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Travel, false, targetType, historyCount));
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
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Travel, false, targetType, historyCount));
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
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Travel, false, screenType, historyCount));
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
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Travel, false, screenType, historyCount));
        }

        private NavigationEvent TravelInternal(Type screenType, IScreen<ControllerType> screen, object data, bool excludeCurrentFromHistory)
        {
            return TravelInternal(screenType, screen, screen.defaultTransition, data, excludeCurrentFromHistory);
        }

        private NavigationEvent TravelInternal(Type screenType, IScreen<ControllerType> screen, in ScreenTransition transition, object data, bool excludeCurrentFromHistory)
        {
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
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Travel, true, screenType, historyCount));
        }

        bool ValidateTravelTarget(Type screenType, out IScreen<ControllerType> screen)
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

                    return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Back, true, activeScreenType, historyCount));
                }
                else if (allowExit)
                {
                    return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Back, true, null, historyCount));
                }
            }
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Back, false, null, historyCount));
        }

        public void Terminate()
        {
            if (_screens != null)
            {
                ClearHistory();
                if (activeScreenType != null)
                {
                    _screens.dictionary[activeScreenType].Close();
                    activeScreenType = null;
                }
            }
        }        

        public void StartNewHistoryGroup()
        {
            _history.StartNewGroup();
        }

        public NavigationEvent ClearLatestHistoryGroup()
        {
            if(_history.ClearLatestGroup())
            {
                return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.History, true, null, historyCount));
            }
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.History, false, null, historyCount));
        }

        public NavigationEvent InsertHistory<T>(in ScreenTransition transition) where T : IScreen<ControllerType>
        {
            _history.Push(typeof(T), in transition);
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.History, true, null, historyCount));
        }

        public NavigationEvent ClearHistory()
        {
            if(_history.Clear())
            {
                return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.History, true, null, historyCount));
            }
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.History, false, null, historyCount));
        }

        private NavigationEvent InvokeNavigationUpdate(NavigationEvent navigationEvent)
        {
            onNavigationUpdate.Invoke(navigationEvent);
            return navigationEvent;
        }
    }
}
