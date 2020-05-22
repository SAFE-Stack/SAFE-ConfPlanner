namespace EventSourced

module EventStore =
  open Agent

  type Msg<'Event> =
    | Get of AsyncReplyChannel<EventResult<'Event>>
    | GetStream of EventSource * AsyncReplyChannel<EventResult<'Event>>
    | Append of EventEnvelope<'Event> list * AsyncReplyChannel<Result<unit,string>>

  let initialize (storage : EventStorage<_>) : EventStore<_> =
    let eventsAppended = Event<EventEnvelope<'Event> list>()

    let proc (inbox : Agent<Msg<_>>) =
      let rec loop () =
        async {
          match! inbox.Receive() with
          | Get reply ->
              try
                let! events = storage.Get()
                do events |> reply.Reply
              with exn ->
                do inbox.Trigger(exn)
                do exn.Message |> Error |> reply.Reply

              return! loop ()


          | GetStream (source,reply) ->
              try
                let! stream = source |> storage.GetStream
                do stream |> reply.Reply
              with exn ->
                do inbox.Trigger(exn)
                do exn.Message |> Error |> reply.Reply

              return! loop ()

          | Append (events,reply) ->
              try
                do! events |> storage.Append
                do eventsAppended.Trigger events
                do reply.Reply (Ok ())
              with exn ->
                do inbox.Trigger(exn)
                do exn.Message |> Error |> reply.Reply

              return! loop ()
        }

      loop ()

    let agent =  Agent<Msg<_>>.Start(proc)

    {
      Get = fun () -> agent.PostAndAsyncReply Get
      GetStream = fun eventSource -> agent.PostAndAsyncReply (fun reply -> GetStream (eventSource,reply))
      Append = fun events -> agent.PostAndAsyncReply (fun reply -> Append (events,reply))
      OnError = agent.OnError
      OnEvents = eventsAppended.Publish
    }
