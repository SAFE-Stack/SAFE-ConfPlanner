module Infrastructure.EventStore

open Infrastructure.Types

type Msg<'Event> =
  | Add of EventsWithCorrelation<'Event>
  | AddSubscriber of Subscriber<EventsWithCorrelation<'Event>>

let eventStore() =
  let state = {
      Events =  []  // lese aus locale file, json deserialize
      Subscriber = []
    }

  MailboxProcessor.Start(fun inbox ->
    let rec loop state =
      async {
        let! msg = inbox.Receive()

        match msg with
        | Add (correlationId,newEvents) ->
            let allEvents = state.Events @ newEvents

            printfn "EventStore new Events: %A" newEvents
            printfn "EventStore all Events: %A" allEvents

            state.Subscriber
            |> List.iter (fun sub -> (correlationId,newEvents) |> sub)

            // speichere Events in Json

            return! loop { state with Events = allEvents }

        | Msg.AddSubscriber subscriber->
            printfn "New EventStore subscriber %A" subscriber
            return! loop { state with Subscriber = subscriber :: state.Subscriber }
      }

    loop state
  )
