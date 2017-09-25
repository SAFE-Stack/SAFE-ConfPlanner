module Infrastructure.EventSourced

open Types
open CommandHandler
open ReadmodelHandler
open QueryManager
open EventStore

let eventSourced (behaviour : Behaviour<'Command,'Event>) (readmodels : Readmodel<'State,'Event,'QueryParameter,'QueryResult> list) : EventSourced<'Command,'Event,'QueryParameter,'State,'QueryResult> =
  let getAllEvents,addEventsToStore =
    eventStore()

  let initCommandHandler,commandHandler,subscribeToEvents =
    commandHandler behaviour

  let projections,queryHandlers =
    readmodels |> initializeReadSide

  // subscribe projections to new events
  do projections |> List.iter subscribeToEvents

  // initialize CommandHandler with events before eventStore subscribes to new events
  do getAllEvents() |> initCommandHandler

  // subscribe eventStore to new events
  do addEventsToStore |> subscribeToEvents

  {
    CommandHandler = commandHandler
    QueryHandler = queryHandlers |> queryManager
    EventSubscriber = subscribeToEvents
  }
