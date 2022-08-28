using System.Text;

namespace MemoryView.Core;

internal static class TypeExtensions
{
    /// <summary> Gets the display name for a type. </summary>
    /// <param name="type">The type.</param>
    public static string GetDisplayName(this Type type)
    {
        // Is it a complicated type?
        if (type.IsGenericType || type.DeclaringType is not null)
        {
            var sb = new StringBuilder();
            PrintDisplayName(sb, type);
            return sb.ToString();
        }

        return GetSimpleTypeName(type);
    }

    private static string GetSimpleTypeName(Type type)
    {
        // Is this a well-known type?
        if (type == typeof(object)) { return "object"; }
        if (type == typeof(string)) { return "string"; }
        if (type.IsPrimitive)
        {
            if (type == typeof(bool)) { return "bool"; }
            if (type == typeof(char)) { return "char"; }
            if (type == typeof(byte)) { return "byte"; }
            if (type == typeof(short)) { return "short"; }
            if (type == typeof(int)) { return "int"; }
            if (type == typeof(long)) { return "long"; }
            if (type == typeof(nint)) { return "nint"; }
            if (type == typeof(sbyte)) { return "sbyte"; }
            if (type == typeof(ushort)) { return "ushort"; }
            if (type == typeof(uint)) { return "uint"; }
            if (type == typeof(ulong)) { return "ulong"; }
            if (type == typeof(nuint)) { return "nuint"; }
            if (type == typeof(float)) { return "float"; }
            if (type == typeof(double)) { return "double"; }
        }

        // Just use the type name, ignoring namespaces.
        return type.Name;
    }

    private static void PrintDisplayName(StringBuilder sb, Type type)
    {
        if (type.IsPrimitive || type == typeof(object) || type == typeof(string))
        {
            sb.Append(GetSimpleTypeName(type));
            return;
        }

        if (type.IsArray)
        {
            PrintDisplayName(sb, type.GetElementType()!);
            sb.Append('[');
            var rank = type.GetArrayRank();
            for (int i = 1; i < rank; i++)
            {
                sb.Append(',');
            }
            sb.Append(']');
            return;
        }

        if (type.IsPointer)
        {
            PrintDisplayName(sb, type.GetElementType()!);
            sb.Append('*');
            return;
        }

        if (type.FullName is null)
        {
            sb.Append(type.Name);
            return;
        }

        var name = type.FullName.AsSpan();

        // Remove encoded type parameters, if any.
        int index = name.IndexOf('[');
        if (index >= 0) { name = name[..index]; }

        // Get generic arguments, if any.
        var args = type.GetGenericArguments();
        var argIdx = 0;

        // Nullable<T> => T?
        if (name.SequenceEqual("System.Nullable`1"))
        {
            PrintDisplayName(sb, args[0]);
            sb.Append('?');
            return;
        }

        // Use a pretty name for ValueTuples.
        if (name.StartsWith("System.ValueTuple`"))
        {
            sb.Append('(');
            PrintValueTupleTail(sb, args);
            return;
        }

        // Remove namespaces
        index = name.LastIndexOf('.');
        if (index >= 0) { name = name.Slice(index + 1); }

        bool nested = false;
        while (!name.IsEmpty)
        {
            // Check for nested type.
            var part = name;
            index = name.IndexOf('+');
            if (index >= 0)
            {
                // Split the outer part from the rest of the name.
                part = name[..index];
                name = name.Slice(index + 1);
            }
            else
            {
                // This is the last part.
                name = "";
            }

            // Separate classes with '.'.
            if (nested) { sb.Append('.'); }
            nested = true;

            // Does this part have any type parameters?
            index = part.IndexOf('`');
            if (index >= 0)
            {
                // Print type parameters.
                sb.Append(part[..index]);
                sb.Append('<');
                var argumentCount = int.Parse(part.Slice(index + 1));
                for (int i = 0; i < argumentCount; i++)
                {
                    if (i > 0) { sb.Append(','); }
                    PrintDisplayName(sb, args[argIdx++]);
                }
                sb.Append('>');
            }
            else
            {
                // No type parameters.
                sb.Append(part);
            }
        }

        return;
    }

    private static void PrintValueTupleTail(StringBuilder sb, Type[] types)
    {
        // Figure out whether TRest is another ValueTuple.
        var len = types.Length;
        Type? rest = null;
        if (len == 8 && types[^1].FullName?.StartsWith("System.ValueTuple`") == true)
        {
            len--;
            rest = types[^1];
        }

        // Print the "normal" types.
        for (int i = 0; i < len; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }
            PrintDisplayName(sb, types[i]);
        }

        if (rest is not null)
        {
            // Print TRest.
            sb.Append(',');
            PrintValueTupleTail(sb, rest.GetGenericArguments());
        }
        else
        {
            // Done.
            sb.Append(')');
        }
    }
}
