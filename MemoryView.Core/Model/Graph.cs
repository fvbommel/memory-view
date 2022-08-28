using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace MemoryView.Core;

public class Graph
{
    internal List<Reference> Roots { get; } = new();

    private Dictionary<object, Node> Cache { get; } = new(new ReferenceEqualityComparer());

    internal ICollection<Node> Nodes => Cache.Values;

    public Graph Add(object root, [CallerArgumentExpression("root")] string? name = null)
    {
        var value = GetOrCreate(root);
        Roots.Add(new(name ?? value?.Label ?? "<unnamed>", value));
        return this;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (var record in Cache.Values.Where(o => !o.Type.IsPrimitive).OrderBy(o => o.ID))
        {
            sb.AppendLine(record.ToString());
        }

        return sb.ToString();
    }

    private Node? GetOrCreate(object? obj)
    {
        if (obj is null)
        {
            return null;
        }

        if (!Cache.TryGetValue(obj, out var result))
        {
            // Create uninitialized object.
            var type = obj.GetType();
            string label;
            if (type.IsPrimitive)
            {
                label = $"{obj} : {type.Name}";
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
                label = $"{type.GetElementType()!.Name}[{string.Join(',', dims)}]";
            }
            else
            {
                label = type.Name;
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
                    data.References.Add(new($"[{i}]", GetOrCreate(list[i])));
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
            data.References.Add(new(field.Name, GetOrCreate(field.GetValue(source))));
        }
    }
}
