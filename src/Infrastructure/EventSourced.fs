module Infrastructure.EventSourced

open Types
open CommandHandler
open ReadHandler
open QueryManager
open EventStore

let eventSourced (behaviour : Behaviour<'CommandPayload,'Event>) (readmodels : Readmodel<'State,'Event,'QueryParameter,'QueryResult> list) store : EventSourced<'CommandPayload,'Event,'QueryParameter,'State,'QueryResult> =
  let getAllEvents,addEventsToStore =
    eventStore store

  let initCommandHandler,commandHandler,eventPublisher =
    commandHandler behaviour

  let projections,queryHandlers =
    readmodels |> initializeReadSide

  // subscribe projections to new events
  do projections |> List.iter eventPublisher

  // initialize CommandHandler with events before eventStore subscribes to new events
  do getAllEvents() |> initCommandHandler

  // subscribe eventStore to new events
  do addEventsToStore |> eventPublisher

  {
    CommandHandler = commandHandler
    QueryManager = queryHandlers |> queryManager
    EventPublisher = eventPublisher
  }
