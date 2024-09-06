namespace UIFramework
{
    /// <summary>
    /// Interface <c>IWindow</c> defines expected contract for all <c>UIFramework</c> windows.
    /// </summary>
    public interface IWindow : IAccessible, IDataRecipient, IUIBehaviour
    {
        bool IsVisible { get; }
        bool IsEnabled { get; set; }
        bool IsInteractable { get; set; }
        int SortOrder { get; set; }
        
        GenericWindowAnimation CreateAnimation(GenericWindowAnimationType type, float length);        

        bool SetWaiting(bool waiting);
    }
}