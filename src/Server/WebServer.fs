module Server.WebServer

open System.IO
open Suave
open Suave.Logging
open System.Net
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors

open Suave.WebSocket

open Infrastructure.EventSourced
open Conference.Api

open Websocket

let dummyWebsocket =
  let projection,queryHandler =
    toProjectionAndQueryHandler Dummy.projection Dummy.queryHandler

  websocket <|
    eventSourced
      Dummy.behaviour
      [projection]
      [queryHandler]
      @".\dummy_eventstore.json"

let conferenceWebsocket =
  let conferenceProjection,conferenceQueryHandler =
    toProjectionAndQueryHandler Conference.projection Conference.queryHandler

  let conferencesProjection,conferencesQueryHandler =
    toProjectionAndQueryHandler Conferences.projection Conferences.queryHandler

  let organizersProjection,organizersQueryHandler =
    toProjectionAndQueryHandler Organizers.projection Organizers.queryHandler

  websocket <|
    eventSourced
      Behaviour.execute
      [conferenceProjection ; conferencesProjection ; organizersProjection]
      [conferenceQueryHandler ; conferencesQueryHandler ; organizersQueryHandler]
      @".\conference_eventstore.json"


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
                path "/api/users/login" >=> Auth.login
            ]

            path "/dummyWebsocket" >=> handShake dummyWebsocket

            path "/conferenceWebsocket" >=> handShake conferenceWebsocket

            NOT_FOUND "Page not found."

        ] >=> logWithLevelStructured Logging.Info logger logFormatStructured

    startWebServer serverConfig app
