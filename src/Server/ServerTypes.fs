/// Module of server domain types.
module Server.ServerTypes

open System


type Event =
  | EventOne of string
  | EventTwo
  | EventThree


type Command =
  | One
  | Two
  | Three
  | Four

type CorrelationId = CorrelationId of System.Guid

/// Represents the rights available for a request
type UserRights =
   { UserName : string }

type ClientMsg =
  | Connect
  | Command of CorrelationId*Command


type ServerMsg =
  | Connected
  | Events of CorrelationId*Event list

