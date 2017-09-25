module Infrastructure.CommandHandler

open Types
open EventStore

type Msg<'Command,'Event> =
  | Init of EventResult<'Event>
  | Command of TransactionId*'Command
  | AddEventSubscriber of Subscriber<TransactionId*'Event list>

type State<'Event> = {
  Events : 'Event list
  EventSubscriber : Subscriber<TransactionId*'Event list> list
}

let informSubscribers data subscribers =
  subscribers |> List.iter (fun sub -> data |> sub)


//  (EventResult<'Event> -> unit) * CommandHandler<'Command> *  EventSubscriber<'Event>

let commandHandler (behaviour : Behaviour<'Command,'Event>) : (EventResult<'Event> -> unit) * CommandHandler<'Command> * EventSubscriber<'Event> =

    let state : State<'Event> = {
      Events = []
      EventSubscriber = []
    }

    let handler =
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

            | Command (transactionId,command) ->
                printfn "CommandHandler received command: %A" command
                let newEvents = behaviour state.Events command
                let allEvents = state.Events @ newEvents

                printfn "EventStore new Events: %A" newEvents
                printfn "EventStore all Events: %A" allEvents

                state.EventSubscriber |> informSubscribers (transactionId,newEvents)

                return! loop { state with Events = allEvents }

            | AddEventSubscriber subscriber ->
                return! loop { state with EventSubscriber = subscriber :: state.EventSubscriber }
          }

        loop state
    )

    let subscribeToEvents =
      AddEventSubscriber >> handler.Post

    let init =
      Init >> handler.Post

    let commandHandler =
      Command >> handler.Post

    init,commandHandler,subscribeToEvents


