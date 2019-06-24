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
    |> AsyncRx.map (fun msg -> (msg |> Events |> ServerMsg, connectionId))

  msgs
  |> AsyncRx.filter (fun (_,id) -> connectionId = id)
  |> AsyncRx.mapAsync(fun (msg,id) ->
      match msg with
      | ClientMsg msg ->
          match msg with
          | Query query ->
              async {
                let! result = eventSourced.QueryManager query

                return (result |> QueryResponse |> ServerMsg,connectionId)
              }

          | Command command ->
              eventSourced.CommandHandler command
              async { return (msg |> ClientMsg ,id) }

      | _ ->
         async { return (msg,id) }
      )
  |> AsyncRx.merge serverMsgs
