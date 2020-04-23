namespace Infrastructure.EventStorage
open Infrastructure

module FileStorage =

  open System.IO
  open Thoth.Json.Net

  let private get store =
    async {
      let! lines = store |> File.ReadAllLinesAsync |> Async.AwaitTask

      return
        lines
        |> List.ofSeq
        |> List.traverseResult Decode.Auto.fromString<EventEnvelope<'Event>>
    }

  let private getStream store source =
    async {
      let! lines = store |> File.ReadAllLinesAsync |> Async.AwaitTask

      return
        lines
        |> List.ofSeq
        |> List.traverseResult Decode.Auto.fromString<EventEnvelope<'Event>>
        |> Result.map (List.filter (fun ee -> ee.Metadata.Source = source))
    }

  let private append store events =
    async {
      use streamWriter = new StreamWriter(store, true)
      let json =
        events
        |> List.map (fun eventEnvelope -> Encode.Auto.toString(0,eventEnvelope))
        |> String.concat System.Environment.NewLine

      do! streamWriter.WriteLineAsync json |> Async.AwaitTask
      do! streamWriter.FlushAsync() |> Async.AwaitTask
    }


  let initialize store : EventStorage<_> =
    {
      Get = fun () -> get store
      GetStream = fun eventSource -> getStream store eventSource
      Append = fun events -> append store events
    }