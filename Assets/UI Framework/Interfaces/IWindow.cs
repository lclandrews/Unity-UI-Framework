namespace UIFramework
{
    /// <summary>
    /// Interface <c>IWindow</c> defines expected contract for all <c>UIFramework</c> windows.
    /// </summary>
    public interface IWindow : IAccessible, IDataRecipient, IUIBehaviour
    {
        bool isVisible { get; }
        bool isEnabled { get; set; }
        bool isInteractable { get; set; }
        int sortOrder { get; set; }
        
        GenericWindowAnimationBase CreateAnimation(GenericWindowAnimationType type, float length);        

        bool SetWaiting(bool waiting);
    }
}