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

let updateStateWithEvents conference events  =
  events |> List.fold Projections.apply conference

let queryForState () =
  ConferenceApi.QueryParameter.State
  |> createQuery
  |> ClientMsg.Query
  |> wsCmd

let init() =
  {
    State = NotAsked
    Mode = Live
    LastEvents = []
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

  | Received (ServerMsg.Events eventSet) ->
      match (model.State, model.Mode) with
      | Success conference, Live ->
          let events =
            eventSet |> snd

          let newConference =
            events |> updateStateWithEvents conference

          { model with
              State = newConference |> Success
              LastEvents = events
          }, Cmd.none

      | _ ->
          model, Cmd.none

  | FinishVotingperiod ->
      match (model.State, model.Mode) with
      | Success conference, Live ->
          model, wsCmd <| ClientMsg.Command (transactionId(),Commands.FinishVotingPeriod)

      | Success conference, WhatIf whatif ->
          let events =
            conference |> Behaviour.finishVotingPeriod

          let newConference =
            events |> updateStateWithEvents conference

          let commands =
             (transactionId(),Commands.FinishVotingPeriod) :: whatif.Commands

          { model with
              State = newConference |> Success
              Mode = { whatif with Events = events ; Commands = commands } |> WhatIf
          }, Cmd.none

      | _ ->
           model, Cmd.none

  | MakeItSo ->
      match model.Mode with
      | Live ->
          model, Cmd.none

      | WhatIf whatif ->
          let wsCmds =
            whatif.Commands
            |> List.rev
            |> List.collect (ClientMsg.Command >> wsCmd)

          { model with
              State = whatif.Conference |> Success
              Mode = Live
          }, wsCmds @ queryForState ()

  | ToggleMode ->
      match (model.State, model.Mode) with
      | Success conference, Live ->
          let whatif =
            {
              Conference = conference
              Commands = []
              Events = []
            }

          { model with Mode = whatif |> WhatIf }, Cmd.none

      | _, WhatIf _ ->
          { model with Mode = Live }, queryForState ()

      | _ ->
          model, Cmd.none



