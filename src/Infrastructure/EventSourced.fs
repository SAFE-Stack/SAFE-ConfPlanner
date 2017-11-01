module Infrastructure.EventSourced

open Types
open CommandHandler
open ReadHandler
open QueryManager
open EventStore

let eventSourced (behaviour : Behaviour<'CommandPayload,'Event>) (readmodels : Readmodel<'State,'Event,'QueryParameter,'QueryResult> list) store : EventSourced<'CommandPayload,'Event,'QueryParameter,'State,'QueryResult> =
  let readEvents,appendEvents =
    eventStore store

  let commandHandler,eventPublisher =
    commandHandler readEvents behaviour

  let projections,queryHandlers =
    readmodels |> initializeReadSide

  // subscribe Projections to new events
  do projections |> List.iter eventPublisher

  // subscribe EventStore to new events
  do appendEvents |> eventPublisher

  {
    CommandHandler = commandHandler
    QueryManager = queryHandlers |> queryManager
    EventPublisher = eventPublisher
  }
