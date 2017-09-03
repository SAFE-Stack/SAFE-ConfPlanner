module FinishVotingPeriodTest

open NUnit.Framework

open ConfPlannerTestsDSL
open Model
open System
open Commands
open Events
open States
open TestData

[<Test>]
let ``Can finish voting period`` () =
  let conference =
    conference
    |> withCallForPapersClosed
  Given conference
  |> When FinishVotingPeriod
  |> ThenExpect [VotingPeriodWasFinished]

[<Test>]
let ``Cannot finish an already finished voting period`` () =
  let conference =
    conference
    |> withFinishedVotingPeriod
    |> withCallForPapersClosed

  Given conference
  |> When FinishVotingPeriod
  |> ThenExpect [FinishingDenied "Voting Period Already Finished"]

let ``Cannot finish a voting period when call of papers is not closed`` () =
  let conference = conference |> withCallForPapersOpen
  Given conference
  |> When FinishVotingPeriod
  |> ThenExpect [FinishingDenied "Call For Papers Not Closed"]

[<Test>]
let ``Voting top x abstracts will be accepted, others will be rejected`` () =
  let talk1 = proposedTalk()
  let talk2 = proposedTalk()
  let talk3 = proposedTalk()
  let voter1 = organizer()
  let voter2 = organizer()
  let voter3 = organizer()
  let votings =
    [
      vote talk3 voter1
      vote talk3 voter2
      vote talk3 voter3
      vote talk2 voter1
      vote talk2 voter2
      vote talk1 voter2
    ]

  let conference =
    conference
    |> withCallForPapersClosed
    |> withVotingPeriodInProgress
    |> withAvailableSlotsForTalks 2
    |> withOrganizers [voter1; voter2; voter3]
    |> withAbstracts [talk1; talk2; talk3]
    |> withVotings votings

  let expectedEvents =
    [
        VotingPeriodWasFinished
        AbstractWasAccepted talk3.Id
        AbstractWasAccepted talk2.Id
        AbstractWasRejected talk1.Id
    ]

  Given conference
  |> When FinishVotingPeriod
  |> ThenExpect expectedEvents

[<Test>]
let ``A veto rejects talks that would otherwise be accepted`` () =
  let talk1 = proposedTalk()
  let talk2 = proposedTalk()
  let talk3 = proposedTalk()
  let voter1 = organizer()
  let voter2 = organizer()
  let voter3 = organizer()
  let votings =
    [
      vote talk3 voter2
      vote talk3 voter3
      vote talk2 voter1
      vote talk2 voter2
      vote talk1 voter2
      veto talk3 voter1
    ]

  let conference =
    conference
    |> withCallForPapersClosed
    |> withVotingPeriodInProgress
    |> withAvailableSlotsForTalks 2
    |> withOrganizers [voter1; voter2; voter3]
    |> withAbstracts [talk1; talk2; talk3]
    |> withVotings votings

  let expectedEvents =
    [
        VotingPeriodWasFinished
        AbstractWasRejected talk3.Id
        AbstractWasAccepted talk2.Id
        AbstractWasAccepted talk1.Id
    ]

  Given conference
  |> When FinishVotingPeriod
  |> ThenExpect expectedEvents
