module FinishVotingPeriodTest

open NUnit.Framework

open ConfPlannerTestsDSL
open Domain
open System
open Commands
open Events
open Errors
open States
open TestData

[<Test>]
let ``Can finish voting period`` () =
  let conference = conference()
  Given conference
  |> When FinishVotingPeriod
  |> ThenStateShouldBe {conference with VotingPeriod = Finished}
  |> WithEvents [VotingPeriodWasFinished]

[<Test>]
let ``Cannot finish an already finished voting period`` () =
  let conference = conference() |> withFinishedVotingPeriod
  Given conference
  |> When FinishVotingPeriod
  |> ShouldFailWith VotingPeriodAlreadyFinished