module Server.WebServer

open System.IO
open Conference.Api.API
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
open Infrastructure.EventSourced
open Conference.Api
open Domain
open Websocket

//let conferenceWebsocket =
//  let conferenceProjection,conferenceQueryHandler =
//    toProjectionAndQueryHandler Conference.projection Conference.queryHandler
//
//  let conferencesProjection,conferencesQueryHandler =
//    toProjectionAndQueryHandler Conferences.projection Conferences.queryHandler
//
//  let organizersProjection,organizersQueryHandler =
//    toProjectionAndQueryHandler Organizers.projection Organizers.queryHandler
//
//  websocket <|
//    eventSourced
//      Domain.Behaviour.execute
//      [conferenceProjection ; conferencesProjection ; organizersProjection]
//      [conferenceQueryHandler ; conferencesQueryHandler ; organizersQueryHandler]
//      @".\conference_eventstore.json"


let eventSourced : EventSourced<Command,Event,QueryParameter> =
  {
    EventStoreInit =
      EventStore.initialize

    EventStorageInit =
      EventStorage.InMemoryStorage.initialize

    CommandHandlerInit =
      CommandHandler.initialize Behaviour.behaviour

    QueryHandler =
      QueryHandler.initialize
        [
//          QueryHandlers.flavours flavoursInStockReadmodel.State db_connection
        ]

    EventListenerInit =
      EventListener.initialize

    EventHandlers =
      [
//        flavoursInStockReadmodel.EventHandler
//        PersistentReadmodels.flavourSoldHandler db_connection
      ]
  } |> EventSourced


let conferenceWebSocket =
  eventSourced
  |> websocket




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

            // path "/dummyWebsocket" >=> handShake dummyWebsocket

            path Server.Urls.Conference  >=> websocketWithAuth handShake conferenceWebSocket

            NOT_FOUND "Page not found."

        ] >=> logWithLevelStructured Logging.Info logger logFormatStructured

    startWebServer serverConfig app
