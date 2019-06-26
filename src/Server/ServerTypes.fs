/// Module of server domain types.
module Server.ServerTypes

open Infrastructure.Types
open Domain.Model
open Server.AuthTypes
#if FABLE_COMPILER
open Thoth.Json
#else
open Thoth.Json.Net
#endif

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
type ClientToServerMsg<'CommandPayload,'QueryParameter,'QueryResult> =
  | Command of Command<'CommandPayload>
  | Query of Query<'QueryParameter>

// Server to Client
type ServerToClientMsg<'Event,'QueryResult> =
  | Events of EventSet<'Event>
  | QueryResponse of QueryResponse<'QueryResult>

type Msg<'Event,'CommandPayload,'QueryParameter,'QueryResult> =
  | ServerToClient of ServerToClientMsg<'Event,'QueryResult>
  | ClientToServer of ClientToServerMsg<'CommandPayload,'QueryParameter,'QueryResult>

let inline encodeMsg (msg : Msg<_,_,_,_>) : string =
  Encode.Auto.toString(4, msg)

let inline decodeMsg (json: string) : Msg<_,_,_,_> option =
  let result = Decode.Auto.fromString<Msg<_,_,_,_>>(json)
  match result with
  | Ok msg -> Some msg
  | Error _ -> None
