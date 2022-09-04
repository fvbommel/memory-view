using System.Text;

namespace MemoryView;

public class Node
{
    private static int InstanceCounter;

    /// <summary> A unique ID value for this node. </summary>
    public int ID { get; }

    /// <summary> Whether this is a boxed value type. </summary>
    public bool IsBoxed { get; set; }

    /// <summary> The label for this node. </summary>
    public string Label { get; }

    /// <summary> The type of this node. </summary>
    public Type Type { get; }

    /// <summary> Fields contained in this node. </summary>
    public List<Field> Fields { get; } = new();

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
        if (Type.IsPrimitive || Type.IsEnum)
        {
            sb.AppendLine($"#{ID}: {Type.GetDisplayName()} = {Label}");
        }
        else
        {
            sb.AppendLine($"#{ID}: {Label}");
        }

        // Members.
        PrintFields(sb, 1);
    }

    internal void PrintFields(StringBuilder sb, int indentLevel)
    {
        foreach (var f in Fields)
        {
            f.Print(sb, indentLevel);
        }
    }
}
