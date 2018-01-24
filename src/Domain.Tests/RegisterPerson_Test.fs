module RegisterPersonTest

open NUnit.Framework

open Domain.Commands
open Domain.Events
open Testbase

[<Test>]
let ``Person can be registered`` () =
  Given []
  |> When (RegisterPerson heimeshoff )
  |> ThenExpect [ PersonRegistered heimeshoff ]

[<Test>]
let ``Person can not be registered if already registered`` () =
  Given [ PersonRegistered heimeshoff ]
  |> When (RegisterPerson heimeshoff)
  |> ThenExpect [ PersonAlreadyRegistered heimeshoff.Id |> Error ]
