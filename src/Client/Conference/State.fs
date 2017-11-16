module Conference.State

open Elmish
open Global

open Server.ServerTypes
open Infrastructure.Types

open Conference.Types
open Conference.Ws
open Conference.Api
open Model

let private updateStateWithEvents conference events  =
  events |> List.fold Projections.apply conference

let private makeStreamId (Model.ConferenceId id) =
  id |> string |> StreamId

let private makeConferenceId (StreamId id) =
  id |> System.Guid.Parse |> ConferenceId

let private eventSetIsForCurrentConference ((_,streamId),_) conference =
  streamId |> makeConferenceId = conference.Id
let private commandHeader id =
  transactionId(), id |> makeStreamId

let queryConference conferenceId =
  conferenceId
  |> API.QueryParameter.Conference
  |> createQuery
  |> ClientMsg.Query
  |> wsCmd

let queryConferences =
  API.QueryParameter.Conferences
  |> createQuery
  |> ClientMsg.Query
  |> wsCmd

let init() =
  {
    Conference = NotAsked
    Conferences = NotAsked
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
          | API.QueryResult.Conference conference ->
              { model with Conference = (conference,Live) |> Success }, Cmd.none

          | API.QueryResult.Conferences conferences ->
              { model with Conferences = conferences |> Success }, Cmd.none

          | API.QueryResult.ConferenceNotFound ->
              model,Cmd.none

  | Received (ServerMsg.Connected) ->
      model, queryConferences

  | Received (ServerMsg.Events eventSet) ->
      match model.Conference with
      | Success (conference, Live) when eventSetIsForCurrentConference eventSet conference  ->
          let events =
            eventSet |> (fun (_,events) -> events)

          let newConference =
            events |> updateStateWithEvents conference

          { model with
              Conference = (newConference,Live) |> Success
              LastEvents = events
          }, Cmd.none

      | _ ->
          model, Cmd.none

  | FinishVotingperiod ->
      match model.Conference with
      | Success (conference, Live) ->
          model, wsCmd <| ClientMsg.Command (conference.Id |> commandHeader,Commands.FinishVotingPeriod)

      | Success (conference, WhatIf whatif) ->
          let events =
            conference |> Behaviour.finishVotingPeriod

          let newConference =
            events |> updateStateWithEvents conference

          let commands =
             (conference.Id |> commandHeader,Commands.FinishVotingPeriod) :: whatif.Commands

          let whatif =
            WhatIf <|
              {
                whatif with
                  Events = events
                  Commands = commands
              }

          { model with Conference = (newConference,whatif) |> Success }, Cmd.none

      | _ ->
           model, Cmd.none

  | ReopenVotingperiod ->
      match model.Conference with
      | Success (conference, Live) ->
          model, wsCmd <| ClientMsg.Command (conference.Id |> commandHeader,Commands.ReopenVotingPeriod)

      | Success (conference, WhatIf whatif) ->
          let events =
            conference |> Behaviour.reopenVotingPeriod

          let newConference =
            events |> updateStateWithEvents conference

          let commands =
             (conference.Id |> commandHeader,Commands.ReopenVotingPeriod) :: whatif.Commands

          let whatif =
            WhatIf <|
              {
                whatif with
                  Events = events
                  Commands = commands
              }

          { model with Conference = (newConference,whatif) |> Success }, Cmd.none

      | _ ->
           model, Cmd.none

  | MakeItSo ->
      match model.Conference with
      | Success (conference, WhatIf whatif)  ->
          let wsCmds =
            whatif.Commands
            |> List.rev
            |> List.collect (ClientMsg.Command >> wsCmd)

          { model with Conference = (whatif.Conference,Live) |> Success },
          wsCmds @ (conference.Id |> queryConference)

      | _ ->
          model, Cmd.none

  | ToggleMode ->
      match model.Conference with
      | Success (conference, Live) ->
          let whatif =
            {
              Conference = conference
              Commands = []
              Events = []
            }

          { model with Conference = (conference, whatif |> WhatIf) |> Success }, Cmd.none

      | Success (conference, WhatIf whatif) ->
          { model with Conference = (conference,Live) |> Success },
          conference.Id |> queryConference

      | _ ->
          model, Cmd.none

  | SwitchToConference conferenceId ->
      model, conferenceId |> queryConference

