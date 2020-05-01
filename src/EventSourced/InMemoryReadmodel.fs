namespace EventSourced

module InMemoryReadmodel =
  open Agent

  type Msg<'Event,'Result> =
    private
    | Notify of EventEnvelope<'Event> list * AsyncReplyChannel<unit>
    | State of AsyncReplyChannel<'Result>

  let readModel (updateState : 'State -> EventEnvelope<'Event> list -> 'State) (initState : 'State) : InMemoryReadModel<'Event, 'State> =
    let agent =
      let eventSubscriber (inbox : Agent<Msg<_,_>>) =
        let rec loop state =
          async {
            let! msg = inbox.Receive()

            match msg with
            | Notify (eventEnvelopes, reply) ->
                reply.Reply ()
                return! loop (eventEnvelopes |> updateState state)

            | State reply ->
                reply.Reply state
                return! loop state
          }

        loop initState

      Agent<Msg<_,_>>.Start(eventSubscriber)

    {
      EventHandler = fun eventEnvelopes -> agent.PostAndAsyncReply(fun reply -> Notify (eventEnvelopes,reply))
      State = fun () -> agent.PostAndAsyncReply State
    }

