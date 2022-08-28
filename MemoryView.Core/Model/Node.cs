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

        foreach (var r in References)
        {
            var v = r.Value;
            if (v is null)
            {
                sb.AppendLine($"  {r.Name} = <null>");
            }
            else if (v.Type.IsPrimitive)
            {
                sb.AppendLine($"  {r.Name} = {v.Label}");
            }
            else
            {
                sb.AppendLine($"  {r.Name} => #{v.ID}");
            }
        }

        return sb.ToString().Trim();
    }
}
