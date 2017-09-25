module Infrastructure.ReadmodelHandler

open Infrastructure.Types

type Msg<'Event,'QueryParameter,'QueryResult> =
  | Events of TransactionId*'Event list
  | Query of Query<'QueryParameter>*AsyncReplyChannel<QueryHandled<'QueryResult>>

type State<'State> = 'State

let private createQueryHandler (handler : MailboxProcessor<Msg<'Event,'QueryParameter,'QueryResult>>) query =
    handler.PostAndReply(fun reply -> Msg.Query <| (query,reply))

let private createReadmodel (handler : MailboxProcessor<Msg<'Event,'QueryParameter,'QueryResult>>) =
  Msg.Events >> handler.Post

let private readModelHandler (read : Readmodel<'State,'Event,'QueryParameter,'QueryResult>) : Projection<'Event> * QueryHandler<'QueryParameter,'QueryResult> =
    let state = read.Projection.InitialState

    let readProcessor =
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

    let queryHandler =
      createQueryHandler readProcessor

    let projection =
      createReadmodel readProcessor

    projection,queryHandler


let initializeReadSide readmodels : Projection<'Event> list * (Query<'QueryParameter> -> QueryHandled<'QueryResult>) list =
  readmodels
    |> List.map readModelHandler
    |> List.unzip

