using System;

namespace UIFramework
{
    public interface IReadOnlyNavigableWindow : IReadOnlyWindow
    {
        bool SupportsHistory { get; }
    }

    public interface INavigableWindow : IReadOnlyNavigableWindow, IWindow, IEquatable<INavigableWindow>
    {
        
    }
}