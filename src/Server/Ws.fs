module Ws


open Suave
open Suave.Http
open Suave.Operators
open Suave.Filters
open Suave.Successful
open Suave.Files
open Suave.RequestErrors
open Suave.Logging
open Suave.Utils

open System
open System.Net

open Suave.Sockets
open Suave.Sockets.Control
open Suave.WebSocket

open Server.ServerTypes

open Server.FableJson


type State = int
let initialState = 0

type CommandHandlerMsg =
  | Command of CorrelationId*Command
  | State of State


type WsMsg =
  | Connected
  | Received of ClientMsg
  | Events of CorrelationId*Event list


let behaviour state command =
  printfn "state in behaviour is %i" state
  match command with
  | Command.One -> [Event.EventOne <| string state]
  | Command.Two -> [Event.EventTwo]
  | Command.Three -> [Event.EventThree]
  | Command.Four -> [Event.EventOne <| string state; Event.EventTwo]


(*
Command:
websocketHandler bekommt Nachricht
sendet Command an Command Handler
Command Handler fragt Projection nach state
CommandHandler führt Command aus und bekommt Events
CommdandHandler gibt Events an EventStore
EventStore speichert Events
EventStore published Events Projection
WebsocketHandler bekommt Events und sendet sie an den Client

wh kennt ch
ch kennt es und pj

es kennt pj

pj kennt ws

das ist eine zitkuläre abhängigkeit, also müssen die Parameter injected werden

Query


*)



let updateState (state: State) msg  =
  match msg with
  | Event.EventOne _ -> state + 1
  | Event.EventTwo -> state + 2
  | Event.EventThree -> state + 3

type Subscriber<'a> = 'a -> unit

type ProjectionMsg =
  | Events of Event list
  | AddSubscriber of Subscriber<State>

type ProjectionState = {
  ReadModel : State
  Subscriber : Subscriber<State> list
}

let stateProjection =
  let state = {
      ReadModel = initialState
      Subscriber = []
    }

  MailboxProcessor.Start(fun inbox ->
    let rec loop state =
      async {
        let! msg = inbox.Receive()

        match msg with
        | ProjectionMsg.Events events ->
            printfn "Projection new events received: %A" events
            let newReadModel =
              events
              |> List.fold updateState state.ReadModel

            state.Subscriber
            |> List.iter (fun sub -> sub newReadModel)

            return! loop { state with ReadModel = events |> List.fold updateState state.ReadModel }

        | ProjectionMsg.AddSubscriber subscriber ->
            printfn "New State subscriber %A" subscriber
            return! loop { state with Subscriber = subscriber :: state.Subscriber }

      }

    loop state
  )

type EventStoreState = {
  Events : Event list
  Subscriber : Subscriber<CorrelationId*Event list> list
}

type EventStoreMsg =
  | Add of CorrelationId*Event list
  | AddSubscriber of Subscriber<CorrelationId*Event list>

let eventStore =
  let state = {
      Events =  [Event.EventOne "hallo"; Event.EventTwo; Event.EventThree]  // lese aus locale file, json deserialize
      Subscriber = []
    }

  MailboxProcessor.Start(fun inbox ->
    let rec loop state =
      async {
        let! msg = inbox.Receive()

        match msg with
        | Add (correlationId,newEvents) ->
            let allEvents = state.Events @ newEvents

            printfn "EventStore new Events: %A" newEvents
            printfn "EventStore all Events: %A" allEvents

            newEvents
            |> ProjectionMsg.Events
            |> stateProjection.Post

            state.Subscriber
            |> List.iter (fun sub -> (correlationId,newEvents) |> sub)

            // speichere Events in Json

            return! loop { state with Events = allEvents }

        | AddSubscriber subscriber->
            printfn "New EventStore subscriber %A" subscriber
            return! loop { state with Subscriber = subscriber :: state.Subscriber }
      }

    loop state
  )

let commandHandler =
  MailboxProcessor.Start(fun inbox ->
      stateProjection.Post <| ProjectionMsg.AddSubscriber (CommandHandlerMsg.State >> inbox.Post)

      let rec loop state =

        async {
          let! msg = inbox.Receive()

          match msg with
          | CommandHandlerMsg.Command (correlationId,command) ->
              printfn "CommandHandler received command: %A" command

              let newEvents = behaviour state command

              eventStore.Post <| EventStoreMsg.Add (correlationId,newEvents)

              return! loop state

          | CommandHandlerMsg.State state ->
              printfn "CommandHandler received state: %A" state
              return! loop state
        }

      loop initialState
  )




let ws2 (webSocket : WebSocket) (context: HttpContext) =
  let emptyResponse = [||] |> ByteSegment

  let webSocketHandler =
    MailboxProcessor.Start(fun inbox ->

      eventStore.Post <| EventStoreMsg.AddSubscriber (WsMsg.Events >> inbox.Post)

      let rec loop() =
        async {
          let! msg = inbox.Receive()

          printfn "WSHandler received: %A" msg

          match msg with
          | Connected ->
              printfn "Connected..."
              return! loop()

          | WsMsg.Received clientMsg  ->
              match clientMsg with
              | ClientMsg.Command (correlationId,command) ->
                  printfn "handle incoming command with correlation %A..." correlationId
                  commandHandler.Post <| CommandHandlerMsg.Command (correlationId,command)
                  return! loop()

              | ClientMsg.Connect ->
                  printfn "ClientMsg.Connect"
                  let response =
                    ServerMsg.Connected
                    |> toJson

                  let byteResponse =
                      response
                      |> System.Text.Encoding.ASCII.GetBytes
                      |> ByteSegment

                  webSocket.send Text byteResponse true
                    |> Async.Ignore
                    |> Async.Start

                  return! loop()

          | WsMsg.Events (correlationId,events) ->
              printfn "events %A for correlation %A will be send to client..." events correlationId
              let response =
                ServerMsg.Events (correlationId,events)
                |> toJson

              let byteResponse =
                  response
                  |> System.Text.Encoding.ASCII.GetBytes
                  |> ByteSegment

              webSocket.send Text byteResponse true
              |> Async.Ignore
              |> Async.Start

              return! loop()
        }

      loop()
    )

  socket {
      let mutable loop = true
      while loop do
          let! msg = webSocket.read()
          match msg with
          | (Opcode.Text, data, true) ->
              let str =
                data
                |> System.Text.Encoding.UTF8.GetString

              let deserialized =
                str
                |> ofJson<ClientMsg>

              printfn "Received: %A" deserialized
              webSocketHandler.Post (WsMsg.Received deserialized)

          | (Ping, _, _) ->
              do! webSocket.send Pong emptyResponse true

          | (Pong, _, _) -> ()

          | (Close, _, _) ->
              do! webSocket.send Close emptyResponse true
              printfn "%s" "Connection closed..."
              loop <- false

          | (op, data, fin) ->
            printfn "Unexpected Message: %A %A %A " op fin data
  }
//
// The FIN byte:
//
// A single message can be sent separated by fragments. The FIN byte indicates the final fragment. Fragments
//
// As an example, this is valid code, and will send only one message to the client:
//
// do! webSocket.send Text firstPart false
// do! webSocket.send Continuation secondPart false
// do! webSocket.send Continuation thirdPart true
//
// More information on the WebSocket protocol can be found at: https://tools.ietf.org/html/rfc6455#page-34
//
