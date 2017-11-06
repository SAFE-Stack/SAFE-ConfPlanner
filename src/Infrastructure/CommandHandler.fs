module Infrastructure.CommandHandler

open Types

type Msg<'CommandPayload,'Event> =
  | Init of EventSet<'Event> list * AsyncReplyChannel<unit>
  | Command of Command<'CommandPayload>
  | AddEventSubscriber of Subscriber<EventSet<'Event>>

type State<'Event> = {
  Events : 'Event list
  EventSubscriber : Subscriber<EventSet<'Event>> list
}

let informSubscribers data subscribers =
  subscribers |> List.iter (fun sub -> data |> sub)

let commandHandler
  (readEvents : ReadEvents<'Event>)
  (appendEvents : AppendEvents<'Event>)
  (behaviour : Behaviour<'CommandPayload,'Event>)
  (projections :  Projection<'Event> list)
  : CommandHandler<'CommandPayload> * EventPublisher<'Event> =

    let state : State<'Event> = {
      Events = []
      EventSubscriber = projections
    }

    let mailbox =
      MailboxProcessor.Start(fun inbox ->
        let rec loop state =

          async {
            let! msg = inbox.Receive()

            match msg with
            | Init (initialEvents,reply) ->
                initialEvents
                |> List.iter (fun data -> state.EventSubscriber |> informSubscribers data)

                reply.Reply ()
                return! loop { state with Events = initialEvents |> List.collect snd }

            | Command (transactionId,payload) ->
                printfn "CommandHandler received command: %A" (transactionId,payload)

                let newEvents = behaviour state.Events payload
                let allEvents = state.Events @ newEvents

                printfn "CommandHandler new Events: %A" newEvents

                do! (transactionId,newEvents) |> appendEvents

                state.EventSubscriber |> informSubscribers (transactionId,newEvents)

                return! loop { state with Events = allEvents }

            | AddEventSubscriber subscriber ->
                return! loop { state with EventSubscriber = subscriber :: state.EventSubscriber }
          }

        loop state
    )

    let initialEvents =
      async {
        let! eventResult = readEvents()
        let initialEvents =
          match eventResult with
          | EventResult.Ok events ->
              printfn "initial events %A" events
              events

          | EventResult.Error _ ->
              []

        return initialEvents
      } |> Async.RunSynchronously

    mailbox.PostAndReply(fun reply -> (initialEvents,reply) |> Init)

    let commandHandler =
      Command >> mailbox.Post

    let eventPublisher =
      AddEventSubscriber >> mailbox.Post

    commandHandler,eventPublisher


