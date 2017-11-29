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

let queryOrganizers =
  API.QueryParameter.Organizers
  |> createQuery
  |> ClientMsg.Query
  |> wsCmd

let init() =
  {
    View = View.NotAsked
    Conferences = RemoteData.NotAsked
    Organizers = RemoteData.NotAsked
    LastEvents = []
    Organizer = OrganizerId <| System.Guid.Parse "311b9fbd-98a2-401e-b9e9-bab15897dad4"
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
              { model with View = (VotingPanel,conference,Live) |> Editor }, Cmd.none

          | API.QueryResult.Conferences conferences ->
              { model with Conferences = conferences |> Success }, Cmd.none

          | API.QueryResult.Organizers organizers ->
              { model with Organizers = organizers |> Success }, Cmd.none

          | API.QueryResult.ConferenceNotFound ->
              model,Cmd.none

  | Received (ServerMsg.Connected) ->
      model, List.concat [ queryConferences ; queryOrganizers ]

  | Received (ServerMsg.Events eventSet) ->
      match model.View with
      | Editor (editor, conference, Live) when eventSetIsForCurrentConference eventSet conference  ->
          let events =
            eventSet |> (fun (_,events) -> events)

          let newConference =
            events |> updateStateWithEvents conference

          { model with
              View = (editor,newConference,Live) |> Editor
              LastEvents = events
          }, Cmd.none

      | _ ->
          model, Cmd.none

  | Vote voting ->
      match model.View with
      | Editor (_, conference, Live) ->
           model, wsCmd <| ClientMsg.Command (conference.Id |> commandHeader, voting |> Commands.Vote)

      | Editor (editor, conference, WhatIf whatif) ->
          let events =
            conference |> Behaviour.vote voting

          let newConference =
            events |> updateStateWithEvents conference

          let commands =
             (conference.Id |> commandHeader, voting |> Commands.Vote) :: whatif.Commands

          let whatif =
            WhatIf <|
              {
                whatif with
                  Events = events
                  Commands = commands
              }

          { model with View = (editor,newConference,whatif) |> Editor }, Cmd.none

      | _ ->
           model, Cmd.none

  | RevokeVoting voting ->
      match model.View with
      | Editor (_, conference, Live) ->
           model, wsCmd <| ClientMsg.Command (conference.Id |> commandHeader, voting |> Commands.RevokeVoting)

      | Editor (editor, conference, WhatIf whatif) ->
          let events =
            conference |> Behaviour.revokeVoting voting

          let newConference =
            events |> updateStateWithEvents conference

          let commands =
             (conference.Id |> commandHeader, voting |> Commands.RevokeVoting) :: whatif.Commands

          let whatif =
            WhatIf <|
              {
                whatif with
                  Events = events
                  Commands = commands
              }

          { model with View = (editor,newConference,whatif) |> Editor }, Cmd.none

      | _ ->
           model, Cmd.none

  | FinishVotingperiod ->
      match model.View with
      | Editor (_, conference, Live) ->
          model, wsCmd <| ClientMsg.Command (conference.Id |> commandHeader,Commands.FinishVotingPeriod)

      | Editor (editor, conference, WhatIf whatif) ->
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

          { model with View = (editor,newConference,whatif) |> Editor }, Cmd.none

      | _ ->
           model, Cmd.none

  | ReopenVotingperiod ->
      match model.View with
      | Editor (_, conference, Live) ->
          model, wsCmd <| ClientMsg.Command (conference.Id |> commandHeader,Commands.ReopenVotingPeriod)

      | Editor (editor, conference, WhatIf whatif) ->
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

          { model with View = (editor,newConference,whatif) |> Editor }, Cmd.none

      | _ ->
           model, Cmd.none

  | AddOrganizerToConference organizer ->
      match model.View with
      | Editor (_, conference, Live) ->
          model, wsCmd <| ClientMsg.Command (conference.Id |> commandHeader, organizer |> Commands.AddOrganizerToConference)

      | Editor (editor, conference, WhatIf whatif) ->
          let events =
            conference |> Behaviour.addOrganizerToConference organizer

          let newConference =
            events |> updateStateWithEvents conference

          let commands =
             (conference.Id |> commandHeader, organizer |> Commands.AddOrganizerToConference) :: whatif.Commands

          let whatif =
            WhatIf <|
              {
                whatif with
                  Events = events
                  Commands = commands
              }

          { model with View = (editor,newConference,whatif) |> Editor }, Cmd.none

      | _ ->
           model, Cmd.none

  | RemoveOrganizerFromConference organizer ->
      match model.View with
      | Editor (_, conference, Live) ->
          model, wsCmd <| ClientMsg.Command (conference.Id |> commandHeader, organizer |> Commands.RemoveOrganizerFromConference)

      | Editor (editor, conference, WhatIf whatif) ->
          let events =
            conference |> Behaviour.removeOrganizerFromConference organizer

          let newConference =
            events |> updateStateWithEvents conference

          let commands =
             (conference.Id |> commandHeader, organizer |> Commands.RemoveOrganizerFromConference) :: whatif.Commands

          let whatif =
            WhatIf <|
              {
                whatif with
                  Events = events
                  Commands = commands
              }

          { model with View = (editor,newConference,whatif) |> Editor }, Cmd.none

      | _ ->
           model, Cmd.none

  | MakeItSo ->
      match model.View with
      | Editor (editor, conference, WhatIf whatif)  ->
          let wsCmds =
            whatif.Commands
            |> List.rev
            |> List.collect (ClientMsg.Command >> wsCmd)

          { model with View = (editor,whatif.Conference,Live) |> Editor },
          wsCmds @ (conference.Id |> queryConference)

      | _ ->
          model, Cmd.none

  | ToggleMode ->
      match model.View with
      | Editor (editor, conference, Live) ->
          let whatif =
            {
              Conference = conference
              Commands = []
              Events = []
            }

          { model with View = (editor,conference, whatif |> WhatIf) |> Editor }, Cmd.none

      | Editor (editor, conference, WhatIf _) ->
          { model with View = (editor, conference,Live) |> Editor },
          conference.Id |> queryConference

      | _ ->
          model, Cmd.none

  | SwitchToConference conferenceId ->
      model, conferenceId |> queryConference

  | SwitchToEditor editor ->
      match model.View with
      | Editor (_, conference, mode) ->
          { model with View = (editor, conference, mode) |> Editor },
          Cmd.none

      | _ ->
          model, Cmd.none


