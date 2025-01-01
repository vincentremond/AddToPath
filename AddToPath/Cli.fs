namespace AddToPath

open Fargo
open Fargo.Operators

[<RequireQualifiedAccess>]
module Cli =

    [<RequireQualifiedAccess>]
    type private Command' =
        | Add
        | List
        | Check

    type Command =
        | Add of string
        | List
        | Check

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
            | Command'.Check -> return Command.Check
            | Command'.Add
            | _ ->
                let! path = arg "path" "The path to add to the PATH user environment variable" |> reqArg
                return Command.Add path
        }
