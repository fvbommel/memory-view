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

        foreach (var record in Cache.Values.Where(o => !o.IsPrimitive).OrderBy(o => o.ID))
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
            bool isPrimitive = false;
            if (type.IsPrimitive)
            {
                label = $"{obj} : {type.Name}";
                isPrimitive = true;
            }
            else if (obj is string contents)
            {
                var escaped = contents.Replace("\"", @"\""");
                label = $"\"{escaped}\" : {type.Name}";
                // isPrimitive = true; // Consider string to be a semi-primitive type.
            }
            else
            {
                label = type.Name;
            }
            result = new Node(label)
            {
                IsPrimitive = isPrimitive,
                IsValueType = type.IsValueType,
            };

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
        // TODO: Specify BindingFlags.DeclaredOnly, manually ascend type hierarchy. (Private base class fields are currently ignored)
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var fields = type.GetFields(flags);
        foreach (var field in fields)
        {
            data.References.Add(new(field.Name, GetOrCreate(field.GetValue(source))));
        }
    }
}
