module Infrastructure.CommandHandler

open Infrastructure.Types
open EventStore

type Msg<'Command,'Event> =
  | Init
  | Command of TransactionId*'Command
  | AddEventSubscriber of Subscriber<TransactionId*'Event>

type State<'Event> = {
  Events : 'Event list
  EventSubscriber : Subscriber<TransactionId*'Event list> list
}

let informSubscribers data subscribers =
  subscribers |> List.iter (fun sub -> data |> sub)

let commandHandler (eventStore : MailboxProcessor<EventStore.Msg<'Event>>) (behaviour : Behaviour<'Command,'Event>) =

    let state : State<'Event> = {
      Events = []
      EventSubscriber = []
    }

    MailboxProcessor.Start(fun inbox ->

        let rec loop state =

          async {
            let! msg = inbox.Receive()

            match msg with
            | Msg.Init ->
                printfn "hier"
                let initialEvents = eventStore.PostAndReply(EventStore.Msg.GetAllEvents)
                printfn "initial events %A" initialEvents
                initialEvents
                |> List.iter (fun data -> state.EventSubscriber |>  informSubscribers data)

                let events =
                  initialEvents
                  |> List.collect snd

                return! loop { state with Events = events  }

            | Msg.Command (transactionId,command) ->
                printfn "CommandHandler received command: %A" command
                let newEvents = behaviour state.Events command
                let allEvents = state.Events @ newEvents

                printfn "EventStore new Events: %A" newEvents
                printfn "EventStore all Events: %A" allEvents

                state.EventSubscriber |> informSubscribers (transactionId,newEvents)

                return! loop { state with Events = allEvents }

            | Msg.AddEventSubscriber subscriber ->
                return! loop { state with EventSubscriber = subscriber :: state.EventSubscriber }
          }

        loop state
    )
