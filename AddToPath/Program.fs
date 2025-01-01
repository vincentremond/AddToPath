module Program

open System
open AddToPath
open Pinicola.FSharp
open Pinicola.FSharp.SpectreConsole
open Pinicola.FSharp.IO

let envVarName = "PATH"

let userEnvVarRawValue =
    Environment.GetEnvironmentVariable(envVarName, EnvironmentVariableTarget.User)

let initialValues = userEnvVarRawValue |> String.split ';' |> List.map String.trim

let fixValue v =
    v
    |> String.trimEndChar '\\'
    |> (fun v ->
        match Directory.exists v with
        | true -> Some v
        | false -> None
    )

let fixedValues =
    initialValues
    |> List.choose fixValue
    |> List.distinctBy String.toLower
    |> List.sortBy (fun s ->
        let sortKey =
            match s with
            | Contains StringComparison.OrdinalIgnoreCase @"\Inkscape\" -> 2
            | _ -> 1

        (sortKey, s)
    )

if fixedValues = initialValues then
    AnsiConsole.markupLineInterpolated $"No changes needed in current [green]{initialValues.Length}[/] entries"
    Environment.Exit 0

let diff = Diff.build initialValues fixedValues

for d in diff do
    match d with
    | Diff.Deleted v -> AnsiConsole.markupLineInterpolated $"[red]{v}[/]"
    | Diff.Inserted v -> AnsiConsole.markupLineInterpolated $"[green]{v}[/]"
    | Diff.Modified v -> AnsiConsole.markupLineInterpolated $"[yellow]{v}[/]"

let newValueAsString = fixedValues |> String.concat ";"

Environment.SetEnvironmentVariable(envVarName, newValueAsString, EnvironmentVariableTarget.User)

AnsiConsole.markupLineInterpolated $"[green]Done![/] New value has {fixedValues.Length} entries. [yellow]Restart your shell to apply changes[/]"
