module Conference.Types

open Application.API
open Global
open Server.ServerTypes
open Domain.Model
open Domain.Events
open Domain.Commands
open EventSourced
open Application
open Elmish.Helper

type NotificationType =
  | Info
  | Success
  | Error

type Animation =
  | Entered
  | Leaving

type Notification =
  Event * TransactionId * Animation

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
  | Received of ServerMsg<Domain.Events.Event>
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
  | RequestNotificationForRemoval of Notification
  | RemoveNotification of Notification
  | ConferenceLoaded of Result<Conference, QueryError>
  | ConferencesLoaded of Result<Conferences, QueryError>
  | OrganizersLoaded of Result<Organizer list, QueryError>
  | CommandResponse of TransactionId * Result<EventEnvelope<Event> list, string>

type WhatIf =
  {
    Conference : Domain.Model.Conference
    Commands : CommandEnvelope<Command> list
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

type Model =
  {
    View : CurrentView
    Conferences : RemoteData<API.Conferences>
    Organizers : RemoteData<Domain.Model.Organizers>
    LastEvents : EventEnvelope<Domain.Events.Event> list option
    Organizer : OrganizerId
    OpenTransactions : Map<TransactionId, Deferred<unit>>
    OpenNotifications : Notification list
  }

let matchEditorWithAvailableEditor editor =
  match editor with
  | Editor.VotingPanel ->
      AvailableEditor.VotingPanel

  | Editor.Organizers ->
      AvailableEditor.Organizers

  | Editor.ConferenceInformation _ ->
      AvailableEditor.ConferenceInformation
