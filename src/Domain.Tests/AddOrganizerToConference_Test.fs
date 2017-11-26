module AddOrganizerToConferenceTest

open NUnit.Framework

open Commands
open Events
open Testbase


[<Test>]
let ``Organizer can be added to a conference`` () =
  Given [ ]
  |> When (AddOrganizerToConference heimeshoff)
  |> ThenExpect [OrganizerAddedToConference heimeshoff]

[<Test>]
let ``Organizer can not be added if already added`` () =
  Given [ OrganizerAddedToConference heimeshoff ]
  |> When (AddOrganizerToConference heimeshoff)
  |> ThenExpect [OrganizerAlreadyAddedToConference]
