using System.Runtime.CompilerServices;

namespace MemoryView.Core;

/// <summary> An equality comparer that compares by reference. </summary>
internal class ReferenceEqualityComparer : IEqualityComparer<object>
{
    public new bool Equals(object? a, object? b) => ReferenceEquals(a, b);

    public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
}
