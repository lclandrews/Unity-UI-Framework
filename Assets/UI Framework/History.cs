using System;
using System.Collections.Generic;

namespace UIFramework
{
    public class History<T>
    {
        public int count { get; private set; }

        private List<Stack<T>> _history = new List<Stack<T>>();

        private int _activeGroup = 0;
        private int _groupCapacity = 0;

        public History(int capacity)
        {
            _history.Add(new Stack<T>(capacity));
        }

        public void Push(T entry)
        {
            _history[_activeGroup].Push(entry);
            count++;
        }

        public T Pop()
        {
            if (_history[_activeGroup].Count > 0)
            {
                count--;
                T entry = _history[_activeGroup].Pop();
                if (_history[_activeGroup].Count == 0 && _activeGroup > 0)
                {
                    _activeGroup--;
                }

                return entry;
            }
            else
            {
                throw new InvalidOperationException("The history stack is empty.");
            }            
        }

        public void StartNewGroup()
        {
            _history.Add(new Stack<T>(_groupCapacity));
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
