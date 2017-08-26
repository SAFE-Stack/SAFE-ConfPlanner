module RevokeVotingTest

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
let ``Can not revoke voting when voting period is already finished`` () =
  let proposedTalk = proposedTalk()
  let voter = organizer()
  let voting = Voting.Vote (proposedTalk.Id,voter.Id)
  let conference =
    conference
    |> withVotingPeriodFinished
    |> withOrganizer voter
    |> withAbstract proposedTalk
    |> withVoting voting

  Given conference
  |> When (RevokeVoting voting)
  |> ShouldFailWith VotingPeriodAlreadyFinished

[<Test>]
let ``Can not revoke voting when organizer did not vote for abstract`` () =
  let proposedTalk = proposedTalk()
  let voter = organizer()
  let voting = Voting.Vote (proposedTalk.Id,voter.Id)
  let conference =
    conference
    |> withVotingPeriodInProgress
    |> withOrganizer voter
    |> withAbstract proposedTalk

  Given conference
  |> When (RevokeVoting voting)
  |> ShouldFailWith OrganizerDidNotVoteForAbstract

[<Test>]
let ``Can revoke voting when constraints are fulfilled`` () =
  let proposedTalk = proposedTalk()
  let organizer = organizer()
  let voting = Voting.Vote (proposedTalk.Id,organizer.Id)
  let conference =
    conference
    |> withVotingPeriodInProgress
    |> withOrganizer organizer
    |> withAbstract proposedTalk
    |> withVoting voting

  Given conference
  |> When (RevokeVoting voting)
  |> ThenStateShouldBe { conference with VotingResults = [] }
  |> WithEvents [VotingWasRevoked voting]


