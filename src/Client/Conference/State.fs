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
    View = CurrentView.NotAsked
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
              { model with View = (VotingPanel,conference,Live) |> Edit }, Cmd.none

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
      | Edit (editor, conference, Live) when eventSetIsForCurrentConference eventSet conference  ->
          let events =
            eventSet |> (fun (_,events) -> events)

          let newConference =
            events |> updateStateWithEvents conference

          { model with
              View = (editor,newConference,Live) |> Edit
              LastEvents = events
          }, Cmd.none

      | _ ->
          model, Cmd.none

  | Vote voting ->
      match model.View with
      | Edit (_, conference, Live) ->
           model, wsCmd <| ClientMsg.Command (conference.Id |> commandHeader, voting |> Commands.Vote)

      | Edit (editor, conference, WhatIf whatif) ->
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

          { model with View = (editor,newConference,whatif) |> Edit }, Cmd.none

      | _ ->
           model, Cmd.none

  | RevokeVoting voting ->
      match model.View with
      | Edit (_, conference, Live) ->
           model, wsCmd <| ClientMsg.Command (conference.Id |> commandHeader, voting |> Commands.RevokeVoting)

      | Edit (editor, conference, WhatIf whatif) ->
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

          { model with View = (editor,newConference,whatif) |> Edit }, Cmd.none

      | _ ->
           model, Cmd.none

  | FinishVotingperiod ->
      match model.View with
      | Edit (_, conference, Live) ->
          model, wsCmd <| ClientMsg.Command (conference.Id |> commandHeader,Commands.FinishVotingPeriod)

      | Edit (editor, conference, WhatIf whatif) ->
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

          { model with View = (editor,newConference,whatif) |> Edit }, Cmd.none

      | _ ->
           model, Cmd.none

  | ReopenVotingperiod ->
      match model.View with
      | Edit (_, conference, Live) ->
          model, wsCmd <| ClientMsg.Command (conference.Id |> commandHeader,Commands.ReopenVotingPeriod)

      | Edit (editor, conference, WhatIf whatif) ->
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

          { model with View = (editor,newConference,whatif) |> Edit }, Cmd.none

      | _ ->
           model, Cmd.none

  | AddOrganizerToConference organizer ->
      match model.View with
      | Edit (_, conference, Live) ->
          model, wsCmd <| ClientMsg.Command (conference.Id |> commandHeader, organizer |> Commands.AddOrganizerToConference)

      | Edit (editor, conference, WhatIf whatif) ->
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

          { model with View = (editor,newConference,whatif) |> Edit }, Cmd.none

      | _ ->
           model, Cmd.none

  | RemoveOrganizerFromConference organizer ->
      match model.View with
      | Edit (_, conference, Live) ->
          model, wsCmd <| ClientMsg.Command (conference.Id |> commandHeader, organizer |> Commands.RemoveOrganizerFromConference)

      | Edit (editor, conference, WhatIf whatif) ->
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

          { model with View = (editor,newConference,whatif) |> Edit }, Cmd.none

      | _ ->
           model, Cmd.none

  | MakeItSo ->
      match model.View with
      | Edit (editor, conference, WhatIf whatif)  ->
          let wsCmds =
            whatif.Commands
            |> List.rev
            |> List.collect (ClientMsg.Command >> wsCmd)

          { model with View = (editor,whatif.Conference,Live) |> Edit },
          wsCmds @ (conference.Id |> queryConference)

      | _ ->
          model, Cmd.none

  | ToggleMode ->
      match model.View with
      | Edit (editor, conference, Live) ->
          let whatif =
            {
              Conference = conference
              Commands = []
              Events = []
            }

          { model with View = (editor,conference, whatif |> WhatIf) |> Edit }, Cmd.none

      | Edit (editor, conference, WhatIf _) ->
          { model with View = (editor, conference,Live) |> Edit },
          conference.Id |> queryConference

      | _ ->
          model, Cmd.none

  | SwitchToConference conferenceId ->
      model, conferenceId |> queryConference

  | SwitchToEditor target ->
      match model.View with
      | Edit (_, conference, mode) ->
          let editor =
            match target with
            | AvailableEditor.ConferenceInformation ->
                ConferenceInformation.State.init conference.Title (conference.AvailableSlotsForTalks |> string)
                |> Editor.ConferenceInformation

            | AvailableEditor.VotingPanel ->
                Editor.VotingPanel

            | AvailableEditor.Organizers ->
                Editor.Organizers

          { model with View = (editor, conference, mode) |> Edit },
          Cmd.none

      | _ ->
          model, Cmd.none

  | ResetConferenceInformation ->
      match model.View with
      | Edit (ConferenceInformation _, conference, mode) ->
          let editor =
            ConferenceInformation.State.init conference.Title (conference.AvailableSlotsForTalks |> string)
            |> Editor.ConferenceInformation

          { model with View = (editor, conference, mode) |> Edit },
          Cmd.none

      | _ ->
          model, Cmd.none

  // | UpdateConferenceInformation ->
  //     match model.View with
  //     | Edit (ConferenceInformation submodel, conference, whatif) when submodel |> ConferenceInformation.Types.isValid ->
  //         let title =
  //           submodel |> ConferenceInformation.Types.title

  //         let availableSlotsForTalks =
  //           submodel |> ConferenceInformation.Types.availableSlotsForTalks

  //         model, wsCmd <| ClientMsg.Command (conference.Id |> commandHeader, organizer |> Commands.RemoveOrganizerFromConference)


  //     | _ ->
  //          model, Cmd.none

  | ConferenceInformationMsg msg ->
      match model.View with
      | Edit (ConferenceInformation submodel, conference, mode) ->
          let newSubmodel =
            submodel |> ConferenceInformation.State.update msg

          { model with View = (ConferenceInformation newSubmodel, conference, mode) |> Edit },
          Cmd.none

      | _ ->
          model, Cmd.none


