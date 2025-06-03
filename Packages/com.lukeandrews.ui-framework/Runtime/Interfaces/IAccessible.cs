using UnityEngine.Extension;

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
        AccessAnimationPlayable AccessAnimationPlayable { get; }

        event IAccessibleAction Opening;
        event IAccessibleAction Opened;
        event IAccessibleAction Closed;
        event IAccessibleAction Closing;

        AccessAnimation GetDefaultAccessAnimation();
        void ResetAnimatedProperties();

        void Init();

        /// <summary>
        /// Open the IAccessible with the provided animation
        /// /// </summary>
        bool Open(in AccessAnimationPlayable animationPlayable, IAccessibleAction onComplete = null);
        /// <summary>
        /// Open the IAccessible with immediately
        /// /// </summary>
        bool Open();

        /// <summary>
        /// Close the IAccessible with the provided animation
        /// </summary>
        /// <param name="animation"> The animation to play when closing the IAccessible. </param>
        /// <returns> True if the the IAccessible was closed. </returns>
        bool Close(in AccessAnimationPlayable animationPlayable, IAccessibleAction onComplete = null);

        /// <summary>
        /// Close the IAccessible immediately with no animation
        /// </summary>
        /// <returns></returns>
        bool Close();

        bool SkipAccessAnimation();
    }
}
