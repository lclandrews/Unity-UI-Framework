using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

namespace UIFramework.UIToolkit
{
    [Serializable]
    public struct ScreenDefinition<ControllerType> where ControllerType : Controller<ControllerType>
    {
        public string name { get { return _name; } }
        public SubclassOf<Screen<ControllerType>> type { get { return _type; } }

        [SerializeField] private string _name;
        [SerializeField] private SubclassOf<Screen<ControllerType>> _type;

        public ScreenDefinition(string name, SubclassOf<Screen<ControllerType>> type)
        {
            _name = name;
            _type = type;
        }
    }

    [Serializable]
    public class ScreenCollector<ControllerType> : IScreenCollector<ControllerType> where ControllerType : Controller<ControllerType>
    {
        [SerializeField] private UIDocument _uiDocument = null;
        [SerializeField] private ScreenDefinition<ControllerType>[] _screenDefinitions = new ScreenDefinition<ControllerType>[0];

        public ScreenCollector() { }

        public IScreen<ControllerType>[] Collect()
        {
            if (_uiDocument == null)
            {
                throw new InvalidOperationException("uiDocument is null.");
            }

            if (_screenDefinitions == null)
            {
                throw new InvalidOperationException("screenDefinitions is null");
            }

            IScreen<ControllerType>[] builtScreens = new IScreen<ControllerType>[_screenDefinitions.Length];
            for (int i = 0; i < _screenDefinitions.Length; i++)
            {
                VisualElement screenVisualElement = _uiDocument.rootVisualElement.Q<VisualElement>(_screenDefinitions[i].name);
                if(screenVisualElement == null)
                {
                    throw new InvalidOperationException(string.Format("Unable to find VisualElement for screen with name: {0}", _screenDefinitions[i].name));
                }
                Screen<ControllerType> screen = _screenDefinitions[i].type.CreateInstance(_uiDocument, screenVisualElement);
                builtScreens[i] = screen;
            }
            return builtScreens;
        }

        public void Collect(List<IScreen<ControllerType>> screens)
        {
            if (_uiDocument == null)
            {
                throw new InvalidOperationException("uiDocument is null.");
            }

            if (_screenDefinitions == null)
            {
                throw new InvalidOperationException("screenDefinitions is null");
            }

            if(screens == null)
            {
                throw new ArgumentNullException("screens");
            }

            screens.Clear();
            screens.Capacity = _screenDefinitions.Length;
            for (int i = 0; i < _screenDefinitions.Length; i++)
            {
                VisualElement screenVisualElement = _uiDocument.rootVisualElement.Q<VisualElement>(_screenDefinitions[i].name);
                if (screenVisualElement == null)
                {
                    throw new InvalidOperationException(string.Format("Unable to find VisualElement for screen with name: {0}", _screenDefinitions[i].name));
                }
                Screen<ControllerType> screen = _screenDefinitions[i].type.CreateInstance(_uiDocument, screenVisualElement);
                screens.Add(screen);
            }
        }
    }
}
