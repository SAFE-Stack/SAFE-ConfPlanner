namespace Infrastructure.EventStorage
open Infrastructure

module PostgresStorage =

  open Npgsql.FSharp
  open Thoth.Json.Net
  open Helper

  let select = "SELECT metadata, payload FROM event_store"
  let order = "ORDER BY recorded_at_utc ASC, event_index ASC"

  let private hydrateEventEnvelopes reader =
    let row = Sql.readRow reader
    option {
      let! metadata = Sql.readString "metadata" row
      let! payload = Sql.readString "payload" row

      let eventEnvelope =
        metadata
        |> Decode.Auto.fromString<EventMetadata>
        |> Result.bind (fun metadata ->
            payload
            |> Decode.Auto.fromString<'Event>
            |> Result.map (fun event -> { Metadata = metadata ; Event = event}))

      return eventEnvelope
    }

  let private get (DB_Connection_String db_connection) =
    async {
      return
        db_connection
        |> Sql.connect
        |> Sql.query (sprintf "%s %s" select order)
        |> Sql.executeReader hydrateEventEnvelopes
        |> List.traverseResult id
    }

  let private getStream (DB_Connection_String db_connection) source =
    async {
      return
        db_connection
        |> Sql.connect
        |> Sql.query (sprintf "%s WHERE source = @source %s" select order)
        |> Sql.parameters [ "@source", SqlValue.Uuid source ]
        |> Sql.executeReader hydrateEventEnvelopes
        |> List.traverseResult id
    }

  let private append (DB_Connection_String db_connection) eventEnvelopes =
    let query = """
      INSERT INTO event_store (source, recorded_at_utc, event_index, metadata, payload)
      VALUES (@source, @recorded_at_utc, @event_index, @metadata, @payload)"""

    let parameters =
      eventEnvelopes
      |> List.mapi (fun index eventEnvelope ->
          [
            "@source", SqlValue.Uuid eventEnvelope.Metadata.Source
            "@recorded_at_utc", SqlValue.Date eventEnvelope.Metadata.RecordedAtUtc
            "@event_index", SqlValue.Int index
            "@metadata", SqlValue.Jsonb <| Encode.Auto.toString(0,eventEnvelope.Metadata)
            "@payload", SqlValue.Jsonb <| Encode.Auto.toString(0,eventEnvelope.Event)
          ])

    db_connection
    |> Sql.connect
    |> Sql.executeTransactionAsync [ query, parameters ]
    |> Async.Ignore


  let initialize db_connection : EventStorage<_> =
    {
      Get = fun () -> get db_connection
      GetStream = fun eventSource -> getStream db_connection eventSource
      Append = fun events -> append db_connection events
    }
