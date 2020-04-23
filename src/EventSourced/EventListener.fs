namespace Infrastructure

module EventListener =
  open Agent

  type Msg<'Event> =
    | Notify of EventEnvelope<'Event> list
    | Subscribe of EventHandler<'Event>


  let notifyEventHandlers events (handlers : EventHandler<_> list) =
    handlers
    |> List.map (fun subscription -> events |> subscription )
    |> Async.Parallel
    |> Async.Ignore

  let initialize () : EventListener<_> =

    let proc (inbox : Agent<Msg<_>>) =
      let rec loop (eventHandlers : EventHandler<'Event> list) =
        async {
          match! inbox.Receive() with
          | Notify events ->
              do! eventHandlers |> notifyEventHandlers events

              return! loop eventHandlers

          | Subscribe listener ->
              return! loop (listener :: eventHandlers)
        }

      loop []

    let agent = Agent<Msg<_>>.Start(proc)

    {
      Notify = Notify >> agent.Post
      Subscribe = Subscribe >> agent.Post
    }