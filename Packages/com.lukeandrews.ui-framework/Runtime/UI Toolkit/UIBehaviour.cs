using System;

namespace UIFramework.UIToolkit
{
    public abstract class UIBehaviour : IUIBehaviour, IDisposable
    {        
        // IUIBehaviour
        public virtual void UpdateUI(float deltaTime)
        {
            
        }

        // IDisposable
        public virtual void Dispose()
        {

        }
    }
}