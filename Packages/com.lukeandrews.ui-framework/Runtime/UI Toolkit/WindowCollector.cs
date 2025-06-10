using System;

using UnityEngine;
using UnityEngine.Serialization;

namespace UIFramework.UIToolkit
{
    public class WindowCollector : WindowCollectorComponent    
    {
        [SerializeField, FormerlySerializedAs("_collectableDefinitions")] private CollectableBehaviour<Window>[] _definitions = new CollectableBehaviour<Window>[0];
        private UIBehaviourDocument _uiBehaviourDocument = null;
        private IWindow[] _cachedWindows = null;

        private void Awake()
        {
            _uiBehaviourDocument = GetComponent<UIBehaviourDocument>();
        }

        public override IWindow[] Collect()
        {
            if (_cachedWindows != null)
            {
                return _cachedWindows;
            }

            if (_uiBehaviourDocument == null)
            {
                throw new InvalidOperationException("UIBehaviourDocument is null.");
            }

            if (_definitions == null)
            {
                throw new InvalidOperationException("collectableDefinitions is null");
            }

            _cachedWindows = new IWindow[_definitions.Length];
            for (int i = 0; i < _definitions.Length; i++)
            {
                IWindow window = _definitions[i].Type.CreateInstance(_uiBehaviourDocument, _definitions[i].Identifier);
                _cachedWindows[i] = window;
            }
            return _cachedWindows;
        }
    }
}
