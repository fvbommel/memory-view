using System.Runtime.CompilerServices;

namespace MemoryView.Core;

/// <summary>
/// An equality comparer that compares reference types by reference
/// and value types by value.
/// </summary>
internal class ReferenceEqualityComparer : IEqualityComparer<object>
{
    public new bool Equals(object? a, object? b)
    {
        // Null is equal to null.
        if (a is null && b is null) return true;

        // Null is not equal to anything else.
        if (a is null || b is null) return false;

        // Type must match.
        var type = a.GetType();
        if (type != b.GetType()) return false;

        // Value types compare by value.
        if (type.IsValueType) return a.Equals(b);

        // Reference types compare by reference.
        return ReferenceEquals(a, b);
    }

    public int GetHashCode(object obj)
    {
        // Null => 0
        if (obj is null) return 0;

        // Use normal hash code for value types.
        if (obj.GetType().IsValueType) return obj.GetHashCode();

        // Use reference hash code for reference types.
        return RuntimeHelpers.GetHashCode(obj);
    }
}
