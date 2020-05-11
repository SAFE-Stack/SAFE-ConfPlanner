namespace EventSourced

module CommandHandler =
  open Agent

  let private asEvents eventEnvelopes =
    eventEnvelopes |> List.map (fun envelope -> envelope.Event)

  let private enveloped source transaction events =
    let now = System.DateTime.UtcNow
    let envelope event =
      {
          Metadata = {
            Source = source
            RecordedAtUtc = now
            Transaction = transaction
          }
          Event = event
      }

    events |> List.map envelope

  type Msg<'Command> =
    | Handle of CommandEnvelope<'Command> * AsyncReplyChannel<Result<unit,string>>

  let initialize (behaviour : Behaviour<_,_>) (eventStore : EventStore<_>) : CommandHandler<_> =
    let proc (inbox : Agent<Msg<_>>) =
      let rec loop () =
        async {
          let! msg = inbox.Receive()

          match msg with
          | Handle (envelope,reply) ->
              let! stream = envelope.EventSource |> eventStore.GetStream

              let newEvents =
                stream |> Result.map (asEvents >> behaviour envelope.Command >> enveloped envelope.EventSource envelope.Transaction)

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
      Handle = fun command -> agent.PostAndAsyncReply (fun reply -> Handle (command,reply))
      OnError = agent.OnError
    }
