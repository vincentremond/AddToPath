namespace AddToPath

open Fargo
open Fargo.Operators

[<RequireQualifiedAccess>]
module Cli =

    [<RequireQualifiedAccess>]
    type Command' =
        | Add
        | List
        | Check

    type Command =
        | Add of Path: string * List: bool
        | List
        | Check of List: bool

    let parser =

        fargo {
            match!
                (cmd "add" null "Add a new path to the PATH user environment variable"
                 |>> Command'.Add)
                <|> (cmd "list" null "List the current PATH user environment variable"
                     |>> Command'.List)
                <|> (cmd "check" null "Check the current PATH user environment variable"
                     |>> Command'.Check)
                <|> (ret Command'.Add)
            with
            | Command'.List -> return Command.List
            | Command'.Check ->
                let! list = flag "list" "l" "List the current PATH user environment variable"
                return Command.Check list
            | Command'.Add ->
                let! path = arg "path" "The path to add to the PATH user environment variable" |> reqArg
                and! list = flag "list" "l" "List the current PATH user environment variable"
                return Command.Add(path, list)
        }
