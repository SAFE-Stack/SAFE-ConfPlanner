module Websocket

open Suave
open Suave.Sockets
open Suave.Sockets.Control
open Suave.WebSocket

open Server.ServerTypes
open Server.FableJson

open Infrastructure.Types

type WebsocketMsg<'Command,'Event> =
  | Connected
  | Received of ClientMsg<'Command>
  | Events of CorrelationId*'Event list

let send (webSocket : WebSocket) (msg : ServerMsg<'Event>) =
  let byteResponse =
    msg
    |> toJson
    |> System.Text.Encoding.ASCII.GetBytes
    |> ByteSegment

  webSocket.send Text byteResponse true
    |> Async.Ignore
    |> Async.Start

let websocket (commandHandler : MailboxProcessor<CommandHandlerMsg<'Command,'State>>) (eventStore : MailboxProcessor<EventStoreMsg<'Event>>) (webSocket : WebSocket) (context: HttpContext) =
  let emptyResponse = [||] |> ByteSegment

  let webSocketHandler =
    MailboxProcessor.Start(fun inbox ->

      eventStore.Post <| EventStoreMsg.AddSubscriber (WebsocketMsg.Events >> inbox.Post)

      let rec loop() =
        async {
          let! msg = inbox.Receive()

          printfn "webSocketHandler received: %A" msg

          match msg with
          | Connected ->
              return! loop()

          | WebsocketMsg.Received clientMsg  ->
              match clientMsg with
              | ClientMsg.Command (correlationId,command) ->
                  printfn "handle incoming command with correlation %A..." correlationId
                  commandHandler.Post <| CommandHandlerMsg.Command (correlationId,command)
                  return! loop()

              | ClientMsg.Connect ->
                  printfn "ClientMsg.Connect"
                  ServerMsg.Connected
                  |> send webSocket

                  return! loop()

          | WebsocketMsg.Events (correlationId,events) ->
              printfn "events %A for correlation %A will be send to client..." events correlationId
              let response =
                ServerMsg.Events (correlationId,events)
                |> send webSocket

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
                |> ofJson<ClientMsg<'Command>>

              printfn "Received: %A" deserialized
              webSocketHandler.Post (WebsocketMsg.Received deserialized)

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
