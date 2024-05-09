using System;

using UnityEngine;
using UnityEngine.UIElements;

namespace UIFramework.UIToolkit
{
    [Serializable]
    public struct CollectableDefinition<UIBehaviourType> where UIBehaviourType : class
    {
        public string name { get { return _name; } }
        public SubclassOf<UIBehaviourType> type { get { return _type; } }

        [SerializeField] private string _name;
        [SerializeField] private SubclassOf<UIBehaviourType> _type;

        public CollectableDefinition(string name, SubclassOf<UIBehaviourType> type)
        {
            _name = name;
            _type = type;
        }
    }

    public class Collector<UIBehaviourType> : CollectorComponent where UIBehaviourType : UIBehaviour
    {
        [SerializeField] private UIDocument _uiDocument = null;
        [SerializeField] private CollectableDefinition<UIBehaviourType>[] _collectableDefinitions = new CollectableDefinition<UIBehaviourType>[0];

        public Collector() { }

        public override CollectableType[] Collect<CollectableType>()
        {
            if (!MethodTypeAssignableFromClassType<UIBehaviourType, CollectableType>())
            {
                throw new InvalidOperationException(string.Format("Collector<{0}> class type cannot be assigned to Collect<{1}> method type.",
                    nameof(UIBehaviourType), nameof(CollectableType)));
            }

            if (_uiDocument == null)
            {
                throw new InvalidOperationException("uiDocument is null.");
            }

            if (_collectableDefinitions == null)
            {
                throw new InvalidOperationException("collectableDefinitions is null");
            }

            UIBehaviourType[] builtBehaviours = new UIBehaviourType[_collectableDefinitions.Length];
            for (int i = 0; i < _collectableDefinitions.Length; i++)
            {
                VisualElement behaviourVisualElement = _uiDocument.rootVisualElement.Q<VisualElement>(_collectableDefinitions[i].name);
                if (behaviourVisualElement == null)
                {
                    throw new InvalidOperationException(string.Format("Unable to find VisualElement for behaviour with name: {0}", _collectableDefinitions[i].name));
                }
                UIBehaviourType behaviour = _collectableDefinitions[i].type.CreateInstance(_uiDocument, behaviourVisualElement);
                builtBehaviours[i] = behaviour;
            }
            return Array.ConvertAll(builtBehaviours, delegate (UIBehaviourType input) { return input as CollectableType; });
        }
    }
}
