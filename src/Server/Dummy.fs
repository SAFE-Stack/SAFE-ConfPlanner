module Dummy

open Infrastructure.Types

type Event =
  | EventOne of string
  | EventTwo
  | EventThree


type Command =
  | One
  | Two
  | Three
  | Four


type State = int

let initialState : State = 0

let updateState (state: State) (msg : Event) : State =
  match msg with
  | Event.EventOne _ -> state + 1
  | Event.EventTwo -> state + 2
  | Event.EventThree -> state + 3

let behaviour state command : Event list =
  printfn "state in behaviour is %i" state
  match command with
  | Command.One -> [EventOne <| string state]
  | Command.Two -> [EventTwo]
  | Command.Three -> [EventThree]
  | Command.Four -> [EventOne <| string state; EventTwo]
