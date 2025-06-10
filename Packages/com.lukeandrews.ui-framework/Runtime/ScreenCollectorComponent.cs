using UnityEngine;

namespace UIFramework
{
    public abstract class ScreenCollectorComponent : MonoBehaviour
    {
        public abstract IScreen[] Collect();
    }
}
