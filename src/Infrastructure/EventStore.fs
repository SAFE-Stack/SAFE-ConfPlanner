module Infrastructure.EventStore

open Types
open FableJson
open System.IO

type Msg<'Event> =
  | GetAllEvents of AsyncReplyChannel<EventResult<'Event>>
  | Events of EventSet<'Event>

let eventStore store : (unit -> EventResult<'Event> Async) * Subscriber<EventSet<'Event>> =
  let mailbox =
    MailboxProcessor.Start(fun inbox ->
      printfn "Start"
      let rec loop() =
          async {
            let! msg = inbox.Receive()

            match msg with
            | Events eventSet ->
                printfn "EventStore received new events: %A" eventSet
                try
                  use streamWriter = new StreamWriter(store, true)

                  eventSet
                  |> toJson
                  |> streamWriter.WriteLine

                  do streamWriter.Flush()
                with
                | :? System.Exception as ex ->
                    ex |> printfn "%A"

                return! loop()

            | GetAllEvents reply ->
                try
                  File.ReadAllLines(store)
                  |> Array.map ofJson<EventSet<'Event>>
                  |> Array.toList
                  |> EventResult.Ok
                  |> reply.Reply

                with
                | :? System.Exception as ex ->
                    printf "%A" ex
                    ex.Message
                    |> EventResult.Error
                    |> reply.Reply

                return! loop()
          }

      loop()

    )

  let read() =
    mailbox.PostAndAsyncReply(GetAllEvents)

  let append =
    Events >> mailbox.Post

  read,append



