module ConfPlannerTestsDSL

open FsUnit
open NUnit.Framework

open Behaviour
open States
open Commands

let Given (state : State) = state

let When command state =
  match execute state command with
  | [] -> None
  | events -> Some events

let eventEquals expected actual =
  set expected = set actual

let ThenExpect expectedEvents actualEvents =
  match actualEvents with
  | Some (actualEvents) ->
      actualEvents |> eventEquals expectedEvents |> should be True //TODO: Explicit output instead of True
  | None -> None |> should equal expectedEvents
