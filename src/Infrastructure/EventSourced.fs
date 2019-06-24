module Infrastructure.EventSourced

open Types
open CommandHandler
open ReadHandler
open QueryManager
open EventStore

let toProjectionAndQueryHandler (projectionDefinition : ProjectionDefinition<'State,'Event>) (queryHandlerDefinition : QueryHandlerDefinition<'QueryParameter,'State,'QueryResult>) =
  readHandler projectionDefinition queryHandlerDefinition

let eventSourced
      (behaviour : Behaviour<'CommandPayload,'Event>)
      (projections : Projection<'Event> list)
      (queryHandlers : QueryHandler<'QueryParameter,'QueryResult> list)
      store
      : EventSourced<'CommandPayload,'Event,'QueryParameter,'QueryResult> =

  let readEvents,appendEvents =
    eventStore store

  let commandHandler,eventPublisher =
    commandHandler readEvents appendEvents behaviour projections

  {
    CommandHandler = commandHandler
    QueryManager = queryHandlers |> queryManager
    EventPublisher = eventPublisher
  }
