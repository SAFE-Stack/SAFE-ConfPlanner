/// Module of server domain types.
module Server.ServerTypes

open Infrastructure.Types
open Domain.Model
open Server.AuthTypes
open Thoth.Json.Net

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
type ClientMsg<'CommandPayload,'QueryParameter,'QueryResult> =
  | Command of Command<'CommandPayload>
  | Query of Query<'QueryParameter>

// Server to Client
type ServerMsg<'Event,'QueryResult> =
  | Events of EventSet<'Event>
  | QueryResponse of QueryResponse<'QueryResult>

type Msg<'Event,'CommandPayload,'QueryParameter,'QueryResult> =
  | ServerMsg of ServerMsg<'Event,'QueryResult>
  | ClientMsg of ClientMsg<'CommandPayload,'QueryParameter,'QueryResult>

  static member Encode (msg: Msg<_,_,_,_>) : string =
    Encode.Auto.toString(4, msg)

  static member Decode (json: string) : Msg<_,_,_,_> option =
    let result = Decode.Auto.fromString<Msg<_,_,_,_>>(json)
    match result with
    | Ok msg -> Some msg
    | Error _ -> None
