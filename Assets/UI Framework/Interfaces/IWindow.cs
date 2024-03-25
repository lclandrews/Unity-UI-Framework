namespace UIFramework
{
    public enum WindowState
    {
        Unitialized,
        Open,
        Opening,
        Closed,
        Closing
    }

    /// <summary>
    /// Interface <c>IWindow</c> defines expected contract for all <c>UIFramework</c> windows.
    /// </summary>
    public interface IWindow : IWindowData, IUIBehaviour
    {
        WindowState state { get; }
        bool isVisible { get; }
        bool isEnabled { get; set; }
        bool isInteractable { get; set; }

        IWindowAnimator animator { get; }

        void Init();

        bool SetWaiting(bool waiting);

        /// <summary>
        /// Open the window with the provided animation
        /// /// </summary>
        bool Open(in WindowAnimation animation);
        /// <summary>
        /// Open the window with immediately
        /// /// </summary>
        bool Open();

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