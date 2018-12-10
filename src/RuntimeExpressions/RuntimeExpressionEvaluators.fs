namespace RuntimeExpressions

open System.Collections.Generic
open System
open System.Numerics
open System.Linq

type EvaluationResult(value : obj, evaluated : bool)=
      member this.Value = value
      member this.Evaluated = evaluated

      new(value : obj) = EvaluationResult(value, true)
      static member val NotEvaluated = new EvaluationResult(null, false) with get

type IVariableEvaluator =
    abstract member Order : int
    abstract member Evaluate: string -> EvaluationResult

type IFunctionEvaluator =
    abstract member Order : int
    abstract member Evaluate: functionName : string * args: IList<obj> -> EvaluationResult 

type DictionaryVariableProvider(values: IDictionary<string, obj>) =

    let _values = values

    interface IVariableEvaluator with
        member this.Order = System.Int32.MaxValue
        member this.Evaluate name = 
            if _values.ContainsKey(name) then new EvaluationResult(_values.[name])
            else EvaluationResult.NotEvaluated

type BuiltInFunctionEvaluator() = 
    
    let doComparison(args: obj list, strFunc : Option<string * string -> bool>, numFunc : decimal * decimal -> bool) =
        if args.Length <> 2 then failwith("Function Equal requires 2 arguments")
        let x = args.[0]
        let y = args.[1]
        match (x, y) with             
            | (:? string, _) | (_, :? string) -> 
                match strFunc with 
                    | None -> EvaluationResult.NotEvaluated
                    | Some f -> new EvaluationResult((if f(x.ToString(), y.ToString()) then 1 else 0) :> obj)
            | _, _ -> new EvaluationResult((if numFunc(Convert.ToDecimal(x), Convert.ToDecimal(y)) then 1 else 0) :> obj)
    
    let doArithmetic(args: obj list, strFunc : Option<string * string -> string>, numFunc : Int64 * Int64 -> Int64, decFunc : Decimal* Decimal-> Decimal) =
        if args.Length <> 2 then failwith("Function requires 2 arguments")
        let x = args.[0]
        let y = args.[1]
        match (x, y) with             
            | (:? string, _) | (_, :? string) -> 
                match strFunc with 
                    | None -> EvaluationResult.NotEvaluated
                    | Some f -> new EvaluationResult(f(x.ToString(), y.ToString()) :> obj)
            | _, _ ->
                match (x, y) with
                    | (:? decimal, _) | (_, :? decimal) -> new EvaluationResult(decFunc(Convert.ToDecimal(x), Convert.ToDecimal(y)) :> obj)    
                    | (:? int64, _) | (_, :? int64) -> new EvaluationResult(numFunc(Convert.ToInt64(x), Convert.ToInt64(y)) :> obj)   
                    | _, _ -> EvaluationResult.NotEvaluated
    


    member private this.Add (args: obj list) = doArithmetic(args, Some(fun (s1, s2) -> String.Concat(Convert.ToString(s1), Convert.ToString(s2))), (fun (v1, v2) -> v1 + v2), (fun (v1, v2) -> v1 + v2))        
    member private this.Subtract (args: obj list) = doArithmetic(args, Some(fun (s1, s2) -> Convert.ToString(s1).Replace(Convert.ToString(s2), "")), (fun (v1, v2) -> v1 - v2), (fun (v1, v2) -> v1 - v2))       
    member private this.Multiply (args: obj list) = doArithmetic(args, None, (fun (v1, v2) -> v1 * v2), (fun (v1, v2) -> v1 * v2))
    member private this.Divide (args: obj list) = doArithmetic(args, None, (fun (v1, v2) -> v1 / v2), (fun (v1, v2) -> v1 / v2))
    
    member private this.Equal               (args: obj list) = doComparison(args, Some(fun (s1, s2) -> s1 = s2), (fun (v1, v2) -> v1 = v2))
    member private this.NoEqual             (args: obj list) = doComparison(args, Some(fun (s1, s2) -> s1 <> s2), (fun (v1, v2) -> v1 <> v2))
    member private this.GreaterThan         (args: obj list) = doComparison(args, Some(fun (s1, s2) -> s1 > s2), (fun (v1, v2) -> v1 > v2))
    member private this.GreaterThanOrEqual  (args: obj list) = doComparison(args, Some(fun (s1, s2) -> s1 >= s2), (fun (v1, v2) -> v1 >= v2))
    member private this.LessThan            (args: obj list) = doComparison(args, Some(fun (s1, s2) -> s1 < s2), (fun (v1, v2) -> v1 < v2))
    member private this.LessThanOrEqual     (args: obj list) = doComparison(args, Some(fun (s1, s2) -> s1 <= s2), (fun (v1, v2) -> v1 <= v2))
    member private this.OpAnd               (args: obj list) = doComparison(args, None, (fun (v1, v2) -> v1 = 1M && v2 = 1M))
    member private this.OpOr                (args: obj list) = doComparison(args, None, (fun (v1, v2) -> v1 = 1M || v2 = 1M))

    member private this._funcMap = [ 
        "$add", this.Add; 
        "$subtract", this.Subtract;
        "$multiply", this.Multiply;
        "$divide", this.Divide;

        "$equal", this.Equal;
        "$notequal", this.NoEqual;
        "$greaterthan", this.GreaterThan;
        "$greaterthanorequal", this.GreaterThanOrEqual;
        "$lessthan", this.LessThan;
        "$lessthanorequal", this.LessThanOrEqual;
        "$and", this.OpAnd;
        "$or", this.OpOr;
        ]

    interface IFunctionEvaluator with
        member this.Order = System.Int32.MaxValue

        member this.Evaluate (name, args) =
            let map = this._funcMap |> Map.ofList
            if map.ContainsKey(name) then map.[name](args |> List.ofSeq)
            else EvaluationResult.NotEvaluated

