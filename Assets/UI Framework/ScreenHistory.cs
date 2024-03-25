using System;
using System.Collections.Generic;

namespace UIFramework
{
    public class ScreenHistory<ControllerType> where ControllerType : Controller<ControllerType>
    {
        private struct Entry
        {
            public Type screenType { get; private set; }
            public ScreenTransition transition { get; private set; }

            public Entry(Type screenType, in ScreenTransition transition)
            {
                this.screenType = screenType;
                this.transition = transition;
            }
        }

        public int count { get; private set; }

        private List<Stack<Entry>> _history = new List<Stack<Entry>>();

        private int _activeGroup = 0;
        private int _groupCapacity = 0;

        public ScreenHistory(int capacity)
        {
            _history.Add(new Stack<Entry>(capacity));
        }

        public void Push(Type screenType, in ScreenTransition transition)
        {
            Entry entry = new Entry(screenType, transition);
            _history[_activeGroup].Push(entry);
            count++;
        }

        public void Pop(out Type screenType, out ScreenTransition transition)
        {
            if (_history[_activeGroup].Count > 0)
            {
                count--;
                Entry entry = _history[_activeGroup].Pop();
                if (_history[_activeGroup].Count == 0 && _activeGroup > 0)
                {
                    _activeGroup--;
                }

                screenType = entry.screenType;
                transition = entry.transition;
            }
            else
            {
                throw new InvalidOperationException("The history stack is empty.");
            }            
        }

        public void StartNewGroup()
        {
            _history.Add(new Stack<Entry>(_groupCapacity));
            _activeGroup = _history.Count - 1;
        }

        public bool ClearLatestGroup()
        {
            if (_history.Count > 1)
            {
                count -= _history[_activeGroup].Count;
                _history.RemoveAt(_activeGroup);
                _activeGroup--;
                return true;
            }
            return false;
        }

        public bool Clear()
        {
            if (count > 0)
            {
                if (_activeGroup > 0)
                {
                    _history.RemoveRange(1, _activeGroup);
                }
                _history[0].Clear();
                count = 0;
                return true;
            }
            return false;
        }
    }
}
