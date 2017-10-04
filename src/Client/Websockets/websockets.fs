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


type RemoteData<'Result> =
  | NotAsked
  | Loading
  | Failure of string
  | Success of 'Result

type Model =
  {
    Info : string
    State : RemoteData<Dummy.State>
    Events : Dummy.Event list
    LastQueryResponse : QueryResponse<Dummy.QueryResult> option
  }

type Msg =
  | Received of ServerMsg<Dummy.Event,Dummy.QueryResult>
  | CommandOne
  | CommandTwo
  | CommandThree
  | CommandFour
  | QueryState
  | QueryStateTimesX of int
  | QueryCanNotBeHandled



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
    State = NotAsked
    Events = []
    LastQueryResponse = None
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


let updateStateWithEvents state events =
  match state with
  | Success state ->
       events
        |> List.fold Dummy.updateState state
        |> Success

  | _ -> state


let update msg model =
  match msg with
  | Received (ServerMsg.Connected) ->
    { model with Info = "connected" }, Cmd.none

  | Received (ServerMsg.Events (transactionId,events)) ->
      console.log (sprintf "New Events %A" events)
      { model with
          Events = model.Events @ events
          State = events |> updateStateWithEvents model.State
      }, Cmd.none


  | Received (ServerMsg.QueryResponse response) ->
      console.log (sprintf "Response for Query (%A): %A" response.QueryId response.Result)
      let model = { model with LastQueryResponse = response |> Some }

      match response.Result with
      | NotHandled ->
         model, Cmd.none

      | Handled result ->
          match result with
          | Dummy.QueryResult.State state ->
              { model with State = state |> Success }, Cmd.none

          | Dummy.QueryResult.StateTimesX state ->
              model, Cmd.none

  | CommandOne ->
      model, wsCmd <| ClientMsg.Command (transactionId(),Dummy.Command.One)

  | CommandTwo ->
      model, wsCmd <| ClientMsg.Command (transactionId(),Dummy.Command.Two)

  | CommandThree ->
      model, wsCmd <| ClientMsg.Command (transactionId(),Dummy.Command.Three)

  | CommandFour ->
      model, wsCmd <| ClientMsg.Command (transactionId(),Dummy.Command.Four)


  | QueryState ->
      let query =
        Dummy.QueryParameter.State
        |> createQuery
        |> ClientMsg.Query
        |> wsCmd

      model, query

  | QueryStateTimesX x ->
      let query =
        Dummy.QueryParameter.StateTimesX x
        |> createQuery
        |> ClientMsg.Query
        |> wsCmd

      model, query

  | QueryCanNotBeHandled ->
      let query =
        Dummy.QueryParameter.CanNotBeHandled
        |> createQuery
        |> ClientMsg.Query
        |> wsCmd

      model, query



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

                      tr []
                        [
                          td [] [ str "Current State" ]
                          td [] [ str <| string model.State ]
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
              simpleButton "Command: Add 1" CommandOne dispatch
              simpleButton "Command: Add 2" CommandTwo dispatch
              simpleButton "Command: Add 3" CommandThree dispatch
              simpleButton "Command: Add 3 if State < 10" CommandFour dispatch
            ]

          div [ ClassName "column" ]
            [
              simpleButton "Query: State" QueryState dispatch
              simpleButton "Query: State Times 3" (QueryStateTimesX 3) dispatch
              simpleButton "Query: Can Not Be Handled" QueryCanNotBeHandled dispatch
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

      div [ ClassName "columns" ]
        [
          div [ ClassName "column" ]
            [
              article [ ClassName "message is-info" ]
                [
                  div [ ClassName "message-header" ]
                    [
                      "Last Query Response" |> str
                    ]
                  div [ ClassName "message-body" ]
                    [
                      model.LastQueryResponse |> viewQueryResponse |> str
                    ]
                ]
            ]
        ]
    ]

