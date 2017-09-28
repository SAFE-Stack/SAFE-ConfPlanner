module Infrastructure.CommandHandler

open Types

type Msg<'CommandPayload,'Event> =
  | Init of EventResult<'Event>
  | Command of Command<'CommandPayload>
  | AddEventSubscriber of Subscriber<EventSet<'Event>>

type State<'Event> = {
  Events : 'Event list
  EventSubscriber : Subscriber<EventSet<'Event>> list
}

let informSubscribers data subscribers =
  subscribers |> List.iter (fun sub -> data |> sub)

let commandHandler (behaviour : Behaviour<'CommandPayload,'Event>) : (EventResult<'Event> -> unit) * CommandHandler<'CommandPayload> * EventPublisher<'Event> =

    let state : State<'Event> = {
      Events = []
      EventSubscriber = []
    }

    let mailbox =
      MailboxProcessor.Start(fun inbox ->
        let rec loop state =

          async {
            let! msg = inbox.Receive()

            match msg with
            | Init eventResult ->
                match eventResult with
                | EventResult.Ok events ->
                    printfn "initial events %A" events

                    events
                    |> List.iter (fun data -> state.EventSubscriber |>  informSubscribers data)

                    return! loop { state with Events = events |> List.collect snd  }

                | EventResult.Error _ ->
                    return! loop state

            | Command (transactionId,payload) ->
                printfn "CommandHandler received command: %A" (transactionId,payload)
                let newEvents = behaviour state.Events payload
                let allEvents = state.Events @ newEvents

                printfn "CommandHandler new Events: %A" newEvents

                state.EventSubscriber |> informSubscribers (transactionId,newEvents)

                return! loop { state with Events = allEvents }

            | AddEventSubscriber subscriber ->
                return! loop { state with EventSubscriber = subscriber :: state.EventSubscriber }
          }

        loop state
    )

    let init =
      Init >> mailbox.Post

    let commandHandler =
      Command >> mailbox.Post

    let eventPublisher =
      AddEventSubscriber >> mailbox.Post

    init,commandHandler,eventPublisher


