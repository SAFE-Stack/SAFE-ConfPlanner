/// Server program entry point module.
module Server.Program

open System
open System.IO
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Giraffe.Serialization.Json
open Giraffe.HttpStatusCodeHandlers.ServerErrors
open Thoth.Json.Giraffe
open Elmish.Streams.AspNetCore.Middleware


open Infrastructure.EventSourced
open Conference.Api
open Domain

let conference () =
  let conferenceProjection,conferenceQueryHandler =
    toProjectionAndQueryHandler Conference.projection Conference.queryHandler

  let conferencesProjection,conferencesQueryHandler =
    toProjectionAndQueryHandler Conferences.projection Conferences.queryHandler

  let organizersProjection,organizersQueryHandler =
    toProjectionAndQueryHandler Organizers.projection Organizers.queryHandler

  eventSourced
    Behaviour.execute
    [conferenceProjection ; conferencesProjection ; organizersProjection]
    [conferenceQueryHandler ; conferencesQueryHandler ; organizersQueryHandler]
    @".\conference_eventstore.json"



let GetEnvVar var =
    match Environment.GetEnvironmentVariable(var) with
    | null -> None
    | value -> Some value

let getPortsOrDefault defaultVal =
    match Environment.GetEnvironmentVariable("GIRAFFE_FABLE_PORT") with
    | null -> defaultVal
    | value -> value |> uint16

let errorHandler (ex : Exception) (logger : ILogger) =
    match ex with
    | _ ->
        logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
        clearResponse >=> INTERNAL_ERROR ex.Message

let configureApp (app : IApplicationBuilder) =
    let conference = conference ()
    let eventObservable = Websocket.eventObservable conference

    app.UseGiraffeErrorHandler(errorHandler)
       .UseWebSockets()
       .UseStream<ServerTypes.Msg<_,_,_,_>>(fun options ->
         { options with
             Stream = Websocket.stream conference eventObservable
             Encode = ServerTypes.Msg<_,_,_,_>.Encode
             Decode = ServerTypes.Msg<_,_,_,_>.Decode
         })
       .UseStaticFiles()
       .UseGiraffe (WebServer.webApp)

let configureServices (services : IServiceCollection) =
    // Add default Giraffe dependencies
    services.AddGiraffe() |> ignore

    services.AddSingleton<IJsonSerializer>(ThothSerializer())
    |> ignore

let configureLogging (loggerBuilder : ILoggingBuilder) =
    loggerBuilder.AddFilter(fun lvl -> lvl.Equals LogLevel.Error)
                 .AddConsole()
                 .AddDebug() |> ignore

[<EntryPoint>]
let main args =
    try
        let args = Array.toList args
        let clientPath =
            match args with
            | clientPath:: _  when Directory.Exists clientPath -> clientPath
            | _ ->
                // did we start from server folder?
                let devPath = Path.Combine("..","Client")
                if Directory.Exists devPath then devPath
                else
                    // maybe we are in root of project?
                    let devPath = Path.Combine("src", "Client")
                    if Directory.Exists devPath then devPath
                    else @"./client"
            |> Path.GetFullPath

        let port = getPortsOrDefault 8085us

        WebHost
            .CreateDefaultBuilder()
            .UseWebRoot(clientPath)
            .UseContentRoot(clientPath)
            .ConfigureLogging(configureLogging)
            .ConfigureServices(configureServices)
            .Configure(Action<IApplicationBuilder> configureApp)
            .UseUrls("http://0.0.0.0:" + port.ToString() + "/")
            .Build()
            .Run()
        0
    with
    | exn ->
        let color = Console.ForegroundColor
        Console.ForegroundColor <- System.ConsoleColor.Red
        Console.WriteLine(exn.Message)
        Console.ForegroundColor <- color
        1
