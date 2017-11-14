module Infrastructure.ReadHandler

open Infrastructure.Types

type Msg<'Event,'QueryParameter,'QueryResult> =
  | Events of EventSet<'Event>
  | Query of Query<'QueryParameter>*AsyncReplyChannel<QueryHandled<'QueryResult>>

type State<'State> = 'State

let private createQueryHandler (handler : MailboxProcessor<Msg<'Event,'QueryParameter,'QueryResult>>) query =
    handler.PostAndReply(fun reply -> Query <| (query,reply))

let private createProjection (handler : MailboxProcessor<Msg<'Event,'QueryParameter,'QueryResult>>) =
  Events >> handler.Post

let readHandler
      (projectionDefinition : ProjectionDefinition<'State,'Event>)
      (queryHandlerDefinition : QueryHandlerDefinition<'QueryParameter,'State,'QueryResult>)
      : Projection<'Event> * QueryHandler<'QueryParameter,'QueryResult> =
    let state = projectionDefinition.InitialState

    let mailbox =
      MailboxProcessor.Start(fun inbox ->

        let rec loop state =
          async {
            let! msg = inbox.Receive()

            match msg with
            | Events ((_,streamId),events) ->
                printfn "ReadModel received new events: %A" events
                let newReadModel =
                  (streamId,events)
                  |> projectionDefinition.UpdateState state

                printfn "New Readmodel: %A" newReadModel

                return! loop newReadModel

            | Query (query,reply) ->
                reply.Reply <| queryHandlerDefinition query state

                return! loop state
          }

        loop state
      )

    let projection =
      mailbox |> createProjection

    let queryHandler =
      mailbox |> createQueryHandler

    projection,queryHandler
