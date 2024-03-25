using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace UIFramework
{
    public interface IModalWindow : IWindow
    {
        public struct Action
        {
            public string label { get; private set; }
            public UnityAction action { get; private set; }

            public Action(string label, UnityAction action)
            {
                this.label = label;
                this.action = action;
            }
        }

        public struct Data
        {
            public string message { get; private set; }
            public List<Action> actions { get; private set; }
            public Color? backgroundColor { get; private set; }

            public Data(string message, List<Action> actions, Color? backgroundColor)
            {
                this.message = message;
                this.actions = actions;
                this.backgroundColor = backgroundColor;
            }

            public Data(string message, List<Action> actions)
            {
                this.message = message;
                this.actions = actions;
                this.backgroundColor = null;
            }
        }

        void Init(Data data);
    }
}