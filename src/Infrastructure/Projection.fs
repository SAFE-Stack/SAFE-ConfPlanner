module Infrastructure.ProjectionHandler

open Infrastructure.Types

type Msg<'Event> =
  | Events of TransactionId*'Event list

type State<'State> = {
  ReadModel : 'State
}

let projectionHandler (projection : Projection<'State,'Event>) =
    let state =
      {
        ReadModel = projection.InitialState
      }

    MailboxProcessor.Start(fun inbox ->

      let rec loop state =
        async {
          let! msg = inbox.Receive()

          match msg with
          | Msg.Events (_,events) ->
              printfn "ProjectionHandler received new events: %A" events
              let newReadModel =
                events
                |> List.fold projection.UpdateState state.ReadModel

              printfn "New Readmodel: %A" newReadModel

              return! loop { state with ReadModel = newReadModel }
        }

      loop state
    )
