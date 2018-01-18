/// Login web part and functions for API web part request authorisation with JWT.
module Server.Auth

open Suave
open Suave.RequestErrors

open Infrastructure.FableJson
open Domain.Model
open Server
open Server.AuthTypes

type IdentityProvider =
  (Username * Password) -> Async<Identity option>

type PermissionProvider<'Permission> =
  Identity -> Async<'Permission>

let rec private oneIdentityOf (identityProviders : IdentityProvider list) (credentials : Credentials)=
  async {
    match identityProviders with
    | identityProvider :: rest ->
        let! result = identityProvider credentials

        match result with
        | None ->
            return! oneIdentityOf rest credentials

        | Some identity ->
            return Some identity

    | _ -> return None
  }

let concatPermissions (permissionProviders : PermissionProvider<Roles.Container> list) identity  =
  async {
    let! permissionAsyncs =
      permissionProviders
      |> List.map (fun provider -> provider identity)
      |> Async.Parallel

    let permissions =
      permissionAsyncs
      |> Array.toList

    return permissions
  }

let adminIdentity =
  "9c29b202-d4b3-4472-bec8-afcd4ecbb9f2"
  |> System.Guid.Parse
  |> Identity

let adminId =
  "4ad17799-6c3c-4e04-b1f0-9d7523cfa172"
  |> System.Guid.Parse
  |> AdminId

let organizerId =
  "311b9fbd-98a2-401e-b9e9-bab15897dad4"
  |> System.Guid.Parse
  |> OrganizerId


let adminIdentityProvider (Username username , Password password) =
  async {
    let identity =
      if (username = "admin" && password = "admin") then
        Some adminIdentity
      else
        None

    return identity
  }

let adminRoleProvider identity =
  async {
    let roles =
      match identity with
      | identity when identity = adminIdentity ->
          Roles.empty
          |> Roles.withAdmin adminId

      | _ ->
          Roles.empty

    return roles
  }

let organizerRoleProvider identity =
  async {
    let roles =
      match identity with
      | identity when identity = adminIdentity ->
          Roles.empty
          |> Roles.withOrganizer organizerId

      | _ ->
          Roles.empty

    return roles
  }




(*
man bräuchte:
* eine IdentityProjection (IdentityProvider)
* eine Identity -> Role list Projection (RoleProvider)


Eine Identity wäre erstmal zum Beispiel ein Organizer
jede Identity kann jede Rolle nur einmal haben => fraglich


wenn einer Identity eine Rolle entzogen wurde, müssen die Konferenzen ebenfalls angepasst werden
*)






/// Login web part that authenticates a user and returns a token in the HTTP body.
let login (ctx: HttpContext) = async {
  let login =
      ctx.request.rawForm
      |> System.Text.Encoding.UTF8.GetString
      |> ofJson<Server.AuthTypes.Login>

  try
      let identity =
        (login.UserName,login.Password)
        |> oneIdentityOf [ adminIdentityProvider ]
        |> Async.RunSynchronously

      match identity with
      | Some identity ->
          let permission =
            identity
            |> concatPermissions [ adminRoleProvider ; organizerRoleProvider ]
            |> Async.RunSynchronously
            |> Roles.concat

          let user : ServerTypes.UserRights<'Permission> =
            {
              UserName = login.UserName
              Identity = identity
              Permission = permission
            }

          let token = JsonWebToken.encode user

          return! Successful.OK token ctx

       | None ->
          let (Username username) = login.UserName

          return! failwithf "Could not authenticate %s" username

  with
  | _ ->
      let (Username username) = login.UserName
      return! UNAUTHORIZED (sprintf "User '%s' can't be logged in." username ) ctx
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
