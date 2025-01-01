namespace AddToPath

open DiffPlex.DiffBuilder

[<RequireQualifiedAccess>]
module Diff =

    type ChangeType =
        | Deleted of string
        | Inserted of string
        | Modified of string

    let private asText (x: string list) : string = x |> String.concat "\n"

    let build old_ new_ =
        let oldText = asText old_
        let newText = asText new_

        let diffs = InlineDiffBuilder.Diff(oldText, newText).Lines |> List.ofSeq

        diffs
        |> List.choose (fun line ->
            match line.Type with
            | DiffPlex.DiffBuilder.Model.ChangeType.Unchanged -> None
            | DiffPlex.DiffBuilder.Model.ChangeType.Deleted -> Some <| Deleted line.Text
            | DiffPlex.DiffBuilder.Model.ChangeType.Inserted -> Some <| Inserted line.Text
            | DiffPlex.DiffBuilder.Model.ChangeType.Imaginary -> failwith "Unexpected Imaginary change type"
            | DiffPlex.DiffBuilder.Model.ChangeType.Modified -> Some <| Modified line.Text
            | _ -> failwith $"Unexpected change type: {line.Type}"
        )
