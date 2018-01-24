module FinishVotingPeriodTest

open NUnit.Framework

open Domain.Commands
open Domain.Events
open Testbase

[<Test>]
let ``Can finish voting period`` () =
  Given
    [
      CallForPapersOpened
      CallForPapersClosed
    ]
  |> When FinishVotingPeriod
  |> ThenExpect [VotingPeriodWasFinished]


[<Test>]
let ``Cannot finish an already finished voting period`` () =
  Given
    [
      CallForPapersOpened
      CallForPapersClosed
      VotingPeriodWasFinished
    ]
  |> When FinishVotingPeriod
  |> ThenExpect [FinishingDenied "Voting Period Already Finished" |> Error]


[<Test>]
let ``Cannot finish a voting period when call for papers is not closed`` () =
  Given [CallForPapersOpened]
  |> When FinishVotingPeriod
  |> ThenExpect [FinishingDenied "Call For Papers Not Closed" |> Error]


[<Test>]
let ``Cannot finish a voting period when not all abstracts have votes from every organizer`` () =
  let talk1 = proposedTalk()
  let talk2 = proposedTalk()
  let talk3 = proposedTalk()

  Given [
    OrganizerAddedToConference heimeshoff.Id
    OrganizerAddedToConference fellien.Id
    OrganizerAddedToConference poepke.Id

    CallForPapersOpened
    TalkWasProposed talk1
    TalkWasProposed talk2
    TalkWasProposed talk3
    CallForPapersClosed

    VotingWasIssued (voteOne talk1 heimeshoff.Id)
    VotingWasIssued (voteOne talk2 heimeshoff.Id)
    VotingWasIssued (voteOne talk3 heimeshoff.Id)
    VotingWasIssued (voteOne talk1 fellien.Id)
    VotingWasIssued (voteOne talk2 fellien.Id)
    VotingWasIssued (voteOne talk1 poepke.Id)
    VotingWasIssued (voteOne talk2 poepke.Id)
    VotingWasIssued (voteOne talk3 poepke.Id)]

  |> When FinishVotingPeriod
  |> ThenExpect [ FinishingDenied "Not all abstracts have been voted for by all organisers" |> Error ]


[<Test>]
let ``Voting top x abstracts will be accepted, others will be rejected`` () =
  let talk1 = proposedTalk()
  let talk2 = proposedTalk()
  let talk3 = proposedTalk()

  Given [
    OrganizerAddedToConference heimeshoff.Id
    OrganizerAddedToConference fellien.Id
    OrganizerAddedToConference poepke.Id
    NumberOfSlotsDecided 2

    CallForPapersOpened
    TalkWasProposed talk1
    TalkWasProposed talk2
    TalkWasProposed talk3
    CallForPapersClosed

    VotingWasIssued (voteZero talk3 heimeshoff.Id)
    VotingWasIssued (voteZero talk3 fellien.Id)
    VotingWasIssued (voteZero talk3 poepke.Id)
    VotingWasIssued (voteOne talk2 heimeshoff.Id)
    VotingWasIssued (voteOne talk2 fellien.Id)
    VotingWasIssued (voteOne talk2 poepke.Id)
    VotingWasIssued (voteZero talk1 heimeshoff.Id)
    VotingWasIssued (voteOne talk1 fellien.Id)
    VotingWasIssued (voteOne talk1 poepke.Id)]

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
    OrganizerAddedToConference heimeshoff.Id
    OrganizerAddedToConference fellien.Id
    OrganizerAddedToConference poepke.Id
    NumberOfSlotsDecided 2

    CallForPapersOpened
    TalkWasProposed talk1
    TalkWasProposed talk2
    TalkWasProposed talk3
    CallForPapersClosed

    VotingWasIssued (voteTwo talk3 heimeshoff.Id)
    VotingWasIssued (veto talk3 fellien.Id)
    VotingWasIssued (voteTwo talk3 poepke.Id)
    VotingWasIssued (voteOne talk2 heimeshoff.Id)
    VotingWasIssued (voteOne talk2 fellien.Id)
    VotingWasIssued (voteOne talk2 poepke.Id)
    VotingWasIssued (voteZero talk1 heimeshoff.Id)
    VotingWasIssued (voteZero talk1 fellien.Id)
    VotingWasIssued (voteZero talk1 poepke.Id)]

  |> When FinishVotingPeriod
  |> ThenExpect [
      VotingPeriodWasFinished
      AbstractWasRejected talk3.Id
      AbstractWasAccepted talk2.Id
      AbstractWasAccepted talk1.Id ]
