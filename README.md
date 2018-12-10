# RuntimeExpressions
Simple runtime expression evaluation for .NET

A simple expression evaluator.

Usage examples:
```cs
var engine = new RuntimeExpressions.EvaluationEngine();
Console.WriteLine(engine.Evaluate<int>("10 + 2"); //12
Console.WriteLine(engine.Evaluate<decimal>("1.1 + (2 * 4) - 0.1"); //9.0
Console.WriteLine(engine.Evaluate<string>("\"Hello, \" + \"world!\""); //Hello, world!
Console.WriteLine(engine.Evaluate<string>("\"ABAABCD\" - \"AB\""); //ACD
```

Can be extended with user-defined functions by implementing `IFunctionEvaluator` and passing instance(s) of the implementation(s) to the contructor of `EvaluationEngine`.

Also supports variables that will be evaluated using implemenataions of `IVariableEvaluator`. A Basic dictionary-based implementation is provided in the class `DictionaryVariableProvider`. 
Example usage:
```cs
var varProvider = new DictionaryVariableProvider(new Dictionary<string, object>
{
    ["V1"] = 2,
    ["V2"] = 2.5
});

_engine = new EvaluationEngine(new[] { varProvider }, null);           
Console.WriteLine(_engine.Evaluate<decimal>("V1 * V2 + 0.5")); //5.5
```
