/// Login web part and functions for API web part request authorisation with JWT.
module Server.Auth

open Domain.Model
let rec private oneOf userprovider (username,password)  =
  match userprovider with
  | provider :: rest ->
      match provider username password with
      | None ->
          oneOf rest (username,password)

      | Some user ->
          Some user

  | _ -> None

let userProvider username password =
  if (username = "test" && password = "test") then
    System.Guid.Parse "311b9fbd-98a2-401e-b9e9-bab15897dad4"
    |> OrganizerId
    |> Some
  else
    None


/// Invokes a function that produces the output for a web part if the HttpContext
/// contains a valid auth token. Use to authorise the expressions in your web part
/// code (e.g. WishList.getWishList).
// let useToken ctx f = async {
//     match ctx.request.queryParam "jwt" with // TODO extract into variable
//     | Choice1Of2 jwt ->
//         match JsonWebToken.isValid jwt with
//         | None ->
//             return! FORBIDDEN "Accessing this API is not allowed" ctx

//         | Some token ->
//             return! f token

//     | _ ->
//         return! BAD_REQUEST "Request doesn't contain a JSON Web Token" ctx
// }


open Giraffe
open RequestErrors
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2

let createUserData (login : AuthTypes.Login) organizerId : ServerTypes.UserData =
  let userRights : ServerTypes.UserRights =
    {
      UserName = login.UserName
      OrganizerId = organizerId
    }

  {
      UserName = login.UserName
      OrganizerId = organizerId
      Token = userRights |> JsonWebToken.encode
  }

/// Authenticates a user and returns a token in the HTTP body.
// DEMO08a - tasks on the server
let login (next : HttpFunc) (ctx : HttpContext) =
  task {
    let! login = ctx.BindJsonAsync<AuthTypes.Login>()

    let organizerId =
      (login.UserName,login.Password)
      |> oneOf [ userProvider ]

    match organizerId with
    | Some organizerId ->
        let data = createUserData login organizerId

        return! ctx.WriteJsonAsync data

     | None ->
        return! UNAUTHORIZED "Bearer" "" (sprintf "User '%s' can't be logged in." login.UserName) next ctx
  }

let private missingToken = RequestErrors.BAD_REQUEST "Request doesn't contain a JSON Web Token"
let private invalidToken = RequestErrors.FORBIDDEN "Accessing this API is not allowed"

/// Checks if the HTTP request has a valid JWT token for API.
/// On success it will invoke the given `f` function by passing in the valid token.
let requiresJwtTokenForAPI f : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        (match ctx.TryGetRequestHeader "Authorization" with
        | Some authHeader ->
            let jwt = authHeader.Replace("Bearer ", "")
            match JsonWebToken.isValid jwt with
            | Some token -> f token
            | None -> invalidToken
        | None -> missingToken) next ctx

