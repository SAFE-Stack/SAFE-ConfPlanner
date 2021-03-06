module Conference.Types

open Application.API
open Config
open Server.ServerTypes
open Domain.Model
open Domain.Events
open Domain.Commands
open EventSourced
open Application
open Utils.Elmish

type NotificationType =
  | Info
  | Success
  | Warning

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
  | AllOrNothing
  | SwitchToConference of ConferenceId
  | SwitchToNewConference
  | SwitchToEditor of AvailableEditor
  | ResetConferenceInformation
  | ScheduleNewConference
  | UpdateConferenceInformation
  | ConferenceInformationMsg of ConferenceInformation.Types.Msg
  | ConferenceQuery of AsyncOperationStatus<Result<Conference, QueryError>>
  | ConferencesQuery of AsyncOperationStatus<Result<Conferences, QueryError>>
  | OrganizersLoaded of AsyncOperationStatus<Result<Organizer list, QueryError>>
  | CommandResponse of TransactionId list * Result<EventEnvelope<Event> list, string>

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
  | NotSuccessful of string
  | ScheduleNewConference of ConferenceInformation.Types.Model
  | Edit of Editor * Domain.Model.Conference * Mode

type Model =
  {
    View : CurrentView
    Conferences : Deferred<API.Conferences>
    Organizers : Deferred<Domain.Model.Organizers>
    LastEvents : EventEnvelope<Domain.Events.Event> list
    Organizer : OrganizerId
    OpenTransactions : Map<TransactionId, Deferred<unit>>
  }

let matchEditorWithAvailableEditor editor =
  match editor with
  | VotingPanel ->
      AvailableEditor.VotingPanel

  | Organizers ->
      AvailableEditor.Organizers

  | ConferenceInformation _ ->
      AvailableEditor.ConferenceInformation
