/// Module of server domain types.
module Server.ServerTypes

open EventSourced
open Domain.Model
open Server.AuthTypes

/// Represents the rights available for a request
type UserRights =
   {
     UserName : string
     OrganizerId : OrganizerId
   }


type UserData =
  {
    UserName : string
    OrganizerId : OrganizerId
    Token : JWT
 }


// Client to Server
type ClientMsg<'Command,'Query> =
  | Connect
  | Command of CommandEnvelope<'Command>
  | Query of 'Query

// Server to Client
type ServerMsg<'Event,'QueryResult> =
  | Connected
  | Events of EventSet<'Event>
  | QueryResponse of QueryResult
