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
let ``Organizer can not be remove if not added`` () =
  Given []
  |> When (RemoveOrganizerFromConference heimeshoff)
  |> ThenExpect [ OrganizerWasNotAddedToConference heimeshoff ]
