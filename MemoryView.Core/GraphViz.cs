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

        foreach (var node in graph.Nodes)
        {
            if (node is not null && !node.IsPrimitive)
            {
                w.WriteLine($"  {node.ID} [label={GetLabel(node)} shape=none margin=0]");

                foreach (var r in node.References)
                {
                    var name = (r.Name);
                    if (r.Value is { IsPrimitive: false })
                    {
                        w.WriteLine($"  {node.ID}:<{name}>:c -> {r.Value.ID}");
                    }
                }
            }
        }

        w.WriteLine("}");
    }

    private static string GetLabel(Node node)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<");
        sb.AppendLine("    <table>");
        sb.AppendLine($"      <tr><td>{HtmlEsc(node.Label)}</td><td>ID: #{node.ID}</td></tr>");

        foreach (var r in node.References)
        {
            var name = HtmlEsc(r.Name);
            var v = r.Value;
            if (v is null)
            {
                sb.AppendLine($"      <tr><td>{name}</td><td>&lt;null&gt;</td></tr>");
            }
            else if (v.IsPrimitive)
            {
                sb.AppendLine($"      <tr><td>{name}</td><td>{HtmlEsc(v.Label)}</td></tr>");
            }
            else
            {
                // sb.AppendLine($"      <tr><td colspan=\"2\" port=\"{name}\">{name}</td></tr>");
                sb.AppendLine($"      <tr><td>{name}</td><td port=\"{name}\"></td></tr>");
            }
        }

        sb.Append("    </table>>");

        return sb.ToString();
    }

    private static string HtmlEsc(string s) => WebUtility.HtmlEncode(s);
}
