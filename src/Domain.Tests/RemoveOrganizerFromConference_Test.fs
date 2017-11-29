module RemoveOrganizerFromConferenceTest

open NUnit.Framework

open Commands
open Events
open Testbase
open ProposeAbstractTest


[<Test>]
let ``Organizer can be removed from a conference`` () =
  Given [ OrganizerAddedToConference heimeshoff ]
  |> When (RemoveOrganizerFromConference heimeshoff)
  |> ThenExpect [ OrganizerRemovedFromConference heimeshoff ]

[<Test>]
let ``When an Organizer is removed all its votings are revoked`` () =
  let talk1 = proposedTalk()
  let talk2 = proposedTalk()
  let talk3 = proposedTalk()

  let vote1 = voteTwo talk1 heimeshoff
  let vote2 = voteOne talk2 heimeshoff
  let vote3 = veto talk3 heimeshoff

  Given
    [
      OrganizerAddedToConference heimeshoff

      TalkWasProposed talk1
      TalkWasProposed talk2
      TalkWasProposed talk3

      VotingWasIssued vote1
      VotingWasIssued vote2
      VotingWasIssued vote3
    ]
  |> When (RemoveOrganizerFromConference heimeshoff)
  |> ThenExpect
      [
        OrganizerRemovedFromConference heimeshoff
        VotingWasRevoked vote1
        VotingWasRevoked vote2
        VotingWasRevoked vote3
      ]

[<Test>]
let ``Organizer can not be remove if not added`` () =
  Given []
  |> When (RemoveOrganizerFromConference heimeshoff)
  |> ThenExpect [ OrganizerWasNotAddedToConference heimeshoff ]
