module FinishVotingPeriodTest

open NUnit.Framework

open ConfPlannerTestsDSL
open Domain
open System
open Commands
open Events
open Errors
open States
open TestData

[<Test>]
let ``Can finish voting period`` () =
  let conference = conference |> withCallForPapersClosed
  Given conference
  |> When FinishVotingPeriod
  |> ThenStateShouldBe {conference with VotingPeriod = Finished}
  |> WithEvents [VotingPeriodWasFinished]

[<Test>]
let ``Cannot finish an already finished voting period`` () =
  let conference =
    conference
    |> withFinishedVotingPeriod
    |> withCallForPapersClosed

  Given conference
  |> When FinishVotingPeriod
  |> ShouldFailWith VotingPeriodAlreadyFinished


let ``Cannot finish an voting period when call of papers is not closed`` () =
  let conference = conference |> withCallForPapersOpen
  Given conference
  |> When FinishVotingPeriod
  |> ShouldFailWith CallForPapersNotClosed


// wie soll bestimmt werden, welche Abstracts accepted oder rejected werden?
// einfach nach votes sortieren, die mit veto raushauen und die top x nehmen?
[<Test>]
let ``Voting top x abstracts will be accepted, others will be rejected`` () =
  let proposedTalk1 = proposedTalk()
  let proposedTalk2 = proposedTalk()
  let proposedTalk3 = proposedTalk()
  let voter1 = organizer()
  let voter2 = organizer()
  let voter3 = organizer()
  let votings =
    [
      Voting.Vote (proposedTalk3.Id,voter1.Id)
      Voting.Vote (proposedTalk3.Id,voter2.Id)
      Voting.Vote (proposedTalk3.Id,voter3.Id)
      Voting.Vote (proposedTalk2.Id,voter1.Id)
      Voting.Vote (proposedTalk2.Id,voter2.Id)
      Voting.Vote (proposedTalk1.Id,voter1.Id)
    ]

  let conference =
    conference
    |> withCallForPapersClosed
    |> withVotingPeriodInProgress
    |> withAvailableSlotsForTalks 2
    |> withOrganizers [voter1; voter2; voter3]
    |> withAbstracts [proposedTalk1; proposedTalk2; proposedTalk3]
    |> withVotings votings

  let events =
    [
        AbstractWasAccepted proposedTalk3.Id
        AbstractWasAccepted proposedTalk2.Id
        VotingPeriodWasFinished
        // AbstractWasRejected proposedTalk1.Id
      ]

  printfn "expected %A" events
  Given conference
  |> When FinishVotingPeriod
  |> ThenIgnoreState conference
  |> WithEvents events





