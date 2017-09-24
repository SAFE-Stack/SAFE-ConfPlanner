module FinishVotingPeriodTest

open System
open NUnit.Framework

open Model
open Commands
open Events
open Testbase

// Scenario
let heimeshoff = { Firstname = "Marco";  Lastname = "Heimeshoff"; Id = OrganizerId <| Guid.NewGuid() }
let fellien = { Firstname = "Janek";  Lastname = "Felien"; Id = OrganizerId <| Guid.NewGuid() }
let poepke = { Firstname = "Conrad";  Lastname = "Poepke"; Id = OrganizerId <| Guid.NewGuid() }


[<Test>]
let ``Can finish voting period`` () =
  Given [
    CallForPapersOpened
    CallForPapersClosed]
  |> When FinishVotingPeriod
  |> ThenExpect [VotingPeriodWasFinished]


[<Test>]
let ``Cannot finish an already finished voting period`` () =
  Given [
    CallForPapersOpened
    CallForPapersClosed
    VotingPeriodWasFinished]
  |> When FinishVotingPeriod
  |> ThenExpect [FinishingDenied "Voting Period Already Finished"]


[<Test>]
let ``Cannot finish a voting period when call for papers is not closed`` () =
  Given [
    CallForPapersOpened]
  |> When FinishVotingPeriod
  |> ThenExpect [FinishingDenied "Call For Papers Not Closed"]


[<Test>]
let ``Cannot finish a voting period when not all abstracts have votes from every organizer`` () =
  let talk1 = proposedTalk()
  let talk2 = proposedTalk()
  let talk3 = proposedTalk()

  Given [
    OrganizerRegistered heimeshoff
    OrganizerRegistered fellien
    OrganizerRegistered poepke

    CallForPapersOpened
    TalkWasProposed talk1
    TalkWasProposed talk2
    TalkWasProposed talk3
    CallForPapersClosed
    
    VotingWasIssued (vote talk1 heimeshoff (Points 1))
    VotingWasIssued (vote talk2 heimeshoff (Points 1))
    VotingWasIssued (vote talk3 heimeshoff (Points 1))
    VotingWasIssued (vote talk1 fellien (Points 1))
    VotingWasIssued (vote talk2 fellien (Points 1))
    VotingWasIssued (vote talk1 poepke (Points 1))
    VotingWasIssued (vote talk2 poepke (Points 1))
    VotingWasIssued (vote talk3 poepke (Points 1))]

  |> When FinishVotingPeriod
  |> ThenExpect [
      FinishingDenied "Not all abstracts have been voted for by all organisers"]


[<Test>]
let ``Voting top x abstracts will be accepted, others will be rejected`` () =
  let talk1 = proposedTalk()
  let talk2 = proposedTalk()
  let talk3 = proposedTalk()

  Given [
    OrganizerRegistered heimeshoff
    OrganizerRegistered fellien
    OrganizerRegistered poepke
    NumberOfSlotsDecided 2

    CallForPapersOpened
    TalkWasProposed talk1
    TalkWasProposed talk2
    TalkWasProposed talk3
    CallForPapersClosed

    VotingWasIssued (vote talk3 heimeshoff (Points -2))
    VotingWasIssued (vote talk3 fellien (Points -2))
    VotingWasIssued (vote talk3 poepke (Points -2))
    VotingWasIssued (vote talk2 heimeshoff (Points 2))
    VotingWasIssued (vote talk2 fellien (Points 2))
    VotingWasIssued (vote talk2 poepke (Points 2))
    VotingWasIssued (vote talk1 heimeshoff (Points -1))
    VotingWasIssued (vote talk1 fellien (Points 1))
    VotingWasIssued (vote talk1 poepke (Points -1))]

  |> When FinishVotingPeriod
  |> ThenExpect [
      VotingPeriodWasFinished
      AbstractWasAccepted talk2.Id
      AbstractWasAccepted talk1.Id
      AbstractWasRejected talk3.Id ]

[<Test>]
let ``A veto rejects talks that would otherwise be accepted`` () =
  let talk1 = proposedTalk()
  let talk2 = proposedTalk()
  let talk3 = proposedTalk()

  Given [
    OrganizerRegistered heimeshoff
    OrganizerRegistered fellien
    OrganizerRegistered poepke
    NumberOfSlotsDecided 2

    CallForPapersOpened
    TalkWasProposed talk1
    TalkWasProposed talk2
    TalkWasProposed talk3
    CallForPapersClosed

    VotingWasIssued (vote talk3 heimeshoff (Points -2))
    VotingWasIssued (veto talk3 fellien)
    VotingWasIssued (vote talk3 poepke (Points -2))
    VotingWasIssued (vote talk2 heimeshoff (Points 2))
    VotingWasIssued (vote talk2 fellien (Points 2))
    VotingWasIssued (vote talk2 poepke (Points 2))
    VotingWasIssued (vote talk1 heimeshoff (Points -1))
    VotingWasIssued (vote talk1 fellien (Points 1))
    VotingWasIssued (vote talk1 poepke (Points -1))]

  |> When FinishVotingPeriod
  |> ThenExpect [
      VotingPeriodWasFinished
      AbstractWasRejected talk3.Id
      AbstractWasAccepted talk2.Id
      AbstractWasAccepted talk1.Id ]
