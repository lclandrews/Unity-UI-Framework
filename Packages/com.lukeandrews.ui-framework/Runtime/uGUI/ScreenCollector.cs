using System;

using UnityEngine;

namespace UIFramework.UGUI
{
    public class ScreenCollector : ScreenCollectorComponent
    {
        [SerializeField] private bool _useAttachedTransform = true;
        [SerializeField] private Transform[] _additionalTransforms = null;
        private IScreen[] _cachedScreens = null;

        public override IScreen[] Collect()
        {
            if (_cachedScreens != null)
            {
                return _cachedScreens;
            }

            _cachedScreens = new IScreen[0];
            Screen[] transformBehaviours = null;

            if (_useAttachedTransform)
            {
                transformBehaviours = transform.GetComponentsInChildren<Screen>(true);
                Array.Resize(ref _cachedScreens, _cachedScreens.Length + transformBehaviours.Length);
                Array.Copy(transformBehaviours, 0, _cachedScreens, _cachedScreens.Length - transformBehaviours.Length, transformBehaviours.Length);
            }

            for (int i = 0; i < _additionalTransforms.Length; i++)
            {
                if (_additionalTransforms[i] != transform)
                {
                    if (_additionalTransforms[i] == null)
                    {
                        throw new InvalidOperationException(string.Format("Attempting to collect UGUI behvaiours while transforms[{0}] is null.", i));
                    }
                    transformBehaviours = _additionalTransforms[i].GetComponentsInChildren<Screen>(true);
                    Array.Resize(ref _cachedScreens, _cachedScreens.Length + transformBehaviours.Length);
                    Array.Copy(transformBehaviours, 0, _cachedScreens, _cachedScreens.Length - transformBehaviours.Length, transformBehaviours.Length);
                }
            }
            return _cachedScreens;
        }
    }
}