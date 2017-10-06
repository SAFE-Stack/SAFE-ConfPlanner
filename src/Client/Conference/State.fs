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

let updateStateWithEvents eventSet state  =
  match state with
  | Success state ->
      eventSet
       |> snd
       |> List.fold Projections.apply state
       |> Success

  | _ -> state

let init() =
  {
    State = NotAsked
  }, Cmd.ofSub startWs


let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
  match msg with
  | Received (ServerMsg.QueryResponse response) ->
      match response.Result with
      | NotHandled ->
          model, Cmd.none

      | Handled result ->
          match result with
          | QueryResult.State state ->

              { model with State = state |> Success }, Cmd.none

  | Received (ServerMsg.Connected) ->
      let query =
        ConferenceApi.QueryParameter.State
          |> createQuery
          |> ClientMsg.Query
          |> wsCmd

      model, query

  | QueryState ->
      let query =
        ConferenceApi.QueryParameter.State
        |> createQuery
        |> ClientMsg.Query
        |> wsCmd
      model, query

  | Received (ServerMsg.Events eventSet) ->
      eventSet
      |> snd
      |> List.iter (fun event -> printfn "%A" event)
      { model with State = model.State |> updateStateWithEvents eventSet}, Cmd.none

  | FinishVotingperid ->
      model, wsCmd <| ClientMsg.Command (transactionId(),Commands.FinishVotingPeriod)

