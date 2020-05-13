module Server.WebServer

open System.IO
open Application
open Domain.Commands
open Domain.Events
open Suave
open Suave.Logging
open System.Net
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors

open Suave.WebSocket
open EventSourced.EventSourced
open EventSourced

open Domain
open Websocket
open Fable.Remoting.Server
open Fable.Remoting.Suave
open Application.API

let conferenceReadmodel = Conference.Query.readmodel()

let eventSourced : EventSourced<Command,Event,QueryParameter> =
  {
    EventStoreInit =
      EventStore.initialize

    EventStorageInit =
      EventStorage.InMemoryStorage.initialize

    CommandHandlerInit =
      CommandHandler.initialize Behaviour.behaviour

    QueryHandler = QueryHandler.initialize []

    EventListenerInit =
      EventListener.initialize

    EventHandlers =
      [
        conferenceReadmodel.EventHandler
      ]
  } |> EventSourced

// Run fixtures
Support.Run.run eventSourced


let conferenceWebSocket : WebSocket -> HttpContext -> Async<Choice<unit,Sockets.Error>>  =
  eventSourced
  |> websocket

let organizerApi : WebPart =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Application.API.OrganizerQueryApi.RouteBuilder
    |> Remoting.fromValue Application.Organizers.api
    |> Remoting.buildWebPart

let conferenceQueryApi : WebPart =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Application.API.ConferenceQueryApi.RouteBuilder
    |> Remoting.fromValue (Application.Conference.Query.port conferenceReadmodel.State)
    |> Remoting.buildWebPart


let commandApi : WebPart =
    Remoting.createApi()
    |> Remoting.withRouteBuilder API.CommandApi<_>.RouteBuilder
    |> Remoting.fromValue (Application.CQN.commandPort eventSourced.HandleCommand)
    |> Remoting.buildWebPart


let start clientPath port =
    printfn "Client-HomePath: %A" clientPath
    if not (Directory.Exists clientPath) then
        failwithf "Client-HomePath '%s' doesn't exist." clientPath

    let logger = Logging.Targets.create Logging.Info [| "Suave" |]
    let serverConfig =
        { defaultConfig with
            logger = Targets.create LogLevel.Debug [|"Server"; "Server" |]
            homeFolder = Some clientPath
            bindings = [ HttpBinding.create HTTP (IPAddress.Parse "0.0.0.0") port] }

    let app =
        choose [
            GET >=> choose
              [
                path "/" >=> Files.browseFileHome "index.html"
                pathRegex @"/(public|js|css|Images)/(.*)\.(css|png|gif|jpg|js|map)" >=> Files.browseHome
              ]

            POST >=> choose [
                path Server.Urls.Login >=> Auth.login
            ]

            path Server.Urls.Conference  >=> websocketWithAuth handShake conferenceWebSocket

            conferenceQueryApi
            organizerApi
            commandApi

            NOT_FOUND "Page not found."

        ] >=> logWithLevelStructured Logging.Info logger logFormatStructured

    startWebServer serverConfig app
