/// Login web part and functions for API web part request authorisation with JWT.
module Server.Auth

open Suave
open Suave.RequestErrors
open Domain.Model
open FableJson


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


/// Login web part that authenticates a user and returns a token in the HTTP body.
let login (ctx: HttpContext) = async {
  let login =
      ctx.request.rawForm
      |> System.Text.Encoding.UTF8.GetString
      |> ofJson<Server.AuthTypes.Login>

  try
      let organizerId =
        (login.UserName,login.Password)
        |> oneOf [ userProvider ]

      match organizerId with
      | Some organizerId ->
          let user : ServerTypes.UserRights =
            {
              UserName = login.UserName
              OrganizerId = organizerId
            }

          let token = JsonWebToken.encode user

          return! Successful.OK token ctx

       | None ->
          return! failwithf "Could not authenticate %s" login.UserName

  with
  | _ -> return! UNAUTHORIZED (sprintf "User '%s' can't be logged in." login.UserName) ctx
}

/// Invokes a function that produces the output for a web part if the HttpContext
/// contains a valid auth token. Use to authorise the expressions in your web part
/// code (e.g. WishList.getWishList).
let useToken ctx f = async {
    match ctx.request.queryParam "jwt" with // TODO extract into variable
    | Choice1Of2 jwt ->
        match JsonWebToken.isValid jwt with
        | None ->
            return! FORBIDDEN "Accessing this API is not allowed" ctx

        | Some token ->
            return! f token

    | _ ->
        return! BAD_REQUEST "Request doesn't contain a JSON Web Token" ctx
}
