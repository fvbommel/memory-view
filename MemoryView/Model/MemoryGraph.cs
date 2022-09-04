using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace MemoryView;

public class MemoryGraph
{
    internal List<Field> Roots { get; } = new();

    private Dictionary<object, Node> NodeMap { get; } = new(new ReferenceEqualityComparer());

    internal IReadOnlyCollection<Node> Nodes => NodeMap.Values;

    public MemoryGraph Add<T>(T root, [CallerArgumentExpression("root")] string? name = null)
    {
        // This method is generic to preserve Nullable<T>, which disappears when boxed.
        var value = GetOrCreate(root, typeof(T));
        Roots.Add(new(name ?? value?.Label ?? "<unnamed>", typeof(T), value));
        return this;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine("[Roots]");
        foreach (var root in Roots)
        {
            if (root.Value is null)
            {
                sb.AppendLine($"{root.Name} = <null>");
            }
            else if (root.DeclaredType.IsPrimitive)
            {
                sb.AppendLine($"{root.Name} : {root.DeclaredType.GetDisplayName()} = {root.Value.Label}");
                root.Value.PrintFields(sb, 1);
            }
            else if (root.DeclaredType.IsValueType)
            {
                // For non-primitive value types, the label is the type.
                sb.AppendLine($"{root.Name} : {root.Value.Label}");
                root.Value.PrintFields(sb, 1);
            }
            else
            {
                sb.AppendLine($"{root.Name} : {root.DeclaredType.GetDisplayName()} => #{root.Value.ID}");
            }
        }

        sb.AppendLine();

        sb.AppendLine("[Heap]");
        foreach (var record in Nodes.Where(o => o.IsBoxed || !o.Type.IsValueType).OrderBy(o => o.ID))
        {
            record.Print(sb);
        }

        return sb.TrimRight().ToString();
    }

    private Node? GetOrCreate(object? obj, Type declaredType)
    {
        // Handle Nullable<T>.
        // This cannot be handled generically because of boxing weirdness:
        // obj is not of type Nullable<T>. It is either null or a boxed T.
        if (declaredType.IsGenericType && declaredType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var element = declaredType.GetGenericArguments()[0];
            var data = new Node(element.Name + "?", declaredType);
            data.Fields.Add(new(nameof(Nullable<int>.HasValue), typeof(bool), GetOrCreate(obj is not null, typeof(bool))));
            data.Fields.Add(new(nameof(Nullable<int>.Value), element, GetOrCreate(obj, element)));
            // No point caching this, it's a value type.
            return data;
        }

        // Any other null value: just return null.
        if (obj is null)
        {
            return null;
        }

        // Check if this object has a node allocated for it already.
        if (!NodeMap.TryGetValue(obj, out var result))
        {
            // From this point on we want the instance type, not the declared type.
            // (The declared type could be a base class or interface type)
            var type = obj.GetType();

            // Create uninitialized object.
            string label;
            if (type.IsPrimitive)
            {
                label = obj.ToString() ?? string.Empty;
            }
            else if (obj is string contents)
            {
                var escaped = contents.Replace("\\", "\\\\").Replace("\"", @"\""");
                label = $"\"{escaped}\"";
            }
            else if (type.IsArray)
            {
                var arr = (Array)obj;
                var dims = new long[arr.Rank];
                for (int i = 0; i < arr.Rank; i++)
                {
                    dims[i] = arr.GetLongLength(i);
                }
                label = $"{type.GetElementType()!.GetDisplayName()}[{string.Join(',', dims)}]";
            }
            else
            {
                label = type.GetDisplayName();

                // If it overrides ToString(), include that in the output.
                MethodInfo? toStringMethod = type.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes);
                if (toStringMethod?.DeclaringType != typeof(object))
                {
                    label = $"{label}\n{obj.ToString()}";
                }
            }
            result = new Node(label, type);

            // Ensure future lookups (including recursive ones in AddFields) will find this object.
            NodeMap[obj] = result;

            if (!type.IsPrimitive && type != typeof(string))
            {
                // Fill in details.
                AddFields(result, type, obj);
            }
        }

        if (result.Type.IsValueType && !declaredType.IsValueType)
        {
            result.IsBoxed = true;
        }

        return result;
    }

    private void AddFields(Node data, Type type, object source)
    {
        if (type.IsArray)
        {
            var arr = (Array)source;
            var elementType = type.GetElementType()!;
            long shownCount = 0;
            if (arr.Rank == 1)
            {
                var list = (IList)source;
                const int MAX_LEN = 3;
                var N = Math.Min(arr.LongLength, MAX_LEN);
                if (arr.LongLength == MAX_LEN + 1)
                {
                    // If we can get rid of the "..." by displaying the
                    // final element, do so.
                    N++;
                }
                for (int i = 0; i < N; i++)
                {
                    data.Fields.Add(new($"[{i}]", elementType, GetOrCreate(list[i], elementType)));
                }
                shownCount = N;
            }
            if (arr.LongLength > shownCount)
            {
                data.Fields.Add(new("...", elementType, null));
            }
            return;
        }

        // Add base fields first.
        if (type.BaseType is not null)
        {
            AddFields(data, type.BaseType, source);
        }

        var flags = BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic;
        var fields = type.GetFields(flags);
        foreach (var field in fields)
        {
            data.Fields.Add(new(field.Name, field.FieldType, GetOrCreate(field.GetValue(source), field.FieldType)));
        }
    }
}
