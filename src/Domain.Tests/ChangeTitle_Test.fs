module ChangeTitleTest

open NUnit.Framework

open Domain.Commands
open Domain.Events
open Testbase


[<Test>]
let ``Title can be changed`` () =
  Given
    [
      TitleChanged "Old Title"
    ]
  |> When (ChangeTitle "New Title")
  |> ThenExpect [ TitleChanged "New Title" ]
