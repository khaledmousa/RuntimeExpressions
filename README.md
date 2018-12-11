# RuntimeExpressions
Simple .net standard runtime expression evaluation

A simple, extensible expression evaluator. The evaluator accepts string expressions and evaluates the expression value. the `EvaluationEngine` is the main class that does the evaluation, and it optionally accepts implementations of `IFunctionEvaluator` and `IVariableEvaluator` that act as extension points to allow evluating custom defined functions and variables.
Below are some examples of basic use cases, all shown in C#

Usage examples:
```cs
var engine = new RuntimeExpressions.EvaluationEngine();
Console.WriteLine(engine.Evaluate<int>("10 + 2"); //12
Console.WriteLine(engine.Evaluate<decimal>("1.1 + (2 * 4) - 0.1"); //9.0
Console.WriteLine(engine.Evaluate<string>("\"Hello, \" + \"world!\""); //Hello, world!
Console.WriteLine(engine.Evaluate<string>("\"ABAABCD\" - \"AB\""); //ACD
```

Can be extended with user-defined functions by implementing `IFunctionEvaluator` and passing instance(s) of the implementation(s) to the contructor of `EvaluationEngine`.
Usage example:
```cs
public class ExtraFuncProvider : IFunctionEvaluator
{
    public int Order => 1;

    public EvaluationResult Evaluate(string functionName, IList<object> args)
    {
        if (args != null && args.Count == 1 && decimal.TryParse(args[0].ToString(), out var x))
        {
            switch (functionName.ToLower())
            {
                case "reverse":
                    return new EvaluationResult(decimal.Parse(new string(x.ToString().Reverse().ToArray())));
                case "negate":
                    return new EvaluationResult(-1 * x);
                default:
                    return EvaluationResult.NotEvaluated;
            }
        }

        return EvaluationResult.NotEvaluated;
    }
}

/*..*/
_engine = new EvaluationEngine(null, new[] { new ExtraFuncProvider() });
Console.WriteLine(_engine.Evaluate<double>("Reverse(194)")); //491
Console.WriteLine(_engine.Evaluate<double>("Negate(11)")); //-11
```


Also supports variables that will be evaluated using implemenataions of `IVariableEvaluator`. A Basic dictionary-based implementation is provided in the class `DictionaryVariableProvider`. 
Usage example:
```cs
var varProvider = new DictionaryVariableProvider(new Dictionary<string, object>
{
    ["V1"] = 2,
    ["V2"] = 2.5
});

_engine = new EvaluationEngine(new[] { varProvider }, null);           
Console.WriteLine(_engine.Evaluate<decimal>("V1 * V2 + 0.5")); //5.5
```
