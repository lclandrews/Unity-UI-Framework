namespace UIFramework
{
    public enum BehaviourState
    {
        Uninitialized,
        Initialized,
        Terminated
    }

    public interface IReadOnlyUIBehaviour
    {
        BehaviourState State { get; }
        public bool IsValid();
    }

    public interface IUIBehaviour : IReadOnlyUIBehaviour
    {
        void Initialize();        
        public void UpdateUI(float deltaTime);
        void Terminate();
    }
}
