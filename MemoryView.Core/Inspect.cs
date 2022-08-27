namespace MemoryView.Core;

public static class Inspect
{
    public static Graph CreateGraph(object root)
    {
        return new Graph().Add(root);
    }
}
