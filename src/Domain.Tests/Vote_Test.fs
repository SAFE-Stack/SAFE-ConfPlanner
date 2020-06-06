module VoteTest

open NUnit.Framework

open Domain.Commands
open Domain.Events
open Testbase


[<Test>]
let ``Can not vote when voting period is already finished`` () =
  let talk = proposedTalk()
  let vote = voteOne talk roman

  Given [
    OrganizerAddedToConference roman
    TalkWasProposed talk
    VotingPeriodWasFinished]
  |> When (Vote vote)
  |> ThenExpect [ VotingDenied "Voting Period Already Finished" |> DomainError ]


[<Test>]
let ``Can not vote when voter is not organizer of conference`` () =
  let talk = proposedTalk()
  let vote = voteOne talk roman

  Given [
    TalkWasProposed talk]
  |> When (Vote vote)
  |> ThenExpect [ VotingDenied "Voter Is Not An Organizer" |> DomainError ]


[<Test>]
let ``Can vote when constraints are fulfilled`` () =
  let talk = proposedTalk()
  let vote = voteOne talk roman

  Given [
    OrganizerAddedToConference roman
    TalkWasProposed talk]
  |> When (Vote vote)
  |> ThenExpect [ VotingWasIssued vote ]


[<Test>]
let ``Voter can change previous vote for an abstract`` () =
  let talk = proposedTalk()
  let vote = voteOne talk roman

  Given
    [
      OrganizerAddedToConference roman
      TalkWasProposed talk
      VotingWasIssued vote
    ]
  |> When (Vote vote)
  |> ThenExpect [ VotingWasIssued vote ]


[<Test>]
let ``Can issue a veto when constraints are fulfilled`` () =
  let talk = proposedTalk()
  let veto= veto talk roman

  Given [
    OrganizerAddedToConference roman
    TalkWasProposed talk]
  |> When (Vote veto)
  |> ThenExpect [ VotingWasIssued veto ]
