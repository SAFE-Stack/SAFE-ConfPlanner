module Conference.Types

open Global
open Infrastructure.Types
open Server.ServerTypes
open Model
open Conference.Api

type Editor =
  | VotingPanel
  | Organizers
  | ConferenceData

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
  | SwitchToEditor of Editor

type WhatIf =
  {
    Conference : Model.Conference
    Commands : Command<Commands.Command> list
    Events : Events.Event list
  }

type Mode =
  | Live
  | WhatIf of WhatIf

type View =
  | NotAsked
  | Loading
  | Error of string
  | Editor of Editor * Model.Conference * Mode

type Model =
  {
    View : View
    Conferences : RemoteData<Conferences.Conferences>
    Organizers : RemoteData<Model.Organizers>
    LastEvents : Events.Event list
    Organizer : OrganizerId
  }
