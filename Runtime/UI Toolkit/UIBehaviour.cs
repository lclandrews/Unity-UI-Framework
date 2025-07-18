using System;

namespace UIFramework.UIToolkit
{
    public abstract class UIBehaviour : IUIBehaviour
    {
        public BehaviourState State { get; private set; } = BehaviourState.Uninitialized;

        // IUIBehaviour
        public abstract bool IsValid();

        public virtual void Initialize() 
        {
            State = BehaviourState.Initialized;
        }

        public virtual void UpdateUI(float deltaTime) { }

        public virtual void Terminate() 
        {
            State = BehaviourState.Terminated;
        }
    }
}