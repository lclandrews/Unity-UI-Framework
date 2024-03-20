namespace UIFramework
{
    public enum WindowState
    {
        Open,
        Opening,
        Closed,
        Closing
    }

    /// <summary>
    /// Interface <c>IWindow</c> defines expected contract for all <c>UIFramework</c> windows.
    /// </summary>
    public interface IWindow : IUIBehaviour
    {
        WindowState state { get; }
        bool isEnabled { get; }
        bool isVisible { get; }

        bool requiresData { get; }
        object data { get; }

        WindowAnimation defaultAnimation { get; }
        IWindowAnimator animator { get; }

        bool Enable();
        bool Disable();

        bool SetWaiting(bool waiting);

        /// <summary>
        /// Open the window with the provided animation and data
        /// /// </summary>
        bool Open(in WindowAnimation animation, object data);
        // Open the window immediately with no animation and the provided data
        bool Open(object data);

        bool UpdateData(object data);
        bool IsValidData(object data);

        /// <summary>
        /// Close the window with the provided animation
        /// </summary>
        /// <param name="animation"> The animation to play when closing the window. </param>
        /// <returns> True if the the window was closed. </returns>
        bool Close(in WindowAnimation animation);

        /// <summary>
        /// Close the window immediately with no animation
        /// </summary>
        /// <returns></returns>
        bool Close();
    }
}