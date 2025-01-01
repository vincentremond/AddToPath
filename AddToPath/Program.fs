module Program

open System
open AddToPath
open Pinicola.FSharp
open Pinicola.FSharp.Fargo
open Pinicola.FSharp.SpectreConsole
open Pinicola.FSharp.IO

let envVarName = "PATH"

let readValues () =
    let userEnvVarRawValue =
        Environment.GetEnvironmentVariable(envVarName, EnvironmentVariableTarget.User)

    let values = userEnvVarRawValue |> String.split ';' |> List.map String.trim
    values

let displayList values =
    () |> Rule.initBlank |> AnsiConsole.write

    for v in values do
        AnsiConsole.markupLineInterpolated $" • {v}"

    () |> Rule.initBlank |> AnsiConsole.write

let displayCurrentValues () =
    let values = readValues ()
    displayList values

let addOrCheck pathToAdd =

    let initialValues = readValues ()

    let newValues =
        match pathToAdd with
        | Some p ->
            let fullPath = Path.getFullPath p
            fullPath :: initialValues
        | None -> initialValues

    let fixValue v =
        v
        |> String.trimEndChar '\\'
        |> (fun v ->
            match Directory.exists v with
            | true -> Some v
            | false -> None
        )

    let fixedValues =
        newValues
        |> List.choose fixValue
        |> List.distinctBy String.toLower
        |> List.sortBy (fun s ->
            let sortKey =
                match s with
                | Contains StringComparison.OrdinalIgnoreCase @"\Inkscape\" -> 3
                | Contains StringComparison.OrdinalIgnoreCase @"\Microsoft\WindowsApps" -> 2
                | Contains StringComparison.OrdinalIgnoreCase @"\JetBrains\Toolbox\" -> 3
                | _ -> 0

            (sortKey, s |> String.toLower)
        )

    if fixedValues = initialValues then
        AnsiConsole.markupLineInterpolated $"No changes needed in current [green]{initialValues.Length}[/] entries"
    else
        let diff = Diff.build initialValues fixedValues

        for d in diff do
            match d with
            | Diff.Deleted v -> AnsiConsole.markupLineInterpolated $"[red]{v}[/]"
            | Diff.Inserted v -> AnsiConsole.markupLineInterpolated $"[green]{v}[/]"
            | Diff.Modified v -> AnsiConsole.markupLineInterpolated $"[yellow]{v}[/]"

        let newValueAsString = fixedValues |> String.concat ";"

        Environment.SetEnvironmentVariable(envVarName, newValueAsString, EnvironmentVariableTarget.User)

        AnsiConsole.markupLineInterpolated $"[green]Done![/] New value has {fixedValues.Length} entries. [yellow]Restart your shell to apply changes[/]"

let list () =
    let values = readValues ()
    AnsiConsole.markupLineInterpolated $"Current value has [green]{values.Length}[/] entries"
    displayList values

FargoCmdLine.run
    "AddToPath"
    Cli.parser
    (fun args ->
        match args with
        | Cli.Command.Check list ->
            addOrCheck None

            if list then
                displayCurrentValues ()
        | Cli.Command.List -> list ()
        | Cli.Command.Add(path, list) ->
            addOrCheck (Some path)

            if list then
                displayCurrentValues ()
    )
