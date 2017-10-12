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
  {
    Info : string
    Events : Dummy.Event list
  }

type Msg =
  | Received of ServerMsg<Dummy.Event,Dummy.QueryResult>
  | Command

let mutable private sendPerWebsocket : ClientMsg<Dummy.Command,Dummy.QueryParameter,Dummy.QueryResult> -> unit =
  fun _ -> failwith "WebSocket not connected"

let private startWs dispatch =
  let onMsg : System.Func<MessageEvent, obj> =
    (fun (wsMsg:MessageEvent) ->
      let msg = ofJson<ServerMsg<Dummy.Event,Dummy.QueryResult>> <| unbox wsMsg.data
      Msg.Received msg |> dispatch
      null) |> unbox // temporary fix until Fable WS Import is upgraded to Fable 1.*

  let ws = Fable.Import.Browser.WebSocket.Create("ws://127.0.0.1:8085/dummyWebsocket")

  let send msg =
    ws.send (toJson msg)

  ws.onopen <- (fun _ -> send (ClientMsg.Connect) ; null)
  ws.onmessage <- onMsg
  ws.onerror <- (fun err -> printfn "%A" err ; null)

  sendPerWebsocket <- send

  ()

let init() =
  {
    Info = "noch nicht connected"
    Events = []
  }, Cmd.ofSub startWs


let wsCmd cmd =
  [fun _ -> sendPerWebsocket cmd]

let transactionId() =
  TransactionId <| System.Guid.NewGuid()

let createQuery query =
  {
    Query.Id = QueryId <| System.Guid.NewGuid()
    Query.Parameter = query
  }

let update msg model =
  match msg with
  | Received (ServerMsg.Connected) ->
    { model with Info = "connected" }, Cmd.none

  | Received (ServerMsg.Events (transactionId,events)) ->
      console.log (sprintf "New Events %A" events)
      { model with Events = model.Events @ events }, Cmd.none

  | Command ->
      model, wsCmd <| ClientMsg.Command (transactionId(),Dummy.Command.One)

  | _ ->
      model, Cmd.none



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



let viewQueryResponse queryResponse =
  match queryResponse with
  | Some response ->
      sprintf "Response for Query (%A): %A" response.QueryId response.Result

  | None ->
      "No query sent, yet"

let root model dispatch =
  div  [ ClassName "container" ]
    [
      div [ ClassName "columns" ]
        [
          div [ ClassName "column" ]
            [
              table [ ClassName "table is-striped"]
                [
                  tbody []
                    [
                      tr []
                        [
                          td [] [ str "Websocket Status" ]
                          td [] [ str <| model.Info ]
                        ]
                    ]
                ]
            ]
        ]

      div
        [ ClassName "columns" ]
        [
          div [ ClassName "column" ]
            [
              simpleButton "Command" Command dispatch
            ]
        ]


      div [ ClassName "columns" ]
        [
          div [ ClassName "column" ]
            [
              article [ ClassName "message is-info" ]
                [
                  div  [ ClassName "message-header" ]
                    [
                      "Events" |> str
                    ]
                  div [ ClassName "message-body" ]
                    [
                      model.Events |> sprintf "%A" |> str
                    ]
                ]
            ]
        ]
    ]

