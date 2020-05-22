namespace EventSourced

module EventSourced =
  open EventSourced

  type EventSourcedConfig<'Command,'Event,'Query> =
    {
      EventStoreInit : EventStorage<'Event> -> EventStore<'Event>
      EventStorageInit : unit -> EventStorage<'Event>
      CommandHandlerInit : EventStore<'Event> -> CommandHandler<'Command,'Event>
      QueryHandler : QueryHandler<'Query>
      EventListenerInit : unit -> EventListener<'Event>
      EventHandlers : EventHandler<'Event> list
    }

  type EventSourced<'Comand,'Event,'Query> (configuration : EventSourcedConfig<'Comand,'Event,'Query>) =

    let eventStorage = configuration.EventStorageInit()

    let eventStore = configuration.EventStoreInit eventStorage

    let commandHandler = configuration.CommandHandlerInit eventStore

    let queryHandler = configuration.QueryHandler

    let eventListener = configuration.EventListenerInit()

    do
      eventStore.OnError.Add(fun exn -> Helper.printError (sprintf "EventStore Error: %s" exn.Message) exn)
      commandHandler.OnError.Add(fun exn -> Helper.printError (sprintf "CommandHandler Error: %s" exn.Message) exn)
      eventStore.OnEvents.Add eventListener.Notify
      configuration.EventHandlers |> List.iter (eventListener.Subscribe >> ignore)

    member __.HandleCommand (envelope : CommandEnvelope<_>) =
      commandHandler.Handle envelope

    member __.HandleQuery query =
      queryHandler.Handle query

    member __.GetAllEvents () =
      eventStore.Get()

    member __.GetStream eventSource =
      eventStore.GetStream eventSource

    member __.Append events =
      eventStore.Append events

    member __.SubscribeToEvents eventHandler =
      eventListener.Subscribe eventHandler

    member __.UnsubscribeFromEvents id =
      eventListener.Unsubscribe id
