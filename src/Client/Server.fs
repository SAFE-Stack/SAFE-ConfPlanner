module App.Server

open Fable.Remoting.Client
open Fable.Core

let conferenceApi =
  Remoting.createApi()
  |> Remoting.withRouteBuilder Application.API.conferenceRouteBuilder
  |> Remoting.buildProxy<Application.API.ConferenceApi>

let organizerApi =
  Remoting.createApi()
  |> Remoting.withRouteBuilder Application.API.organizerRouteBuilder
  |> Remoting.buildProxy<Application.API.OrganizerApi>
