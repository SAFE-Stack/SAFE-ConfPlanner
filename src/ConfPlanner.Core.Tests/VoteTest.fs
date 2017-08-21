module VoteTest

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
let ``Can not vote when voting period is already finished`` () =
  let proposedAbstract = proposedAbstract()
  let voter = organizer1
  let voting = Voting.Vote (proposedAbstract,voter.Id)
  let conference =
    conference
    |> withVotingPeriodFinished
    |> withOrganizer voter
    |> withProposedAbstract proposedAbstract

  Given conference
  |> When (Vote voting)
  |> ShouldFailWith VotingPeriodAlreadyFinished

[<Test>]
let ``Can not vote when organizer already voted for abstract`` () =
  let proposedAbstract = proposedAbstract()
  let voter = organizer1
  let voting = Voting.Vote (proposedAbstract,voter.Id)
  let conference =
    conference
    |> withVotingPeriodInProgress
    |> withOrganizer voter
    |> withProposedAbstract proposedAbstract
    |> withVoting voting

  Given conference
  |> When (Vote voting)
  |> ShouldFailWith VotingAlreadyIssued

[<Test>]
let ``Can not vote when voter is not organizer of conference`` () =
  let proposedAbstract = proposedAbstract()
  let voter = organizer1
  let voting = Voting.Vote (proposedAbstract,voter.Id)
  let conference =
    conference
    |> withVotingPeriodInProgress
    |> withProposedAbstract proposedAbstract

  Given conference
  |> When (Vote voting)
  |> ShouldFailWith VoterIsNotAnOrganizer

[<Test>]
let ``Can not vote when voter already voted max number of times`` () =
  let proposedAbstract1 = proposedAbstract()
  let proposedAbstract2 = proposedAbstract()
  let voter = organizer1
  let voting = Voting.Vote (proposedAbstract2,voter.Id)
  let conference =
    conference
    |> withVotingPeriodInProgress
    |> withProposedAbstract proposedAbstract1
    |> withOrganizer voter
    |> withMaxVotesPerOrganizer 1
    |> withVoting (Voting.Vote (proposedAbstract1,voter.Id))

  Given conference
  |> When (Vote voting)
  |> ShouldFailWith MaxNumberOfVotesExceeded

[<Test>]
let ``Can vote when constraints are fulfilled`` () =
  let proposedAbstract = proposedAbstract()
  let organizer = organizer1
  let voting = Voting.Vote (proposedAbstract,organizer.Id)
  let conference =
    conference
    |> withVotingPeriodInProgress
    |> withOrganizer organizer
    |> withProposedAbstract proposedAbstract

  Given conference
  |> When (Vote voting)
  |> ThenStateShouldBe { conference with VotingResults = voting :: conference.VotingResults }
  |> WithEvents [VotingWasIssued voting]


