module ProposeAbstractTest

open NUnit.Framework

open Commands
open Events
open Testbase

// Scenario
let talk = proposedTalk()

[<Test>]
let ``Can propose an abstract when Call for Papers is open`` () =
  Given
    [
      OrganizerAddedToConference heimeshoff
      CallForPapersOpened
    ]
  |> When (ProposeAbstract talk)
  |> ThenExpect [ AbstractWasProposed talk ]


[<Test>]
let ``Can not propose an abstract when Call for Papers is not opened yet`` () =
  Given [ OrganizerAddedToConference heimeshoff ]
  |> When (ProposeAbstract talk)
  |> ThenExpect [ ProposingDenied "Call For Papers Not Opened" |> Error ]


[<Test>]
let ``Can not propose an abstract when Call for Papers is already closed`` () =
  Given
    [
      OrganizerAddedToConference heimeshoff
      CallForPapersClosed
    ]
  |> When (ProposeAbstract talk)
  |> ThenExpect [ ProposingDenied "Call For Papers Closed" |> Error]
