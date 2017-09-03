module ConfPlannerTestsDSL

open FsUnit
open Chessie.ErrorHandling
open NUnit.Framework

open CommandHandlers
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
      actualEvents |> eventEquals expectedEvents |> should be True //mache schöner
  | None -> None |> should equal expectedEvents