using UnityEngine;
using UnityEngine.Extension;

namespace UIFramework
{
    public abstract class ModalDialog : MonoBehaviour, IUpdatable
    {
        protected IWindow DefaultWindow { get; }

        public void ManagedUpdate()
        {
            
        }
    }
}