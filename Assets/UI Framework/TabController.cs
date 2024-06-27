using UnityEngine.Extension;

using System;

namespace UIFramework
{
    public class TabController : IUIBehaviour
    {
        private ObjectTypeMap<IWindow> _windows = null;

        public IWindow ActiveTabWindow { get { return _activeTabWindow; } }
        private IWindow _activeTabWindow = null;

        public int ActiveTabIndex { get { return _activeTabIndex; } }
        private int _activeTabIndex = 0;

        private TabController() { }

        public TabController(IWindow[] windows, int activeTabIndex = 0)
        {
            Populate(windows, activeTabIndex);
        }

        public void UpdateUI(float deltaTime)
        {
            for (int i = 0; i < _windows.array.Length; i++)
            {
                if (_windows.array[i].IsVisible)
                {
                    _windows.array[i].UpdateUI(deltaTime);
                }
            }
        }

        public void Clear()
        {
            if (_activeTabWindow != null)
            {
                _activeTabWindow.Close();
            }
            _activeTabWindow = null;
            _windows = null;
            _activeTabIndex = 0;
        }

        public void Populate(IWindow[] windows, int activeTabIndex = 0)
        {
            if (_windows != null)
            {
                Clear();
            }

            _windows = new ObjectTypeMap<IWindow>(windows);
            for (int i = 0; i < _windows.array.Length; i++)
            {
                _windows.array[i].Init();
                if (i == activeTabIndex)
                {
                    _windows.array[i].Open();
                    _activeTabWindow = _windows.array[i];
                    _activeTabIndex = i;
                }
            }

            if (_activeTabWindow == null && _windows.array.Length > 0)
            {
                _activeTabWindow = _windows.array[0];
                _activeTabWindow.Open();
            }
        }

        public IWindow SetActive<WindowType>(GenericWindowAnimationType type = GenericWindowAnimationType.Fade, float length = 0.5F, EasingMode easingMode = EasingMode.EaseInOut) where WindowType : IWindow
        {
            return SetActiveInternal<WindowType>(new WindowAccessPlayable(type, length, easingMode));
        }

        public IWindow SetActive<WindowType>(in WindowAccessPlayable animation) where WindowType : IWindow
        {
            return SetActiveInternal<WindowType>(in animation);
        }

        public IWindow SetActive<WindowType>(object data, GenericWindowAnimationType type = GenericWindowAnimationType.Fade, float length = 0.5F, EasingMode easingMode = EasingMode.EaseInOut) where WindowType : IWindow
        {
            IWindow window = SetActiveInternal<WindowType>(new WindowAccessPlayable(type, length, easingMode));
            window.SetData(data);
            return window;
        }

        public IWindow SetActive<WindowType>(object data, in WindowAccessPlayable animation) where WindowType : IWindow
        {
            IWindow window = SetActiveInternal<WindowType>(in animation);
            window.SetData(data);
            return window;
        }

        public IWindow SetActiveIndex(int index, GenericWindowAnimationType type = GenericWindowAnimationType.Fade, float length = 0.5F, EasingMode easingMode = EasingMode.EaseInOut)
        {
            return SetActiveIndexInternal(index, new WindowAccessPlayable(type, length, easingMode));
        }

        public IWindow SetActiveIndex(int index, in WindowAccessPlayable animation)
        {
            return SetActiveIndexInternal(index, in animation);
        }

        public IWindow SetActiveIndex(int index, object data, GenericWindowAnimationType type = GenericWindowAnimationType.Fade, float length = 0.5F, EasingMode easingMode = EasingMode.EaseInOut)
        {
            IWindow window = SetActiveIndexInternal(index, new WindowAccessPlayable(type, length, easingMode));
            window.SetData(data);
            return window;
        }

        public IWindow SetActiveIndex(int index, object data, in WindowAccessPlayable animation)
        {
            IWindow window = SetActiveIndexInternal(index, in animation);
            window.SetData(data);
            return window;
        }

        private IWindow SetActiveInternal<WindowType>(in WindowAccessPlayable animation) where WindowType : IWindow
        {
            if (_activeTabWindow != null)
            {
                Type windowType = typeof(WindowType);
                if (windowType != _activeTabWindow.GetType())
                {
                    IWindow targetWindow;
                    if (_windows.dictionary.TryGetValue(windowType, out targetWindow))
                    {
                        for (int i = 0; i < _windows.array.Length; i++)
                        {
                            if (_windows.array[i] == targetWindow)
                            {
                                _activeTabIndex = i;
                                break;
                            }
                        }

                        AnimationPlayable closeAnimationPlayable;
                        if (_activeTabWindow.AccessAnimationPlayable.Animation != null)
                        {
                            closeAnimationPlayable = _activeTabWindow.AccessAnimationPlayable.CreateInverse();
                        }
                        else
                        {
                            closeAnimationPlayable = _activeTabWindow.CreateDefaultAccessAnimation(animation.Length).CreatePlayable(animation.Length, AccessOperation.Close, 0.0F, animation.EasingMode, TimeMode.Scaled);
                        }
                        _activeTabWindow.Close(in closeAnimationPlayable);
                        _activeTabWindow = targetWindow;
                        _activeTabWindow.Open(animation.CreatePlayable(_activeTabWindow, AccessOperation.Open, 0.0F, TimeMode.Scaled));
                    }
                }
            }
            return _activeTabWindow;
        }

        private IWindow SetActiveIndexInternal(int index, in WindowAccessPlayable animation)
        {
            if (index != ActiveTabIndex)
            {
                if (_activeTabWindow != null)
                {
                    if (index >= 0 && index < _windows.array.Length)
                    {
                        _activeTabIndex = index;
                        IWindow targetWindow = _windows.array[index];

                        AnimationPlayable closeAnimationPlayable;
                        if (_activeTabWindow.AccessAnimationPlayable.Animation != null)
                        {
                            closeAnimationPlayable = _activeTabWindow.AccessAnimationPlayable.CreateInverse();
                        }
                        else
                        {
                            closeAnimationPlayable = _activeTabWindow.CreateDefaultAccessAnimation(animation.Length).CreatePlayable(animation.Length, AccessOperation.Close, 0.0F, animation.EasingMode, TimeMode.Scaled);
                        }
                        _activeTabWindow.Close(in closeAnimationPlayable);
                        _activeTabWindow = targetWindow;
                        _activeTabWindow.Open(animation.CreatePlayable(_activeTabWindow, AccessOperation.Open, 0.0F, TimeMode.Scaled));
                    }
                }
            }
            return _activeTabWindow;
        }
    }
}