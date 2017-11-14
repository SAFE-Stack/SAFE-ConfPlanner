module Dummy

open Infrastructure.Types

type Event =
  | EventOne
  | EventTwo
  | EventThree

type Command =
  | One
  | Two
  | Three
  | Four

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
  | Event.EventOne -> state + 1
  | Event.EventTwo -> state + 2
  | Event.EventThree -> state + 3

let private evolveState state (_, events) =
  events |> List.fold updateState state

let projection : ProjectionDefinition<State, Event>=
  {
    InitialState = initialState
    UpdateState = evolveState
  }

let behaviour events command : Event list =
  let state =
    events |> List.fold updateState initialState

  match command with
  | Command.One ->
      [EventOne]

  | Command.Two ->
      [EventTwo]

  | Command.Three ->
      [EventThree]

  | Command.Four ->
      if state > 10 then
        []
      else
        [EventOne; EventTwo]

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




