module Server.Websocket

open EventSourced
open EventSourced.EventSourced
open Suave
open Suave.Sockets.Control
open Suave.WebSocket

open FableJson
open ServerTypes

type Msg<'Command,'Event,'QueryParameter> =
  | Connected
  | Closed
  | Received of ClientMsg<'Command>
  | Events of EventEnvelope<'Event> list

let send (webSocket : WebSocket) (msg : ServerMsg<'Event>) =
  let byteResponse =
    msg
    |> toJson
    |> System.Text.Encoding.ASCII.GetBytes
    |> Suave.Sockets.ByteSegment

  webSocket.send Text byteResponse true
    |> Async.Ignore
    |> Async.Start

let emptyResponse =
  [||] |> Suave.Sockets.ByteSegment

let websocket
  (eventSourced : EventSourced<'Command,'Event,'Query>)
  (webSocket : WebSocket)
  (context: HttpContext) =

    let webSocketHandler =
      MailboxProcessor.Start(fun inbox ->
//        eventSourced.EventPublisher (Msg.Events >> inbox.Post)
        // TODO Subscribe/Unsubscribe events


        let rec loop() =
          async {
            let! msg = inbox.Receive()

            match msg with
            | Connected ->
                printfn "webSocketHandler connected"
                return! loop()

            | Received clientMsg  ->
                match clientMsg with
                | Command envelope ->
                    printfn "handle incoming command with envelope %A..." envelope
                    do! (eventSourced.HandleCommand envelope |> Async.Ignore) // TODO: think of result
                    return! loop()

                | Connect ->
                    printfn "ClientMsg.Connect"
                    ServerMsg.Connected
                    |> send webSocket

                    return! loop()

            | Events (events : EventEnvelope<'Event> list) ->
                printfn "events %A will be send to client..." events
                let response =
                  ServerMsg.Events events
                  |> send webSocket

                return! loop()

            | Closed ->
                 printfn "Client closed connection"
          }

        loop()
      )

    socket {
        let mutable loop = true
        while loop do
            let! msg = webSocket.read()
            match msg with
            | (Text, data, true) ->
                let str =
                  data
                  |> System.Text.Encoding.UTF8.GetString

                let deserialized =
                  str
                  |> ofJson<ClientMsg<'Command>>

                printfn "Received: %A" deserialized
                webSocketHandler.Post (Msg.Received deserialized)

            | (Ping, _, _) ->
                do! webSocket.send Pong emptyResponse true

            | (Pong, _, _) -> ()

            | (Close, _, _) ->
                printfn "%s %s" "Connection closed..." (webSocket.ToString())
                Msg.Closed |> webSocketHandler.Post
                do! webSocket.send Close emptyResponse true
                loop <- false

            | (op, data, fin) ->
              printfn "Unexpected Message: %A %A %A " op fin data
    }

let websocketWithAuth handshake websocket (ctx: HttpContext) =
  Server.Auth.useToken ctx (fun token -> async { return! handshake websocket ctx })

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
