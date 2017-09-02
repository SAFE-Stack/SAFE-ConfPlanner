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
  let talk = proposedTalk()
  let voter = organizer()
  let voting = vote talk voter
  let conference =
    conference
    |> withVotingPeriodFinished
    |> withOrganizer voter
    |> withAbstract talk
    |> withVoting voting

  Given conference
  |> When (RevokeVoting voting)
  |> ShouldFailWith VotingPeriodAlreadyFinished

[<Test>]
let ``Can not revoke voting when organizer did not vote for abstract`` () =
  let talk = proposedTalk()
  let voter = organizer()
  let voting = vote talk voter
  let conference =
    conference
    |> withVotingPeriodInProgress
    |> withOrganizer voter
    |> withAbstract talk

  Given conference
  |> When (RevokeVoting voting)
  |> ShouldFailWith OrganizerDidNotVoteForAbstract

[<Test>]
let ``Can revoke voting when constraints are fulfilled`` () =
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
  |> When (RevokeVoting voting)
  |> ThenStateShouldBe (conference |> withVotings [])
  |> WithEvents [VotingWasRevoked voting]


