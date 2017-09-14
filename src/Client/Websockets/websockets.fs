module Ws

open Elmish
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Browser
open Fable.Helpers.React
open Fable.Helpers.React.Props

open Infrastructure.Types
open Server.ServerTypes

type Model =
  { Info : string
    Events : Dummy.Event list }

type Msg =
  | Received of ServerMsg<Dummy.Event>
  | CommandOne
  | CommandTwo
  | CommandThree

let mutable private sendPerWebsocket : ClientMsg<Dummy.Command> -> unit = (fun _ -> failwith "WebSocket not connected")

let startWs dispatch =
  let onMsg : System.Func<MessageEvent, obj> =
    (fun (wsMsg:MessageEvent) ->
      let msg =  ofJson<ServerMsg<Dummy.Event>> <| unbox wsMsg.data
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

let init() =
  { Info = "noch nicht connected"; Events = [] }, Cmd.ofSub startWs


let wsCmd cmd =
  [fun _ -> sendPerWebsocket cmd]


let transactionId() =
  TransactionId <| System.Guid.NewGuid()


let update msg model =
  match msg with
  | Received (ServerMsg.Connected) ->
    { model with Info = "connected" }, Cmd.none

  | Received (ServerMsg.Events (transactionId,events)) ->
      console.log (sprintf "New Events %A" events)
      { model with Events = model.Events @ events }, Cmd.none

  | CommandOne ->
      model, wsCmd <| ClientMsg.Command (transactionId(),Dummy.Command.One)

  | CommandTwo ->
      model, wsCmd <| ClientMsg.Command (transactionId(),Dummy.Command.Two)

  | CommandThree ->
      model, wsCmd <| ClientMsg.Command (transactionId(),Dummy.Command.Three)



// ------------ VIEW -------------
open Fable.Helpers.React
open Fable.Helpers.React.Props

let simpleButton txt action dispatch =
  div
    [ ClassName "column" ]
    [ a
        [ ClassName "button"
          OnClick (fun _ -> action |> dispatch) ]
        [ str txt ] ]



let root model dispatch =
  div
    []
    [
      div
        [ ClassName "columns is-vcentered" ]
        [ div [ ClassName "column" ] [ ]
          simpleButton "CommandOne" CommandOne dispatch
          simpleButton "CommandTwo" CommandTwo dispatch
          simpleButton "CommandThree" CommandThree dispatch
          div [ ClassName "column" ] [ ]
        ]

      div
        [ ClassName "columns is-vcentered" ]
        [ div [ ClassName "column" ] [ str <| model.Info ]
        ]

      div
        [ ClassName "columns is-vcentered" ]
        [ div [ ClassName "column" ] [ str <| (model.Events |> sprintf "%A") ]
        ]
    ]

