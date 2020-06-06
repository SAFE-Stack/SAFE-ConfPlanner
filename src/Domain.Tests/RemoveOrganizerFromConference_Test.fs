module RemoveOrganizerFromConferenceTest

open NUnit.Framework

open Domain.Commands
open Domain.Events
open Testbase

[<Test>]
let ``Organizer can be removed from a conference`` () =
  Given [ OrganizerAddedToConference roman ]
  |> When (RemoveOrganizerFromConference roman)
  |> ThenExpect [ OrganizerRemovedFromConference roman ]

[<Test>]
let ``When an Organizer is removed all its votings are revoked`` () =
  let talk1 = proposedTalk()
  let talk2 = proposedTalk()
  let talk3 = proposedTalk()

  let vote1 = voteTwo talk1 roman
  let vote2 = voteOne talk2 roman
  let vote3 = veto talk3 roman

  Given
    [
      OrganizerAddedToConference roman

      TalkWasProposed talk1
      TalkWasProposed talk2
      TalkWasProposed talk3

      VotingWasIssued vote1
      VotingWasIssued vote2
      VotingWasIssued vote3
    ]
  |> When (RemoveOrganizerFromConference roman)
  |> ThenExpect
      [
        OrganizerRemovedFromConference roman
        VotingWasRevoked vote1
        VotingWasRevoked vote2
        VotingWasRevoked vote3
      ]

[<Test>]
let ``Organizer can not be remove if not added`` () =
  Given []
  |> When (RemoveOrganizerFromConference roman)
  |> ThenExpect [ OrganizerWasNotAddedToConference roman |> DomainError ]
