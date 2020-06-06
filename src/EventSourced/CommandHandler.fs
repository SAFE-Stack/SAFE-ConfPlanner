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

  let private singleEventSourceInBatch commandEnvelopes =
    match commandEnvelopes |> List.map (fun envelope -> envelope.EventSource) |> List.distinct with
    | [eventSource] -> Ok eventSource
    | [] -> Error "Command batches need at least one command"
    | _ -> Error "Command batches are only allowed for a single event source"


  type Msg<'Command,'Event> =
    | Handle of CommandEnvelope<'Command> * AsyncReplyChannel<Result<EventEnvelope<'Event>list,string>>
    | HandleBatch of CommandEnvelope<'Command> list * AsyncReplyChannel<Result<EventEnvelope<'Event>list,string>>

  let initialize (behaviour : Behaviour<_,_>) eventIsError (eventStore : EventStore<_>) : CommandHandler<_,_> =
    let appendEvents events =
      async {
        let! result = eventStore.Append events
        return result |> Result.map (fun _ -> events)
      }

    let applyBehaviour commandEnvelope history  =
      history
      |> asEvents
      |> behaviour commandEnvelope.Command
      |> enveloped commandEnvelope.EventSource commandEnvelope.Transaction

    let proc (inbox : Agent<Msg<_,_>>) =
      let rec loop () =
        async {
          let! msg = inbox.Receive()

          match msg with
          | Handle (commandEnvelope,reply) ->
              let! stream = commandEnvelope.EventSource |> eventStore.GetStream

              let! result =
                match stream with
                | Ok history ->
                    history
                    |> applyBehaviour commandEnvelope
                    |> appendEvents

                | Error err ->
                    async { return Error err }

              do reply.Reply result

              return! loop ()

          | HandleBatch(commandEnvelopes, reply) ->
              let! result =
                match commandEnvelopes |> singleEventSourceInBatch with
                | Ok eventSource ->
                    async {
                      match! eventStore.GetStream eventSource with
                      | Ok history ->
                          let eventEnvelopes =
                            commandEnvelopes
                            |> List.fold (fun events commandEnvelope ->
                                let new_events = (history @ events) |> applyBehaviour commandEnvelope
                                events @ new_events) []

                          return!
                            match eventEnvelopes |> List.filter (fun ee -> ee.Event |> eventIsError) with
                            | [] -> eventEnvelopes |> appendEvents
                            | errors -> errors |> appendEvents

                      | Error err -> return Error err
                    }

                | Error err -> async { return Error err }

              do reply.Reply result

              return! loop ()
        }

      loop ()

    let agent = Agent<Msg<_,_>>.Start(proc)

    {
      Handle = fun command -> agent.PostAndAsyncReply (fun reply -> Handle (command,reply))
      HandleBatch = fun commands -> agent.PostAndAsyncReply (fun reply -> HandleBatch (commands,reply))
      OnError = agent.OnError
    }
