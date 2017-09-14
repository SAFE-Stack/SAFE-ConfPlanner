module Infrastructure.CommandHandler

open Infrastructure.Types

type Msg<'Command,'Event> =
  | Command of TransactionId*'Command
  | AddEventSubscriber of Subscriber<TransactionId*'Event>

type State<'Event> = {
  Events : 'Event list
  EventSubscriber : Subscriber<TransactionId*'Event list> list
}

let commandHandler (behaviour : Behaviour<'Command,'Event>) =

    let state : State<'Event> = {
      Events = []  // lese aus locale file, json deserialize
      EventSubscriber = []
    }

    MailboxProcessor.Start(fun inbox ->

        let rec loop state =

          async {
            let! msg = inbox.Receive()

            match msg with
            | Msg.Command (transactionId,command) ->
                printfn "CommandHandler received command: %A" command
                let newEvents = behaviour state.Events command
                let allEvents = state.Events @ newEvents

                printfn "EventStore new Events: %A" newEvents
                printfn "EventStore all Events: %A" allEvents

                state.EventSubscriber
                |> List.iter (fun sub -> (transactionId,newEvents) |> sub)

                // speichere Events in Json

                return! loop { state with Events = allEvents }

            | Msg.AddEventSubscriber subscriber ->
                return! loop { state with EventSubscriber = subscriber :: state.EventSubscriber }
          }

        loop state
    )
