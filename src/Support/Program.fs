open Infrastructure.EventStore
open Support

let readEvents,appendEvents =
  eventStore @"..\Server\conference_eventstore.json"

[<EntryPoint>]
let main argv =
  [Conference1.eventSets(); Conference2.eventSets()]
  |> List.concat
  |> List.map (fun eventSet -> async { do! appendEvents eventSet})
  |> List.iter Async.RunSynchronously
  |> ignore

  0
