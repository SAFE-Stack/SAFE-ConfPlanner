module Conference.Types

open Global
open Infrastructure.Types
open Server.ServerTypes
open Model
open Conference.Api

type AvailableEditor =
  | VotingPanel
  | Organizers
  | ConferenceInformation


type Msg =
  | Received of ServerMsg<Events.Event,API.QueryResult>
  | Vote of Voting
  | RevokeVoting of Voting
  | FinishVotingperiod
  | ToggleMode
  | ReopenVotingperiod
  | AddOrganizerToConference of Organizer
  | RemoveOrganizerFromConference of Organizer
  | MakeItSo
  | SwitchToConference of ConferenceId
  | SwitchToEditor of AvailableEditor
  | ResetConferenceInformation
  | UpdateConferenceInformation
  | ConferenceInformationMsg of ConferenceInformation.Types.Msg

type WhatIf =
  {
    Conference : Model.Conference
    Commands : Command<Commands.Command> list
    Events : Events.Event list
  }

type Mode =
  | Live
  | WhatIf of WhatIf

type Editor =
  | VotingPanel
  | Organizers
  | ConferenceInformation of ConferenceInformation.Types.Model

type CurrentView =
  | NotAsked
  | Loading
  | Error of string
  | Edit of Editor * Model.Conference * Mode

type Model =
  {
    View : CurrentView
    Conferences : RemoteData<Conferences.Conferences>
    Organizers : RemoteData<Model.Organizers>
    LastEvents : Events.Event list
    Organizer : OrganizerId
  }

let matchEditorWithAvailableEditor editor =
  match editor with
  | Editor.VotingPanel ->
      AvailableEditor.VotingPanel

  | Editor.Organizers ->
      AvailableEditor.Organizers

  | Editor.ConferenceInformation _ ->
      AvailableEditor.ConferenceInformation
