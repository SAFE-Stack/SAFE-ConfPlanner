module Infrastructure.CommandHandler

open Types

type Msg<'CommandPayload,'Event> =
  | Init
  | Command of Command<'CommandPayload>
  | AddEventSubscriber of Subscriber<EventSet<'Event>>

type State<'Event> = {
  InitialEvents : EventSet<'Event> list
  Events : 'Event list
  EventSubscriber : Subscriber<EventSet<'Event>> list
}

let informSubscribers data subscribers =
  subscribers |> List.iter (fun sub -> data |> sub)

let commandHandler read (behaviour : Behaviour<'CommandPayload,'Event>) : CommandHandler<'CommandPayload> * EventPublisher<'Event> =
    let state : State<'Event> = {
      InitialEvents = []
      Events = []
      EventSubscriber = []
    }

    let mailbox =
      MailboxProcessor.Start(fun inbox ->
        let rec loop state =

          async {
            let! msg = inbox.Receive()

            match msg with
            | Init ->
                state.InitialEvents
                |> List.iter (fun data -> state.EventSubscriber |> informSubscribers data)

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

        let initialEvents =
          match read() with
          | EventResult.Ok events ->
              printfn "initial events %A" events
              events

          | EventResult.Error _ ->
              []

        loop { state with InitialEvents = initialEvents}
    )

    let commandHandler =
      Command >> mailbox.Post

    let eventPublisher =
      AddEventSubscriber >> mailbox.Post

    commandHandler,eventPublisher


