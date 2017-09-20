/// Module of server domain types.
module Server.ServerTypes

open System

open Infrastructure.Types


/// Represents the rights available for a request
type UserRights =
   { UserName : string }


// Client to Server
type ClientMsg<'Command,'QueryParameter,'QueryResult> =
  | Connect
  | Command of TransactionId*'Command
  | Query of Query<'QueryParameter>

// Server to Client
type ServerMsg<'Event,'QueryResult> =
  | Connected
  | Events of TransactionId*'Event list
  | QueryResponse of QueryResponse<'QueryResult>
