using System;

using UnityEngine;
using UnityEngine.Serialization;

namespace UIFramework.UIToolkit
{
    [RequireComponent(typeof(UIBehaviourDocument))]
    public class ScreenCollector : ScreenCollectorComponent
    {
        [SerializeField, FormerlySerializedAs("_collectableDefinitions")] private CollectableBehaviour<Screen>[] _definitions = new CollectableBehaviour<Screen>[0];
        private UIBehaviourDocument _uiBehaviourDocument = null;
        private IScreen[] _cachedScreens = null;

        private UIBehaviourDocument GetUIBehaviourDocument()
        {
            if (_uiBehaviourDocument == null)
            {
                _uiBehaviourDocument = GetComponent<UIBehaviourDocument>();
            }
            return _uiBehaviourDocument;
        }

        public override IScreen[] Collect()
        {
            if (_cachedScreens != null)
            {
                return _cachedScreens;
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

            _cachedScreens = new IScreen[_definitions.Length];
            for (int i = 0; i < _definitions.Length; i++)
            {
                IScreen screen = _definitions[i].Type.CreateInstance(behaviourDocument, _definitions[i].Identifier);
                _cachedScreens[i] = screen;
            }
            return _cachedScreens;
        }
    }
}
