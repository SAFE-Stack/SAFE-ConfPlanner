/// Module of server domain types.
module Server.ServerTypes

open System

open Infrastructure.Types


/// Represents the rights available for a request
type UserRights =
   { UserName : string }


// Client to Server
type ClientMsg<'Command> =
  | Connect
  | Command of TransactionId*'Command

// Server to Client
type ServerMsg<'Event> =
  | Connected
  | Events of TransactionId*'Event list
