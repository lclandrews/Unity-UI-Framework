namespace UIFramework
{
    public delegate void IAccessibleAction(IAccessible accessible);

    public enum AccessOperation
    {
        Open,
        Close
    }

    public enum AccessState
    {
        Unitialized,
        Open,
        Opening,
        Closed,
        Closing
    }    

    public interface IAccessible
    {
        AccessState AccessState { get; }

        AnimationPlayer.PlaybackData AccessAnimationPlaybackData { get; }
        AnimationPlayable AccessAnimationPlayable { get; }

        WindowAccessAnimation GetDefaultAccessAnimation();
        void ResetAnimatedProperties();

        void Init();

        /// <summary>
        /// Open the IAccessible with the provided animation
        /// /// </summary>
        bool Open(in AnimationPlayable animationPlayable, IAccessibleAction onComplete = null);
        /// <summary>
        /// Open the IAccessible with immediately
        /// /// </summary>
        bool Open();

        /// <summary>
        /// Close the IAccessible with the provided animation
        /// </summary>
        /// <param name="animation"> The animation to play when closing the IAccessible. </param>
        /// <returns> True if the the IAccessible was closed. </returns>
        bool Close(in AnimationPlayable animationPlayable, IAccessibleAction onComplete = null);

        /// <summary>
        /// Close the IAccessible immediately with no animation
        /// </summary>
        /// <returns></returns>
        bool Close();

        bool SkipAccessAnimation();
    }
}
