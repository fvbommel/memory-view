using System.Net;
using System.Text;

namespace MemoryView;

public static class GraphViz
{
    public static void WriteDot(this Graph graph, TextWriter w)
    {
        w.WriteLine("digraph memory {");
        w.WriteLine("  rankdir=LR");
        w.WriteLine();
        w.WriteLine("  edge [dir=both arrowtail=dot tailclip=false]");

        var edges = new List<string>();

        WriteRootNodes(w, graph.Roots, edges);
        w.WriteLine();

        WriteHeapNodes(w, graph.Nodes, edges);
        w.WriteLine();

        foreach (var edge in edges)
        {
            w.WriteLine($"  {edge}");
        }

        w.WriteLine("}");
    }

    private static void WriteRootNodes(TextWriter w, IEnumerable<Reference> references, List<string> edges)
    {
        w.WriteLine($"  roots [label=<<table cellborder=\"0\" rows=\"*\" columns=\"*\">");
        w.WriteLine($"      <tr><td colspan=\"3\"><b>Roots</b></td></tr>");

        int idx = 0;
        foreach (var r in references)
        {
            w.WriteLine($"      {GetRefLabel(r, edges, ("roots", $"{idx++}_"))}");
        }

        w.WriteLine($"    </table>> shape=none margin=0]");
    }

    private static void WriteHeapNodes(TextWriter w, IEnumerable<Node?> nodes, List<string> edges)
    {
        w.WriteLine($"  subgraph cluster_Heap {{");
        w.WriteLine($"    graph [label=\"Heap\"]");
        w.WriteLine();
        foreach (var node in nodes)
        {
            if (node is { Type.IsValueType: false })
            {
                if (node.Type == typeof(string))
                {
                    w.WriteLine($"    {node.ID} [label={StringEsc(node.Label)} shape=rect style=rounded]");
                }
                else
                {
                    w.WriteLine($"    {node.ID} [label=<");
                    w.WriteLine($"      {GetNodeLabel(node, edges, (node.ID.ToString(), ""))}> shape=none margin=0]");
                }
            }
            else if (node is { IsBoxed: true })
            {
                w.WriteLine($"    {node.ID} [label=<");
                w.WriteLine("      <table cellborder=\"0\" rows=\"*\" columns=\"*\">");
                if (node.Type.IsPrimitive)
                {
                    w.WriteLine($"        <tr><td><b>Box</b></td><td><b>{HtmlEsc(node.Type.GetDisplayName())}</b></td><td>{HtmlEsc(node.Label)}</td></tr>");
                }
                else
                {
                    w.WriteLine($"        <tr><td><b>Box</b></td><td colspan=\"2\"><b>{HtmlEsc(node.Label)}</b></td></tr>");
                }
                foreach (var r in node.References)
                {
                    w.WriteLine("        " + GetRefLabel(r, edges, (node.ID.ToString(), "")));
                }

                w.WriteLine("      </table>> shape=none margin=0]");
            }
        }
        w.WriteLine("  }");
    }

    private static string GetNodeLabel(Node node, List<string> edges, (string node, string field) prefix)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<table cellborder=\"0\" rows=\"*\" columns=\"*\">");
        sb.AppendLine($"        <tr><td colspan=\"3\"><b>{HtmlEsc(node.Label)}</b></td></tr>");

        int idx = 0;
        foreach (var r in node.References)
        {
            // We update the prefix here because base classes may declare identically-named fields.
            sb.AppendLine("        " + GetRefLabel(r, edges, (prefix.node, prefix.field + idx + "|")));
            idx++;
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
            if (r.Name == "...")
            {
                // Trailing array elements.
                return $"<tr><td colspan=\"3\">...</td></tr>";
            }
            return $"<tr><td>{name}</td><td>{HtmlEsc(r.DeclaredType.GetDisplayName())}</td><td><i>null</i></td></tr>";
        }
        else if (r.DeclaredType.IsPrimitive)
        {
            return $"<tr><td>{name}</td><td>{r.DeclaredType.GetDisplayName()}</td><td>{v.Label}</td></tr>";
        }
        else if (r.DeclaredType.IsValueType)
        {
            var subPrefix = (prefix.node, prefix.field + r.Name + "|");
            return $"<tr><td>{name}</td><td colspan=\"2\">{GetNodeLabel(v, edges, subPrefix)}</td></tr>";
        }
        else
        {
            var type = HtmlEsc(r.DeclaredType.GetDisplayName());
            var port = HtmlEsc(prefix.field) + name;

            edges.Add($"{prefix.node}:{StringEsc(prefix.field + r.Name)}:c -> {v.ID}");

            return $"<tr><td>{name}</td><td>{type}</td><td port=\"{port}\">&nbsp;&nbsp;</td></tr>";
        }
    }

    private static string HtmlEsc(string s) => WebUtility.HtmlEncode(s);

    private static string StringEsc(string s)
        => '"' + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + '"';
}
