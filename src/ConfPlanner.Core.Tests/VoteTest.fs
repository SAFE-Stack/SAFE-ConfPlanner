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
  let proposedTalk = proposedTalk()
  let voter = organizer()
  let voting = Voting.Vote (proposedTalk.Id,voter.Id)
  let conference =
    conference
    |> withVotingPeriodFinished
    |> withOrganizer voter
    |> withAbstract proposedTalk

  Given conference
  |> When (Vote voting)
  |> ShouldFailWith VotingPeriodAlreadyFinished

[<Test>]
let ``Can not vote when organizer already voted for abstract`` () =
  let proposedTalk = proposedTalk()
  let voter = organizer()
  let voting = Voting.Vote (proposedTalk.Id,voter.Id)
  let conference =
    conference
    |> withVotingPeriodInProgress
    |> withOrganizer voter
    |> withAbstract proposedTalk
    |> withVoting voting

  Given conference
  |> When (Vote voting)
  |> ShouldFailWith VotingAlreadyIssued

[<Test>]
let ``Can not vote when voter is not organizer of conference`` () =
  let proposedTalk = proposedTalk()
  let voter = organizer()
  let voting = Voting.Vote (proposedTalk.Id,voter.Id)
  let conference =
    conference
    |> withVotingPeriodInProgress
    |> withAbstract proposedTalk

  Given conference
  |> When (Vote voting)
  |> ShouldFailWith VoterIsNotAnOrganizer

[<Test>]
let ``Can not vote when voter already voted max number of times`` () =
  let proposedTalk1 = proposedTalk()
  let proposedTalk2 = proposedTalk()
  let voter = organizer()
  let voting = Voting.Vote (proposedTalk2.Id,voter.Id)
  let conference =
    conference
    |> withVotingPeriodInProgress
    |> withAbstract proposedTalk1
    |> withOrganizer voter
    |> withMaxVotesPerOrganizer 1
    |> withVoting (Voting.Vote (proposedTalk1.Id,voter.Id))

  Given conference
  |> When (Vote voting)
  |> ShouldFailWith MaxNumberOfVotesExceeded

[<Test>]
let ``Can not issue veto when voter already vetoed max number of times`` () =
  let proposedTalk1 = proposedTalk()
  let proposedTalk2 = proposedTalk()
  let voter = organizer()
  let veto = Voting.Veto (proposedTalk2.Id,voter.Id)
  let conference =
    conference
    |> withVotingPeriodInProgress
    |> withAbstract proposedTalk1
    |> withOrganizer voter
    |> withMaxVetosPerOrganizer 1
    |> withVoting (Voting.Veto (proposedTalk1.Id,voter.Id))

  Given conference
  |> When (Vote veto)
  |> ShouldFailWith MaxNumberOfVetosExceeded

[<Test>]
let ``Can vote when constraints are fulfilled`` () =
  let proposedTalk = proposedTalk()
  let organizer = organizer()
  let voting = Voting.Vote (proposedTalk.Id,organizer.Id)
  let conference =
    conference
    |> withVotingPeriodInProgress
    |> withOrganizer organizer
    |> withAbstract proposedTalk

  Given conference
  |> When (Vote voting)
  |> ThenStateShouldBe { conference with VotingResults = voting :: conference.VotingResults }
  |> WithEvents [VotingWasIssued voting]

[<Test>]
let ``Can issue a veto when constraints are fulfilled`` () =
  let proposedTalk = proposedTalk()
  let organizer = organizer()
  let veto = Voting.Veto (proposedTalk.Id,organizer.Id)
  let conference =
    conference
    |> withVotingPeriodInProgress
    |> withOrganizer organizer
    |> withAbstract proposedTalk

  Given conference
  |> When (Vote veto)
  |> ThenStateShouldBe { conference with VotingResults = veto :: conference.VotingResults }
  |> WithEvents [VotingWasIssued veto]


