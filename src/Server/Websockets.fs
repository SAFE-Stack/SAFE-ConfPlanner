// module Websocket

// open Suave
// open Suave.Sockets.Control
// open Suave.WebSocket

// open Server.ServerTypes


// open Infrastructure.FableJson
// open Infrastructure.Types

// type Msg<'CommandPayload,'Event,'QueryParameter,'QueryResult> =
//   | Connected
//   | Closed
//   | Received of ClientMsg<'CommandPayload,'QueryParameter,'QueryResult>
//   | Events of EventSet<'Event>
//   | QueryResponse of QueryResponse<'QueryResult>

// let send (webSocket : WebSocket) (msg : ServerMsg<'Event,'QueryResult>) =
//   let byteResponse =
//     msg
//     |> toJson
//     |> System.Text.Encoding.ASCII.GetBytes
//     |> Suave.Sockets.ByteSegment

//   webSocket.send Text byteResponse true
//     |> Async.Ignore
//     |> Async.Start

// let emptyResponse =
//   [||] |> Suave.Sockets.ByteSegment

// let websocket
//   (eventSourced : EventSourced<'CommandPayload,'Event,'QueryParameter,'State,'QueryResult>)
//   (webSocket : WebSocket)
//   (context: HttpContext) =

//     let webSocketHandler =
//       MailboxProcessor.Start(fun inbox ->
//         eventSourced.EventPublisher (Msg.Events >> inbox.Post)

//         let queryReplyChannel = Msg.QueryResponse >> inbox.Post

//         let rec loop() =
//           async {
//             let! msg = inbox.Receive()

//             match msg with
//             | Connected ->
//                 printfn "webSocketHandler connected"
//                 return! loop()

//             | Received clientMsg  ->
//                 match clientMsg with
//                 | Command (header,command as payload) ->
//                     printfn "handle incoming command with header %A..." header
//                     eventSourced.CommandHandler payload
//                     return! loop()

//                 | Query query ->
//                     printfn "handle incoming query %A..." query
//                     eventSourced.QueryManager (query,queryReplyChannel)
//                     return! loop()

//                 | Connect ->
//                     printfn "ClientMsg.Connect"
//                     ServerMsg.Connected
//                     |> send webSocket

//                     return! loop()

//             | Events (header,events as payload) ->
//                 printfn "events %A for header %A will be send to client..." events header
//                 let response =
//                   ServerMsg.Events payload
//                   |> send webSocket

//                 return! loop()

//             | Msg.QueryResponse response ->
//                 printfn "query response %A will be send to client..." response
//                 let response =
//                   ServerMsg.QueryResponse response
//                   |> send webSocket

//                 return! loop()

//             | Closed ->
//                  printfn "Client closed connection"
//           }

//         loop()
//       )

//     socket {
//         let mutable loop = true
//         while loop do
//             let! msg = webSocket.read()
//             match msg with
//             | (Text, data, true) ->
//                 let str =
//                   data
//                   |> System.Text.Encoding.UTF8.GetString

//                 let deserialized =
//                   str
//                   |> ofJson<ClientMsg<'CommandPayload,'QueryParameter,'QueryResult>>

//                 printfn "Received: %A" deserialized
//                 webSocketHandler.Post (Msg.Received deserialized)

//             | (Ping, _, _) ->
//                 do! webSocket.send Pong emptyResponse true

//             | (Pong, _, _) -> ()

//             | (Close, _, _) ->
//                 printfn "%s %s" "Connection closed..." (webSocket.ToString())
//                 Msg.Closed |> webSocketHandler.Post
//                 do! webSocket.send Close emptyResponse true
//                 loop <- false

//             | (op, data, fin) ->
//               printfn "Unexpected Message: %A %A %A " op fin data
//     }

// let websocketWithAuth handshake websocket (ctx: HttpContext) =
//   Server.Auth.useToken ctx (fun token -> async { return! handshake websocket ctx })

// //
// // The FIN byte:
// //
// // A single message can be sent separated by fragments. The FIN byte indicates the final fragment. Fragments
// //
// // As an example, this is valid code, and will send only one message to the client:
// //
// // do! webSocket.send Text firstPart false
// // do! webSocket.send Continuation secondPart false
// // do! webSocket.send Continuation thirdPart true
// //
// // More information on the WebSocket protocol can be found at: https://tools.ietf.org/html/rfc6455#page-34
// //
