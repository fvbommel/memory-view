using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace MemoryView.Core;

public class Graph
{
    internal List<Reference> Roots { get; } = new();

    private Dictionary<object, Node> Cache { get; } = new(new ReferenceEqualityComparer());

    private Dictionary<(object, Type), Node> NullableCache { get; } = new();

    internal ICollection<Node> Nodes => Cache.Values;

    public Graph Add<T>(T root, [CallerArgumentExpression("root")] string? name = null)
    {
        // This method is generic to preserve Nullable<T>, which disappears when boxed.
        var value = GetOrCreate(root, typeof(T));
        Roots.Add(new(name ?? value?.Label ?? "<unnamed>", value));
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
            else if (root.Value.Type.IsValueType)
            {
                sb.AppendLine($"{root.Name} = {root.Value.Label}");
                root.Value.PrintTo(sb, 1);
            }
            else
            {
                sb.AppendLine($"{root.Name} => #{root.Value.ID}");
            }
        }

        sb.AppendLine();

        sb.AppendLine("[Heap]");
        foreach (var record in Cache.Values.Where(o => !o.Type.IsValueType).OrderBy(o => o.ID))
        {
            sb.AppendLine(record.ToString());
        }

        return sb.ToString();
    }

    private Node? GetOrCreate(object? obj, Type type)
    {
        // Handle Nullable<T>.
        // This cannot be handled generically because of boxing weirdness:
        // obj is not of type Nullable<T>. It is either null or a boxed T.
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var element = type.GetGenericArguments()[0];
            var data = new Node(element.Name + "?", type);
            data.References.Add(new(nameof(Nullable<int>.HasValue), GetOrCreate(obj is not null, typeof(bool))));
            data.References.Add(new(nameof(Nullable<int>.Value), GetOrCreate(obj, element)));
            // No point caching this, it's a value type.
            return data;
        }

        // Any other null value: just return null.
        if (obj is null)
        {
            return null;
        }

        // Check if this object has a node allocated for it already.
        if (!Cache.TryGetValue(obj, out var result))
        {
            // From this point on we want the instance type, not the declared type.
            // (The declared type could be a base class or interface type)
            type = obj.GetType();

            // Create uninitialized object.
            string label;
            if (type.IsPrimitive)
            {
                label = $"{obj} : {type.GetDisplayName()}";
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
            }
            result = new Node(label, type);

            // Ensure future lookups (including recursive ones in AddFields) will find this object.
            Cache[obj] = result;

            if (!type.IsPrimitive)
            {
                // Fill in details.
                AddFields(result, type, obj);
            }
        }
        return result;
    }

    private void AddFields(Node data, Type type, object source)
    {
        if (type.IsArray)
        {
            var arr = (Array)source;
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
                    data.References.Add(new($"[{i}]", GetOrCreate(list[i], type.GetElementType()!)));
                }
                shownCount = N;
            }
            if (arr.LongLength > shownCount)
            {
                data.References.Add(new("...", null));
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
            data.References.Add(new(field.Name, GetOrCreate(field.GetValue(source), field.FieldType)));
        }
    }
}
