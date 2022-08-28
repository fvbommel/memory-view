using MemoryView.Core;

var root = new TestClass
{
    Next = new TestClass(),
};

var graph = Inspect.CreateGraph(root);

if (args.Contains("--dot"))
{
    graph.WriteDot(Console.Out);
}
else
{
    Console.WriteLine(graph);
}

public class BaseClass
{
    private int _privateBaseInt = -1;

    public readonly int BaseField = 123;

    public override string ToString()
    {
        return _privateBaseInt.ToString();
    }
}

public class TestClass : BaseClass
{
    private string _privateString = "hunter2";

    public Tuple<string, int?> Tuple { get; } = System.Tuple.Create("foo", (int?)5);

    public (string, int) ValueTuple { get; } = ("foo", 5);

    public TestClass? Next { get; set; }

    public override string ToString() => $"{_privateString}, {Tuple}";
}
