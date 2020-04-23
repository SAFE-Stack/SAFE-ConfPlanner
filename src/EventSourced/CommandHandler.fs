namespace Infrastructure

module CommandHandler =
  open Agent

  let private asEvents eventEnvelopes =
    eventEnvelopes |> List.map (fun envelope -> envelope.Event)

  let private enveloped source events =
    let now = System.DateTime.UtcNow
    let envelope event =
      {
          Metadata = {
            Source = source
            RecordedAtUtc = now
          }
          Event = event
      }

    events |> List.map envelope

  type Msg<'Command> =
    | Handle of EventSource * 'Command * AsyncReplyChannel<Result<unit,string>>

  let initialize (behaviour : Behaviour<_,_>) (eventStore : EventStore<_>) : CommandHandler<_> =
    let proc (inbox : Agent<Msg<_>>) =
      let rec loop () =
        async {
          let! msg = inbox.Receive()

          match msg with
          | Handle (eventSource,command,reply) ->
              let! stream = eventSource |> eventStore.GetStream

              let newEvents =
                stream |> Result.map (asEvents >> behaviour command >> enveloped eventSource)

              let! result =
                newEvents
                |> function
                    | Ok events -> eventStore.Append events
                    | Error err -> async { return Error err }

              do reply.Reply result

              return! loop ()
        }

      loop ()

    let agent = Agent<Msg<_>>.Start(proc)

    {
      Handle = fun source command -> agent.PostAndAsyncReply (fun reply -> Handle (source,command,reply))
      OnError = agent.OnError
    }