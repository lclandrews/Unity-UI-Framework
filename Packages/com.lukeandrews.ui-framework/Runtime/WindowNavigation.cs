using System;
using System.Collections.Generic;

using UnityEngine;

namespace UIFramework
{
    public struct WindowNavigationEvent
    {
        public bool Success { get; private set; }
        public INavigableWindow PreviousActiveWindow { get; private set; }
        public INavigableWindow ActiveWindow { get; private set; }
        public int HistoryCount { get; private set; }
        public bool IsLocked { get; private set; }

        public WindowNavigationEvent(bool success, INavigableWindow previousActiveWindow, INavigableWindow activeWindow, int historyCount, bool isLocked)
        {
            this.Success = success;
            this.PreviousActiveWindow = previousActiveWindow;
            this.ActiveWindow = activeWindow;
            this.HistoryCount = historyCount;
            this.IsLocked = isLocked;
        }
    }

    public delegate void WindowNavigationUpdate(WindowNavigationEvent navigationEvent);

    public class WindowNavigation<Navigable> where Navigable : INavigableWindow
    {
        public IReadOnlyDictionary<Type, Navigable> Windows { get { return _windows; } }
        private Dictionary<Type, Navigable> _windows = null;

        public Navigable ActiveWindow
        {
            get
            {
                return ActiveWindowType != null ? _windows[ActiveWindowType] : default;
            }
        }

        public Type ActiveWindowType { get; private set; } = null;

        public int HistoryCount { get { return _history.Count; } }

        public WindowNavigationUpdate OnNavigationUpdate = null;

        public bool IsLocked
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
            return InvokeNavigationUpdate(new WindowNavigationEvent(false, null, ActiveWindow, HistoryCount, IsLocked));
        }

        public WindowNavigationEvent Travel(Navigable targetWindow, bool excludeCurrentFromHistory = false)
        {
            Type targetWindowType = targetWindow.GetType();
            if (ValidateTravelTarget(targetWindowType, targetWindow))
            {
                return Travel(targetWindowType, targetWindow, excludeCurrentFromHistory);
            }
            return InvokeNavigationUpdate(new WindowNavigationEvent(false, null, ActiveWindow, HistoryCount, IsLocked));
        }

        private WindowNavigationEvent Travel(Type windowType, Navigable window, bool excludeCurrentFromHistory)
        {
            if (IsLocked)
            {
                return InvokeNavigationUpdate(new WindowNavigationEvent(false, null, ActiveWindow, HistoryCount, IsLocked));
            }

            if (ActiveWindowType != null && ActiveWindow.SupportsHistory && !excludeCurrentFromHistory)
            {
                _history.Push(ActiveWindowType);
            }

            Navigable previousActiveWindow = ActiveWindow;
            ActiveWindowType = windowType;
            return InvokeNavigationUpdate(new WindowNavigationEvent(true, previousActiveWindow, window, HistoryCount, IsLocked));
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
            if (ActiveWindowType == windowType)
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
            if (ActiveWindow != null)
            {
                if (IsLocked)
                {
                    return InvokeNavigationUpdate(new WindowNavigationEvent(false, null, ActiveWindow, HistoryCount, IsLocked));
                }

                if (HistoryCount > 0)
                {
                    Type targetWindowType = _history.Pop();
                    Navigable targetWindow = _windows[targetWindowType];

                    Navigable previousActiveWindow = ActiveWindow;
                    ActiveWindowType = targetWindowType;

                    return InvokeNavigationUpdate(new WindowNavigationEvent(true, previousActiveWindow, targetWindow, HistoryCount, IsLocked));
                }
            }
            return InvokeNavigationUpdate(new WindowNavigationEvent(false, null, ActiveWindow, HistoryCount, IsLocked));
        }

        public WindowNavigationEvent Lock()
        {
            IsLocked = true;
            return InvokeNavigationUpdate(new WindowNavigationEvent(IsLocked == true, null, ActiveWindow, HistoryCount, IsLocked));
        }

        public WindowNavigationEvent Unlock()
        {
            IsLocked = false;
            return InvokeNavigationUpdate(new WindowNavigationEvent(IsLocked == false, null, ActiveWindow, HistoryCount, IsLocked));
        }

        public WindowNavigationEvent Clear()
        {
            if (ActiveWindowType != null)
            {
                IsLocked = false;

                Navigable previousActiveWindow = ActiveWindow;
                ClearHistory();
                ActiveWindowType = null;
                return InvokeNavigationUpdate(new WindowNavigationEvent(true, previousActiveWindow, null, HistoryCount, IsLocked));
            }
            return InvokeNavigationUpdate(new WindowNavigationEvent(false, null, ActiveWindow, HistoryCount, IsLocked));
        }

        public void StartNewHistoryGroup()
        {
            _history.StartNewGroup();
        }

        public WindowNavigationEvent ClearLatestHistoryGroup()
        {
            if (_history.ClearLatestGroup())
            {
                return InvokeNavigationUpdate(new WindowNavigationEvent(true, null, ActiveWindow, HistoryCount, IsLocked));
            }
            return InvokeNavigationUpdate(new WindowNavigationEvent(false, null, ActiveWindow, HistoryCount, IsLocked));
        }

        public WindowNavigationEvent InsertHistory<T>() where T : Navigable
        {
            _history.Push(typeof(T));
            return InvokeNavigationUpdate(new WindowNavigationEvent(true, null, ActiveWindow, HistoryCount, IsLocked));
        }

        public WindowNavigationEvent ClearHistory()
        {
            if (_history.Clear())
            {
                return InvokeNavigationUpdate(new WindowNavigationEvent(true, null, ActiveWindow, HistoryCount, IsLocked));
            }
            return InvokeNavigationUpdate(new WindowNavigationEvent(false, null, ActiveWindow, HistoryCount, IsLocked));
        }

        private WindowNavigationEvent InvokeNavigationUpdate(WindowNavigationEvent navigationEvent)
        {
            OnNavigationUpdate.Invoke(navigationEvent);
            return navigationEvent;
        }
    }
}
