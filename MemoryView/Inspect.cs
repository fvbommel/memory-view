using System.Runtime.CompilerServices;

namespace MemoryView;

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

    public static Graph CreateGraph<T1, T2, T3, T4>(
        T1 root1,
        T2 root2,
        T3 root3,
        T4 root4,
        [CallerArgumentExpression("root1")] string? name1 = null,
        [CallerArgumentExpression("root2")] string? name2 = null,
        [CallerArgumentExpression("root3")] string? name3 = null,
        [CallerArgumentExpression("root4")] string? name4 = null)
    {
        // This method is generic to preserve Nullable<T>, which disappears when boxed.
        return new Graph()
            .Add(root1, name1)
            .Add(root2, name2)
            .Add(root3, name3)
            .Add(root4, name4);
    }

    public static Graph CreateGraph<T1, T2, T3, T4, T5>(
        T1 root1,
        T2 root2,
        T3 root3,
        T4 root4,
        T5 root5,
        [CallerArgumentExpression("root1")] string? name1 = null,
        [CallerArgumentExpression("root2")] string? name2 = null,
        [CallerArgumentExpression("root3")] string? name3 = null,
        [CallerArgumentExpression("root4")] string? name4 = null,
        [CallerArgumentExpression("root5")] string? name5 = null)
    {
        // This method is generic to preserve Nullable<T>, which disappears when boxed.
        return new Graph()
            .Add(root1, name1)
            .Add(root2, name2)
            .Add(root3, name3)
            .Add(root4, name4)
            .Add(root5, name5);
    }
}
