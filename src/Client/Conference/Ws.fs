module Conference.Ws

open Fable.Import.Browser
open Fable.Core.JsInterop
open Conference.Types
open Server.ServerTypes
open Infrastructure.Types
open Conference.Api

let private websocketNotConnected =
  fun _ -> failwith "WebSocket not connected"

let mutable private sendPerWebsocket : ClientMsg<Domain.Commands.Command,API.QueryParameter,API.QueryResult> -> unit =
  websocketNotConnected

let mutable closeWebsocket : unit -> unit =
  websocketNotConnected

let startWs token dispatch =
  let onMsg : System.Func<MessageEvent, obj> =
    (fun (wsMsg : MessageEvent) ->
      let msg =
        ofJson<ServerMsg<Domain.Events.Event,API.QueryResult>> <| unbox wsMsg.data

      Msg.Received msg |> dispatch

      null
    ) |> unbox // temporary fix until Fable WS Import is upgraded to Fable 1.*

  let ws = Fable.Import.Browser.WebSocket.Create("ws://127.0.0.1:8085" + Server.Urls.Conference + "?jwt=" + token)

  let send msg =
    ws.send (toJson msg)

  ws.onopen <- (fun _ -> send (ClientMsg.Connect) ; null)
  ws.onmessage <- onMsg
  ws.onerror <- (fun err -> printfn "%A" err ; null)

  sendPerWebsocket <- send

  closeWebsocket <- (fun _ -> sendPerWebsocket <- websocketNotConnected ; ws.close (1000.,"") |> ignore )

  ()

let stopWs _ =
  closeWebsocket ()

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
