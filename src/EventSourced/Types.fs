namespace EventSourced

type TransactionId = TransactionId of System.Guid

type EventSource = System.Guid

type EventProducer<'Event> =
  'Event list -> 'Event list

type EventMetadata =
  {
    Source : EventSource
    RecordedAtUtc : System.DateTime
  }

type EventEnvelope<'Event> =
  {
    Metadata : EventMetadata
    Event : 'Event
  }

type EventSet<'Event> =
  {
    TransactionId : TransactionId
    Events : EventEnvelope<'Event> list
  }

type EventHandler<'Event> =
  EventEnvelope<'Event> list -> Async<unit>

type EventResult<'Event> =
  Result<EventEnvelope<'Event> list, string>

type EventStore<'Event> =
  {
    Get : unit -> Async<EventResult<'Event>>
    GetStream : EventSource -> Async<EventResult<'Event>>
    Append : EventSet<'Event> -> Async<Result<unit, string>>
    OnError : IEvent<exn>
    OnEvents : IEvent<EventSet<'Event>>
  }

type EventListener<'Event> =
  {
    Subscribe : EventHandler<'Event> -> unit
    Notify : EventEnvelope<'Event> list -> unit
  }

type EventStorage<'Event> =
  {
    Get : unit -> Async<EventResult<'Event>>
    GetStream : EventSource -> Async<EventResult<'Event>>
    Append : EventSet<'Event> -> Async<unit>
  }

type Projection<'State,'Event> =
  {
    Init : 'State
    Update : 'State -> 'Event -> 'State
  }

type QueryResult =
  | Handled of obj
  | NotHandled
  | QueryError of string

type  QueryHandler<'Query> =
  {
    Handle : 'Query -> Async<QueryResult>
  }

type ReadModel<'Event, 'State> =
  {
    EventHandler : EventHandler<'Event>
    State : unit -> Async<'State>
  }


type CommandEnvelope<'Command> =
  {
    Transaction : TransactionId
    EventSource : EventSource
    Command : 'Command
  }

type CommandHandler<'Command> =
  {
    Handle : CommandEnvelope<'Command> -> Async<Result<unit,string>>
    OnError : IEvent<exn>
  }

type Behaviour<'Command,'Event> =
  'Command -> EventProducer<'Event>

type DB_Connection_String = DB_Connection_String of string
