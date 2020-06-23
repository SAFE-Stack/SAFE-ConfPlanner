namespace EventSourced.EventStorage

open EventSourced
open Npgsql.FSharp
open Thoth.Json.Net
open Helper

module PostgresStorage =

    let select =
        "SELECT metadata, payload FROM event_store"

    let order =
        "ORDER BY recorded_at_utc ASC, event_index ASC"

    let private hydrateEventEnvelopes (reader: RowReader) =
        option {
            let! metadata = reader.textOrNone "metadata"
            let! payload = reader.textOrNone "payload"

            let eventEnvelope =
                metadata
                |> Decode.Auto.fromString<EventMetadata>
                |> Result.bind (fun metadata ->
                    payload
                    |> Decode.Auto.fromString<'Event>
                    |> Result.map (fun event -> { Metadata = metadata; Event = event }))

            return eventEnvelope
        }

    let private get (DB_Connection_String dbConnection): Async<Result<EventEnvelope<'Event> list, string>> =
        async {
            let result =
                dbConnection
                |> Sql.connect
                |> Sql.query (sprintf "%s %s" select order)
                |> Sql.execute hydrateEventEnvelopes
                |> Result.mapError (fun e -> e.Message)
                |> Result.map (fun t -> t |> List.choose id)
                |> Result.map (fun t -> List.traverseResult id t)

            match result with
            | Ok r -> return r
            | Error ex -> return Error ex
        }

    let private getStream (DB_Connection_String dbConnection) source: Async<EventResult<'Event>> =
        async {
            let result =
                dbConnection
                |> Sql.connect
                |> Sql.query (sprintf "%s WHERE source = @source %s" select order)
                |> Sql.parameters [ "@source", SqlValue.Uuid source ]
                |> Sql.execute hydrateEventEnvelopes
                |> Result.mapError (fun e -> e.Message)
                |> Result.map (fun t -> t |> List.choose id)
                |> Result.map (fun t -> List.traverseResult id t)

            match result with
            | Ok r -> return r
            | Error ex -> return Error ex
        }

    let private append (DB_Connection_String dbConnection) eventEnvelopes =
        let query = """
      INSERT INTO event_store (source, recorded_at_utc, event_index, metadata, payload)
      VALUES (@source, @recorded_at_utc, @event_index, @metadata, @payload)"""

        let parameters =
            eventEnvelopes
            |> List.mapi (fun index eventEnvelope ->
                [ "@source", SqlValue.Uuid eventEnvelope.Metadata.Source
                  "@recorded_at_utc", SqlValue.Date eventEnvelope.Metadata.RecordedAtUtc
                  "@event_index", SqlValue.Int index
                  "@metadata",
                  SqlValue.Jsonb
                  <| Encode.Auto.toString (0, eventEnvelope.Metadata)
                  "@payload",
                  SqlValue.Jsonb
                  <| Encode.Auto.toString (0, eventEnvelope.Event) ])

        dbConnection
        |> Sql.connect
        |> Sql.executeTransactionAsync [ query, parameters ]
        |> Async.Ignore


    let initialize dbConnection: EventStorage<_> =
        { Get = fun () -> get dbConnection
          GetStream = fun eventSource -> getStream dbConnection eventSource
          Append = fun events -> append dbConnection events }
