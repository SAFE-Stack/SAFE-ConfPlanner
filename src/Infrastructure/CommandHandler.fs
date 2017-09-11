module Infrastructure.CommandHandler

open Infrastructure.Types
open Infrastructure.EventStore
open Infrastructure.Projection

let commandHandler (eventStore : MailboxProcessor<EventStoreMsg<'Event>>) (stateProjection : MailboxProcessor<ProjectionMsg<'Event,'State>>) (eventSourced : EventSourced<'State,'Command,'Event>) =
  MailboxProcessor.Start(fun inbox ->
      stateProjection.Post <| ProjectionMsg.AddSubscriber (CommandHandlerMsg.State >> inbox.Post)

      let rec loop state =

        async {
          let! msg = inbox.Receive()

          match msg with
          | CommandHandlerMsg.Command (correlationId,command) ->
              printfn "CommandHandler received command: %A" command
              let newEvents = eventSourced.Behaviour state command

              eventStore.Post <| EventStoreMsg.Add (correlationId,newEvents)

              return! loop state

          | CommandHandlerMsg.State state ->
              printfn "CommandHandler received state: %A" state
              return! loop state
        }

      loop eventSourced.InitialState
  )
