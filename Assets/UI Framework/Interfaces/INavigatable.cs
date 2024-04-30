using System;

namespace UIFramework
{
    public interface INavigatable : IEquatable<INavigatable>
    {
        bool supportsHistory { get; }
        WindowTransition defaultTransition { get; }
    }
}