using System;
using UnityEngine;

namespace UIFramework
{
    public abstract class CollectorComponent : MonoBehaviour
    {
        public abstract CollectableType[] Collect<CollectableType>() where CollectableType : class, IUIBehaviour;

        protected bool MethodTypeAssignableFromClassType<ClassType, MethodType>()
        {
            Type classType = typeof(ClassType);
            Type methodType = typeof(MethodType);
            return methodType.IsAssignableFrom(classType);
        }
    }
}
