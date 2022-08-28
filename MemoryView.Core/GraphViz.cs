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

        var edges = new List<string>();

        WriteRootNodes(w, graph.Roots, edges);
        WriteHeapNodes(w, graph.Nodes, edges);

        foreach (var edge in edges)
        {
            w.WriteLine($"  {edge}");
        }

        w.WriteLine("}");
    }

    private static void WriteRootNodes(TextWriter w, IEnumerable<Reference> references, List<string> edges)
    {
        var name = "Stack";
        w.WriteLine($"  subgraph cluster_{name} {{");
        w.WriteLine($"    graph [label=\"{name}\"]");
        w.WriteLine($"    roots [label=<<table>");
        w.WriteLine($"      <tr><td colspan=\"2\"><b>Roots</b></td>]</tr>");

        foreach (var r in references)
        {
            w.WriteLine($"      {GetRefLabel(r, edges, ("roots", ""))}");
        }

        w.WriteLine($"    </table>> shape=none margin=0]");
        w.WriteLine("  }");
    }

    private static void WriteHeapNodes(TextWriter w, IEnumerable<Node?> nodes, List<string> edges)
    {
        w.WriteLine($"  subgraph cluster_Heap {{");
        w.WriteLine($"    graph [label=\"Heap\"]");
        foreach (var node in nodes)
        {
            if (node is { IsValueType: false })
            {
                w.WriteLine($"    {node.ID} [label=<{GetNodeLabel(node, edges, (node.ID.ToString(), ""))}> shape = none margin = 0]");
            }
        }
        w.WriteLine("  }");
    }

    private static string GetNodeLabel(Node node, List<string> edges, (string node, string field) prefix)
    {
        var sb = new StringBuilder();

        sb.AppendLine("      <table>");
        sb.AppendLine($"        <tr><td colspan=\"2\"><b>{HtmlEsc(node.Label)}</b></td>]</tr>");

        foreach (var r in node.References)
        {
            sb.AppendLine("        " + GetRefLabel(r, edges, prefix));
        }

        sb.Append("      </table>");

        return sb.ToString();
    }

    private static string GetRefLabel(Reference r, List<string> edges, (string node, string field) prefix)
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
        else if (v.IsValueType)
        {
            var subPrefix = (prefix.node, prefix.field + r.Name + "|");
            return $"<tr><td>{name}</td><td>{GetNodeLabel(v, edges, subPrefix)}</td></tr>";
        }
        else
        {
            var port = HtmlEsc(prefix.field) + name;

            edges.Add($"{prefix.node}:{StringEsc(prefix.field + r.Name)}:c -> {v.ID}");

            return $"<tr><td>{name}</td><td port=\"{port}\">&nbsp;&nbsp;</td></tr>";
        }
    }

    private static string HtmlEsc(string s) => WebUtility.HtmlEncode(s);

    private static string StringEsc(string s)
        => '"' + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + '"';
}
