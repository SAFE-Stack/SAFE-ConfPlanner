module Infrastructure.EventSourced

open Types
open CommandHandler
open ReadHandler
open QueryManager
open EventStore
let eventSourced (behaviour : Behaviour<'CommandPayload,'Event>) (readmodels : Readmodel<'State,'Event,'QueryParameter,'QueryResult> list) store : EventSourced<'CommandPayload,'Event,'QueryParameter,'State,'QueryResult> =
  let readEvents,appendEvents =
    eventStore store

  let projections,queryHandlers =
    readmodels |> initializeReadSide

  let commandHandler,eventPublisher =
    commandHandler readEvents appendEvents behaviour projections

  {
    CommandHandler = commandHandler
    QueryManager = queryHandlers |> queryManager
    EventPublisher = eventPublisher
  }
