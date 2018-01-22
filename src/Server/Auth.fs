/// Login web part and functions for API web part request authorisation with JWT.
module Server.Auth

open Suave
open Suave.RequestErrors

open Infrastructure.FableJson
open Infrastructure.Auth
open Server

(*
man bräuchte:
* eine IdentityProjection (IdentityProvider)
* eine Identity -> Role list Projection (RoleProvider)


Eine Identity wäre erstmal zum Beispiel ein Organizer
jede Identity kann jede Rolle nur einmal haben => fraglich


wenn einer Identity eine Rolle entzogen wurde, müssen die Konferenzen ebenfalls angepasst werden
*)



/// Login web part that authenticates a user and returns a token in the HTTP body.
let login (identityProvider : IdentityProvider)  (userProvider: UserProvider<'User>) (permissionProvider: PermissionProvider<'User,'Permission>) (ctx: HttpContext) =
  async {
    let login =
        ctx.request.rawForm
        |> System.Text.Encoding.UTF8.GetString
        |> ofJson<Infrastructure.Auth.Login>

    try
        let identityOption =
          identityProvider (login.UserName, login.Password)

        let userOption =
          identityOption
          |> Option.bind userProvider

        match identityOption, userOption with
        | Some identity, Some user ->
            let permission =
              user |> permissionProvider

            let user : ServerTypes.UserRights<'User, 'Permission> =
              {
                Username = login.UserName
                Identity = identity
                User = user
                Permissions = permission
              }

            let token = JsonWebToken.encode user

            return! Successful.OK token ctx

         | _ ->
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
