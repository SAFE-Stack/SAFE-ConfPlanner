module ProposeAbstractTest

open NUnit.Framework

open Domain.Commands
open Domain.Events
open Testbase

// Scenario
let talk = proposedTalk()

[<Test>]
let ``Can propose an abstract when Call for Papers is open`` () =
  Given
    [
      OrganizerAddedToConference roman
      CallForPapersOpened
    ]
  |> When (ProposeAbstract talk)
  |> ThenExpect [ AbstractWasProposed talk ]


[<Test>]
let ``Can not propose an abstract when Call for Papers is not opened yet`` () =
  Given [ OrganizerAddedToConference roman ]
  |> When (ProposeAbstract talk)
  |> ThenExpect [ ProposingDenied "Call For Papers Not Opened" |> DomainError ]


[<Test>]
let ``Can not propose an abstract when Call for Papers is already closed`` () =
  Given
    [
      OrganizerAddedToConference roman
      CallForPapersClosed
    ]
  |> When (ProposeAbstract talk)
  |> ThenExpect [ ProposingDenied "Call For Papers Closed" |> DomainError]
