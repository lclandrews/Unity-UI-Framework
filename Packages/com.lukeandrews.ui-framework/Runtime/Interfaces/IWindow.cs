namespace UIFramework
{
    public interface IReadOnlyWindow : IReadOnlyUIBehaviour, IReadOnlyAccessible
    {
        string Identifier { get; }
        bool IsVisible { get; }
        bool IsEnabled { get; }
        bool IsInteractable { get; }
        int SortOrder { get; }
    }

    /// <summary>
    /// Interface <c>IWindow</c> defines expected contract for all <c>UIFramework</c> windows.
    /// </summary>
    public interface IWindow : IReadOnlyWindow, IAccessible, IDataRecipient, IUIBehaviour
    {
        new bool IsEnabled { get; set; }
        new bool IsInteractable { get; set; }
        new int SortOrder { get; set; }
        
        GenericWindowAnimation GetAnimation(GenericWindowAnimationType type);        

        bool SetWaiting(bool waiting);
    }
}