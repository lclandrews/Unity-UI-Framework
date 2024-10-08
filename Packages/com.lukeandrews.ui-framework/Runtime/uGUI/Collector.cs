using System;

using UnityEngine;

namespace UIFramework.UGUI
{
    public class Collector<UIBehaviourType> : CollectorComponent where UIBehaviourType : UIBehaviour
    {
        [SerializeField] private bool _useAttachedTransform = true;
        [SerializeField] private Transform[] _additionalTransforms = null;

        public override CollectableType[] Collect<CollectableType>()
        {
            if(!MethodTypeAssignableFromClassType<UIBehaviourType, CollectableType>())
            {
                throw new InvalidOperationException(string.Format("Collector<{0}> class type cannot be assigned to Collect<{1}> method type.", 
                    nameof(UIBehaviourType), nameof(CollectableType)));
            }

            UIBehaviourType[] results = new UIBehaviourType[0];
            UIBehaviourType[] transformBehaviours = null;

            if(_useAttachedTransform)
            {
                transformBehaviours = transform.GetComponentsInChildren<UIBehaviourType>(true);
                Array.Resize(ref results, results.Length + transformBehaviours.Length);
                Array.Copy(transformBehaviours, 0, results, results.Length - transformBehaviours.Length, transformBehaviours.Length);
            }

            for (int i = 0; i < _additionalTransforms.Length; i++)
            {
                if (_additionalTransforms[i] != transform)
                {
                    if (_additionalTransforms[i] == null)
                    {
                        throw new InvalidOperationException(string.Format("Attempting to collect UGUI behvaiours while transforms[{0}] is null.", i));
                    }
                    transformBehaviours = _additionalTransforms[i].GetComponentsInChildren<UIBehaviourType>(true);
                    Array.Resize(ref results, results.Length + transformBehaviours.Length);
                    Array.Copy(transformBehaviours, 0, results, results.Length - transformBehaviours.Length, transformBehaviours.Length);
                }                
            }
            return Array.ConvertAll(results, delegate(UIBehaviourType input) { return input as CollectableType; });
        }
    }
}