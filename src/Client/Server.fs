module App.Server

open Fable.Remoting.Client
open Fable.Core

[<Emit("config.baseUrl")>]
let baseUrl : string = jsNative


let conferenceApi =
  Remoting.createApi()
  |> Remoting.withRouteBuilder Application.API.conferenceRouteBuilder
//  |> Remoting.withBaseUrl baseUrl
  |> Remoting.buildProxy<Application.API.ConferenceApi>

let organizerApi =
  Remoting.createApi()
  |> Remoting.withRouteBuilder Application.API.organizerRouteBuilder
//  |> Remoting.withBaseUrl baseUrl
  |> Remoting.buildProxy<Application.API.OrganizerApi>
