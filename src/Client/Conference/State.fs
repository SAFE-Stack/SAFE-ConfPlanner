module Conference.State

open Elmish
open Fable.Import.Browser
open Fable.Core.JsInterop
open Global
open Conference.Types
open Server.ServerTypes
open Infrastructure.Types

let mutable private sendPerWebsocket : ClientMsg<Dummy.Command,Dummy.QueryParameter,Dummy.QueryResult> -> unit =
  fun _ -> failwith "WebSocket not connected"

let private startWs dispatch =
  let onMsg : System.Func<MessageEvent, obj> =
    (fun (wsMsg:MessageEvent) ->
      let msg =  ofJson<ServerMsg<Events.Event,Dummy.QueryResult>> <| unbox wsMsg.data
      Msg.Received msg |> dispatch
      null) |> unbox // temporary fix until Fable WS Import is upgraded to Fable 1.*

  let ws = Fable.Import.Browser.WebSocket.Create("ws://127.0.0.1:8085/websocket")

  let send msg =
    ws.send (toJson msg)

  ws.onopen <- (fun _ -> send (ClientMsg.Connect) ; null)
  ws.onmessage <- onMsg
  ws.onerror <- (fun err -> printfn "%A" err ; null)

  sendPerWebsocket <- send

  ()


let wsCmd cmd =
  [fun _ -> sendPerWebsocket cmd]

let transactionId() =
  TransactionId <| System.Guid.NewGuid()

let createQuery query =
  {
    Query.Id = QueryId <| System.Guid.NewGuid()
    Query.Parameter = query
  }


let updateStateWithEvents state events =
  match state with
  | Success state ->
       events
        |> List.fold Projections.apply state
        |> Success

  | _ -> state

let init() =
  {
    Info = "noch nicht connected"
    State = NotAsked
  }, Cmd.ofSub startWs


let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
  model, Cmd.none

