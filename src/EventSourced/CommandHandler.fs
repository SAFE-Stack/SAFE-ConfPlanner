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

  type Msg<'Command,'Event> =
    | Handle of CommandEnvelope<'Command> * AsyncReplyChannel<Result<EventEnvelope<'Event>list,string>>

  let initialize (behaviour : Behaviour<_,_>) (eventStore : EventStore<_>) : CommandHandler<_,_> =
    let appendEvents events =
      async {
        let! result = eventStore.Append events
        return result |> Result.map (fun _ -> events)
      }

    let proc (inbox : Agent<Msg<_,_>>) =
      let rec loop () =
        async {
          let! msg = inbox.Receive()

          match msg with
          | Handle (envelope,reply) ->
              let! stream = envelope.EventSource |> eventStore.GetStream

              let! result =
                match stream with
                | Ok events ->
                    events
                    |> asEvents
                    |> behaviour envelope.Command
                    |> enveloped envelope.EventSource envelope.Transaction
                    |> appendEvents

                | Error err ->
                    async { return Error err }

              do reply.Reply result

              return! loop ()
        }

      loop ()

    let agent = Agent<Msg<_,_>>.Start(proc)

    {
      Handle = fun command -> agent.PostAndAsyncReply (fun reply -> Handle (command,reply))
      OnError = agent.OnError
    }
