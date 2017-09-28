module Conference

open Infrastructure.Types
open Commands
open Events

type State = int

type QueryParameter =
  | State
  | StateTimesX of int
  | CanNotBeHandled


type QueryResult =
  | State of int
  | StateTimesX of int

let initialState : State = 0

let updateState (state: State) (msg : Event) : State =
  match msg with
  | Events.CallForPapersClosed  -> state + 1
  | Event.VotingPeriodWasFinished -> state + 2

let projection : Projection<State, Event>=
  {
    InitialState = initialState
    UpdateState = updateState
  }

let behaviour events command : Event list =
  let state =
    events
    |> List.fold projection.UpdateState projection.InitialState

  match command with
  | Command.FinishVotingPeriod  -> [VotingPeriodWasFinished]

let queryHandler (query : Query<QueryParameter>) (state : State) : QueryHandled<QueryResult> =
  match query.Parameter with
  | QueryParameter.State ->
      state
      |> QueryResult.State
      |> Handled

  | QueryParameter.StateTimesX int ->
      int*state
      |> QueryResult.StateTimesX
      |> Handled

  | _ -> NotHandled




