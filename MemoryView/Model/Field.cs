using System.Text;

namespace MemoryView;

public record struct Field(string Name, Type DeclaredType, Node? Value)
{
    public override string ToString()
    {
        var sb = new StringBuilder();
        Print(sb, 0);
        return sb.TrimRight().ToString();
    }

    internal void Print(StringBuilder sb, int indentLevel)
    {
        // Ensure indentation.
        for (int i = 0; i < indentLevel; i++)
        {
            sb.Append("    ");
        }

        // Name and type.
        sb.Append($"{Name} : {DeclaredType.GetDisplayName()}");

        // Value.
        if (Value is null)
        {
            sb.AppendLine(" = <null>");
        }
        else if (DeclaredType.IsPrimitive)
        {
            sb.AppendLine($" = {Value.Label}");
        }
        else if (DeclaredType.IsValueType)
        {
            sb.AppendLine();
            Value.PrintFields(sb, indentLevel + 1);
        }
        else
        {
            sb.AppendLine($" => #{Value.ID}");
        }
    }
}
