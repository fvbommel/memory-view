using System.Diagnostics;
using MemoryView;

var root = new TestClass
{
    Next = new TestClass(),
};

var graph = Inspect.CreateGraph(root);

if (args.Contains("--dot"))
{
    graph.WriteDot(Console.Out);
}
else if (args.Contains("--show"))
{
    using var process = new Process()
    {
        StartInfo = new()
        {
            FileName = "xdot",
            ArgumentList = { "-" },
            RedirectStandardInput = true,
        },
    };

    process.Start();
    graph.WriteDot(process.StandardInput);
    process.StandardInput.Close();
}
else
{
    Console.WriteLine(graph);
}

public class BaseClass
{
    private string _privateString = "password123";

    public readonly int BaseField = 123;

    public virtual string GetString()
    {
        return _privateString;
    }
}

public class TestClass : BaseClass
{
    private string _privateString = "hunter\n2";

    public Tuple<string, int?> Tuple { get; } = System.Tuple.Create("foo", (int?)5);

    public (string, int) ValueTuple { get; } = ("bar", 5);

    public TestClass? Next { get; set; }

    public override string GetString() => $"{_privateString}, {Tuple}";
}
