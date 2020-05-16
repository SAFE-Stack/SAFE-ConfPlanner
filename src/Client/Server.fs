module App.Server

open Fable.Remoting.Client
open Domain.Commands
open Domain.Events

let conferenceApi =
  Remoting.createApi()
  |> Remoting.withRouteBuilder Application.API.ConferenceQueryApi.RouteBuilder
  |> Remoting.buildProxy<Application.API.ConferenceQueryApi>

let organizerApi =
  Remoting.createApi()
  |> Remoting.withRouteBuilder Application.API.OrganizerQueryApi.RouteBuilder
  |> Remoting.buildProxy<Application.API.OrganizerQueryApi>

let commandPort =
  Remoting.createApi()
  |> Remoting.withRouteBuilder Application.API.CommandApi<_,_>.RouteBuilder
  |> Remoting.buildProxy<Application.API.CommandApi<Command,Event>>
