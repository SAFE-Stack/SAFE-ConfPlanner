/// Module of server domain types.
module Server.ServerTypes

open System
open Infrastructure.Types


// Client to Server
type ClientMsg<'CommandPayload,'QueryParameter,'QueryResult> =
  | Connect
  | Command of Command<'CommandPayload>
  | Query of Query<'QueryParameter>

// Server to Client
type ServerMsg<'Event,'QueryResult> =
  | Connected
  | Events of EventSet<'Event>
  | QueryResponse of QueryResponse<'QueryResult>
