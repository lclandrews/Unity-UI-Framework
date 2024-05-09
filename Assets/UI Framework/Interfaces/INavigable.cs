using System;

namespace UIFramework
{
    public interface INavigable : IEquatable<INavigable>
    {
        bool supportsHistory { get; }
        WindowTransitionPlayable defaultTransition { get; }
    }
}