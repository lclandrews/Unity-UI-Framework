using System;
using System.Collections.Generic;

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

        private List<Stack<Type>> _history = new List<Stack<Type>>();
        private int activeHistoryGroup = 0;
        public int historyCount { get; private set; }

        public NavigationUpdate onNavigationUpdate = null;

        private ScreenNavigation() { }

        public ScreenNavigation(ScreenCollection<ControllerType> screens)
        {
            if (screens == null)
            {
                throw new ArgumentNullException("screens");
            }
            _screens = screens;
            _history.Add(new Stack<Type>(_screens.array.Length));
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

            targetScreen.Open(targetScreenData);
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

        public NavigationEvent Travel<ScreenType>(in WindowAnimation transition, object data, bool excludeCurrentFromHistory = false)
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

        public NavigationEvent Travel(Type screenType, in WindowAnimation transition, object data, bool excludeCurrentFromHistory = false)
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
            return TravelInternal(screenType, screen, screen.defaultAnimation, data, excludeCurrentFromHistory);
        }

        private NavigationEvent TravelInternal(Type screenType, IScreen<ControllerType> screen, in WindowAnimation transition, object data, bool excludeCurrentFromHistory)
        {
            if (activeScreenType != null)
            {
                WindowAnimation closeAnimation = transition.CreateInverseAnimation();
                IScreen<ControllerType> activeScreen = _screens.dictionary[activeScreenType];
                activeScreen.Close(in closeAnimation);
                if (activeScreen.supportsHistory && !excludeCurrentFromHistory)
                {
                    PushToHistory(activeScreenType);
                }
            }
            screen.Open(in transition, data);
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
                    WindowAnimation closeAnimation = activeScreen.animator.animation.CreateInverseAnimation();
                    activeScreen.Close(in closeAnimation);

                    Type targetScreenType = PopFromHistory();
                    IScreen<ControllerType> targetScreen = _screens.dictionary[targetScreenType];

                    WindowAnimation openAnimation = targetScreen.animator.animation.CreateInverseAnimation();
                    targetScreen.Open(in openAnimation, targetScreen.data);
                    activeScreenType = targetScreenType;

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

        private void PushToHistory(Type type)
        {
            _history[activeHistoryGroup].Push(type);
            historyCount++;
        }

        private Type PopFromHistory()
        {
            if(_history[activeHistoryGroup].Count > 0)
            {
                historyCount--;
                Type type = _history[activeHistoryGroup].Pop();
                if(_history[activeHistoryGroup].Count == 0 && activeHistoryGroup > 0)
                {
                    activeHistoryGroup--;
                }
            }

            throw new InvalidOperationException("The history stack is empty.");
        }

        public void StartNewHistoryGroup()
        {
            _history.Add(new Stack<Type>(_screens.array.Length));
            activeHistoryGroup = _history.Count - 1;
        }

        public NavigationEvent ClearLatestHistoryGroup()
        {
            if(_history.Count > 1)
            {
                historyCount -= _history[activeHistoryGroup].Count;
                _history.RemoveAt(activeHistoryGroup);
                activeHistoryGroup--;
                return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.History, true, null, historyCount));
            }
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.History, false, null, historyCount));
        }

        public NavigationEvent InsertHistory<T>() where T : IScreen<ControllerType>
        {
            PushToHistory(typeof(T));
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.History, true, null, historyCount));
        }

        public NavigationEvent ClearHistory()
        {
            if(historyCount > 0)
            {
                if (activeHistoryGroup > 0)
                {
                    _history.RemoveRange(1, activeHistoryGroup);
                }
                _history[0].Clear();
                historyCount = 0;
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
