module Testbase

open FsUnit
open NUnit.Framework

open System
open Domain.Model
open Domain.Behaviour
open Domain.Events

let Given (events : Event list) = events

let When command history =
  match execute history command with
  | [] -> None
  | events -> Some events

let eventEquals expected actual =
  set expected = set actual

let ThenExpect expectedEvents actualEvents =
  match actualEvents with
  | Some (actualEvents) ->
      // printfn "expected %A" (expectedEvents |> List.length)
      // printfn "actualEvents %A" (actualEvents |> List.length)
      // should equal expectedEvents actualEvents
      CollectionAssert.AreEquivalent(expectedEvents, actualEvents)

  | None ->
      None |> should equal expectedEvents

let voteTwo (abstr: ConferenceAbstract) (organizer: PersonId) =
   Voting.Voting (abstr.Id,organizer, Two)

let voteOne (abstr: ConferenceAbstract) (organizer: PersonId) =
   Voting.Voting (abstr.Id,organizer, One)

let voteZero (abstr: ConferenceAbstract) (organizer: PersonId) =
   Voting.Voting (abstr.Id,organizer, Zero)

let veto (abstr: ConferenceAbstract) (organizer: PersonId) =
   Voting.Voting (abstr.Id,organizer, Veto)


let proposedTalk() =
   {
      Id = AbstractId <| Guid.NewGuid()
      Duration = 45.
      Speakers = []
      Text = "SomeTalk"
      Status = Proposed
      Type = Talk
   }

let heimeshoff =
  person "Marco" "Heimeshoff" (Guid.NewGuid() |> string)

let fellien =
  person "Janek" "Fellien" (Guid.NewGuid() |> string)

let poepke =
  person "Conrad" "Poepke" (Guid.NewGuid() |> string)
