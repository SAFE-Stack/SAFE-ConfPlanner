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
  let proposedAbstract = proposedAbstract
  Given conference
  |> When (ProposeAbstract proposedAbstract)
  |> ThenStateShouldBe { conference with ProposedAbstracts = proposedAbstract :: conference.ProposedAbstracts }
  |> WithEvents [AbstractWasProposed proposedAbstract]

[<Test>]
let ``Can not propose an abstract when Call for Papers is not opened yet`` () =
  let conference = conference |> withCallForPapersNotOpened
  let proposedAbstract = proposedAbstract
  Given conference
  |> When (ProposeAbstract proposedAbstract)
  |> ShouldFailWith CallForPapersNotOpened

[<Test>]
let ``Can not propose an abstract when Call for Papers is already closed`` () =
  let conference = conference |> withCallForPapersClosed
  let proposedAbstract = proposedAbstract
  Given conference
  |> When (ProposeAbstract proposedAbstract)
  |> ShouldFailWith CallForPapersClosed