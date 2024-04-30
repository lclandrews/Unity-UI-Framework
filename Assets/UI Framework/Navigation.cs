using System;

using UnityEngine;

namespace UIFramework
{
    public struct NavigationEvent
    {
        public enum Type
        {
            Init,
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
    // TransitionManager could be factored out from the Navigation class.
    // Instead of the navigation component being responsible for transitions, the controller
    // reads the responses of returned navigation events to invoke a given transition.
    // The side effect of this would be creating wrapper functions for all travel functions on the 
    // controller class, also there is a issue where in this current implementation the history stack of navigation is
    // coupled with a transition. This moves toward a model where the controller is the single point of responsibility for
    // controlling all dependant components instead of chaining dependency in this way.
    public class Navigation<Navigatable> where Navigatable : INavigatable, IWindow
    {
        private ArrayDictionary<Navigatable> _elements = null;

        public Navigatable activeElement
        {
            get
            {
                return activeElementType != null ? _elements.dictionary[activeElementType] : default;
            }
        }

        public Type activeElementType { get; private set; } = null;

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

        private History _history = null;
        private TransitionManager _transitionManager = null;

        private Navigation() { }

        public Navigation(ArrayDictionary<Navigatable> elements)
        {
            if (elements == null)
            {
                throw new ArgumentNullException(nameof(elements));
            }
            _elements = elements;
            _history = new History(_elements.array.Length);
            _transitionManager = new TransitionManager();
        }

        public NavigationEvent Init<ElementType>(object targetElementData) where ElementType : Navigatable
        {
            Type targetElementType = typeof(ElementType);
            Navigatable targetElement;
            if (!ValidateInit(targetElementType, out targetElement))
            {
                throw new Exception(string.Format("Attempted to initialize ElementNavigation with a element type of {0} that has not been found.", targetElementType.ToString()));
            }

            return InitInternal(targetElementType, targetElement, targetElementData, null);
        }

        public NavigationEvent Init<ElementType>(in WindowAnimation animation, object targetElementData) where ElementType : Navigatable
        {
            Type targetElementType = typeof(ElementType);
            Navigatable targetElement;
            if (!ValidateInit(targetElementType, out targetElement))
            {
                throw new Exception(string.Format("Attempted to initialize ElementNavigation with a element type of {0} that has not been found.", targetElementType.ToString()));
            }

            return InitInternal(targetElementType, targetElement, targetElementData, animation);
        }

        public NavigationEvent Init(Navigatable targetElement, object targetElementData)
        {
            Type targetElementType = targetElement.GetType();
            if (!ValidateInit(targetElementType, targetElement))
            {
                throw new Exception(string.Format("Attempted to initialize ElementNavigation with a element type of {0} that has not been found.", targetElementType.ToString()));
            }

            return InitInternal(targetElementType, targetElement, targetElementData, null);
        }

        public NavigationEvent Init(Navigatable targetElement, in WindowAnimation animation, object targetElementData)
        {
            Type targetElementType = targetElement.GetType();
            if (!ValidateInit(targetElementType, targetElement))
            {
                throw new Exception(string.Format("Attempted to initialize ElementNavigation with a element type of {0} that has not been found.", targetElementType.ToString()));
            }

            return InitInternal(targetElementType, targetElement, targetElementData, animation);
        }

        public NavigationEvent Init(Type targetElementType, object targetElementData)
        {
            if (!targetElementType.IsSubclassOf(typeof(Navigatable)))
            {
                throw new Exception("Attempted to initialize ElementNavigation with unrecognised type.");
            }

            Navigatable targetElement;
            if (!ValidateInit(targetElementType, out targetElement))
            {
                throw new Exception(string.Format("Attempted to initialize ElementNavigation with a element type of {0} that has not been found.", targetElementType.ToString()));
            }

            return InitInternal(targetElementType, targetElement, targetElementData, null);
        }

        public NavigationEvent Init(Type targetElementType, in WindowAnimation animation, object targetElementData)
        {
            if (!targetElementType.IsSubclassOf(typeof(Navigatable)))
            {
                throw new Exception("Attempted to initialize ElementNavigation with unrecognised type.");
            }

            Navigatable targetElement;
            if (!ValidateInit(targetElementType, out targetElement))
            {
                throw new Exception(string.Format("Attempted to initialize ElementNavigation with a element type of {0} that has not been found.", targetElementType.ToString()));
            }

            return InitInternal(targetElementType, targetElement, targetElementData, animation);
        }

        private NavigationEvent InitInternal(Type elementType, Navigatable element, object data, WindowAnimation? windowAnimation)
        {
            activeElementType = elementType;
            element.SetData(data);   
            if(windowAnimation != null)
            {
                element.Open(windowAnimation.Value);
            }
            else
            {
                element.Open();
            }
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Init, true, elementType, historyCount, isLocked));
        }

        private bool ValidateInit(Type elementType, Navigatable element)
        {
            Navigatable cachedElement;
            if(!ValidateInit(elementType, out cachedElement))
            {
                return false;
            }

            if(!cachedElement.Equals(element))
            {
                Debug.LogWarning(string.Format("Unable to init with: {0}, the element type exists in navigation but the instance of the element provided does not.", elementType.ToString()));
                return false;
            }
            return true;
        }

        private bool ValidateInit(Type elementType, out Navigatable element)
        {
            if (_elements == null)
            {
                throw new Exception("Attempted to initialize ElementNavigation with no elements set.");
            }

            if (activeElementType != null)
            {
                throw new Exception("Attempted to initialize ElementNavigation that has already been initialized.");
            }

            element = default;

            if (!_elements.dictionary.TryGetValue(elementType, out element))
            {
                return false;
            }
            return true;
        }

        public NavigationEvent Travel<ElementType>(in WindowTransition transition, object data, bool excludeCurrentFromHistory = false)
            where ElementType : Navigatable
        {
            Type targetType = typeof(ElementType);
            Navigatable targetElement;
            if(ValidateTravelTarget(targetType, out targetElement))
            {
                return InvokeNavigationUpdate(TravelInternal(targetType, targetElement, in transition, data, excludeCurrentFromHistory));
            }
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Travel, false, targetType, historyCount, isLocked));
        }

        public NavigationEvent Travel<ElementType>(object data, bool excludeCurrentFromHistory = false)
            where ElementType : Navigatable
        {
            Type targetType = typeof(ElementType);
            Navigatable targetElement;
            if (ValidateTravelTarget(targetType, out targetElement))
            {
                return InvokeNavigationUpdate(TravelInternal(targetType, targetElement, data, excludeCurrentFromHistory));
            }
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Travel, false, targetType, historyCount, isLocked));
        }

        public NavigationEvent Travel(Navigatable targetElement, in WindowTransition transition, object data, bool excludeCurrentFromHistory = false)
        {
            Type targetElementType = targetElement.GetType();
            if (ValidateTravelTarget(targetElementType, targetElement))
            {
                return TravelInternal(targetElementType, targetElement, in transition, data, excludeCurrentFromHistory);
            }
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Travel, false, targetElementType, historyCount, isLocked));
        }

        public NavigationEvent Travel(Navigatable targetElement, object data, bool excludeCurrentFromHistory = false)
        {
            Type targetElementType = targetElement.GetType();
            if (ValidateTravelTarget(targetElementType, targetElement))
            {
                return TravelInternal(targetElementType, targetElement, data, excludeCurrentFromHistory);
            }
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Travel, false, targetElementType, historyCount, isLocked));
        }

        public NavigationEvent Travel(Type elementType, in WindowTransition transition, object data, bool excludeCurrentFromHistory = false)
        {
            if (elementType.IsSubclassOf(typeof(Navigatable)))
            {
                Navigatable targetElement;
                if (ValidateTravelTarget(elementType, out targetElement))
                {
                    return TravelInternal(elementType, targetElement, in transition, data, excludeCurrentFromHistory);
                }
            }
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Travel, false, elementType, historyCount, isLocked));
        }

        public NavigationEvent Travel(Type elementType, object data, bool excludeCurrentFromHistory = false)
        {
            if (elementType.IsSubclassOf(typeof(Navigatable)))
            {
                Navigatable targetElement;
                if (ValidateTravelTarget(elementType, out targetElement))
                {
                    return TravelInternal(elementType, targetElement, data, excludeCurrentFromHistory);
                }
            }
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Travel, false, elementType, historyCount, isLocked));
        }

        private NavigationEvent TravelInternal(Type elementType, Navigatable element, object data, bool excludeCurrentFromHistory)
        {
            return TravelInternal(elementType, element, element.defaultTransition, data, excludeCurrentFromHistory);
        }

        private NavigationEvent TravelInternal(Type elementType, Navigatable element, in WindowTransition transition, object data, bool excludeCurrentFromHistory)
        {
            if(isLocked)
            {
                return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Travel, false, elementType, historyCount, isLocked));
            }

            if(activeElementType == null)
            {
                throw new InvalidOperationException("Unable to travel while there is no active element");
            }

            Navigatable activeElement = _elements.dictionary[activeElementType];
            if (activeElement.supportsHistory && !excludeCurrentFromHistory)
            {
                _history.Push(activeElementType, in transition);
            }

            element.SetData(data);
            _transitionManager.Transition(in transition, activeElement, element);

            activeElementType = elementType;
            return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Travel, true, elementType, historyCount, isLocked));
        }

        private bool ValidateTravelTarget(Type elementType, Navigatable element)
        {            
            Navigatable cachedElement = default;
            if(!ValidateTravelTarget(elementType, out cachedElement))
            {
                return false;
            }

            if(!cachedElement.Equals(element))
            {
                Debug.LogWarning(string.Format("Unable to travel to: {0}, the element type exists in navigation but the instance of the element provided does not.", elementType.ToString()));
                return false;
            }
            return true;
        }

        private bool ValidateTravelTarget(Type elementType, out Navigatable element)
        {
            element = default;
            if(activeElementType == elementType)
            {
                Debug.LogWarning(string.Format("Unable to travel to: {0} as it is already the active element.", elementType.ToString()));
                return false;
            }

            if(!_elements.dictionary.TryGetValue(elementType, out element))
            {
                Debug.LogWarning(string.Format("Unable to travel to: {0}, not found in navigation.", elementType.ToString()));
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

            Navigatable activeElement;
            if (_elements.dictionary.TryGetValue(activeElementType, out activeElement))
            {
                if (historyCount > 0)
                {                    
                    Type previousElementType;
                    WindowTransition transition; 
                    _history.Pop(out previousElementType, out transition);
                    Navigatable previousElement = _elements.dictionary[previousElementType];

                    previousElement.SetData(previousElement.data);
                    _transitionManager.ReverseTransition(in transition, previousElement, activeElement);

                    activeElementType = previousElementType;

                    return InvokeNavigationUpdate(new NavigationEvent(NavigationEvent.Type.Back, true, activeElementType, historyCount, isLocked));
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
            Navigatable activeElement = this.activeElement;
            TerminateInternal();
            _transitionManager.Terminate(true);
            activeElement.Close();
        }

        public void Terminate(in WindowAnimation animation)
        {
            Navigatable activeElement = this.activeElement;
            TerminateInternal();
            _transitionManager.Terminate(false);
            activeElement.Close(in animation);
        }

        private void TerminateInternal()
        {
            if (_elements == null)
            {
                throw new Exception("Attempted to terminate ElementNavigation with no elements set.");
            }

            if (activeElementType == null)
            {
                throw new Exception("Attempted to terminate ElementNavigation that has not been intialized.");
            }

            isLocked = false;

            ClearHistory();            
            activeElementType = null;
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

        public NavigationEvent InsertHistory<T>(in WindowTransition transition) where T : Navigatable
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
