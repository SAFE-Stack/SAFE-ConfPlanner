module Infrastructure.ProjectionHandler

open Infrastructure.Types

type Msg<'Event,'State> =
  | Events of EventsWithCorrelation<'Event>
  | AddSubscriber of Subscriber<'State>

type State<'State> = {
  ReadModel : 'State
  Subscriber : Subscriber<'State> list
}

let projectionHandler
  (eventSubscriber : EventSubscriber<'Event>)
  (projection : Projection<'State,'Command,'Event>) =

    let state = {
        ReadModel = projection.InitialState
        Subscriber = []
      }

    MailboxProcessor.Start(fun inbox ->
      eventSubscriber (Msg.Events >> inbox.Post)

      let rec loop state =
        async {
          let! msg = inbox.Receive()

          match msg with
          | Msg.Events (_,events) ->
              printfn "Projection new events received: %A" events
              let newReadModel =
                events
                |> List.fold projection.UpdateState state.ReadModel

              state.Subscriber
              |> List.iter (fun sub -> sub newReadModel)

              return! loop { state with ReadModel = newReadModel }

          | Msg.AddSubscriber subscriber ->
              printfn "New State subscriber %A" subscriber
              return! loop { state with Subscriber = subscriber :: state.Subscriber }
        }

      loop state
    )
