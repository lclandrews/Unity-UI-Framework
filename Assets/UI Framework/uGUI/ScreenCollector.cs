using System;
using System.Collections.Generic;

using UnityEngine;

namespace UIFramework.UGUI
{
    [Serializable]
    public class ScreenCollector<ControllerType> : IScreenCollector<ControllerType> where ControllerType : Controller<ControllerType>
    {
        [SerializeField] private Transform[] rootScreenTransforms = null;

        public ScreenCollector() { }

        void IScreenCollector<ControllerType>.Collect(List<IScreen<ControllerType>> screens)
        {
            if (rootScreenTransforms == null)
            {
                throw new InvalidOperationException("rootScreenTransforms is null.");
            }

            if (screens == null)
            {
                throw new ArgumentNullException("screens");
            }

            screens.Clear();
            for (int i = 0; i < rootScreenTransforms.Length; i++)
            {
                if (rootScreenTransforms[i] == null)
                {
                    throw new InvalidOperationException(string.Format("Attempting to collect UGUI screens while transforms[{0}] is null.", i));
                }
                screens.AddRange(rootScreenTransforms[i].GetComponentsInChildren<IScreen<ControllerType>>(true));
            }
        }

        IScreen<ControllerType>[] IScreenCollector<ControllerType>.Collect()
        {
            if (rootScreenTransforms == null)
            {
                throw new InvalidOperationException("rootScreenTransforms is null.");
            }

            IScreen<ControllerType>[] results = new IScreen<ControllerType>[0];
            IScreen<ControllerType>[] transformScreens = null;
            for (int i = 0; i < rootScreenTransforms.Length; i++)
            {
                if (rootScreenTransforms[i] == null)
                {
                    throw new InvalidOperationException(string.Format("Attempting to collect UGUI screens while transforms[{0}] is null.", i));
                }
                transformScreens = rootScreenTransforms[i].GetComponentsInChildren<IScreen<ControllerType>>(true);
                Array.Resize(ref results, results.Length + transformScreens.Length);
                Array.Copy(transformScreens, 0, results, results.Length - transformScreens.Length, transformScreens.Length);
            }
            return results;
        }
    }
}