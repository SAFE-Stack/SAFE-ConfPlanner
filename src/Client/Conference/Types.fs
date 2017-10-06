module Conference.Types

open Global
open Infrastructure.Types
open Server.ServerTypes

type Msg =
  | Received of ServerMsg<Events.Event,ConferenceApi.QueryResult>
  | FinishVotingperiod
  | ToggleMode
  | ReopenVotingperiod
  | MakeItSo

type WhatIf =
  {
    Conference : Model.Conference
    Commands : Command<Commands.Command> list
    Events : Events.Event list
  }


type Mode =
  | Live
  | WhatIf of WhatIf

type Model =
  {
    State : RemoteData<Model.Conference>
    LastEvents : Events.Event list
    Mode : Mode
  }
