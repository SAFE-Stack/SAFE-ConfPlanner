module Infrastructure.Types

type Subscriber<'a> =
  'a -> unit

type TransactionId =
  TransactionId of System.Guid

type QueryId =
  QueryId of System.Guid

type StreamId =
  StreamId of string

type Behaviour<'CommandPayload,'Event> =
  'Event list -> 'CommandPayload -> 'Event list

type ProjectionDefinition<'State,'Event> =
  {
    InitialState : 'State
    UpdateState : 'State -> StreamId * 'Event list -> 'State
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

type QueryHandlerDefinition<'QueryParameter,'State,'QueryResult> =
  Query<'QueryParameter> -> 'State -> QueryHandled<'QueryResult>

type QueryResponseChannel<'QueryResult> =
  QueryResponse<'QueryResult> -> unit

type MessageHeader =
  TransactionId * StreamId

type EventSet<'Event> =
  MessageHeader * 'Event list

type EventResult<'Event> =
  Result<EventSet<'Event> list, string>

type ReadEvents<'Event> =
  unit -> EventResult<'Event> Async

type AppendEvents<'Event> =
  EventSet<'Event> -> Async<unit>

type Projection<'Event> =
  EventSet<'Event> -> unit

type Command<'CommandPayload> =
  MessageHeader * 'CommandPayload

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

