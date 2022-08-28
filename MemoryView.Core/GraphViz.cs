using System.Net;
using System.Text;
using MemoryView.Core;

namespace MemoryView.Core;

public static class GraphViz
{
    public static void WriteDot(this Graph graph, TextWriter w)
    {
        w.WriteLine("digraph memory {");
        w.WriteLine("  rankdir=LR");
        w.WriteLine("  node [shape=record style=rounded]");
        w.WriteLine("  edge [dir=both arrowtail=dot tailclip=false]");

        IEnumerable<Node?> nodes = graph.Nodes;
        var roots = graph.Roots;
        WriteRootNodes(w, roots);
        WriteHeapNodes(w, nodes);

        WriteRootEdges(w, roots);
        WriteHeapEdges(w, nodes);

        w.WriteLine("}");
    }

    private static void WriteRootNodes(TextWriter w, IEnumerable<Reference> references)
    {
        var name = "Stack";
        w.WriteLine($"  subgraph cluster_{name} {{");
        w.WriteLine($"    graph [label=\"{name}\"]");
        w.WriteLine($"    roots [label=<<table>");

        int i = 1;
        foreach (var r in references)
        {
            w.WriteLine($"    {GetRefLabel(r, i.ToString())}");
            i++;
        }

        w.WriteLine($"    </table>> shape=none margin=0]");
        w.WriteLine("  }");
    }

    private static void WriteRootEdges(TextWriter w, IEnumerable<Reference> references)
    {
        int i = 1;
        foreach (var r in references)
        {
            var node = r.Value;
            if (node is not null)
            {
                w.WriteLine($"  roots:{i}:c -> {node.ID}");
            }
            i++;
        }
    }

    private static void WriteHeapNodes(TextWriter w, IEnumerable<Node?> nodes)
    {
        w.WriteLine($"  subgraph cluster_Heap {{");
        w.WriteLine($"    graph [label=\"Heap\"]");
        foreach (var node in nodes)
        {
            if (node is { IsPrimitive: false })
            {
                w.WriteLine($"    {node.ID} [label={GetNodeLabel(node)} shape=none margin=0]");
            }
        }
        w.WriteLine("  }");
    }

    private static void WriteHeapEdges(TextWriter w, IEnumerable<Node?> nodes)
    {
        foreach (var node in nodes)
        {
            if (node is not null && !node.IsPrimitive)
            {
                foreach (var r in node.References)
                {
                    if (r.Value is { IsPrimitive: false })
                    {
                        w.WriteLine($"  {node.ID}:<{r.Name}>:c -> {r.Value.ID}");
                    }
                }
            }
        }
    }

    private static string GetNodeLabel(Node node)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<");
        sb.AppendLine("    <table>");
        sb.AppendLine($"      <tr><td>{HtmlEsc(node.Label)}</td><td>ID: #{node.ID}</td></tr>");

        foreach (var r in node.References)
        {
            sb.AppendLine("      " + GetRefLabel(r));
        }

        sb.Append("    </table>>");

        return sb.ToString();
    }

    private static string GetRefLabel(Reference r, string? port = null)
    {
        var name = HtmlEsc(r.Name);
        var v = r.Value;
        if (v is null)
        {
            return $"<tr><td>{name}</td><td>&lt;null&gt;</td></tr>";
        }
        else if (v.IsPrimitive)
        {
            return $"<tr><td>{name}</td><td>{HtmlEsc(v.Label)}</td></tr>";
        }
        else
        {
            return $"<tr><td>{name}</td><td port=\"{port ?? name}\">&nbsp;&nbsp;</td></tr>";
        }
    }

    private static string HtmlEsc(string s) => WebUtility.HtmlEncode(s);
}
