module ProposeAbstractTest

open NUnit.Framework

open ConfPlannerTestsDSL
open Model
open System
open Commands
open Events
open States
open TestData

[<Test>]
let ``Can propose an abstract when Call for Papers is open`` () =
  let conference = conference |> withCallForPapersOpen
  let talk = proposedTalk()
  Given conference
  |> When (ProposeAbstract talk)
  |> ThenExpect [AbstractWasProposed talk]

[<Test>]
let ``Can not propose an abstract when Call for Papers is not opened yet`` () =
  let conference = conference |> withCallForPapersNotOpened
  Given conference
  |> When (ProposeAbstract <| proposedTalk())
  |> ThenExpect [ProposingDenied "Call For Papers Not Opened"]

[<Test>]
let ``Can not propose an abstract when Call for Papers is already closed`` () =
  let conference = conference |> withCallForPapersClosed
  Given conference
  |> When (ProposeAbstract <| proposedTalk())
  |> ThenExpect [ProposingDenied "Call For Papers Closed"]
