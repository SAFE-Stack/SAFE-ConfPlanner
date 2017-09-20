module Websocket

open Suave
open Suave.Sockets
open Suave.Sockets.Control
open Suave.WebSocket

open Server.ServerTypes
open Server.FableJson

open Infrastructure.Types

type Msg<'Command,'Event,'QueryParameter,'QueryResult> =
  | Connected
  | Received of ClientMsg<'Command,'QueryParameter,'QueryResult>
  | Events of TransactionId*'Event list
  | QueryResponse of QueryResponse<'QueryResult>

let send (webSocket : WebSocket) (msg : ServerMsg<'Event,'QueryResult>) =
  let byteResponse =
    msg
    |> toJson
    |> System.Text.Encoding.ASCII.GetBytes
    |> ByteSegment

  webSocket.send Text byteResponse true
    |> Async.Ignore
    |> Async.Start

let websocket
  (eventSourced : EventSourced<'Command,'Event,'QueryParameter,'State,'QueryResult>)
  (webSocket : WebSocket)
  (context: HttpContext) =
    let emptyResponse = [||] |> ByteSegment

    let webSocketHandler =
      MailboxProcessor.Start(fun inbox ->
        eventSourced.EventSubscriber (Msg.Events >> inbox.Post)

        let queryReplyChannel = Msg.QueryResponse >> inbox.Post

        let rec loop() =
          async {
            let! msg = inbox.Receive()

            match msg with
            | Connected ->
                printfn "webSocketHandler connected"
                return! loop()

            | Msg.Received clientMsg  ->
                match clientMsg with
                | ClientMsg.Command (transactionId,command) ->
                    printfn "handle incoming command with transactionId %A..." transactionId
                    eventSourced.CommandHandler (transactionId,command)
                    return! loop()

                | ClientMsg.Query query ->
                    printfn "handle incoming query %A..." query
                    eventSourced.QueryHandler (query,queryReplyChannel)
                    return! loop()

                | ClientMsg.Connect ->
                    printfn "ClientMsg.Connect"
                    ServerMsg.Connected
                    |> send webSocket

                    return! loop()

            | Msg.Events (transactionId,events) ->
                printfn "events %A for transactionId %A will be send to client..." events transactionId
                let response =
                  ServerMsg.Events (transactionId,events)
                  |> send webSocket

                return! loop()

            | Msg.QueryResponse response ->
                printfn "query response %A will be send to client..." response
                let response =
                  ServerMsg.QueryResponse response
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
                  |> ofJson<ClientMsg<'Command,'QueryParameter,'QueryResult>>

                printfn "Received: %A" deserialized
                webSocketHandler.Post (Msg.Received deserialized)

            | (Ping, _, _) ->
                do! webSocket.send Pong emptyResponse true

            | (Pong, _, _) -> ()

            | (Close, _, _) ->
                printfn "%s %s" "Connection closed..." (webSocket.ToString())
                do! webSocket.send Close emptyResponse true
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
