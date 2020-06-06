module AddOrganizerToConferenceTest

open NUnit.Framework

open Domain.Commands
open Domain.Events
open Testbase

[<Test>]
let ``Organizer can be added to a conference`` () =
  Given []
  |> When (AddOrganizerToConference roman)
  |> ThenExpect [ OrganizerAddedToConference roman ]

[<Test>]
let ``Organizer can not be added if already added`` () =
  Given [ OrganizerAddedToConference roman ]
  |> When (AddOrganizerToConference roman)
  |> ThenExpect [ OrganizerAlreadyAddedToConference roman |> DomainError ]
