module FinishVotingPeriodTest

open System
open NUnit.Framework

open Model
open Commands
open Events
open Testbase

// Scenario
let fellien = { Firstname = "Janek";  Lastname = "Fellien"; Id = OrganizerId <| Guid.NewGuid() }
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
  |> ThenExpect [FinishingDenied "Voting Period Already Finished" |> Error]


[<Test>]
let ``Cannot finish a voting period when call for papers is not closed`` () =
  Given [
    CallForPapersOpened]
  |> When FinishVotingPeriod
  |> ThenExpect [FinishingDenied "Call For Papers Not Closed" |> Error]


[<Test>]
let ``Cannot finish a voting period when not all abstracts have votes from every organizer`` () =
  let talk1 = proposedTalk()
  let talk2 = proposedTalk()
  let talk3 = proposedTalk()

  Given [
    OrganizerAddedToConference heimeshoff
    OrganizerAddedToConference fellien
    OrganizerAddedToConference poepke

    CallForPapersOpened
    TalkWasProposed talk1
    TalkWasProposed talk2
    TalkWasProposed talk3
    CallForPapersClosed

    VotingWasIssued (voteOne talk1 heimeshoff)
    VotingWasIssued (voteOne talk2 heimeshoff)
    VotingWasIssued (voteOne talk3 heimeshoff)
    VotingWasIssued (voteOne talk1 fellien)
    VotingWasIssued (voteOne talk2 fellien)
    VotingWasIssued (voteOne talk1 poepke)
    VotingWasIssued (voteOne talk2 poepke)
    VotingWasIssued (voteOne talk3 poepke)]

  |> When FinishVotingPeriod
  |> ThenExpect [ FinishingDenied "Not all abstracts have been voted for by all organisers" |> Error ]


[<Test>]
let ``Voting top x abstracts will be accepted, others will be rejected`` () =
  let talk1 = proposedTalk()
  let talk2 = proposedTalk()
  let talk3 = proposedTalk()

  Given [
    OrganizerAddedToConference heimeshoff
    OrganizerAddedToConference fellien
    OrganizerAddedToConference poepke
    NumberOfSlotsDecided 2

    CallForPapersOpened
    TalkWasProposed talk1
    TalkWasProposed talk2
    TalkWasProposed talk3
    CallForPapersClosed

    VotingWasIssued (voteZero talk3 heimeshoff)
    VotingWasIssued (voteZero talk3 fellien)
    VotingWasIssued (voteZero talk3 poepke)
    VotingWasIssued (voteOne talk2 heimeshoff)
    VotingWasIssued (voteOne talk2 fellien)
    VotingWasIssued (voteOne talk2 poepke)
    VotingWasIssued (voteZero talk1 heimeshoff)
    VotingWasIssued (voteOne talk1 fellien)
    VotingWasIssued (voteOne talk1 poepke)]

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
    OrganizerAddedToConference heimeshoff
    OrganizerAddedToConference fellien
    OrganizerAddedToConference poepke
    NumberOfSlotsDecided 2

    CallForPapersOpened
    TalkWasProposed talk1
    TalkWasProposed talk2
    TalkWasProposed talk3
    CallForPapersClosed

    VotingWasIssued (voteTwo talk3 heimeshoff)
    VotingWasIssued (veto talk3 fellien)
    VotingWasIssued (voteTwo talk3 poepke)
    VotingWasIssued (voteOne talk2 heimeshoff)
    VotingWasIssued (voteOne talk2 fellien)
    VotingWasIssued (voteOne talk2 poepke)
    VotingWasIssued (voteZero talk1 heimeshoff)
    VotingWasIssued (voteZero talk1 fellien)
    VotingWasIssued (voteZero talk1 poepke)]

  |> When FinishVotingPeriod
  |> ThenExpect [
      VotingPeriodWasFinished
      AbstractWasRejected talk3.Id
      AbstractWasAccepted talk2.Id
      AbstractWasAccepted talk1.Id ]
