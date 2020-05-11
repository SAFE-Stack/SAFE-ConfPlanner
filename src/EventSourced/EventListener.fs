namespace EventSourced

module EventListener =
  open Agent

  type Msg<'Event> =
    | Notify of EventEnvelope<'Event> list
    | Subscribe of EventHandler<'Event> * AsyncReplyChannel<System.Guid>
    | Unsubscribe of System.Guid

  let notifyEventHandlers (events : EventEnvelope<_> list) (handlers : Map<System.Guid,EventHandler<_>>) =
    // printfn "Send events to %i handlers" (Map.count handlers)
    handlers
    |> Map.toList
    |> List.map (fun (_,subscription) -> events |> subscription )
    |> Async.Parallel
    |> Async.Ignore

  let initialize () : EventListener<_> =

    let proc (inbox : Agent<Msg<_>>) =
      let rec loop (eventHandlers : Map<System.Guid, EventHandler<'Event>>) =
        async {
          match! inbox.Receive() with
          | Notify eventSet ->
              do! eventHandlers |> notifyEventHandlers eventSet

              return! loop eventHandlers

          | Subscribe (listener,reply) ->
              let subscription = System.Guid.NewGuid()

              reply.Reply subscription
              return! loop (eventHandlers |> Map.add subscription listener)

          | Unsubscribe subscription ->
              return! loop (eventHandlers |> Map.remove subscription)
        }

      loop Map.empty

    let agent = Agent<Msg<_>>.Start(proc)

    {
      Subscribe = fun eventHandler -> agent.PostAndReply (fun reply -> Subscribe(eventHandler, reply))
      Unsubscribe = Unsubscribe >> agent.Post
      Notify = Notify >> agent.Post
    }
