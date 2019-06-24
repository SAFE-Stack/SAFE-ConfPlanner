module Infrastructure.QueryManager

open Infrastructure.Types

type Msg<'QueryParameter,'QueryResult> =
  | Query of Query<'QueryParameter> * AsyncReplyChannel<'QueryResult>

let rec private oneOf (queryHandler : QueryHandler<'QueryParameter,'QueryResult> list) query : QueryHandled<'QueryResult> =
  match queryHandler with
  | handler :: rest ->
      match handler query with
      | NotHandled ->
          oneOf rest query

      | Handled response ->
          Handled response

  | _ -> NotHandled

let queryManager (queryHandler : QueryHandler<'QueryParameter,'QueryResult> list) =
  let mailbox =
    MailboxProcessor.Start(fun inbox ->
      let rec loop() =
        async {
          let! msg = inbox.Receive()

          match msg with
          | Query (query,reply)->
              {
                QueryResponse.QueryId = query.Id
                QueryResponse.Result = oneOf queryHandler query
              }
              |> reply.Reply

          return! loop()
        }

      loop()
    )

  let queryHandler query =
    mailbox.PostAndAsyncReply(fun reply -> Query(query,reply))

  queryHandler


