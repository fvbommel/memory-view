using System.Runtime.CompilerServices;

namespace MemoryView.Core;

public static class Inspect
{
    public static Graph CreateGraph<T>(
        T root,
        [CallerArgumentExpression("root")] string? name = null)
    {
        // This method is generic to preserve Nullable<T>, which disappears when boxed.
        return new Graph()
            .Add(root, name);
    }

    public static Graph CreateGraph<T1, T2>(
        T1 root1,
        T2 root2,
        [CallerArgumentExpression("root1")] string? name1 = null,
        [CallerArgumentExpression("root2")] string? name2 = null)
    {
        // This method is generic to preserve Nullable<T>, which disappears when boxed.
        return new Graph()
            .Add(root1, name1)
            .Add(root2, name2);
    }

    public static Graph CreateGraph<T1, T2, T3>(
        T1 root1,
        T2 root2,
        T3 root3,
        [CallerArgumentExpression("root1")] string? name1 = null,
        [CallerArgumentExpression("root2")] string? name2 = null,
        [CallerArgumentExpression("root3")] string? name3 = null)
    {
        // This method is generic to preserve Nullable<T>, which disappears when boxed.
        return new Graph()
            .Add(root1, name1)
            .Add(root2, name2)
            .Add(root3, name3);
    }
}
