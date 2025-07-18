using System;

using UnityEngine;
using UnityEngine.Serialization;

namespace UIFramework.UIToolkit
{
    [RequireComponent(typeof(UIBehaviourDocument))]
    public class WindowCollector : WindowCollectorComponent
    {
        [SerializeField, FormerlySerializedAs("_collectableDefinitions")] private CollectableBehaviour<Window>[] _definitions = new CollectableBehaviour<Window>[0];
        private UIBehaviourDocument _uiBehaviourDocument = null;
        private IWindow[] _cachedWindows = null;

        private UIBehaviourDocument GetUIBehaviourDocument()
        {
            if (_uiBehaviourDocument == null)
            {
                _uiBehaviourDocument = GetComponent<UIBehaviourDocument>();
            }
            return _uiBehaviourDocument;
        }

        public override IWindow[] Collect()
        {
            if (_cachedWindows != null)
            {
                return _cachedWindows;
            }

            UIBehaviourDocument behaviourDocument = GetUIBehaviourDocument();
            if (behaviourDocument == null)
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
                IWindow window = _definitions[i].Type.CreateInstance(behaviourDocument, _definitions[i].Identifier);
                _cachedWindows[i] = window;
            }
            return _cachedWindows;
        }
    }
}
