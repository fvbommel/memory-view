using System.Text;

namespace MemoryView.Core;

public class Node
{
    private static int InstanceCounter;

    public int ID { get; }

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

        sb.AppendLine($"#{ID}: {Label}");

        // No need to print members of strings.
        if (Type != typeof(string))
        {
            PrintTo(sb, 1);
        }

        return sb.ToString().Trim();
    }

    internal void PrintTo(StringBuilder sb, int indentLevel)
    {
        foreach (var r in References)
        {
            // Ensure indentation.
            for (int i = 0; i < indentLevel; i++)
            {
                sb.Append("    ");
            }

            var v = r.Value;
            if (v is null)
            {
                sb.AppendLine($"{r.Name} = <null>");
            }
            else if (v.Type.IsPrimitive)
            {
                sb.AppendLine($"{r.Name} = {v.Label}");
            }
            else if (v.Type.IsValueType)
            {
                sb.AppendLine($"{r.Name} = {v.Label}");
                v.PrintTo(sb, indentLevel + 1);
            }
            else
            {
                sb.AppendLine($"{r.Name} => #{v.ID}");
            }
        }
    }
}
