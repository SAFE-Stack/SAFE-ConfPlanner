module Conference.State

open Elmish
open Global
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Browser

open Server.ServerTypes
open Infrastructure.Types

open Conference.Types
open Conference.Ws
open ConferenceApi

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
  match msg with
  | Received (ServerMsg.QueryResponse response) ->
      console.log (sprintf "Response for Query (%A): %A" response.QueryId response.Result)
      match response.Result with
      | NotHandled -> model, Cmd.none
      | Handled result ->
          match result with
          | QueryResult.State state -> { model with State = state |> Success }, Cmd.none
  | Received (ServerMsg.Connected) ->
    let query =
      ConferenceApi.QueryParameter.State
        |> createQuery
        |> ClientMsg.Query
        |> wsCmd
    { model with Info = "connected" }, query
  | QueryState ->
      let query =
        ConferenceApi.QueryParameter.State
        |> createQuery
        |> ClientMsg.Query
        |> wsCmd
      model, query
  | Received(_) ->
    console.log "meger"
    model, Cmd.none
  | FinishVotingperid ->
      model, wsCmd <| ClientMsg.Command (transactionId(),Commands.FinishVotingPeriod)

