module Infrastructure.Types

type Subscriber<'a> = 'a -> unit

type TransactionId = TransactionId of System.Guid

type QueryId = QueryId of System.Guid

type EventSet<'Event> = 'Event list

type Behaviour<'Command,'Event> = 'Event list -> 'Command -> 'Event list

type Projection<'State,'Event> =
  {
    InitialState : 'State
    UpdateState : 'State -> 'Event -> 'State
  }

type Query<'QueryParameter> = {
  Id : QueryId
  Parameter : 'QueryParameter
}

type QueryHandled<'QueryResult> =
  | Handled of 'QueryResult
  | NotHandled

type QueryResponse<'QueryResult>  = {
  QueryId : QueryId
  Result : QueryHandled<'QueryResult>
}

type QueryHandler<'QueryParameter,'QueryResult> =
  Query<'QueryParameter> -> QueryHandled<'QueryResult>

type QueryHandlerWithState<'QueryParameter,'State,'QueryResult> =
  Query<'QueryParameter> -> 'State -> QueryHandled<'QueryResult>

type QueryResponseChannel<'QueryResult> =
  QueryResponse<'QueryResult> -> unit

type CommandHandler<'Command> = TransactionId*'Command -> unit

type EventSubscriber<'Event> = Subscriber<TransactionId*EventSet<'Event>> -> unit

type Readmodel<'State,'Event,'QueryParameter,'QueryResult> = {
  Projection : Projection<'State,'Event>
  QueryHandler : QueryHandlerWithState<'QueryParameter,'State,'QueryResult>
}


type EventSourced<'Command,'Event,'QueryParameter,'State,'QueryResult> =
  {
     CommandHandler : CommandHandler<'Command>
     QueryHandler : (Query<'QueryParameter> * QueryResponseChannel<'QueryResult>) -> unit
     EventSubscriber : EventSubscriber<'Event>
  }


type EventResult<'Event> = Result<(TransactionId * 'Event list) list, string>

