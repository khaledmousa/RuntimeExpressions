namespace RuntimeExpressions

open RuntimeExpressionTree
open Microsoft.FSharp.Text.Lexing
open System.Linq
open System
open System.Linq
open System.Collections.Generic
open System.Linq.Expressions
open System.Linq.Expressions

type AssignmentExpression = {Variable: string; Expression: RuntimeExpressionTree.Expression}

type ParseResult = 
    | AssignmentResult of AssignmentExpression
    | ExpressionResult of RuntimeExpressionTree.Expression

type EvaluationEngine (variableEvaluators:IVariableEvaluator[], functionEvaluators:IFunctionEvaluator[]) =
                
        let _varEvaluators = 
            match variableEvaluators with 
                | null -> [] 
                | x -> x |> List.ofArray

        let _funcEvaluators = 
            match functionEvaluators with 
                | null -> [new BuiltInFunctionEvaluator() :> IFunctionEvaluator] 
                | fs -> (new BuiltInFunctionEvaluator() :> IFunctionEvaluator)::(fs |> List.ofArray)                

        let rec EvalVariable(name : string, evaluators : IVariableEvaluator list) =
            match evaluators with
                | [] -> EvaluationResult.NotEvaluated
                | h::t ->
                    let potentialResult = h.Evaluate(name)
                    if potentialResult.Evaluated then potentialResult
                    else EvalVariable(name, t)
        
        let rec EvalFunction(name : string, args : IList<obj>, evaluators : IFunctionEvaluator list) =
            match evaluators with
                | [] -> EvaluationResult.NotEvaluated
                | h::t ->
                    let potentialResult = h.Evaluate(name, args)
                    if potentialResult.Evaluated then potentialResult
                    else EvalFunction(name, args, t)
    
        let ValueAsExpression(value : Object) =
            if value = null then 
                Long(0L) //need to think about this
            else
                match value.GetType() with
                    | t when t = typeof<Int32> -> Long(Convert.ToInt64(value))
                    | t when t = typeof<Int64> -> Long(Convert.ToInt64(value))
                    | t when t = typeof<Decimal> -> Num(Convert.ToDecimal(value))
                    | t when t = typeof<Double> -> Num(Convert.ToDecimal(value))
                    | t when t = typeof<String> -> Literal(value.ToString())
                    | t -> failwith(String.Concat("Type ", t.Name, " not supported"))

        let Parse x =
            let lexbuf = LexBuffer<_>.FromString x
            let y = REParser.start RELexer.tokenize lexbuf    
            y 

        let rec EvaluateInternal(expressionTree : RuntimeExpressionTree.Expression) =            
            match expressionTree with
                | Id d -> 
                    match EvalVariable(d, _varEvaluators |> List.sortBy(fun e -> e.Order)) with
                        | e when e.Evaluated = true -> ValueAsExpression(e.Value)
                        | _ -> failwith(String.Concat("Cannot evaluate ", d))
                | Long i -> Long i
                | Num i -> Num i
                | Literal i -> Literal i
                | Invoc(func, args) -> 
                    match EvalFunction(func, (args |> List.map EvaluateInternal |> List.map (fun e -> e.UnderlyingValue)).ToList(), _funcEvaluators |> List.sortBy(fun e -> e.Order)) with
                        | e when e.Evaluated = true -> ValueAsExpression(e.Value)
                        | _ -> failwith(String.Concat("Cannot evaluate ", func))                    
                | BinaryExpression(e1, op, e2) -> 
                    let v1 = EvaluateInternal(e1)
                    let v2 = EvaluateInternal(e2)
                    match op with
                        | Plus      -> EvaluateInternal(Invoc("$add", [e1; e2]))
                        | Minus     -> EvaluateInternal(Invoc("$subtract", [e1; e2]))
                        | Mult      -> EvaluateInternal(Invoc("$multiply", [e1; e2]))
                        | Div       -> EvaluateInternal(Invoc("$divide", [e1; e2]))
                        | Eq        -> EvaluateInternal(Invoc("$equal", [e1; e2]))
                        | Ne        -> EvaluateInternal(Invoc("$notequal", [e1; e2]))
                        | Gt        -> EvaluateInternal(Invoc("$greaterthan", [e1; e2]))
                        | Gte       -> EvaluateInternal(Invoc("$greaterthanorequal", [e1; e2]))
                        | Lt        -> EvaluateInternal(Invoc("$lessthan", [e1; e2]))
                        | Lte       -> EvaluateInternal(Invoc("$lessthanorequal", [e1; e2]))
                        | OpAnd     -> EvaluateInternal(Invoc("$and", [e1; e2]))
                        | OpOr      -> EvaluateInternal(Invoc("$or", [e1; e2]))
                        | _         -> failwith(String.Concat("Operator ", op.ToString() ,"not supported"))               
        
        new() = EvaluationEngine(null, null)

        member this.ParseString(expression) =
            let tree = Parse expression
            match tree with 
                | Assignment(v, e) -> AssignmentResult({Variable = v; Expression = e})
                | Expression(e) -> ExpressionResult(e)

        member this.Evaluate(expression) = 
            let expressionTree = Parse expression
            match expressionTree with             
                | RuntimeExpressionTree.Expression e -> (EvaluateInternal e).UnderlyingValue
                | _ -> failwith("Cannot evaluate assignment")
             

        member this.Evaluate<'T>(expression) = 
            let expressionTree = Parse expression
            match expressionTree with             
                | RuntimeExpressionTree.Expression e -> 
                    let result = (EvaluateInternal e).UnderlyingValue
                    Convert.ChangeType(result, typedefof<'T>)
                | _ -> failwith("Cannot evaluate assignment")

        member this.EvaluateExpression<'T>(expression) =             
            let result = (EvaluateInternal expression).UnderlyingValue
            Convert.ChangeType(result, typedefof<'T>)
            
