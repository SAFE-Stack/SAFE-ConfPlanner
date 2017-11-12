module ScheduleConferenceTest

open System
open NUnit.Framework

open Model
open Commands
open Events
open Testbase

// Scenario
let conference = emptyConference()

printfn "conference %A" conference

// [<Test>] does not work and I dont know why
// let ``Can schedule a conference`` () =
//   printfn "Can schedule a conference %A" conference
//   Given []
//   |> When (ScheduleConference conference)
//   |> ThenExpect [ConferenceScheduled conference]

[<Test>]
let ``Same conference can not be scheduled twice`` () =
  printfn "Same conference can not be scheduled twice %A" conference
  Given [ConferenceScheduled conference]
  |> When (ScheduleConference conference)
  |> ThenExpect [ConferenceAlreadyScheduled]
