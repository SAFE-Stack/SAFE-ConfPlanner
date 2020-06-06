module RevokeVotingTest

open NUnit.Framework

open Domain.Commands
open Domain.Events
open Testbase


[<Test>]
let ``A given vote can be revoked`` () =
  let talk = proposedTalk()
  let voting = voteOne talk roman

  Given
    [
      OrganizerAddedToConference roman
      TalkWasProposed talk
      VotingWasIssued voting
    ]
  |> When (RevokeVoting voting)
  |> ThenExpect [ VotingWasRevoked voting ]


[<Test>]
let ``Voting can not be revoked when voting period is already finished`` () =
  let talk = proposedTalk()
  let voting = voteOne talk roman

  Given
    [
      OrganizerAddedToConference roman
      TalkWasProposed talk
      VotingWasIssued voting
      VotingPeriodWasFinished
    ]
  |> When (RevokeVoting voting)
  |> ThenExpect [ RevocationOfVotingWasDenied (voting,"Voting Period Already Finished") |> DomainError ]

[<Test>]
let ``Voting can not be revoked when not issued`` () =
  let talk = proposedTalk()
  let voting = voteOne talk roman

  Given
    [
      OrganizerAddedToConference roman
      TalkWasProposed <| proposedTalk()
    ]
  |> When (RevokeVoting voting)
  |> ThenExpect [ RevocationOfVotingWasDenied (voting,"Voting Not Issued") |> DomainError ]
