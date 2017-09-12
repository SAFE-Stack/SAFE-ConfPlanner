module Infrastructure.Types

type Subscriber<'a> = 'a -> unit

type CorrelationId = CorrelationId of System.Guid

type EventsWithCorrelation<'Event> = CorrelationId*'Event list

type EventStoreState<'Event> = {
  Events : 'Event list
  Subscriber : Subscriber<EventsWithCorrelation<'Event>> list
}

type AddEvents<'Event> = EventsWithCorrelation<'Event> -> unit

type Behaviour<'State,'Command,'Event> = 'State -> 'Command -> 'Event list

type Projection<'State,'Command,'Event> =
  {
    InitialState : 'State
    UpdateState : 'State -> 'Event -> 'State
  }

type CommandHandler<'Command> = CorrelationId*'Command -> unit

type EventSubscriber<'Event> = Subscriber<EventsWithCorrelation<'Event>> -> unit

type EventSourced<'Command, 'Event> =
  {
     CommandHandler : CommandHandler<'Command>
     EventSubscriber : EventSubscriber<'Event>
  }

