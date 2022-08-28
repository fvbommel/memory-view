using System.Runtime.CompilerServices;

namespace MemoryView.Core;

public static class Inspect
{
    public static Graph CreateGraph(
        object root,
        [CallerArgumentExpression("root")] string? name = null)
    {
        return new Graph()
            .Add(root, name);
    }

    public static Graph CreateGraph(
        object root1,
        object root2,
        [CallerArgumentExpression("root1")] string? name1 = null,
        [CallerArgumentExpression("root2")] string? name2 = null)
    {
        return new Graph()
            .Add(root1, name1)
            .Add(root2, name2);
    }

    public static Graph CreateGraph(
        object root1,
        object root2,
        object root3,
        [CallerArgumentExpression("root1")] string? name1 = null,
        [CallerArgumentExpression("root2")] string? name2 = null,
        [CallerArgumentExpression("root3")] string? name3 = null)
    {
        return new Graph()
            .Add(root1, name1)
            .Add(root2, name2)
            .Add(root3, name3);
    }
}
