using System.Text;

namespace MemoryView;

public class Node
{
    private static int InstanceCounter;

    public int ID { get; }

    public bool IsBoxed { get; set; }

    public string Label { get; }

    public Type Type { get; }

    public List<Reference> References { get; } = new();

    public Node(string label, Type type)
    {
        ID = Interlocked.Increment(ref InstanceCounter);
        Label = label;
        Type = type;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        Print(sb);

        return sb.TrimRight().ToString();
    }

    internal void Print(StringBuilder sb)
    {
        // Header.
        if (Type.IsPrimitive)
        {
            sb.AppendLine($"#{ID}: {Type.GetDisplayName()} = {Label}");
        }
        else
        {
            sb.AppendLine($"#{ID}: {Label}");
        }

        // Members.
        PrintReferences(sb, 1);
    }

    internal void PrintReferences(StringBuilder sb, int indentLevel)
    {
        foreach (var r in References)
        {
            r.Print(sb, indentLevel);
        }
    }
}
