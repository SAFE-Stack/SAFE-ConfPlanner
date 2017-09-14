module Infrastructure.EventSourced

open Infrastructure.Types
open Infrastructure.CommandHandler
open Infrastructure.ProjectionHandler

(*
 Todo: Projections als Map um sie abfragen zu können?

*)


let eventSourced (behaviour : Behaviour<'Command,'Event>) (projections : Projection<'State,'Event> list) : EventSourced<'Command, 'Event> =
  let commandHandler =
    commandHandler behaviour

  let eventSubscription =
    CommandHandler.Msg.AddEventSubscriber >> commandHandler.Post

  projections
  |> List.map projectionHandler
  |> List.map (fun handler -> ProjectionHandler.Msg.Events >> handler.Post)
  |> List.iter eventSubscription

  {
    CommandHandler = CommandHandler.Msg.Command >> commandHandler.Post
    EventSubscriber = eventSubscription
  }
