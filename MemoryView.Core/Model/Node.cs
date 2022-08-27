using System.Text;

namespace MemoryView.Core;

public class Node
{
    private static int InstanceCounter;

    public int ID { get; }

    public bool IsPrimitive { get; init; }

    public string Label { get; }

    public List<(string Name, Node? Value)> References { get; } = new();

    public Node(string label)
    {
        ID = Interlocked.Increment(ref InstanceCounter);
        Label = label;
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
            else if (v.IsPrimitive)
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
