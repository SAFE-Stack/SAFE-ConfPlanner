module Helper

  open System

  let waitForAnyKey () =
    Console.ReadKey() |> ignore

  let inline printError message details =
    Console.ForegroundColor <- ConsoleColor.Red
    printfn "\n%s" message
    Console.ResetColor()
    printfn "%A" details

  let printUl list =
    list
    |> List.iteri (fun i item -> printfn " %i: %A" (i+1) item)

  let printEvents header events =
    match events with
    | Ok events ->
        events
        |> List.length
        |> printfn "\nHistory for %s (Length: %i)" header

        events |> printUl

    | Error error -> printError (sprintf "Error when retrieving events: %s" error) ""

    waitForAnyKey()

  let runAsync asnc =
    asnc |> Async.RunSynchronously

  let printQueryResults header result =
    result
    |> runAsync
    |> function
      | EventSourced.QueryResult.Handled result ->
          printfn "\n%s: %A" header result

      | EventSourced.QueryResult.NotHandled ->
          printfn "\n%s: NOT HANDLED" header

      | EventSourced.QueryResult.QueryError error ->
          printError (sprintf "Query Error: %s" error) ""

    waitForAnyKey()


  let printCommandResults header result =
    match result with
    | Ok _ ->
        printfn "\n%s: %A" header result

    | Error error ->
        printError (sprintf "Command Error: %s" error) ""

    waitForAnyKey()


  type OptionBuilder() =
    member x.Bind(v,f) = Option.bind f v
    member x.Return v = Some v
    member x.ReturnFrom o = o
    member x.Zero () = None

  let option = OptionBuilder()
