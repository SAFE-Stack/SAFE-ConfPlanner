module AddOrganizerToConferenceTest

open NUnit.Framework

open Domain.Commands
open Domain.Events
open Testbase

[<Test>]
let ``Organizer can be added to a conference`` () =
  Given []
  |> When (AddOrganizerToConference heimeshoff.Id )
  |> ThenExpect [ OrganizerAddedToConference heimeshoff.Id ]

[<Test>]
let ``Organizer can not be added if already added`` () =
  Given [ OrganizerAddedToConference heimeshoff.Id ]
  |> When (AddOrganizerToConference heimeshoff.Id)
  |> ThenExpect [ OrganizerAlreadyAddedToConference heimeshoff.Id |> Error ]
