module VoteTest

open NUnit.Framework

open Commands
open Events
open Testbase


[<Test>]
let ``Can not vote when voting period is already finished`` () =
  let talk = proposedTalk()
  let vote = voteOne talk heimeshoff

  Given [
    OrganizerAddedToConference heimeshoff
    TalkWasProposed talk
    VotingPeriodWasFinished]
  |> When (Vote vote)
  |> ThenExpect [ VotingDenied "Voting Period Already Finished" |> Error ]


[<Test>]
let ``Can not vote when voter is not organizer of conference`` () =
  let talk = proposedTalk()
  let vote = voteOne talk heimeshoff

  Given [
    TalkWasProposed talk]
  |> When (Vote vote)
  |> ThenExpect [ VotingDenied "Voter Is Not An Organizer" |> Error ]


[<Test>]
let ``Can vote when constraints are fulfilled`` () =
  let talk = proposedTalk()
  let vote = voteOne talk heimeshoff

  Given [
    OrganizerAddedToConference heimeshoff
    TalkWasProposed talk]
  |> When (Vote vote)
  |> ThenExpect [ VotingWasIssued vote ]


[<Test>]
let ``Voter can change previous vote for an abstract`` () =
  let talk = proposedTalk()
  let vote = voteOne talk heimeshoff

  Given
    [
      OrganizerAddedToConference heimeshoff
      TalkWasProposed talk
      VotingWasIssued vote
    ]
  |> When (Vote vote)
  |> ThenExpect [ VotingWasIssued vote ]


[<Test>]
let ``Can issue a veto when constraints are fulfilled`` () =
  let talk = proposedTalk()
  let veto= veto talk heimeshoff

  Given [
    OrganizerAddedToConference heimeshoff
    TalkWasProposed talk]
  |> When (Vote veto)
  |> ThenExpect [ VotingWasIssued veto ]
