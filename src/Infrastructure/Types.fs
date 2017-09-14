module Infrastructure.Types

type Subscriber<'a> = 'a -> unit

type TransactionId = TransactionId of System.Guid

type EventSet<'Event> = 'Event list

type Behaviour<'Command,'Event> = 'Event list -> 'Command -> 'Event list

type Projection<'State,'Event> =
  {
    InitialState : 'State
    UpdateState : 'State -> 'Event -> 'State
  }

type CommandHandler<'Command> = TransactionId*'Command -> unit

type EventSubscriber<'Event> = Subscriber<TransactionId*EventSet<'Event>> -> unit

type EventSourced<'Command, 'Event> =
  {
     CommandHandler : CommandHandler<'Command>
     EventSubscriber : EventSubscriber<'Event>
  }

