module Infrastructure.ReadmodelHandler

open Infrastructure.Types

type Msg<'Event,'QueryParameter,'QueryResult> =
  | Events of TransactionId*'Event list
  | Query of Query<'QueryParameter>*AsyncReplyChannel<QueryHandled<'QueryResult>>

type State<'State> = 'State

let readModelHandler (read : Readmodel<'State,'Event,'QueryParameter,'QueryResult>) : MailboxProcessor<Msg<'Event,'QueryParameter,'QueryResult>> =
    let state = read.Projection.InitialState

    MailboxProcessor.Start(fun inbox ->

      let rec loop state =
        async {
          let! msg = inbox.Receive()

          match msg with
          | Msg.Events (_,events) ->
              printfn "ReadModel received new events: %A" events
              let newReadModel =
                events
                |> List.fold read.Projection.UpdateState state

              printfn "New Readmodel: %A" newReadModel

              return! loop newReadModel

          | Msg.Query (query,reply) ->
              reply.Reply <| read.QueryHandler query state

              return! loop state
        }

      loop state
    )
