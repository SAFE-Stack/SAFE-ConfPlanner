namespace Application

open API
open Domain.Model
open System

module Organizers =

  let roman = { Firstname = "Roman";  Lastname = "Sachse"; Id = OrganizerId <| Guid.Parse "311b9fbd-98a2-401e-b9e9-bab15897dad4" }
  let marco = { Firstname = "Marco";  Lastname = "Heimeshoff"; Id = OrganizerId <| Guid.Parse "8d26085d-5f6b-414f-83fb-127392fd259a" }
  let gien = { Firstname = "Gien";  Lastname = "Verschatse"; Id = OrganizerId <| Guid.Parse "e50bef33-3646-4e5d-8881-9a82548dbc73" }
  let dylan = { Firstname = "Dylan";  Lastname = "Beattie"; Id = OrganizerId <| Guid.Parse "6351508b-fffb-48f2-aab9-780364bd7148" }


  let organizers =
    [
      roman
      marco
      gien
      dylan
    ]


  let api =
    {
      organizers = fun () -> async { return Ok organizers }
    }
