module Support.Run

open System
open Domain.Model
open EventSourced
open EventSourced.EventSourced
open Support

let private enveloped (ConferenceId source) events =
  let metadata =
    {
      Source = source
      RecordedAtUtc = DateTime.Now
      Transaction = TransactionId <| Guid.NewGuid()
    }

  events
  |> List.map (fun e -> { Metadata = metadata ; Event = e })


let run (eventSourced : EventSourced<_,_,_>) =
  async {
    do! (Conference1.events |> enveloped Conference1.conferenceId |> eventSourced.Append |> Async.Ignore)
    do! (Conference2.events |> enveloped Conference2.conferenceId |> eventSourced.Append |> Async.Ignore)
  } |> Async.RunSynchronously

