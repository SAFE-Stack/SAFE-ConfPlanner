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
  let talk = proposedTalk()
  let voter = organizer()
  let voting = Voting.Vote (talk.Id,voter.Id)
  let conference =
    conference
    |> withVotingPeriodFinished
    |> withOrganizer voter
    |> withAbstract talk

  Given conference
  |> When (Vote voting)
  |> ShouldFailWith VotingPeriodAlreadyFinished

[<Test>]
let ``Can not vote when organizer already voted for abstract`` () =
  let talk = proposedTalk()
  let voter = organizer()
  let voting = vote talk voter
  let conference =
    conference
    |> withVotingPeriodInProgress
    |> withOrganizer voter
    |> withAbstract talk
    |> withVoting voting

  Given conference
  |> When (Vote voting)
  |> ShouldFailWith VotingAlreadyIssued

[<Test>]
let ``Can not vote when voter is not organizer of conference`` () =
  let talk = proposedTalk()
  let voter = organizer()
  let voting = vote talk voter
  let conference =
    conference
    |> withVotingPeriodInProgress
    |> withAbstract talk

  Given conference
  |> When (Vote voting)
  |> ShouldFailWith VoterIsNotAnOrganizer

[<Test>]
let ``Can not vote when voter already voted max number of times`` () =
  let talk1 = proposedTalk()
  let talk2 = proposedTalk()
  let voter = organizer()
  let voting = vote talk1 voter
  let conference =
    conference
    |> withVotingPeriodInProgress
    |> withAbstract talk1
    |> withOrganizer voter
    |> withMaxVotesPerOrganizer 1
    |> withVoting (vote talk2 voter)

  Given conference
  |> When (Vote voting)
  |> ShouldFailWith MaxNumberOfVotesExceeded

[<Test>]
let ``Can not issue veto when voter already vetoed max number of times`` () =
  let talk1 = proposedTalk()
  let talk2 = proposedTalk()
  let voter = organizer()
  let conference =
    conference
    |> withVotingPeriodInProgress
    |> withAbstract talk1
    |> withOrganizer voter
    |> withMaxVetosPerOrganizer 1
    |> withVoting (veto talk1 voter)

  Given conference
  |> When (Vote (veto talk2 voter))
  |> ShouldFailWith MaxNumberOfVetosExceeded

[<Test>]
let ``Can vote when constraints are fulfilled`` () =
  let talk = proposedTalk()
  let voter = organizer()
  let voting = vote talk voter
  let conference =
    conference
    |> withVotingPeriodInProgress
    |> withOrganizer voter
    |> withAbstract talk

  Given conference
  |> When (Vote voting)
  |> ThenStateShouldBe (conference |> withVoting voting)
  |> WithEvents [VotingWasIssued voting]

[<Test>]
let ``Can issue a veto when constraints are fulfilled`` () =
  let talk = proposedTalk()
  let voter = organizer()
  let veto = vote talk voter
  let conference =
    conference
    |> withVotingPeriodInProgress
    |> withOrganizer voter
    |> withAbstract talk

  Given conference
  |> When (Vote veto)
  |> ThenStateShouldBe (conference |> withVoting veto)
  |> WithEvents [VotingWasIssued veto]


