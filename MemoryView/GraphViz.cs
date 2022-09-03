using System.Net;
using System.Text;

namespace MemoryView;

public static class GraphViz
{
    /// <summary> Writes the memory graph out in GraphViz "dot" format. </summary>
    /// <param name="graph"> The graph. </param>
    /// <param name="w"> The writer to use. </param>
    public static void WriteDot(this MemoryGraph graph, TextWriter w)
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

    private static void WriteRootNodes(TextWriter w, IEnumerable<Field> fields, List<string> edges)
    {
        w.WriteLine($"  roots [label=<<table cellborder=\"0\" rows=\"*\" columns=\"*\">");
        w.WriteLine($"      <tr><td colspan=\"3\"><b>Roots</b></td></tr>");

        int idx = 0;
        foreach (var f in fields)
        {
            w.WriteLine($"      {GetFieldLabel(f, edges, ("roots", $"{idx++}_"))}");
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
                foreach (var f in node.Fields)
                {
                    w.WriteLine("        " + GetFieldLabel(f, edges, (node.ID.ToString(), "")));
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
        foreach (var f in node.Fields)
        {
            // We update the prefix here because base classes may declare identically-named fields.
            sb.AppendLine("        " + GetFieldLabel(f, edges, (prefix.node, prefix.field + idx + "|")));
            idx++;
        }

        sb.Append("      </table>");

        return sb.ToString();
    }

    private static string GetFieldLabel(Field f, List<string> edges, (string node, string field) prefix)
    {
        var name = HtmlEsc(f.Name);
        var v = f.Value;
        if (v is null)
        {
            if (f.Name == "...")
            {
                // Trailing array elements.
                return $"<tr><td colspan=\"3\">...</td></tr>";
            }
            return $"<tr><td>{name}</td><td>{HtmlEsc(f.DeclaredType.GetDisplayName())}</td><td><i>null</i></td></tr>";
        }
        else if (f.DeclaredType.IsPrimitive)
        {
            return $"<tr><td>{name}</td><td>{f.DeclaredType.GetDisplayName()}</td><td>{v.Label}</td></tr>";
        }
        else if (f.DeclaredType.IsValueType)
        {
            var subPrefix = (prefix.node, prefix.field + f.Name + "|");
            return $"<tr><td>{name}</td><td colspan=\"2\">{GetNodeLabel(v, edges, subPrefix)}</td></tr>";
        }
        else
        {
            var type = HtmlEsc(f.DeclaredType.GetDisplayName());
            var port = HtmlEsc(prefix.field) + name;

            edges.Add($"{prefix.node}:{StringEsc(prefix.field + f.Name)}:c -> {v.ID}");

            return $"<tr><td>{name}</td><td>{type}</td><td port=\"{port}\">&nbsp;&nbsp;</td></tr>";
        }
    }

    private static string HtmlEsc(string s) => WebUtility.HtmlEncode(s).Replace("\n", "<br/>");

    private static string StringEsc(string s)
    {
        var sb = new StringBuilder();
        sb.Append('_'); // Reserve space for quote character, but don't let it be escaped below.
        sb.Append(s);

        // Escape special characters.
        sb.Replace("\\", "\\\\"); // Escape existing backslashes.
        sb.Replace("\"", "\\\""); // Add backslashes before quotes.
        sb.Replace("\n", "\\n"); // Escape newlines.

        // Surround with quotes.
        sb[0] = '"';
        sb.Append('"');

        return sb.ToString();
    }
}
