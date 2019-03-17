module RuntimeExpressionTree

type operator = Plus | Minus | Mult | Div | Eq | Ne | Lt | Lte | Gt | Gte | OpAnd | OpOr

type Expression = 
    | Long of int64
    | Num of decimal 
    | Id of string
    | Literal of string
    | BinaryExpression of Expression * operator * Expression
    | Invoc of string * Expression list    
        with
        member this.UnderlyingValue =
            match this with
                | Long v -> box(v)
                | Num v -> box(v)
                | Id v -> box(v)
                | Literal v -> box(v)
                | _ -> failwith("UnderlyingValue is complex")

type Assignment = string * Expression

type expr =
        | Expression of Expression
        | Assignment of Assignment