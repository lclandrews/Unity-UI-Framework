using System;

using UnityEngine;

namespace UIFramework.UGUI
{
    public class WindowCollector : WindowCollectorComponent
    {
        [SerializeField] private bool _useAttachedTransform = true;
        [SerializeField] private Transform[] _additionalTransforms = null;
        private IWindow[] _cachedWindows = null;

        public override IWindow[] Collect()
        {
            if (_cachedWindows != null)
            {
                return _cachedWindows;
            }

            _cachedWindows = new IWindow[0];
            Window[] transformBehaviours = null;

            if (_useAttachedTransform)
            {
                transformBehaviours = transform.GetComponentsInChildren<Window>(true);
                Array.Resize(ref _cachedWindows, _cachedWindows.Length + transformBehaviours.Length);
                Array.Copy(transformBehaviours, 0, _cachedWindows, _cachedWindows.Length - transformBehaviours.Length, transformBehaviours.Length);
            }

            for (int i = 0; i < _additionalTransforms.Length; i++)
            {
                if (_additionalTransforms[i] != transform)
                {
                    if (_additionalTransforms[i] == null)
                    {
                        throw new InvalidOperationException(string.Format("Attempting to collect UGUI behvaiours while transforms[{0}] is null.", i));
                    }
                    transformBehaviours = _additionalTransforms[i].GetComponentsInChildren<Window>(true);
                    Array.Resize(ref _cachedWindows, _cachedWindows.Length + transformBehaviours.Length);
                    Array.Copy(transformBehaviours, 0, _cachedWindows, _cachedWindows.Length - transformBehaviours.Length, transformBehaviours.Length);
                }
            }
            return _cachedWindows;
        }
    }
}