module ConfPlannerTestsDSL

open FsUnit
open Chessie.ErrorHandling
open NUnit.Framework

open CommandHandlers
open States
open Commands
open Errors

let Given (state : State) = state

let When command state : Command*State = (command, state)

let ThenStateShouldBe expectedState (command, state) =
  match evolve state command with
  | Ok((actualState,events),_) ->
      actualState |> should equal expectedState
      events |> Some
  | Bad errs ->
      sprintf "Expected : %A, But Actual : %A" expectedState errs.Head
      |> Assert.Fail
      None

let WithEvents expectedEvents actualEvents =
  match actualEvents with
  | Some (actualEvents) ->
    actualEvents |> should equal expectedEvents
  | None -> None |> should equal expectedEvents

let ShouldFailWith (expectedError : Error) (command, state) =
  match evolve state command with
  | Bad errs -> errs.Head |> should equal expectedError
  | Ok(r,_) ->
      sprintf "Expected : %A, But Actual : %A" expectedError r
      |> Assert.Fail
