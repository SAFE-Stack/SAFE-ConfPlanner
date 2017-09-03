module FinishVotingPeriodTest

open NUnit.Framework

open ConfPlannerTestsDSL
open Domain
open System
open Commands
open Events
open States
open TestData

// [<Test>]
// let ``Can finish voting period`` () =
//   let conference = conference |> withCallForPapersClosed
//   Given conference
//   |> When FinishVotingPeriod
//   |> WithEvents [VotingPeriodWasFinished]

// [<Test>]
// let ``Cannot finish an already finished voting period`` () =
//   let conference =
//     conference
//     |> withFinishedVotingPeriod
//     |> withCallForPapersClosed

//   Given conference
//   |> When FinishVotingPeriod
//   |> ShouldFailWith VotingPeriodAlreadyFinished


// let ``Cannot finish an voting period when call of papers is not closed`` () =
//   let conference = conference |> withCallForPapersOpen
//   Given conference
//   |> When FinishVotingPeriod
//   |> ShouldFailWith CallForPapersNotClosed


// // wie soll bestimmt werden, welche Abstracts accepted oder rejected werden?
// // einfach nach votes sortieren, die mit veto raushauen und die top x nehmen?
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
//       vote talk1 voter2
//     ]

//   let conference =
//     conference
//     |> withCallForPapersClosed
//     |> withVotingPeriodInProgress
//     |> withAvailableSlotsForTalks 2
//     |> withOrganizers [voter1; voter2; voter3]
//     |> withAbstracts [talk1; talk2; talk3]
//     |> withVotings votings

//   let expectedState =
//     conference
//     |> withFinishedVotingPeriod
//     |> withAbstracts [rejected talk1; accepted talk2; accepted talk3]

//   let expectedEvents =
//     [
//         VotingPeriodWasFinished
//         AbstractWasAccepted talk3.Id
//         AbstractWasAccepted talk2.Id
//         AbstractWasRejected talk1.Id
//     ]

//   Given conference
//   |> When FinishVotingPeriod
//   |> WithEvents expectedEvents


// [<Test>]
// let ``A veto rejects talks that would otherwise be accepted`` () =
//   let talk1 = proposedTalk()
//   let talk2 = proposedTalk()
//   let talk3 = proposedTalk()
//   let voter1 = organizer()
//   let voter2 = organizer()
//   let voter3 = organizer()
//   let votings =
//     [
//       vote talk3 voter2
//       vote talk3 voter3
//       vote talk2 voter1
//       vote talk2 voter2
//       vote talk1 voter2
//       veto talk3 voter1
//     ]

//   let conference =
//     conference
//     |> withCallForPapersClosed
//     |> withVotingPeriodInProgress
//     |> withAvailableSlotsForTalks 2
//     |> withOrganizers [voter1; voter2; voter3]
//     |> withAbstracts [talk1; talk2; talk3]
//     |> withVotings votings

//   let expectedState =
//     conference
//     |> withFinishedVotingPeriod
//     |> withAbstracts
//         [
//           accepted talk1
//           accepted talk2
//           rejected talk3
//         ]

//   let expectedEvents =
//     [
//         VotingPeriodWasFinished
//         AbstractWasRejected talk3.Id
//         AbstractWasAccepted talk2.Id
//         AbstractWasAccepted talk1.Id
//     ]

//   Given conference
//   |> When FinishVotingPeriod
//   |> WithEvents expectedEvents
