using System.Text;

namespace MemoryView.Core;

internal static class StringBuilderExtensions
{
    public static StringBuilder TrimRight(this StringBuilder sb)
    {
        var i = sb.Length;
        while (i > 0 && char.IsWhiteSpace(sb[i - 1]))
        {
            i--;
        }
        return sb.Remove(i, sb.Length - i);
    }
}
