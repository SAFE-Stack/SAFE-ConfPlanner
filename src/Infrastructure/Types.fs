module Infrastructure.Types

type Subscriber<'a> = 'a -> unit

type CorrelationId = CorrelationId of System.Guid

type CommandHandlerMsg<'Command,'State> =
  | Command of CorrelationId*'Command
  | State of 'State


type EventStoreState<'Event> = {
  Events : 'Event list
  Subscriber : Subscriber<CorrelationId*'Event list> list
}

type EventStoreMsg<'Event> =
  | Add of CorrelationId*'Event list
  | AddSubscriber of Subscriber<CorrelationId*'Event list>


type ProjectionMsg<'Event,'State> =
  | Events of 'Event list
  | AddSubscriber of Subscriber<'State>

type ProjectionState<'State> = {
  ReadModel : 'State
  Subscriber : Subscriber<'State> list
}

// Client to Server
type ClientMsg<'Command> =
  | Connect
  | Command of CorrelationId*'Command

// Server to Client
type ServerMsg<'Event> =
  | Connected
  | Events of CorrelationId*'Event list

type Behaviour<'State,'Command,'Event> = 'State -> 'Command -> 'Event list

type UpdateState<'State,'Event> = 'State -> 'Event -> 'State

