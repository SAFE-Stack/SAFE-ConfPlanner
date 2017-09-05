module VoteTest

open NUnit.Framework

open ConfPlannerTestsDSL
open Model
open System
open Commands
open Events
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
  |> ThenExpect [VotingDenied "Voting Period Already Finished"]

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
  |> ThenExpect [VotingDenied "Voter Is Not An Organizer"]

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
  |> ThenExpect [VotingWasIssued voting]

[<Test>]
let ``Voter can change previous vote for an abstract`` () =
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
  |> ThenExpect [VotingWasIssued voting]

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
  |> ThenExpect [VotingWasIssued veto]
