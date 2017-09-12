module Infrastructure.EventSourced

open Infrastructure.Types
open Infrastructure.CommandHandler
open Infrastructure.EventStore
open Infrastructure.ProjectionHandler

(*
 Todo: mehr als eine Projection möglich machen

*)

let private buildEventStore() =
  let eventStore =
    eventStore()

  let eventSubscription =
    EventStore.Msg.AddSubscriber >> eventStore.Post

  let addEvents =
    EventStore.Msg.Add >> eventStore.Post

  addEvents, eventSubscription

let eventSourced (behaviour : Behaviour<'State,'Command,'Event>) (projection : Projection<'State,'Command,'Event>) : EventSourced<'Command, 'Event> =
  let addEvents,eventSubscriber = buildEventStore()

  let stateSubscription =
    let projectionHandler =
      projectionHandler eventSubscriber projection

    ProjectionHandler.Msg.AddSubscriber >> projectionHandler.Post

  let commandHandler =
    let commandHandler =
      commandHandler addEvents stateSubscription behaviour projection

    CommandHandler.Msg.Command >> commandHandler.Post

  {
    CommandHandler = commandHandler
    EventSubscriber = eventSubscriber
  }
