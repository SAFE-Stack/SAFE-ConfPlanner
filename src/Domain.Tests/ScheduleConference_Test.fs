module ScheduleConferenceTest

open NUnit.Framework

open Domain.Model
open Domain.Commands
open Domain.Events
open Testbase

// Scenario
let conference = emptyConference()

// [<Test>] does not work and I dont know why
// let ``Can schedule a conference`` () =
//   printfn "Can schedule a conference %A" conference
//   Given []
//   |> When (ScheduleConference conference)
//   |> ThenExpect [ConferenceScheduled conference]

[<Test>]
let ``Same conference can not be scheduled twice`` () =
  Given [ ConferenceScheduled conference ]
  |> When (ScheduleConference conference)
  |> ThenExpect [ ConferenceAlreadyScheduled |> Error]
