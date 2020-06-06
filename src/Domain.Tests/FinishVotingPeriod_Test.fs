module FinishVotingPeriodTest

open System
open NUnit.Framework

open Domain.Model
open Domain.Commands
open Domain.Events
open Testbase

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
  |> ThenExpect [FinishingDenied "Voting Period Already Finished" |> DomainError]


[<Test>]
let ``Cannot finish a voting period when call for papers is not closed`` () =
  Given [
    CallForPapersOpened]
  |> When FinishVotingPeriod
  |> ThenExpect [FinishingDenied "Call For Papers Not Closed" |> DomainError]


[<Test>]
let ``Cannot finish a voting period when not all abstracts have votes from every organizer`` () =
  let talk1 = proposedTalk()
  let talk2 = proposedTalk()
  let talk3 = proposedTalk()

  Given [
    OrganizerAddedToConference roman
    OrganizerAddedToConference marco

    CallForPapersOpened
    TalkWasProposed talk1
    TalkWasProposed talk2
    TalkWasProposed talk3
    CallForPapersClosed

    VotingWasIssued (voteOne talk1 roman)
    VotingWasIssued (voteOne talk2 roman)
    VotingWasIssued (voteOne talk1 marco)
    VotingWasIssued (voteOne talk2 marco)
    VotingWasIssued (voteOne talk3 marco)]

  |> When FinishVotingPeriod
  |> ThenExpect [ FinishingDenied "Not all abstracts have been voted for by all organisers" |> DomainError ]


[<Test>]
let ``Voting top x abstracts will be accepted, others will be rejected`` () =
  let talk1 = proposedTalk()
  let talk2 = proposedTalk()
  let talk3 = proposedTalk()

  Given [
    OrganizerAddedToConference roman
    OrganizerAddedToConference marco
    NumberOfSlotsDecided 2

    CallForPapersOpened
    TalkWasProposed talk1
    TalkWasProposed talk2
    TalkWasProposed talk3
    CallForPapersClosed

    VotingWasIssued (voteZero talk3 roman)
    VotingWasIssued (voteZero talk3 marco)
    VotingWasIssued (voteOne talk2 roman)
    VotingWasIssued (voteOne talk2 marco)
    VotingWasIssued (voteOne talk1 roman)
    VotingWasIssued (voteOne talk1 marco)]

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
    OrganizerAddedToConference roman
    OrganizerAddedToConference marco
    NumberOfSlotsDecided 2

    CallForPapersOpened
    TalkWasProposed talk1
    TalkWasProposed talk2
    TalkWasProposed talk3
    CallForPapersClosed

    VotingWasIssued (voteTwo talk3 roman)
    VotingWasIssued (veto talk3 roman)
    VotingWasIssued (voteTwo talk3 marco)
    VotingWasIssued (voteOne talk2 roman)
    VotingWasIssued (voteOne talk2 roman)
    VotingWasIssued (voteOne talk2 marco)
    VotingWasIssued (voteZero talk1 roman)
    VotingWasIssued (voteZero talk1 roman)
    VotingWasIssued (voteZero talk1 marco)]

  |> When FinishVotingPeriod
  |> ThenExpect [
      VotingPeriodWasFinished
      AbstractWasRejected talk3.Id
      AbstractWasAccepted talk2.Id
      AbstractWasAccepted talk1.Id ]
