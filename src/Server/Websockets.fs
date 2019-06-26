module Server.Websocket

open FSharp.Control
open Elmish.Streams.AspNetCore.Middleware
open Server.ServerTypes
open Infrastructure.Types


let eventObservable eventSourced =
  let dispatcher, observable = AsyncRx.subject ()

  let eventSubscriber events  =
    events
    |> dispatcher.OnNextAsync
    |> Async.StartImmediate

  eventSourced.EventPublisher eventSubscriber

  observable

let stream (eventSourced : EventSourced<_,_,_,_>) eventObservable (connectionId: ConnectionId) (msgs: IAsyncObservable<Msg<_,_,_,_>*ConnectionId>) : IAsyncObservable<Msg<_,_,_,_>*ConnectionId> =
  let serverMsgs =
    eventObservable
    |> AsyncRx.map (fun msg -> (msg |> Events |> ServerToClient, connectionId))

  msgs
  |> AsyncRx.map (fun (msg,id) -> printfn "msg: %A, id: %A" msg id; (msg,id))
  |> AsyncRx.filter (fun (_,id) -> printfn "connectionId: %A, id: %A" connectionId id; connectionId = id)
  |> AsyncRx.mapAsync(fun (msg,id) ->
      match msg with
      | ClientToServer msg ->
          match msg with
          | Query query ->
              async {
                let! result = eventSourced.QueryManager query

                return (result |> QueryResponse |> ServerToClient,connectionId)
              }

          | Command command ->
              eventSourced.CommandHandler command
              async { return (msg |> ClientToServer ,id) }

      | _ ->
         async { return (msg,id) }
      )
  |> AsyncRx.merge serverMsgs
