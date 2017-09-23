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
  |> ThenExpect [
      VotingPeriodWasFinished]

[<Test>]
let ``Cannot finish an already finished voting period`` () =
  let conference =
    conference
    |> withFinishedVotingPeriod
    |> withCallForPapersClosed

  Given conference
  |> When FinishVotingPeriod
  |> ThenExpect [
      FinishingDenied "Voting Period Already Finished"]

[<Test>]
let ``Cannot finish a voting period when call for papers is not closed`` () =
  let conference = conference |> withCallForPapersOpen
  Given conference
  |> When FinishVotingPeriod
  |> ThenExpect [FinishingDenied "Call For Papers Not Closed"]

[<Test>]
let ``Cannot finish a voting period when not all abstracts have votes from every organizer`` () =
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
      vote talk2 voter3
      vote talk1 voter1
      vote talk1 voter2
      vote talk1 voter3
    ]
  let conference =
    conference
    |> withCallForPapersClosed
    |> withOrganizers [voter1; voter2; voter3]
    |> withAbstracts [talk1; talk2; talk3]
    |> withVotings votings

  Given conference
  |> When FinishVotingPeriod
  |> ThenExpect [
      FinishingDenied "Not all abstracts have been voted for by all organisers"]

// [<Test>]
// let ``Voting top x abstracts will be accepted, others will be rejected`` () =
//   let talk1 = proposedTalk()
//   let talk2 = proposedTalk()
//   let talk3 = proposedTalk()
//   let voter1 = organizer()
//   let voter2 = organizer()
//   let voter3 = organizer()
//   let votings =
//     [
//       vote talk3 voter1
//       vote talk3 voter2
//       vote talk3 voter3
//       vote talk2 voter1
//       vote talk2 voter2
//       vote talk2 voter3
//       vote talk1 voter1
//       vote talk1 voter2
//       vote talk1 voter3
//     ]

//   let conference =
//     conference
//     |> withCallForPapersClosed
//     |> withVotingPeriodInProgress
//     |> withAvailableSlotsForTalks 2
//     |> withOrganizers [voter1; voter2; voter3]
//     |> withAbstracts [talk1; talk2; talk3]
//     |> withVotings votings

//   Given conference
//   |> When FinishVotingPeriod
//   |> ThenExpect [
//       VotingPeriodWasFinished
//       AbstractWasAccepted talk3.Id
//       AbstractWasAccepted talk2.Id
//       AbstractWasRejected talk1.Id ]

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
      vote talk2 voter3
      vote talk1 voter1
      vote talk1 voter2
      vote talk1 voter3
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

  Given conference
  |> When FinishVotingPeriod
  |> ThenExpect [
      VotingPeriodWasFinished
      AbstractWasRejected talk3.Id
      AbstractWasAccepted talk2.Id
      AbstractWasAccepted talk1.Id ]
