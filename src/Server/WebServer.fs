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

let conferenceReadmodel = Conference.readmodel()

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
    |> Remoting.withRouteBuilder Application.API.organizerRouteBuilder
    |> Remoting.fromValue Application.Organizers.api
    |> Remoting.buildWebPart

let conferenceApi : WebPart =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Application.API.conferenceRouteBuilder
    |> Remoting.fromValue (Application.Conference.api conferenceReadmodel.State)
    |> Remoting.buildWebPart

let conferenceCommandApi : WebPart =
    Remoting.createApi()
    |> Remoting.withRouteBuilder API.ConferenceCommandApi.RouteBuilder
    |> Remoting.fromValue (Conference.Command.api eventSourced.HandleCommand)
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

            conferenceApi
            organizerApi
            conferenceCommandApi

            NOT_FOUND "Page not found."

        ] >=> logWithLevelStructured Logging.Info logger logFormatStructured

    startWebServer serverConfig app
