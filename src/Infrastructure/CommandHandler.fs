module Infrastructure.CommandHandler

open Types

type Msg<'CommandPayload,'Event> =
  | Init of EventSet<'Event> list * AsyncReplyChannel<unit>
  | Command of Command<'CommandPayload>
  | AddEventSubscriber of Subscriber<EventSet<'Event>>

type State<'Event> = {
  EventSets : EventSet<'Event> list
  EventSubscriber : Subscriber<EventSet<'Event>> list
}

let getStream streamId events =
  events
  |> List.filter (fun ((_,id), _) -> streamId = id)
  |> List.collect (fun (_,events) -> events)

let informSubscribers data subscribers =
  subscribers |> List.iter (fun sub -> data |> sub)

let commandHandler
  (readEvents : ReadEvents<'Event>)
  (appendEvents : AppendEvents<'Event>)
  (behaviour : Behaviour<'CommandPayload,'Event>)
  (projections :  Projection<'Event> list)
  : CommandHandler<'CommandPayload> * EventPublisher<'Event> =

    let state : State<'Event> = {
      EventSets = []
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
                return! loop { state with EventSets = initialEvents }

            | Command ((transactionId,streamId),payload) ->
                printfn "CommandHandler received command: %A" ((transactionId,streamId),payload)

                let stream = state.EventSets |> getStream streamId
                let newEvents = behaviour stream payload  // Todo flip und pipe
                let newEventSet = ((transactionId,streamId),newEvents)
                let allEvents = state.EventSets @ [newEventSet]

                printfn "CommandHandler new newEventSet: %A" newEventSet

                do! newEventSet |> appendEvents

                state.EventSubscriber |> informSubscribers newEventSet

                return! loop { state with EventSets = allEvents }

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


