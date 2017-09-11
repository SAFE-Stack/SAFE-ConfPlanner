module Infrastructure.Projection

open Infrastructure.Types
let projection (initialState : 'State) (updateState : UpdateState<'State,'Event>) =
  let state = {
      ReadModel = initialState
      Subscriber = []
    }

  MailboxProcessor.Start(fun inbox ->
    let rec loop state =
      async {
        let! msg = inbox.Receive()

        match msg with
        | ProjectionMsg.Events events ->
            printfn "Projection new events received: %A" events
            let newReadModel =
              events
              |> List.fold updateState state.ReadModel

            state.Subscriber
            |> List.iter (fun sub -> sub newReadModel)

            return! loop { state with ReadModel = newReadModel }

        | ProjectionMsg.AddSubscriber subscriber ->
            printfn "New State subscriber %A" subscriber
            return! loop { state with Subscriber = subscriber :: state.Subscriber }
      }

    loop state
  )
