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
type ClientMsg<'Command> =
  | Connect

// Server to Client
type ServerMsg<'Event> =
  | Connected
  | Events of EventEnvelope<'Event> list
