using System;
using System.Collections.Generic;

using UnityEngine;

namespace UIFramework
{
    public struct WindowNavigationEvent
    {
        public bool success { get; private set; }
        public INavigableWindow previousActiveWindow { get; private set; }
        public INavigableWindow activeWindow { get; private set; }
        public int historyCount { get; private set; }
        public bool isLocked { get; private set; }

        public WindowNavigationEvent(bool success, INavigableWindow previousActiveWindow, INavigableWindow activeWindow, int historyCount, bool isLocked)
        {
            this.success = success;
            this.previousActiveWindow = previousActiveWindow;
            this.activeWindow = activeWindow;
            this.historyCount = historyCount;
            this.isLocked = isLocked;
        }
    }

    public delegate void WindowNavigationUpdate(WindowNavigationEvent navigationEvent);

    public class WindowNavigation<Navigable> where Navigable : INavigableWindow
    {
        private Dictionary<Type, Navigable> _windows = null;

        public Navigable activeWindow
        {
            get
            {
                return activeWindowType != null ? _windows[activeWindowType] : default;
            }
        }

        public Type activeWindowType { get; private set; } = null;

        public int historyCount { get { return _history.count; } }

        public WindowNavigationUpdate onNavigationUpdate = null;

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

        private History<Type> _history = null;

        private WindowNavigation() { }

        public WindowNavigation(Dictionary<Type, Navigable> windows)
        {
            if (windows == null)
            {
                throw new ArgumentNullException(nameof(windows));
            }
            _windows = windows;
            _history = new History<Type>(_windows.Count);
        }

        public WindowNavigationEvent Travel<WindowType>(bool excludeCurrentFromHistory = false)
            where WindowType : Navigable
        {
            Type targetType = typeof(WindowType);
            Navigable targetWindow;
            if (ValidateTravelTarget(targetType, out targetWindow))
            {
                return InvokeNavigationUpdate(Travel(targetType, targetWindow, excludeCurrentFromHistory));
            }
            return InvokeNavigationUpdate(new WindowNavigationEvent(false, null, activeWindow, historyCount, isLocked));
        }

        public WindowNavigationEvent Travel(Navigable targetWindow, bool excludeCurrentFromHistory = false)
        {
            Type targetWindowType = targetWindow.GetType();
            if (ValidateTravelTarget(targetWindowType, targetWindow))
            {
                return Travel(targetWindowType, targetWindow, excludeCurrentFromHistory);
            }
            return InvokeNavigationUpdate(new WindowNavigationEvent(false, null, activeWindow, historyCount, isLocked));
        }

        private WindowNavigationEvent Travel(Type windowType, Navigable window, bool excludeCurrentFromHistory)
        {
            if (isLocked)
            {
                return InvokeNavigationUpdate(new WindowNavigationEvent(false, null, activeWindow, historyCount, isLocked));
            }

            if (activeWindowType != null && activeWindow.supportsHistory && !excludeCurrentFromHistory)
            {
                _history.Push(activeWindowType);
            }

            Navigable previousActiveWindow = activeWindow;
            activeWindowType = windowType;
            return InvokeNavigationUpdate(new WindowNavigationEvent(true, previousActiveWindow, window, historyCount, isLocked));
        }

        private bool ValidateTravelTarget(Type windowType, Navigable window)
        {
            Navigable cachedWindow = default;
            if (!ValidateTravelTarget(windowType, out cachedWindow))
            {
                return false;
            }

            if (!cachedWindow.Equals(window))
            {
                throw new InvalidOperationException(string.Format("Unable to travel to: {0}, the window type exists in navigation but the instance of the window provided does not.", windowType.ToString()));
            }
            return true;
        }

        private bool ValidateTravelTarget(Type windowType, out Navigable window)
        {
            window = default;
            if (activeWindowType == windowType)
            {
                Debug.LogWarning(string.Format("Unable to travel to: {0} as it is already the active window.", windowType.ToString()));
                return false;
            }

            if (!_windows.TryGetValue(windowType, out window))
            {
                throw new InvalidOperationException(string.Format("Unable to travel to: {0}, not found in navigation.", windowType.ToString()));
            }
            return true;
        }

        public WindowNavigationEvent Back()
        {
            if (activeWindow != null)
            {
                if (isLocked)
                {
                    return InvokeNavigationUpdate(new WindowNavigationEvent(false, null, activeWindow, historyCount, isLocked));
                }

                if (historyCount > 0)
                {
                    Type targetWindowType = _history.Pop();
                    Navigable targetWindow = _windows[targetWindowType];

                    Navigable previousActiveWindow = activeWindow;
                    activeWindowType = targetWindowType;

                    return InvokeNavigationUpdate(new WindowNavigationEvent(true, previousActiveWindow, targetWindow, historyCount, isLocked));
                }
            }
            return InvokeNavigationUpdate(new WindowNavigationEvent(false, null, activeWindow, historyCount, isLocked));
        }

        public WindowNavigationEvent Lock()
        {
            isLocked = true;
            return InvokeNavigationUpdate(new WindowNavigationEvent(isLocked == true, null, activeWindow, historyCount, isLocked));
        }

        public WindowNavigationEvent Unlock()
        {
            isLocked = false;
            return InvokeNavigationUpdate(new WindowNavigationEvent(isLocked == false, null, activeWindow, historyCount, isLocked));
        }

        public WindowNavigationEvent Clear()
        {
            if (activeWindowType != null)
            {
                isLocked = false;

                Navigable previousActiveWindow = activeWindow;
                ClearHistory();
                activeWindowType = null;
                return InvokeNavigationUpdate(new WindowNavigationEvent(true, previousActiveWindow, null, historyCount, isLocked));
            }
            return InvokeNavigationUpdate(new WindowNavigationEvent(false, null, activeWindow, historyCount, isLocked));
        }

        public void StartNewHistoryGroup()
        {
            _history.StartNewGroup();
        }

        public WindowNavigationEvent ClearLatestHistoryGroup()
        {
            if (_history.ClearLatestGroup())
            {
                return InvokeNavigationUpdate(new WindowNavigationEvent(true, null, activeWindow, historyCount, isLocked));
            }
            return InvokeNavigationUpdate(new WindowNavigationEvent(false, null, activeWindow, historyCount, isLocked));
        }

        public WindowNavigationEvent InsertHistory<T>() where T : Navigable
        {
            _history.Push(typeof(T));
            return InvokeNavigationUpdate(new WindowNavigationEvent(true, null, activeWindow, historyCount, isLocked));
        }

        public WindowNavigationEvent ClearHistory()
        {
            if (_history.Clear())
            {
                return InvokeNavigationUpdate(new WindowNavigationEvent(true, null, activeWindow, historyCount, isLocked));
            }
            return InvokeNavigationUpdate(new WindowNavigationEvent(false, null, activeWindow, historyCount, isLocked));
        }

        private WindowNavigationEvent InvokeNavigationUpdate(WindowNavigationEvent navigationEvent)
        {
            onNavigationUpdate.Invoke(navigationEvent);
            return navigationEvent;
        }
    }
}
