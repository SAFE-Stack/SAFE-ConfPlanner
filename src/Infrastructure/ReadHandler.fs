module Infrastructure.ReadHandler

open Infrastructure.Types

type Msg<'Event,'QueryParameter,'QueryResult> =
  | Events of EventSet<'Event>
  | Query of Query<'QueryParameter>*AsyncReplyChannel<QueryHandled<'QueryResult>>

type State<'State> = 'State

let private createQueryHandler (handler : MailboxProcessor<Msg<'Event,'QueryParameter,'QueryResult>>) query =
    handler.PostAndReply(fun reply -> Msg.Query <| (query,reply))

let private createProjection (handler : MailboxProcessor<Msg<'Event,'QueryParameter,'QueryResult>>) =
  Msg.Events >> handler.Post

let private readHandler (read : Readmodel<'State,'Event,'QueryParameter,'QueryResult>) : Projection<'Event> * QueryHandler<'QueryParameter,'QueryResult> =
    let state = read.Projection.InitialState

    let mailbox =
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

    let projection =
      mailbox |> createProjection

    let queryHandler =
      mailbox |> createQueryHandler

    projection,queryHandler


let initializeReadSide readmodels : Projection<'Event> list * QueryHandler<'QueryParameter,'QueryResult> list =
  readmodels
    |> List.map readHandler
    |> List.unzip
