module App.Server

open Fable.Remoting.Client
open Fable.Core

let conferenceApi =
  Remoting.createApi()
  |> Remoting.withRouteBuilder Application.API.conferenceRouteBuilder
  |> Remoting.buildProxy<Application.API.ConferenceQueryApi>

let conferenceCommandApi =
  Remoting.createApi()
  |> Remoting.withRouteBuilder Application.API.ConferenceCommandApi.RouteBuilder
  |> Remoting.buildProxy<Application.API.ConferenceCommandApi>

let organizerApi =
  Remoting.createApi()
  |> Remoting.withRouteBuilder Application.API.organizerRouteBuilder
  |> Remoting.buildProxy<Application.API.OrganizerApi>
