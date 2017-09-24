module Infrastructure.EventSourced

open Types
open CommandHandler
open ReadmodelHandler
open QueryManager
open EventStore

let private createQueryHandler (handler : MailboxProcessor<Msg<'Event,'QueryParameter,'QueryResult>>) query =
    handler.PostAndReply(fun reply -> ReadmodelHandler.Msg.Query <| (query,reply))

let eventSourced (behaviour : Behaviour<'Command,'Event>) (readmodels : Readmodel<'State,'Event,'QueryParameter,'QueryResult> list) : EventSourced<'Command,'Event,'QueryParameter,'State,'QueryResult> =
  let eventStore = eventStore()

  let commandHandler =
    commandHandler eventStore behaviour

  let subscribeToEvents =
    CommandHandler.Msg.AddEventSubscriber >> commandHandler.Post

  let readmodels =
    readmodels |> List.map readModelHandler

  // subscribe projections to new events
  do readmodels
      |> List.map (fun handler -> ReadmodelHandler.Msg.Events >> handler.Post)
      |> List.iter subscribeToEvents

  // initialize CommandHandler before eventStore subscribes to new events
  do commandHandler.Post CommandHandler.Msg.Init

  do eventStore
      |> fun eventStore -> EventStore.Msg.AddEvents >> eventStore.Post
      |> subscribeToEvents

  // create a queryManager with all available queryHandlers
  let queryManager =
    readmodels
    |> List.map createQueryHandler
    |> queryManager


  {
    CommandHandler = CommandHandler.Msg.Command >> commandHandler.Post
    QueryHandler = QueryManager.Msg.Query >> queryManager.Post
    EventSubscriber = subscribeToEvents
  }
