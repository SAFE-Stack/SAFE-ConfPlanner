module Infrastructure.Types

type Subscriber<'a> =
  'a -> unit

type TransactionId =
  TransactionId of System.Guid

type QueryId =
  QueryId of System.Guid

type Behaviour<'CommandPayload,'Event> =
  'Event list -> 'CommandPayload -> 'Event list

type Projection<'State,'Event> =
  {
    InitialState : 'State
    UpdateState : 'State -> 'Event -> 'State
  }

type Query<'QueryParameter> =
  {
    Id : QueryId
    Parameter : 'QueryParameter
  }

type QueryHandled<'QueryResult> =
  | Handled of 'QueryResult
  | NotHandled

type QueryResponse<'QueryResult> =
  {
    QueryId : QueryId
    Result : QueryHandled<'QueryResult>
  }

type QueryHandler<'QueryParameter,'QueryResult> =
  Query<'QueryParameter> -> QueryHandled<'QueryResult>

type QueryHandlerWithState<'QueryParameter,'State,'QueryResult> =
  Query<'QueryParameter> -> 'State -> QueryHandled<'QueryResult>

type QueryResponseChannel<'QueryResult> =
  QueryResponse<'QueryResult> -> unit

type EventSet<'Event> =
  TransactionId * 'Event list

type EventResult<'Event> =
  Result<EventSet<'Event> list, string>

type Projection<'Event> =
  EventSet<'Event> -> unit

type Readmodel<'State,'Event,'QueryParameter,'QueryResult> =
  {
    Projection : Projection<'State,'Event>
    QueryHandler : QueryHandlerWithState<'QueryParameter,'State,'QueryResult>
  }

type Command<'CommandPayload> =
  TransactionId * 'CommandPayload

type CommandHandler<'CommandPayload> =
  Command<'CommandPayload> -> unit

type EventPublisher<'Event> =
  Subscriber<EventSet<'Event>> -> unit

type QueryManager<'QueryParameter,'QueryResult> =
  Query<'QueryParameter> * QueryResponseChannel<'QueryResult> -> unit

type EventSourced<'CommandPayload,'Event,'QueryParameter,'State,'QueryResult> =
  {
     CommandHandler : CommandHandler<'CommandPayload>
     QueryManager : QueryManager<'QueryParameter,'QueryResult>
     EventPublisher : EventPublisher<'Event>
  }

