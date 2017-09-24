module Testbase

open FsUnit
open NUnit.Framework

open System
open Model
open Behaviour
open Commands
open Events


let Given (events : Event list) = events

let When command history =
  match execute history command with
  | [] -> None
  | events -> Some events

let eventEquals expected actual =
  set expected = set actual

let ThenExpect expectedEvents actualEvents =
  match actualEvents with
  | Some (actualEvents) -> CollectionAssert.AreEquivalent(expectedEvents, actualEvents)
  | None -> None |> should equal expectedEvents


let vote (abstr: ConferenceAbstract) (organizer: Organizer) (value: Points) =
   Voting.Vote (abstr.Id,organizer.Id, value)

let veto (abstr: ConferenceAbstract) (organizer: Organizer) =
   Voting.Veto (abstr.Id,organizer.Id)

let proposedTalk() =
   {
      Id = AbstractId <| Guid.NewGuid()
      Duration = 45.
      Speakers = []
      Text = "SomeTalk"
      Status = Proposed
      Type = Talk
   }
