module RemoveOrganizerFromConferenceTest

open NUnit.Framework

open Domain.Commands
open Domain.Events
open Testbase

[<Test>]
let ``Organizer can be removed from a conference`` () =
  Given [ OrganizerAddedToConference heimeshoff.Id ]
  |> When (RemoveOrganizerFromConference heimeshoff.Id)
  |> ThenExpect [ OrganizerRemovedFromConference heimeshoff.Id ]

[<Test>]
let ``When an Organizer is removed all its votings are revoked`` () =
  let talk1 = proposedTalk()
  let talk2 = proposedTalk()
  let talk3 = proposedTalk()

  let vote1 = voteTwo talk1 heimeshoff.Id
  let vote2 = voteOne talk2 heimeshoff.Id
  let vote3 = veto talk3 heimeshoff.Id

  Given
    [
      OrganizerAddedToConference heimeshoff.Id

      TalkWasProposed talk1
      TalkWasProposed talk2
      TalkWasProposed talk3

      VotingWasIssued vote1
      VotingWasIssued vote2
      VotingWasIssued vote3
    ]
  |> When (RemoveOrganizerFromConference heimeshoff.Id)
  |> ThenExpect
      [
        OrganizerRemovedFromConference heimeshoff.Id
        VotingWasRevoked vote1
        VotingWasRevoked vote2
        VotingWasRevoked vote3
      ]

[<Test>]
let ``Organizer can not be remove if not added`` () =
  Given []
  |> When (RemoveOrganizerFromConference heimeshoff.Id)
  |> ThenExpect [ OrganizerWasNotAddedToConference heimeshoff.Id |> Error ]
