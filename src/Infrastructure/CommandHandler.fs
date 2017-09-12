module Infrastructure.CommandHandler

open Infrastructure.Types

type Msg<'Command,'State> =
  | Command of CorrelationId*'Command
  | State of 'State

let commandHandler
  (addEvents : AddEvents<'Event>)
  (stateProjection : Subscriber<'State> -> unit)
  (behaviour : Behaviour<'State,'Command,'Event>)
  (projection : Projection<'State,'Command,'Event>) =

    MailboxProcessor.Start(fun inbox ->
        stateProjection (State >> inbox.Post)

        let rec loop state =

          async {
            let! msg = inbox.Receive()

            match msg with
            | Msg.Command (correlationId,command) ->
                printfn "CommandHandler received command: %A" command
                let newEvents = behaviour state command

                addEvents (correlationId,newEvents)

                return! loop state

            | Msg.State state ->
                printfn "CommandHandler received state: %A" state
                return! loop state
          }

        loop projection.InitialState
    )
