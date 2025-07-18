using UnityEngine;

namespace UIFramework
{
    public abstract class WindowCollectorComponent : MonoBehaviour
    {
        public abstract IWindow[] Collect();
    }
}
