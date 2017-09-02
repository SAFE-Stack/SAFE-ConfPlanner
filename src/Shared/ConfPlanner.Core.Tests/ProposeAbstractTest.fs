module ProposeAbstractTest

open NUnit.Framework

open ConfPlannerTestsDSL
open Domain
open System
open Commands
open Events
open Errors
open States
open TestData

[<Test>]
let ``Can propose an abstract when Call for Papers is open`` () =
  let conference = conference |> withCallForPapersOpen
  let talk = proposedTalk()
  Given conference
  |> When (ProposeAbstract talk)
  |> ThenStateShouldBe (conference |> withAbstract talk)
  |> WithEvents [AbstractWasProposed talk]

[<Test>]
let ``Can not propose an abstract when Call for Papers is not opened yet`` () =
  let conference = conference |> withCallForPapersNotOpened
  Given conference
  |> When (ProposeAbstract <| proposedTalk())
  |> ShouldFailWith CallForPapersNotOpened

[<Test>]
let ``Can not propose an abstract when Call for Papers is already closed`` () =
  let conference = conference |> withCallForPapersClosed
  Given conference
  |> When (ProposeAbstract <| proposedTalk())
  |> ShouldFailWith CallForPapersClosed