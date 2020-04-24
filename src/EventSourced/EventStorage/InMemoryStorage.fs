namespace EventSourced.EventStorage
open EventSourced

module InMemoryStorage =
  open Agent

  type Msg<'Event> =
    private
    | Get of AsyncReplyChannel<EventResult<'Event>>
    | GetStream of EventSource * AsyncReplyChannel<EventResult<'Event>>
    | Append of EventEnvelope<'Event> list * AsyncReplyChannel<unit>

  let private streamFor source history =
    history |> List.filter (fun ee -> ee.Metadata.Source = source)

  let initialize () : EventStorage<'Event> =
    let history : EventEnvelope<'Event> list = []

    let proc (inbox : Agent<Msg<_>>) =
      let rec loop history =
        async {
          let! msg = inbox.Receive()

          match msg with
          | Get reply ->
              history
              |> Ok
              |> reply.Reply

              return! loop history

          | GetStream (source,reply) ->
              history
              |> streamFor source
              |> Ok
              |> reply.Reply

              return! loop history

          | Append (events,reply) ->
              reply.Reply ()
              return! loop (history @ events)
        }

      loop history

    let agent = Agent<Msg<_>>.Start(proc)

    {
      Get = fun () ->  agent.PostAndAsyncReply Get
      GetStream = fun eventSource -> agent.PostAndAsyncReply (fun reply -> (eventSource,reply) |> GetStream)
      Append = fun events -> agent.PostAndAsyncReply (fun reply -> (events,reply) |> Append)
    }
