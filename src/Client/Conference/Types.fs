module Conference.Types

open Global
open Infrastructure.Types
open Server.ServerTypes
open Domain.Model
open Domain.Events
open Conference.Api

type AvailableEditor =
  | VotingPanel
  | Organizers
  | ConferenceInformation

type WhatIfMsg =
  | Vote of Voting
  | RevokeVoting of Voting
  | FinishVotingperiod
  | ReopenVotingperiod
  | AddOrganizerToConference of Organizer
  | RemoveOrganizerFromConference of Organizer
  | ChangeTitle of string
  | DecideNumberOfSlots of int

type Msg =
  | Received of ServerMsg<Domain.Events.Event,API.QueryResult>
  | WhatIfMsg of WhatIfMsg
  | ToggleMode
  | MakeItSo
  | SwitchToConference of ConferenceId
  | SwitchToNewConference
  | SwitchToEditor of AvailableEditor
  | ResetConferenceInformation
  | ScheduleNewConference
  | UpdateConferenceInformation
  | ConferenceInformationMsg of ConferenceInformation.Types.Msg
  | RemoveNotification of Event

type WhatIf =
  {
    Conference : Domain.Model.Conference
    Commands : Command<Domain.Commands.Command> list
    Events : Domain.Events.Event list
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
  | ScheduleNewConference of ConferenceInformation.Types.Model
  | Edit of Editor * Domain.Model.Conference * Mode

type Notification =
  Notification of text : string * Event

type NotificationType =
  | Info
  | Success
  | Error

type Model =
  {
    View : CurrentView
    Conferences : RemoteData<Conferences.Conferences>
    Organizers : RemoteData<Domain.Model.Organizers>
    LastEvents : Domain.Events.Event list
    Organizer : OrganizerId
    OpenTransactions : TransactionId list
    OpenNotifications : Event list
  }

let matchEditorWithAvailableEditor editor =
  match editor with
  | Editor.VotingPanel ->
      AvailableEditor.VotingPanel

  | Editor.Organizers ->
      AvailableEditor.Organizers

  | Editor.ConferenceInformation _ ->
      AvailableEditor.ConferenceInformation
