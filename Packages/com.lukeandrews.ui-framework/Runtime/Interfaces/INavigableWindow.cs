using System;

namespace UIFramework
{
    public interface INavigableWindow : IWindow, IEquatable<INavigableWindow>
    {
        bool SupportsHistory { get; }
    }
}