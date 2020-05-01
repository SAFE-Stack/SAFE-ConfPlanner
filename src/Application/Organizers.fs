namespace Application

open API
open Domain.Model
open EventSourced

module Organizers =

  let heimeshoff = organizer "Marco" "Heimeshoff" "311b9fbd-98a2-401e-b9e9-bab15897dad4"
  let fellien = organizer "Janek" "Felien" "489cc178-9698-458e-9c4e-95488e159f6d"
  let poepke = organizer "Conrad" "Poepke" "6740b188-e5ec-425d-8e97-c8bc6fb0c35a"

  let sachse = organizer "Roman" "Sachse" "f193649a-0901-4c43-9168-37ef306d3262"

  let organizers =
    [
      heimeshoff
      fellien
      poepke
      sachse
    ]

  let queryHandler (query : QueryParameter) : Async<QueryResult> =
    match query with
    | QueryParameter.Organizers ->
        organizers
        |> QueryResult.Organizers
        |> box
        |> Handled

    | _ -> NotHandled
    |> fun x -> async { return x }
