module Testbase

open FsUnit
open NUnit.Framework

open System
open Domain.Model
open Domain.Behaviour
open Domain.Events

let Given (events : Event list) = events

let When command history =
  match behaviour command history with
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

let voteTwo (abstr: ConferenceAbstract) (organizer: Organizer) =
   Voting.Voting (abstr.Id,organizer.Id, Two)

let voteOne (abstr: ConferenceAbstract) (organizer: Organizer) =
   Voting.Voting (abstr.Id,organizer.Id, One)

let voteZero (abstr: ConferenceAbstract) (organizer: Organizer) =
   Voting.Voting (abstr.Id,organizer.Id, Zero)

let veto (abstr: ConferenceAbstract) (organizer: Organizer) =
   Voting.Voting (abstr.Id,organizer.Id, Veto)


let proposedTalk() =
   {
      Id = AbstractId <| Guid.NewGuid()
      Duration = 45.
      Speakers = []
      Text = "SomeTalk"
      Status = Proposed
      Type = Talk
   }

let roman =
  { Firstname = "Roman" ; Lastname = "Sachse" ; Id = Guid.NewGuid() |> OrganizerId }

let marco =
  { Firstname = "Marco";  Lastname = "Heimeshoff"; Id = OrganizerId <| Guid.NewGuid() }
